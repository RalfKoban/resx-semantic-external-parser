using NUnit.Framework;
using ResXSemanticParser;

namespace Tests
{
    [TestFixture]
    public class ParserTests
    {
        [Test]
        public void Bla()
        {
            Parser.TryParse(@"D:\Private\MiKo Solutions\resx-semantic-external-parser\test.resx", out var yamlContent);

            Assert.That(yamlContent, Is.EqualTo("bla"));
        }
    }
}
