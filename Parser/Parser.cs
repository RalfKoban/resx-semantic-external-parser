using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using ResXSemanticParser.Yaml;

using File = System.IO.File;

namespace ResXSemanticParser
{
    public static class Parser
    {
        private static readonly string _lineEnding = Environment.NewLine;

        public static bool TryParse(string path, out string yamlContent)
        {
            var parsingFine = TryParseFile(path, out Yaml.File file);
            yamlContent = file.ToYamlString();
            return parsingFine;
        }

        public static bool TryParseFile(string path, out Yaml.File yamlContent)
        {
            yamlContent = null;

            var allText = File.ReadAllText(path);
            var lines = allText.Split(new[] {_lineEnding }, StringSplitOptions.None);

            XDocument document = null;
            var parsingErrors = string.Empty;
            try
            {
                document = XDocument.Parse(allText, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            }
            catch (Exception ex)
            {
                parsingErrors = ex.Message;
            }

            var parsingFine = string.IsNullOrWhiteSpace(parsingErrors);
            if (parsingFine)
            {
                var file = YamlFile(lines, path);

                var root = YamlRoot(file, document, lines, allText);

                // adjust footer
                file.FooterSpan = new CharacterSpan(root.FooterSpan.End + 1, allText.Length - 1);

                YamlInfrastructureCommentAndSchema(root, document, lines, allText);
                YamlResHeader(root, document, lines);
                YamlData(root, document, lines);
                yamlContent = file;
            }

            return parsingFine;
        }

        private static Yaml.File YamlFile(string[] lines, string fileName)
        {
            return new Yaml.File
                       {
                           Name = fileName,
                           LocationSpan = new LocationSpan(new LineInfo(1, 0), new LineInfo(lines.Length, lines.Last().Length)),
                           FooterSpan = new CharacterSpan(0, -1),
                       };
        }

        private static LineInfo GetFirstLineInfo(string tag, string[] lines)
        {
            var lineNumber = 0;
            var linePosition = -1;
            foreach (var line in lines)
            {
                lineNumber++;

                linePosition = line.IndexOf(tag, StringComparison.OrdinalIgnoreCase);
                if (linePosition != -1)
                {
                    linePosition += tag.Length;
                    break;
                }
            }

            return new LineInfo(lineNumber, linePosition);
        }

        private static LineInfo GetLastLineInfo(string tag, string[] lines)
        {
            var lineNumber = 0;
            var linePosition = -1;
            foreach (var line in lines)
            {
                lineNumber++;

                linePosition = line.LastIndexOf(tag, StringComparison.OrdinalIgnoreCase);
                if (linePosition != -1)
                {
                    linePosition += tag.Length;
                    break;
                }
            }

            return new LineInfo(lineNumber, linePosition);
        }

        private static Container YamlRoot(Yaml.File file, XDocument document, string[] lines, string allText)
        {
            const string TAG = "root";
            const string STARTTAG = "<"+ TAG + ">";
            const string ENDTAG = "</"+ TAG + ">";

            var root = document.Descendants(TAG).First();

            var endLine = GetLastLineInfo(ENDTAG, lines);

            var headerSpan = GetFirstCharacterSpan(STARTTAG, allText);
            var footerSpan = GetLastCharacterSpan(ENDTAG, allText);

            var yamlRoot = new Container
                               {
                                   Type = "root",
                                   Name = "root",
                                   LocationSpan = new LocationSpan(YamlStart(root), endLine),
                                   HeaderSpan = headerSpan,
                                   FooterSpan = footerSpan,

                               };
            file.Children.Add(yamlRoot);

            return yamlRoot;
        }

        private static CharacterSpan GetLastCharacterSpan(string tag, string allText)
        {
            var value = tag + _lineEnding;
            var index = allText.LastIndexOf(value, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                value = tag;
                index = allText.LastIndexOf(value, StringComparison.OrdinalIgnoreCase);
            }

            return new CharacterSpan(index, index + value.Length - 1);
        }

        private static CharacterSpan GetFirstCharacterSpan(string tag, string allText)
        {
            var value = tag + _lineEnding;
            var index = allText.IndexOf(value, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                value = tag;
                index = allText.IndexOf(value, StringComparison.OrdinalIgnoreCase);
            }

            return new CharacterSpan(index, index + value.Length - 1);
        }

        private static void YamlInfrastructureCommentAndSchema(Container parent, XDocument document, string[] lines, string allText)
        {
            const string ENDTAG = "</xsd:schema>";

            var startLine = GetFirstLineInfo("<!-- ", lines);
            startLine = new LineInfo(startLine.LineNumber, 1); // we want to start at first

            var endLine = GetLastLineInfo(ENDTAG, lines);
            endLine = new LineInfo(endLine.LineNumber, endLine.LinePosition + _lineEnding.Length); // we want to include line breaks
            var endTagSpan = GetLastCharacterSpan(ENDTAG, allText);

            var node = new TerminalNode
                           {
                               Type = "schema",
                               Name = "schema",
                               LocationSpan = new LocationSpan(startLine, endLine),
                               Span = new CharacterSpan(parent.HeaderSpan.End + 1, endTagSpan.End),
                           };
            parent.Children.Add(node);
        }

        private static void YamlResHeader(Container parent, XDocument document, string[] lines)
        {
            foreach (var header in document.Descendants("resheader"))
            {
                var container = YamlContainer(lines, header);

                YamlValue(container, header, lines);

                parent.Children.Add(container);
            }
        }

        private static void YamlData(Container parent, XDocument document, string[] lines)
        {
            foreach (var data in document.Descendants("data"))
            {
                var container = YamlContainer(lines, data);

                YamlValue(container, data, lines);
                YamlComment(container, data, lines);

                parent.Children.Add(container);
            }
        }

        private static void YamlValue(Container container, XElement data, string[] lines)
        {
            foreach (var value in data.Descendants("value"))
            {
                var valueNode = YamlTerminalNode(lines, value);
                container.Children.Add(valueNode);
            }
        }

        private static void YamlComment(Container container, XElement data, string[] lines)
        {
            foreach (var comment in data.Descendants("comment"))
            {
                var commentNode = YamlTerminalNode(lines, comment);
                container.Children.Add(commentNode);
            }
        }

        private static Container YamlContainer(string[] lines, XElement element)
        {
            var textAfterClosingTag = element.NodesAfterSelf().First();
            var nodeAfterTag = element.Nodes().First();

            var headerStartPosition = GetCharacterPositionAtLineStart(element, lines);
            var headerEndPosition = GetCharacterPositionAtLineEnd(nodeAfterTag, lines);

            var footerStartPosition = GetCharacterPositionAtLineStart(textAfterClosingTag, lines);
            var footerEndPosition = GetCharacterPositionAtLineEnd(textAfterClosingTag, lines);

            var container = new Container
                                {
                                    Type = element.Name.LocalName,
                                    Name = element.Attribute("name").Value,
                                    LocationSpan = new LocationSpan(YamlStart(element), YamlEnd(textAfterClosingTag)),
                                    HeaderSpan = new CharacterSpan(headerStartPosition, headerEndPosition),
                                    FooterSpan = new CharacterSpan(footerStartPosition, footerEndPosition),
                                };
            return container;
        }

        private static TerminalNode YamlTerminalNode(string[] lines, XElement element)
        {
            var textAfterClosingTag = element.NodesAfterSelf().First();

            var startPosition = GetCharacterPositionAtLineStart(element, lines);
            var endPosition = GetCharacterPositionAtLineEnd(textAfterClosingTag, lines);

            var terminalNode = new TerminalNode
                                   {
                                       Type = element.Name.LocalName,
                                       Name = element.Name.LocalName,
                                       LocationSpan = new LocationSpan(YamlStart(element), YamlEnd(textAfterClosingTag)),
                                       Span = new CharacterSpan(startPosition, endPosition),
                                   };
            return terminalNode;
        }

        private static LineInfo YamlStart(IXmlLineInfo node) => YamlLine(node.LineNumber, node.LinePosition - 1);

        private static LineInfo YamlEnd(IXmlLineInfo node)
        {
            var content = node.ToString();
            if (node is XText text)
            {
                content = text.Value;
                var charactersToTake = content.IndexOf(_lineEnding, StringComparison.OrdinalIgnoreCase) + _lineEnding.Length;
                content = content.Substring(0, charactersToTake);
            }

            return YamlLine(node.LineNumber, node.LinePosition - 1 + content.Length);
        }

        private static LineInfo YamlLine(int lineNumber, int linePosition) => new LineInfo(lineNumber, linePosition);

        private static int GetCharacterPositionAtLineStart(IXmlLineInfo info, string[] lines) => CharactersUntilLine(lines, info.LineNumber - 1);
        
        private static int GetCharacterPositionAtLineEnd(IXmlLineInfo info, string[] lines) => CharactersUntilLine(lines, info.LineNumber) - 1;

        private static int CharactersUntilLine(string[] lines, int linesToTake)
        {
            var lineEndingLengths = linesToTake * _lineEnding.Length;
            var charactersBefore = lines.Take(linesToTake).Sum(_ => _.Length) + lineEndingLengths;
            return charactersBefore;
        }
    }
}