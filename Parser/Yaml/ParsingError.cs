namespace ResXSemanticParser.Yaml
{
    public sealed class ParsingError
    {
        public LineInfo Location { get; set; }

        public string ErrorMessage { get; set; }
    }
}