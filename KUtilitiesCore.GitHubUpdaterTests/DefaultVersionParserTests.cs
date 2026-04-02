using Microsoft.VisualStudio.TestTools.UnitTesting;
using KUtilitiesCore.GitHubUpdater.Helpers;
using System;

namespace KUtilitiesCore.GitHubUpdaterTests
{
    [TestClass]
    public class DefaultVersionParserTests
    {
        [TestMethod]
        [DataRow("v1.2.3-qa", 1, 2, 3, "qa")]
        [DataRow("2.0.0", 2, 0, 0, "")]
        [DataRow("release-1.5-dev", 1, 5, 0, "dev")]
        [DataRow("v1.0", 1, 0, 0, "")]
        public void Parse_ShouldExtractVersionAndChannel(string tagName, int major, int minor, int build, string expectedChannel)
        {
            var parser = new DefaultVersionParser();
            
            var version = parser.Parse(tagName);
            var channel = parser.GetChannel(tagName);

            Assert.AreEqual(new Version(major, minor, build), version);
            Assert.AreEqual(expectedChannel, channel);
        }

        [TestMethod]
        [DataRow("", 0, 0, 0, "")]
        [DataRow(null, 0, 0, 0, "")]
        [DataRow("no-version", 0, 0, 0, "")]
        public void Parse_ShouldHandleInvalidInput(string tagName, int major, int minor, int build, string expectedChannel)
        {
            var parser = new DefaultVersionParser();
            
            var version = parser.Parse(tagName);
            var channel = parser.GetChannel(tagName);

            Assert.AreEqual(new Version(major, minor, build), version);
            Assert.AreEqual(expectedChannel, channel);
        }
    }
}
