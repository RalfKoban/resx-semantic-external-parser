using System.Collections.Generic;
using System.Linq;
using System.Text;

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

            var builder = new StringBuilder()
                            .Append("type: ").AppendLine("file")
                            .Append("name: ").AppendLine(Name)
                            .Append("locationSpan: ").AppendLine(LocationSpan.ToYamlString())
                            .Append("footerSpan: ").AppendLine(FooterSpan.ToYamlString())
                            .Append("parsingErrorsDetected: ").AppendLine(parsingErrorsDetected.ToString());

            if (Children.Any())
            {
                builder.AppendLine("children: ");

                foreach (var child in Children)
                {
                    builder.AppendLine("- ");
                    child.FillYamlString(builder, 3);
                    builder.AppendLine();
                }
            }

            return builder.ToString();
        }
    }
}