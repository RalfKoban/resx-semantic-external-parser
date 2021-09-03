using System;
using System.IO;
using System.Linq;

using MiKoSolutions.SemanticParsers.ResX.Yaml;

using NUnit.Framework;

using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace MiKoSolutions.SemanticParsers.ResX
{
    [TestFixture]
    public class ParserTests
    {
        private Yaml.File ObjectUnderTest { get; set; }

        [SetUp]
        public void PrepareTest()
        {
            var codeBasePath = Directory.GetParent(new Uri(typeof(ParserTests).Assembly.CodeBase).LocalPath);
            var filePath = Path.Combine(codeBasePath.Parent.FullName, "test.resx");

            Parser.TryParseFile(filePath, out var file);
            ObjectUnderTest = file;
        }

        [Test]
        public void File_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(ObjectUnderTest.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 0)), "wrong location start");
                Assert.That(ObjectUnderTest.LocationSpan.End, Is.EqualTo(new LineInfo(144, 0)), "wrong location end");

                Assert.That(ObjectUnderTest.FooterSpan, Is.EqualTo(new CharacterSpan(6695, 6696)), "wrong footer span");
            });
        }

        [Test]
        public void Root_matches()
        {
            Assert.Multiple(() =>
            {
                var node = ObjectUnderTest.Children.First(_ => _.Name == "root");

                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 1)), "wrong location start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(142, 7)), "wrong location end");

                Assert.That(node.HeaderSpan, Is.EqualTo(new CharacterSpan(0, 47)), "wrong header span");
                Assert.That(node.FooterSpan, Is.EqualTo(new CharacterSpan(6686, 6694)), "wrong footer span");
            });
        }

        [TestCase("schema",        3, 1, 107,  17,   48, 5266)]
        [TestCase("resmimetype", 108, 1, 110,  16, 5267, 5356)]
        [TestCase("version",     111, 1, 113,  16, 5357, 5426)]
        [TestCase("reader",      114, 1, 116,  16, 5427, 5616)]
        [TestCase("writer",      117, 1, 119,  16, 5617, 5806)]
        [TestCase("Image1",      121, 1, 123,  11, 5947, 6175)]
        [TestCase("String1",     124, 1, 126,  11, 6176, 6264)]
        [TestCase("String2",     127, 1, 132,  11, 6265, 6418)]
        [TestCase("String3",     133, 1, 135,  11, 6419, 6507)]
        [TestCase("String4",     136, 1, 138,  11, 6508, 6596)]
        [TestCase("String5: a",  139, 1, 141,  11, 6597, 6685)]
        public void Child_matches(string name, int startLineNr, int startLinePos, int endLineNr, int endLinePos, int startSpan, int stopSpan)
        {
            Assert.Multiple(() =>
            {
                var node = ObjectUnderTest.Children.SelectMany(_ => _.Children.OfType<TerminalNode>()).First(_ => _.Name == name);

                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(startLineNr, startLinePos)), "wrong location start for " + name);
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(endLineNr, endLinePos)), "wrong location end for " + name);

                Assert.That(node.Span, Is.EqualTo(new CharacterSpan(startSpan, stopSpan)), "wrong span for " + name);
            });
        }

        [Test]
        public void Assembly_matches()
        {
            Assert.Multiple(() =>
            {
                var node = ObjectUnderTest.Children.SelectMany(_ => _.Children.OfType<TerminalNode>()).First(_ => _.Type == "assembly");

                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(120, 1)));
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(120, 140)));

                Assert.That(node.Span, Is.EqualTo(new CharacterSpan(5807, 5946)));
            });
        }

        [Test]
        public void RoundTrip_Yaml_can_be_read()
        {
            var yaml = ObjectUnderTest.ToYamlString();

            VerifyRead(yaml);
        }

        private static void VerifyRead(string yaml)
        {
            try
            {
                var stream = new YamlStream();
                stream.Load(new StringReader(yaml));
            }
            catch (YamlException ex)
            {
                Assert.Fail(ex.Message + Environment.NewLine + yaml);
            }
        }
    }
}
