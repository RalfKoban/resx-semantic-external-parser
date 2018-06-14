using System.Diagnostics;
using System.Text;

namespace MiKoSolutions.SemanticParsers.ResX.Yaml
{
    [DebuggerDisplay("Type={Type}, Name={Name}")]
    public abstract class ContainerOrTerminalNode
    {
        public string Type { get; set; }

        public string Name { get; set; }

        public LocationSpan LocationSpan { get; set; }

        public virtual void FillYamlString(StringBuilder builder, int intendation)
        {
            var intended = IntendedString.From(intendation);

            builder.Append(intended).Append("type: ").AppendLine(Type);
            builder.Append(intended).Append("name: ").AppendLine(Name);
            builder.Append(intended).Append("locationSpan: ").AppendLine(LocationSpan.ToYamlString());
        }
    }
}