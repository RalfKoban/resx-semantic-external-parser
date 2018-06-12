using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResXSemanticParser.Yaml
{
    public sealed class Container : ContainerOrTerminalNode
    {
        public CharacterSpan HeaderSpan { get; set; }

        public CharacterSpan FooterSpan { get; set; }

        public List<ContainerOrTerminalNode> Children { get; } = new List<ContainerOrTerminalNode>();

        public override void FillYamlString(StringBuilder builder, int intendation)
        {
            base.FillYamlString(builder, intendation);

            var intended = new string(Enumerable.Repeat(' ', intendation).ToArray());

            builder.Append(intended).Append("headerSpan: ").AppendLine(HeaderSpan.ToYamlString());
            builder.Append(intended).Append("footerSpan: ").AppendLine(FooterSpan.ToYamlString());

            if (Children.Any())
            {
                builder.Append(intended).AppendLine("children: ");

                foreach (var child in Children)
                {
                    builder.Append(intended).AppendLine("- ");
                    child.FillYamlString(builder, intendation + 3);
                    builder.AppendLine();
                }
            }
        }
    }
}