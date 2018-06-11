using System;
using System.Collections.Generic;
using System.Linq;

namespace ResXSemanticParser.Yaml
{
    public sealed class File
    {
        public string Name { get; set; }

        public LocationSpan LocationSpan { get; set; }

        public CharacterSpan FooterSpan { get; set; }

        public List<ParsingError> ParsingErrors { get; } = new List<ParsingError>();

        public List<Container> Children { get; } = new List<Container>();

        public string ToYamlString()
        {
            var parsingErrorsDetected = ParsingErrors.Any();

            var parts = new List<string>
                            {
                                "type: file",
                                $"name: {Name}",
                                LocationSpan.ToYamlString(),
                                FooterSpan.ToYamlString("footerSpan"),
                                $"parsingErrorsDetected: {parsingErrorsDetected}"
                            };

            if (Children.Any())
            {
                parts.Add("children:");
                foreach (var child in Children)
                {
                    parts.Add("- " + child.ToYamlString());
                }
            }

            parts.Add(string.Empty);

            return string.Join(Environment.NewLine, parts);
        }
    }
}