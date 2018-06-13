using System;
using System.IO;
using System.Linq;

using NUnit.Framework;

using ResXSemanticParser.Yaml;

using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

using File = ResXSemanticParser.Yaml.File;
using Parser = ResXSemanticParser.Parser;

namespace Tests
{
    [TestFixture]
    public class ParserTests
    {
        private File ObjectUnderTest { get; set; }

        [SetUp]
        public void PrepareTest()
        {
            var codeBasePath = Directory.GetParent(new Uri(typeof(ParserTests).Assembly.CodeBase).LocalPath);
            var filePath = Path.Combine(codeBasePath.Parent.Parent.FullName, "test.resx");

            Parser.TryParseFile(filePath, out var file);
            ObjectUnderTest = file;
        }

        [Test]
        public void File_matches()
        {
            Assert.That(ObjectUnderTest.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 0)));
            Assert.That(ObjectUnderTest.LocationSpan.End, Is.EqualTo(new LineInfo(141, 0)));

            Assert.That(ObjectUnderTest.FooterSpan, Is.EqualTo(new CharacterSpan(6606, 6607)));
        }

        [Test]
        public void Root_matches()
        {
            var node = ObjectUnderTest.Children.Single();

            Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(2, 1)));
            Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(139, 7)));

            Assert.That(node.HeaderSpan, Is.EqualTo(new CharacterSpan(40, 47)));
            Assert.That(node.FooterSpan, Is.EqualTo(new CharacterSpan(6597, 6605)));
        }

        [Test]
        public void Schema_matches()
        {
            var node = ObjectUnderTest.Children.Single().Children.OfType<TerminalNode>().First();

            Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(3, 1)));
            Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(107, 17)));

            Assert.That(node.Span, Is.EqualTo(new CharacterSpan(48, 5266)));
        }

        [Test]
        public void ResHeader_1_matches()
        {
            var node = ObjectUnderTest.Children.Single().Children.OfType<TerminalNode>().First(_ => _.Name == "resmimetype");

            Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(108, 1)));
            Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(110, 16)));

            Assert.That(node.Span, Is.EqualTo(new CharacterSpan(5267, 5356)));
        }

        [Test]
        public void ResHeader_2_matches()
        {
            var node = ObjectUnderTest.Children.Single().Children.OfType<TerminalNode>().First(_ => _.Name == "version");

            Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(111, 1)));
            Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(113, 16)));

            Assert.That(node.Span, Is.EqualTo(new CharacterSpan(5357, 5426)));
        }

        [Test]
        public void ResHeader_3_matches()
        {
            var node = ObjectUnderTest.Children.Single().Children.OfType<TerminalNode>().First(_ => _.Name == "reader");

            Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(114, 1)));
            Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(116, 16)));

            Assert.That(node.Span, Is.EqualTo(new CharacterSpan(5427, 5616)));
        }

        [Test]
        public void ResHeader_4_matches()
        {
            var node = ObjectUnderTest.Children.Single().Children.OfType<TerminalNode>().First(_ => _.Name == "writer");

            Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(117, 1)));
            Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(119, 16)));

            Assert.That(node.Span, Is.EqualTo(new CharacterSpan(5617, 5806)));
        }

        [Test]
        public void Assembly_matches()
        {
            var node = ObjectUnderTest.Children.Single().Children.OfType<TerminalNode>().First(_ => _.Type == "assembly");

            Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(120, 1)));
            Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(120, 140)));

            Assert.That(node.Span, Is.EqualTo(new CharacterSpan(5807, 5946)));
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
