using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ResXSemanticParser
{
    class Program
    {
        /*
                static void Main(string[] args)
                {
                    var shell = args[0]; // reserved for future usage
                    var flagFile = args[1];

                    File.Create(flagFile).Close();

                    while (true)
                    {
                        var fileToParse = Console.ReadLine();

                        if ("end".Equals(fileToParse, StringComparison.OrdinalIgnoreCase))
                        {
                            // session is done
                            return;
                        }

                        var encodingToUse = Console.ReadLine();
                        var outputFileToWrite = Console.ReadLine();

                        var success = Parser.TryParse(fileToParse, out var yamlContent);
                        File.WriteAllText(outputFileToWrite, yamlContent);

                        Console.WriteLine(success ? "OK" : "KO");
                    }
                }
        */

        static void Main(string[] args)
        {
            var shell = args[0];
            var flagFile = args[1];

            Console.WriteLine($"Shell: {shell} FlagFile: {flagFile}");

            
            Parser.TryParse(@"D:\Private\MiKo Solutions\resx-semantic-external-parser\test.resx", out var yamlContent);
            Console.WriteLine(yamlContent);
            Console.ReadLine();
        }
    }

    static class Parser
    {
        private const string TYPE_FILE = "file"; // must be fine
        private const string TYPE_DATA = "data"; // can be named anything
        private const string TYPE_VALUE = "value"; // can be named anything

        private const int MARGIN_FILE = 3;
        private const int MARGIN_DATA = 3;
        private const int MARGIN_VALUE = 3;

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
            YamlFile(builder, lines, allText, path, !parsingFine);

            if (parsingFine)
            {
                YamlData(builder, document, lines, allText);
            }

            yamlContent = builder.ToString();
            return parsingFine;
        }

        private static void YamlFile(StringBuilder builder, string[] lines, string allText, string fileName, bool parsingErrorsDetected)
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

        private static void YamlData(StringBuilder builder, XDocument document, string[] lines, string allText)
        {
            var additionalLineEndingCharacters = Environment.NewLine.Length;
            foreach (var data in document.Descendants("data"))
            {
                var contents = new[]
                                {
                               $"- type: {TYPE_DATA}",
                               $"  name: {data.Attribute("name").Value}",
                                YamlSpan("  locationSpan", YamlStart(data), YamlEnd(data, lines, additionalLineEndingCharacters)),
                             // TODO: HEADER / FOOTER SPAN
                                "  children:",
                                    string.Empty,
                           };

                foreach (var content in contents)
                {
                    WriteLine(MARGIN_DATA, builder, content);
                }

                GetStartingCharacterPosition(data, lines, allText);

                YamlValue(builder, data, lines, allText);
                YamlComment(builder, data, lines, allText);
            }
        }

        private static void YamlComment(StringBuilder builder, XElement datas, string[] lines, string allText)
        {
            foreach (var comments in datas.Descendants("comment"))
            {
                GetStartingCharacterPosition(comments, lines, allText);
            }
        }

        private static void YamlValue(StringBuilder builder, XElement datas, string[] lines, string allText)
        {
            foreach (var values in datas.Descendants("value"))
            {
                var intendation = Intendation(9);

                GetStartingCharacterPosition(values, lines, allText);
            }
        }

        private static void GetStartingCharacterPosition(XElement element, string[] lines, string allText)
        {
            var info = (IXmlLineInfo) element;
            var lineNumber = info.LineNumber;
            var linePosition = info.LinePosition;
            var position = GetStartingCharacterPosition(lines, lineNumber, linePosition);
            var remaining = new string(allText.Remove(0, position).Take(10).ToArray()) + "...";

            // Console.WriteLine("Line: {0}, Position: {1}, CharPosition: {2}, Remaining: \"{3}\"", lineNumber, linePosition, position, remaining);
        }

        private static int GetStartingCharacterPosition(string[] lines, int lineNumber, int linePosition)
        {
            var lineEndingsCharactersLength = Environment.NewLine.Length;

            var linesToTake = lineNumber - 1;
            var linesEndingsToTake = linesToTake - 1; // because I'm in last line of linesToTake, hence I'm not allowed to add that line ending as well
            var lineEndingLengths = linesEndingsToTake * lineEndingsCharactersLength;

            var charactersBefore = lines.Take(linesToTake).Sum(_ => _.Length) + lineEndingLengths;

            return charactersBefore + linePosition;
        }

        private static string Intendation(int count)
        {
            return new string(Enumerable.Repeat(' ', count).ToArray());
        }

        private static void WriteLine(int intendation, StringBuilder builder, string content)
        {
            builder.Append(Intendation(intendation)).AppendLine(content);
        }

        private static string YamlSpan(string name, int start, int end) => $"{name}: [{start},{end}]";

        private static string YamlSpan(string name, params string[] nested) => $"{name}: {{ {string.Join(", ", nested)} }}";

        private static string YamlStart(IXmlLineInfo node) => YamlSpan("start", node.LineNumber, node.LinePosition);

        private static string YamlEnd(IXmlLineInfo node, string[] lines, int additionalCharacters) => YamlSpan("end", node.LineNumber, lines[node.LineNumber - 1].Length + additionalCharacters);
    }
}
