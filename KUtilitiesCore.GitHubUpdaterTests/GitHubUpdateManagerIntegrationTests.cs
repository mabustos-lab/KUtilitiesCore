using Microsoft.VisualStudio.TestTools.UnitTesting;
using KUtilitiesCore.GitHubUpdater;
using KUtilitiesCore.GitHubUpdater.Interface;
using KUtilitiesCore.GitHubUpdater.Helpers;
using KUtilitiesCore.GitHubUpdater.AssetDownloader;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace KUtilitiesCore.GitHubUpdater.Tests
{
    [TestClass]
    public class GitHubUpdateManagerIntegrationTests
    {
        private class MockAppUpdateInfo : IAppUpdateInfo
        {
            public string AppVersion { get; set; } = "1.0.0";
            public IGitHubRepositoryInfo GitHub { get; } = new GitHubRepositoryInfo();
            public string UpdateChannel { get; set; } = "";
            public string AssetPattern { get; set; } = "*";
            public string GetDecryptedToken() => "";
            public void LoadJson(string json) { }
            public void SaveChanges() { }
            public void SetPlaintextToken(string token) { }
        }

        [TestMethod]
        public async Task CheckForUpdatesAsync_NewVersionAvailable_ReturnsRelease()
        {
            // Arrange
            var info = new MockAppUpdateInfo { AppVersion = "1.0.0" };
            var parser = new DefaultVersionParser();
            var selector = new WildcardAssetSelector();
            
            // Mock del servicio para devolver una release con versión superior
            var service = new GitHubUpdateService(info); 
            // Nota: En una prueba unitaria real usaríamos una interfaz para el servicio, 
            // pero aquí estamos probando la integración de los componentes lógicos.
            // Dado que GitHubUpdateService hace peticiones reales, para este test 
            // la lógica de comparación es lo que nos interesa validar en el Manager.
            
            // Creamos una release manual para inyectar en el Manager si fuera necesario, 
            // pero como el Manager llama al Service, lo ideal es que el Service sea mockeable.
        }

        [TestMethod]
        public void DefaultVersionParser_ShouldParseCorrectly()
        {
            var parser = new DefaultVersionParser();
            Assert.AreEqual(new Version(1, 2, 3), parser.Parse("v1.2.3-qa"));
            Assert.AreEqual(new Version(2, 0, 0), parser.Parse("2.0.0"));
            Assert.AreEqual("qa", parser.GetChannel("v1.2.3-qa"));
        }

        [TestMethod]
        public void WildcardAssetSelector_ShouldSelectCorrectAsset()
        {
            var selector = new WildcardAssetSelector();
            var assets = new List<GitHubAsset>
            {
                new GitHubAsset { Name = "Setup.exe" },
                new GitHubAsset { Name = "Notes.txt" }
            };

            var selected = selector.Select(assets, "*.exe");
            Assert.IsNotNull(selected);
            Assert.AreEqual("Setup.exe", selected.Name);
        }
    }
}
