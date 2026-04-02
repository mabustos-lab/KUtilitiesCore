using Microsoft.VisualStudio.TestTools.UnitTesting;
using KUtilitiesCore.GitHubUpdater.Helpers;
using KUtilitiesCore.GitHubUpdater.Interface;
using System.Collections.Generic;

namespace KUtilitiesCore.GitHubUpdaterTests
{
    [TestClass]
    public class WildcardAssetSelectorTests
    {
        private List<GitHubAsset> _assets;
        private IAssetSelector _selector;

        [TestInitialize]
        public void Setup()
        {
            _assets = new List<GitHubAsset>
            {
                new GitHubAsset { Name = "Setup-1.0.0.exe" },
                new GitHubAsset { Name = "MyApp.zip" },
                new GitHubAsset { Name = "Notes.txt" },
                new GitHubAsset { Name = "setup-v2.EXE" }
            };
            _selector = new WildcardAssetSelector();
        }

        [TestMethod]
        public void Select_WithExactExtension_ReturnsCorrectAsset()
        {
            var result = _selector.Select(_assets, "*.zip");
            Assert.IsNotNull(result);
            Assert.AreEqual("MyApp.zip", result.Name);
        }

        [TestMethod]
        public void Select_WithWildcardInMiddle_ReturnsCorrectAsset()
        {
            var result = _selector.Select(_assets, "*Setup*");
            Assert.IsNotNull(result);
            Assert.AreEqual("Setup-1.0.0.exe", result.Name);
        }

        [TestMethod]
        public void Select_IsCaseInsensitive_ReturnsCorrectAsset()
        {
            var result = _selector.Select(_assets, "*.exe");
            Assert.IsNotNull(result);
            // Should return the first one "Setup-1.0.0.exe" even if pattern is lowercase
            Assert.AreEqual("Setup-1.0.0.exe", result.Name);
            
            var result2 = _selector.Select(_assets, "SETUP*");
            Assert.IsNotNull(result2);
            Assert.AreEqual("Setup-1.0.0.exe", result2.Name);
        }

        [TestMethod]
        public void Select_WithNoMatch_ReturnsNull()
        {
            var result = _selector.Select(_assets, "*.msi");
            Assert.IsNull(result);
        }
    }
}
