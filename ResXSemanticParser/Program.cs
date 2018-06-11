using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ResXSemanticParser
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            if (args.Length != 2)
                return -1;

            var shell = args[0]; // reserved for future usage
            var flagFile = args[1];

            File.WriteAllBytes(flagFile, new byte[] { 0x42 });

            while (true)
            {
                var fileToParse = await Console.In.ReadLineAsync();

                if ("end".Equals(fileToParse, StringComparison.OrdinalIgnoreCase) || fileToParse == null)
                {
                    // session is done
                    return 0;
                }

                var encodingToUse = Console.In.ReadLine();
                var outputFileToWrite = Console.In.ReadLine();

                Debug.WriteLine($"File to parse: '{fileToParse}'", "RKN Semantic");
                Debug.WriteLine($"Encoding: '{encodingToUse}'", "RKN Semantic");
                Debug.WriteLine($"File to write: {outputFileToWrite}", "RKN Semantic");

                var success = Parser.TryParse(fileToParse, out var yamlContent) ? "OK" : "KO";

                Debug.WriteLine($"Parsed result: {success}", "RKN Semantic");

                File.WriteAllText(outputFileToWrite, yamlContent);

                Console.WriteLine(success);
            }
        }

        static void Main2(string[] args)
        {
            var shell = args[0];
            var flagFile = args[1];

            Console.WriteLine($"Shell: {shell} FlagFile: {flagFile}");

            Parser.TryParse(@"D:\Private\MiKo Solutions\resx-semantic-external-parser\test.resx", out var yamlContent);
            Console.WriteLine(yamlContent);
            Console.ReadLine();
        }
    }
}
