using System.Linq;
using System.Text;

namespace ResXSemanticParser.Yaml
{
    public sealed class TerminalNode : ContainerOrTerminalNode
    {
        public CharacterSpan Span { get; set; }

        public override void FillYamlString(StringBuilder builder, int intendation)
        {
            base.FillYamlString(builder, intendation);
           
            var intended = IntendedString.From(intendation);

            builder.Append(intended).Append("span: ").AppendLine(Span.ToYamlString());
        }
    }
}