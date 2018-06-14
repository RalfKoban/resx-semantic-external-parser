using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MiKoSolutions.SemanticParsers.ResX
{
    public static class Program
    {
        const string Category = "RKN Semantic";

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

                Debug.WriteLine($"File to parse: '{fileToParse}'", Category);
                Debug.WriteLine($"Encoding: '{encodingToUse}'", Category);
                Debug.WriteLine($"File to write: {outputFileToWrite}", Category);

                try
                {
                    var success = Parser.TryParse(fileToParse, out var yamlContent) ? "OK" : "KO";

                    Debug.WriteLine($"Parsed result: {success}", Category);

                    File.WriteAllText(outputFileToWrite, yamlContent);

                    Console.WriteLine(success);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception: {ex}", Category);
                    throw;
                }
            }
        }
    }
}
