using System;

namespace ResXSemanticParser.Yaml
{
    public sealed class CharacterSpan : IEquatable<CharacterSpan>
    {
        public CharacterSpan(int start, int end)
        {
            Start = start;
            End = end;
        }

        public int Start { get; }

        public int End { get; }

        public override string ToString() => $"Start={Start}, End={End}";

        public string ToYamlString() => $"[{Start}, {End}]";

        public bool Equals(CharacterSpan other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Start == other.Start && End == other.End;
        }

        public override bool Equals(object obj) => Equals(obj as CharacterSpan);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Start * 397) ^ End;
            }
        }
    }
}