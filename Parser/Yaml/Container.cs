using System;
using System.Collections.Generic;
using System.Linq;

namespace ResXSemanticParser.Yaml
{
    public sealed class Container : ContainerOrTerminalNode
    {
        public CharacterSpan HeaderSpan { get; set; }

        public CharacterSpan FooterSpan { get; set; }

        public List<ContainerOrTerminalNode> Children { get; } = new List<ContainerOrTerminalNode>();

        public override string ToYamlString()
        {
            var parts = new List<string>
                            {
                                base.ToYamlString(),
                                HeaderSpan.ToYamlString("headerSpan"),
                                FooterSpan.ToYamlString("footerSpan"),
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