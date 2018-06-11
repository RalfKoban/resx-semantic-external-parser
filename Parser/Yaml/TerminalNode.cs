using System;

namespace ResXSemanticParser.Yaml
{
    public sealed class TerminalNode : ContainerOrTerminalNode
    {
        public CharacterSpan Span { get; set; }

        public override string ToYamlString()
        {
            var parts = new[]
                            {
                                base.ToYamlString(),
                                Span.ToYamlString("span"),
                            };

            return string.Join(Environment.NewLine, parts);
        }
    }
}