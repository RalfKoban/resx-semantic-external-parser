using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ResXSemanticParser.Yaml
{
    [DebuggerDisplay("Type={Type}, Name={Name}")]
    public abstract class ContainerOrTerminalNode
    {
        public string Type { get; set; }

        public string Name { get; set; }

        public LocationSpan LocationSpan { get; set; }

        public virtual string ToYamlString()
        {
            var parts = new List<string>
                            {
                                $"type: {Type}",
                                $"name: {Name}",
                                LocationSpan.ToYamlString(),
                            };

            return string.Join(Environment.NewLine, parts);
        }
    }
}