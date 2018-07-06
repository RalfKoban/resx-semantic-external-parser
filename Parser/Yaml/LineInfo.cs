using System;

namespace MiKoSolutions.SemanticParsers.ResX.Yaml
{
    public sealed class LineInfo : IEquatable<LineInfo>
    {
        public LineInfo(int lineNumber, int linePosition)
        {
            LineNumber = lineNumber;
            LinePosition = linePosition;
        }

        public int LineNumber { get; }

        public int LinePosition { get; }

        public override string ToString() => $"LineNumber={LineNumber}, LinePosition={LinePosition}";

        public string ToYamlString() => $"[{LineNumber}, {LinePosition}]";

        public bool Equals(LineInfo other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return LineNumber == other.LineNumber && LinePosition == other.LinePosition;
        }

        public override bool Equals(object obj) => Equals(obj as LineInfo);

        public override int GetHashCode()
        {
            unchecked
            {
                return (LineNumber * 397) ^ LinePosition;
            }
        }
    }
}