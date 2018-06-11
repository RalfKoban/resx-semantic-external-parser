using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ResXSemanticParser
{
    public static class Parser
    {
        private const string TYPE_FILE = "file"; // must be fine
        private const string TYPE_ROOT = "root"; // can be named anything
        private const string TYPE_DATA = "data"; // can be named anything
        private const string TYPE_RESHEADER = "resheader"; // can be named anything
        private const string TYPE_VALUE = "value"; // can be named anything
        private const string TYPE_COMMENT = "comment"; // can be named anything

        private const int MARGIN_FILE = 3;
        private const int MARGIN_ROOT = 3;
        private const int MARGIN_DATA = 6;
        private const int MARGIN_RESHEADER = 6;
        private const int MARGIN_VALUE = 9;
        private const int MARGIN_COMMENT = 9;

        private static readonly string _lineEnding = Environment.NewLine;


        public static bool TryParse(string path, out string yamlContent)
        {
            var lines = File.ReadAllLines(path);
            var allText = File.ReadAllText(path);

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

            var builder = new StringBuilder();
            YamlFile(builder, lines, path, !parsingFine);

            if (parsingFine)
            {
                YamlRoot(builder, document, lines, allText);
                YamlInfrastructureCommentAndSchema(builder, document, lines);
                YamlResHeader(builder, document, lines);
                YamlData(builder, document, lines);
            }

            yamlContent = builder.ToString();
            return parsingFine;
        }

        private static void YamlFile(StringBuilder builder, string[] lines, string fileName, bool parsingErrorsDetected)
        {
            var contents = new[]
                               {
                                   $"type: {TYPE_FILE}",
                                   $"name: {fileName}",
                                   YamlSpan("locationSpan", YamlSpan("start", 1, 0), YamlSpan("end", lines.Length + 1, lines.Last().Length)),
                                   YamlSpan("footerSpan", 0, -1),
                                   $"parsingErrorsDetected: {parsingErrorsDetected}",
                                   "children:",
                                   string.Empty,
                               };

            foreach (var content in contents)
            {
                WriteLine(MARGIN_FILE, builder, content);
            }
        }

        private static void YamlRoot(StringBuilder builder, XDocument document, string[] lines, string allText)
        {
            const string ENDTAG = "</root>";

            foreach (var root in document.Descendants("root"))
            {
                var lineCount = 0;
                var linePosition = -1;
                foreach (var line in lines)
                {
                    lineCount++;

                    linePosition = line.LastIndexOf(ENDTAG, StringComparison.OrdinalIgnoreCase);
                    if (linePosition != -1)
                    {
                        linePosition += ENDTAG.Length;
                        break;
                    }
                }

                var startPosition = GetCharacterPositionAtLineStart(root, lines);
                var endPosition = allText.LastIndexOf(ENDTAG, StringComparison.OrdinalIgnoreCase) + ENDTAG.Length - 1;

                var contents = new[]
                                   {
                                       $"- type: {TYPE_ROOT}",
                                       "  name: root",
                                       YamlSpan("  locationSpan", YamlStart(root), YamlEnd(lineCount, linePosition)),
                                       YamlSpan("  span", startPosition, endPosition),
                                       "  children:",
                                       string.Empty,
                                   };

                foreach (var content in contents)
                {
                    WriteLine(MARGIN_ROOT, builder, content);
                }
            }
        }

        private static void YamlInfrastructureCommentAndSchema(StringBuilder builder, XDocument document, string[] lines)
        {
        }

        private static void YamlResHeader(StringBuilder builder, XDocument document, string[] lines)
        {
            foreach (var header in document.Descendants("resheader"))
            {
                var textAfterClosingTag = header.NodesAfterSelf().First();

                var startPosition = GetCharacterPositionAtLineStart(header, lines);
                var endPosition = GetCharacterPositionAtLineEnd(textAfterClosingTag, lines);

                var contents = new[]
                                   {
                                       $"- type: {TYPE_RESHEADER}",
                                       $"  name: {header.Attribute("name").Value}",
                                       YamlSpan("  locationSpan", YamlStart(header), YamlEnd(textAfterClosingTag)),
                                       YamlSpan("  span", startPosition, endPosition),
                                       string.Empty,
                                   };

                foreach (var content in contents)
                {
                    WriteLine(MARGIN_RESHEADER, builder, content);
                }
            }
        }

            private static void YamlData(StringBuilder builder, XDocument document, string[] lines)
        {
            foreach (var data in document.Descendants("data"))
            {
                var textAfterClosingTag = data.NodesAfterSelf().First();
                var nodeAfterTag = data.Nodes().First();

                var headerStartPosition = GetCharacterPositionAtLineStart(data, lines);
                var headerEndPosition = GetCharacterPositionAtLineEnd(nodeAfterTag, lines);

                var footerStartPosition = GetCharacterPositionAtLineStart(textAfterClosingTag, lines);
                var footerEndPosition = GetCharacterPositionAtLineEnd(textAfterClosingTag, lines);

                var contents = new[]
                                   {
                                       $"- type: {TYPE_DATA}",
                                       $"  name: {data.Attribute("name").Value}",
                                       YamlSpan("  locationSpan", YamlStart(data), YamlEnd(textAfterClosingTag)),
                                       YamlSpan("  headerSpan", headerStartPosition, headerEndPosition),
                                       YamlSpan("  footerSpan", footerStartPosition, footerEndPosition),
                                       "  children:",
                                       string.Empty,
                                   };

                foreach (var content in contents)
                {
                    WriteLine(MARGIN_DATA, builder, content);
                }

                YamlValue(builder, data, lines);
                YamlComment(builder, data, lines);
            }
        }

        private static void YamlValue(StringBuilder builder, XElement datas, string[] lines)
        {
            foreach (var value in datas.Descendants("value"))
            {
                var textAfterClosingTag = value.NodesAfterSelf().First();

                var startPosition = GetCharacterPositionAtLineStart(value, lines);
                var endPosition = GetCharacterPositionAtLineEnd(textAfterClosingTag, lines);

                var contents = new[]
                                   {
                                       $"- type: {TYPE_VALUE}",
                                       "  name: value",
                                       YamlSpan("  locationSpan", YamlStart(value), YamlEnd(textAfterClosingTag)),
                                       YamlSpan("  span", startPosition, endPosition),
                                       string.Empty,
                                   };

                foreach (var content in contents)
                {
                    WriteLine(MARGIN_VALUE, builder, content);
                }
            }
        }

        private static void YamlComment(StringBuilder builder, XElement datas, string[] lines)
        {
            foreach (var comment in datas.Descendants("comment"))
            {
                var textAfterClosingTag = comment.NodesAfterSelf().First();

                var startPosition = GetCharacterPositionAtLineStart(comment, lines);
                var endPosition = GetCharacterPositionAtLineEnd(textAfterClosingTag, lines);

                var contents = new[]
                                   {
                                       $"- type: {TYPE_COMMENT}",
                                       "  name: comment",
                                       YamlSpan("  locationSpan", YamlStart(comment), YamlEnd(textAfterClosingTag)),
                                       YamlSpan("  span", startPosition, endPosition),
                                       string.Empty,
                                   };

                foreach (var content in contents)
                {
                    WriteLine(MARGIN_COMMENT, builder, content);
                }
            }
        }

        private static string Intendation(int count) => new string(Enumerable.Repeat(' ', count).ToArray());

        private static void WriteLine(int intendation, StringBuilder builder, string content) => builder.Append(Intendation(intendation)).AppendLine(content);

        private static string YamlSpan(string name, int start, int end) => $"{name}: [{start}, {end}]";

        private static string YamlSpan(string name, params string[] nested) => $"{name}: {{ {string.Join(", ", nested)} }}";

        private static string YamlStart(IXmlLineInfo node) => YamlStart(node.LineNumber, node.LinePosition - 1);

        private static string YamlStart(int lineNumber, int linePosition) => YamlSpan("start", lineNumber, linePosition);

        private static string YamlEnd(IXmlLineInfo node)
        {
            var content = node.ToString();
            if (node is XText text)
            {
                content = text.Value;
                var charactersToTake = content.IndexOf(_lineEnding, StringComparison.OrdinalIgnoreCase) + _lineEnding.Length;
                content = content.Substring(0, charactersToTake);
            }

            return YamlEnd(node.LineNumber, node.LinePosition - 1 + content.Length);
        }

        private static string YamlEnd(int lineNumber, int linePosition) => YamlSpan("end", lineNumber, linePosition);

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