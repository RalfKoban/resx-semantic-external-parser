using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
        public void File_LocationSpan_matches()
        {
            var fileSpan = ObjectUnderTest.LocationSpan;

            Assert.That(fileSpan.Start, Is.EqualTo(new LineInfo(1, 0)));
            Assert.That(fileSpan.End, Is.EqualTo(new LineInfo(141, 0)));
        }

        [Test]
        public void File_FooterSpan_matches()
        {
            Assert.That(ObjectUnderTest.FooterSpan, Is.EqualTo(new CharacterSpan(6606, 6607)));
        }

        [Test]
        public void Root_LocationSpan_matches()
        {
            var root = ObjectUnderTest.Children.Single();

            Assert.That(root.LocationSpan.Start, Is.EqualTo(new LineInfo(2, 1)));
            Assert.That(root.LocationSpan.End, Is.EqualTo(new LineInfo(139, 7)));
        }

        [Test]
        public void Root_HeaderSpan_matches()
        {
            var root = ObjectUnderTest.Children.Single();

            Assert.That(root.HeaderSpan, Is.EqualTo(new CharacterSpan(40, 47)));
        }

        [Test]
        public void Root_FooterSpan_matches()
        {
            var root = ObjectUnderTest.Children.Single();

            Assert.That(root.FooterSpan, Is.EqualTo(new CharacterSpan(6597, 6605)));
        }

        [Test]
        public void Schema_LocationSpan_matches()
        {
            var schema = ObjectUnderTest.Children.Single().Children.OfType<TerminalNode>().First();
            var span = schema.LocationSpan;

            Assert.That(span.Start, Is.EqualTo(new LineInfo(3, 1)));
            Assert.That(span.End, Is.EqualTo(new LineInfo(107, 17)));
        }

        [Test]
        public void Schema_Span_matches()
        {
            var schema = ObjectUnderTest.Children.Single().Children.OfType<TerminalNode>().First();

            Assert.That(schema.Span, Is.EqualTo(new CharacterSpan(48, 5266)));
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

        [Test]
        public void Bla()
        {
            var s = @"type: file
name: Z:\Workspaces\25368\9\Tests\test.resx
locationSpan : {start: [1, 0], end: [141, 0]}
footerSpan: [6606, 6607]
parsingErrorsDetected: False
children:
- type: root
  name: root
  locationSpan : {start: [2, 1], end: [139, 7]}
  headerSpan: [40, 47]
  footerSpan: [6597, 6605]";

            VerifyRead(s);
        }
    }
}
