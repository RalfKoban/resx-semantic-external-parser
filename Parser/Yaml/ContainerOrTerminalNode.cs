﻿using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ResXSemanticParser.Yaml
{
    [DebuggerDisplay("Type={Type}, Name={Name}")]
    public abstract class ContainerOrTerminalNode
    {
        public string Type { get; set; }

        public string Name { get; set; }

        public LocationSpan LocationSpan { get; set; }

        public virtual void FillYamlString(StringBuilder builder, int intendation)
        {
            var intended = new string(Enumerable.Repeat(' ', intendation).ToArray());

            builder.Append(intended).Append("type: ").AppendLine(Type);
            builder.Append(intended).Append("name: ").AppendLine(Name);
            builder.Append(intended).Append("locationSpan: ").AppendLine(LocationSpan.ToYamlString());
        }
    }
}