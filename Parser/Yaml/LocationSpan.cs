using System;

namespace MiKoSolutions.SemanticParsers.ResX.Yaml
{
    public sealed class LocationSpan : IEquatable<LocationSpan>
    {
        public LocationSpan(LineInfo start, LineInfo end)
        {
            Start = start;
            End = end;
        }

        public LineInfo Start { get; }

        public LineInfo End { get; }

        public string ToYamlString() => $"{{start: {Start.ToYamlString()}, end: {End.ToYamlString()}}}";

        public bool Equals(LocationSpan other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Start, other.Start) && Equals(End, other.End);
        }

        public override bool Equals(object obj) => Equals(obj as LocationSpan);

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Start != null ? Start.GetHashCode() : 0) * 397) ^ (End != null ? End.GetHashCode() : 0);
            }
        }
    }
}