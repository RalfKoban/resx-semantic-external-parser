using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using MiKoSolutions.SemanticParsers.ResX.Yaml;

using File = System.IO.File;

namespace MiKoSolutions.SemanticParsers.ResX
{
    public static class Parser
    {
        private static readonly string LineEnding = Environment.NewLine;

        public static bool TryParse(string path, out string yamlContent)
        {
            var parsingFine = TryParseFile(path, out Yaml.File file);
            yamlContent = file.ToYamlString();
            return parsingFine;
        }

        public static bool TryParseFile(string path, out Yaml.File yamlContent)
        {
            var allText = File.ReadAllText(path);

            if (string.IsNullOrWhiteSpace(allText))
            {
                yamlContent = new Yaml.File
                                  {
                                      Name = path,
                                      LocationSpan = new LocationSpan(new LineInfo(0, 0), new LineInfo(0, 0)),
                                      FooterSpan = new CharacterSpan(0, -1),
                                  };
                return true;
            }

            return CreateYaml(path, allText, out yamlContent);
        }

        private static bool CreateYaml(string path, string allText, out Yaml.File yamlContent)
        {
            var lines = allText.Split(new[] { LineEnding }, StringSplitOptions.None);

            var file = YamlFile(lines, path);

            XDocument document = null;
            try
            {
                document = XDocument.Parse(allText, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            }
            catch (Exception ex)
            {
                file.ParsingErrors.Add(new ParsingError { ErrorMessage = ex.Message });
            }

            var parsingFine = file.ParsingErrors.Count == 0;
            if (parsingFine)
            {
                var root = YamlRoot(lines, allText);

                // adjust footer
                file.FooterSpan = new CharacterSpan(root.FooterSpan.End + 1, allText.Length - 1);

                YamlInfrastructureCommentAndSchema(root, lines, allText);

                file.Children.Add(root);
                root.Children.AddRange(Yaml("resheader", document, lines));
                root.Children.AddRange(Yaml("data", document, lines));
                root.Children.AddRange(Yaml("assembly", document, lines));

                // sort based on span
                root.Children.Sort(new AscendingSpanComparer());
            }

            yamlContent = file;
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

        private static Container YamlRoot(string[] lines, string allText)
        {
            const string TAG = "root";
            const string STARTTAG = "<" + TAG + ">";
            const string ENDTAG = "</" + TAG + ">";

            var endLine = GetLastLineInfo(ENDTAG, lines);

            var headerSpan = GetFirstCharacterSpan(STARTTAG, allText);

            // adjust root header to include XML header
            headerSpan = new CharacterSpan(0, headerSpan.End);

            var footerSpan = GetLastCharacterSpan(ENDTAG, allText);

            return new Container
                       {
                           Type = TAG,
                           Name = TAG,
                           LocationSpan = new LocationSpan(YamlLine(1, 1), endLine),
                           HeaderSpan = headerSpan,
                           FooterSpan = footerSpan,
                       };
        }

        private static CharacterSpan GetLastCharacterSpan(string tag, string allText)
        {
            var value = tag + LineEnding;
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
            var value = tag + LineEnding;
            var index = allText.IndexOf(value, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                value = tag;
                index = allText.IndexOf(value, StringComparison.OrdinalIgnoreCase);
            }

            return new CharacterSpan(index, index + value.Length - 1);
        }

        private static void YamlInfrastructureCommentAndSchema(Container parent, string[] lines, string allText)
        {
            const string ENDTAG = "</xsd:schema>";

            if (allText.LastIndexOf(ENDTAG, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var startLine = GetFirstLineInfo("<!-- ", lines);
                startLine = new LineInfo(startLine.LineNumber, 1); // we want to start at first

                var endLine = GetLastLineInfo(ENDTAG, lines);
                endLine = new LineInfo(endLine.LineNumber, endLine.LinePosition + LineEnding.Length); // we want to include line breaks
                var endTagSpan = GetLastCharacterSpan(ENDTAG, allText);

                parent.Children.Add(new TerminalNode
                                        {
                                            Type = "schema",
                                            Name = "schema",
                                            LocationSpan = new LocationSpan(startLine, endLine),
                                            Span = new CharacterSpan(parent.HeaderSpan.End + 1, endTagSpan.End),
                                        });
            }
        }

        private static IEnumerable<TerminalNode> Yaml(string name, XDocument document, string[] lines) => document.Descendants(name).Select(_ => YamlTerminalNode(_, lines));

        private static TerminalNode YamlTerminalNode(XElement element, string[] lines)
        {
            var textAfterClosingTag = element.NodesAfterSelf().First();

            var startPosition = GetCharacterPositionAtLineStart(element, lines);
            var endPosition = GetCharacterPositionAtLineEnd(textAfterClosingTag, lines);

            var localName = element.Name.LocalName;

            return new TerminalNode
                       {
                           Type = localName,
                           Name = element.Attribute("name")?.Value ?? localName,
                           LocationSpan = new LocationSpan(YamlLineStart(element), YamlLineEnd(textAfterClosingTag)),
                           Span = new CharacterSpan(startPosition, endPosition),
                       };
        }

        private static LineInfo YamlLineStart(IXmlLineInfo node) => YamlLine(node.LineNumber, 1);

        private static LineInfo YamlLineEnd(IXmlLineInfo node)
        {
            var content = node.ToString();

            if (node is XText)
            {
                var charactersToTake = content.IndexOf(LineEnding, StringComparison.OrdinalIgnoreCase) + LineEnding.Length;
                content = content.Substring(0, charactersToTake);
            }

            return YamlLine(node.LineNumber, node.LinePosition - 1 + content.Length);
        }

        private static LineInfo YamlLine(int lineNumber, int linePosition) => new LineInfo(lineNumber, linePosition);

        private static int GetCharacterPositionAtLineStart(IXmlLineInfo info, string[] lines) => CharactersUntilLine(lines, info.LineNumber - 1);

        private static int GetCharacterPositionAtLineEnd(IXmlLineInfo info, string[] lines) => CharactersUntilLine(lines, info.LineNumber) - 1;

        private static int CharactersUntilLine(string[] lines, int linesToTake)
        {
            var lineEndingLengths = linesToTake * LineEnding.Length;
            var charactersBefore = lines.Take(linesToTake).Sum(_ => _.Length) + lineEndingLengths;
            return charactersBefore;
        }

        private sealed class AscendingSpanComparer : IComparer<ContainerOrTerminalNode>
        {
            public int Compare(ContainerOrTerminalNode x, ContainerOrTerminalNode y)
            {
                if (x is TerminalNode tX)
                {
                    if (y is TerminalNode tY)
                    {
                        return tX.Span.Start - tY.Span.Start;
                    }

                    if (y is Container cY)
                    {
                        return tX.Span.Start - cY.HeaderSpan.Start;
                    }
                }

                if (x is Container cX)
                {
                    if (y is TerminalNode tY)
                    {
                        return cX.HeaderSpan.Start - tY.Span.Start;
                    }

                    if (y is Container cY)
                    {
                        return cX.HeaderSpan.Start - cY.HeaderSpan.Start;
                    }
                }

                return 0;
            }
        }
}
}