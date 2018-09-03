﻿using System;
using System.IO;
using System.Linq;

using MiKoSolutions.SemanticParsers.ResX.Yaml;

using NUnit.Framework;

namespace MiKoSolutions.SemanticParsers.ResX
{
    [TestFixture]
    public class ParserAdditionalTests
    {
        private Yaml.File ObjectUnderTest { get; set; }

        [SetUp]
        public void PrepareTest()
        {
            var codeBasePath = Directory.GetParent(new Uri(typeof(ParserTests).Assembly.CodeBase).LocalPath);
            var filePath = Path.Combine(codeBasePath.Parent.FullName, "test2.resx");

            Parser.TryParseFile(filePath, out var file);
            ObjectUnderTest = file;
        }

        [Test]
        public void File_matches()
        {
            Assert.That(ObjectUnderTest.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 0)));
            Assert.That(ObjectUnderTest.LocationSpan.End, Is.EqualTo(new LineInfo(64, 7)));

            Assert.That(ObjectUnderTest.FooterSpan, Is.EqualTo(new CharacterSpan(0, -1)));
        }

        [Test]
        public void Root_matches()
        {
            var node = ObjectUnderTest.Children.First(_ => _.Name == "root");

            Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 1)));
            Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(64, 7)));

            Assert.That(node.HeaderSpan, Is.EqualTo(new CharacterSpan(0, 47)));
            Assert.That(node.FooterSpan, Is.EqualTo(new CharacterSpan(2933, 2939)));
        }

        [TestCase("schema",       3, 1, 48, 17,   48, 2305)]
        [TestCase("resmimetype", 49, 1, 51, 16, 2306, 2395)]
        [TestCase("version",     52, 1, 54, 16, 2396, 2465)]
        [TestCase("reader",      55, 1, 57, 16, 2466, 2655)]
        [TestCase("writer",      58, 1, 60, 16, 2656, 2845)]
        [TestCase("String1",     61, 1, 63, 11, 2846, 2932)]
        public void Child_matches(string name, int startLineNr, int startLinePos, int endLineNr, int endLinePos, int startSpan, int stopSpan)
        {
            Assert.Multiple(() =>
            {
                var node = ObjectUnderTest.Children.SelectMany(_ => _.Children.OfType<TerminalNode>()).First(_ => _.Name == name);

                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(startLineNr, startLinePos)));
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(endLineNr, endLinePos)));

                Assert.That(node.Span, Is.EqualTo(new CharacterSpan(startSpan, stopSpan)));
            });
        }
    }
}