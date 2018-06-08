using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

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
}
