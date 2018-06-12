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

            var intended = new string(Enumerable.Repeat(' ', intendation).ToArray());

            builder.Append(intended).Append("span: ").AppendLine(Span.ToYamlString());
        }
    }
}