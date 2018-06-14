using System.Text;

namespace MiKoSolutions.SemanticParsers.ResX.Yaml
{
    public sealed class ParsingError
    {
        public LineInfo Location { get; set; }

        public string ErrorMessage { get; set; }

        public void FillYamlString(StringBuilder builder, int intendation)
        {
            var intended = IntendedString.From(intendation);

            builder.Append(intended).Append("location: ").AppendLine(Location.ToYamlString());
            builder.Append(intended).Append("message: ").AppendLine(ErrorMessage);
        }
    }
}