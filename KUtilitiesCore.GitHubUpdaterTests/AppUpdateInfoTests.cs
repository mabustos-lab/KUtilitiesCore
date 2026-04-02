using KUtilitiesCore.GitHubUpdater;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KUtilitiesCore.GitHubUpdaterTests
{
    [TestClass]
    public sealed class AppUpdateInfoTests
    {
        [TestMethod]
        public void AppUpdateInfo_PropiedadesYToken_SonAsignadasCorrectamente()
        {
            // Arrange
            var appUpdateInfo = new AppUpdateInfo();
            string version = "v1.0.0-test";
            string canal = "Production";
            string owner = "mabustos-lab";
            string repo = "master";
            string token = "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";

            // Act
            appUpdateInfo.AppVersion = version;
            appUpdateInfo.UpdateChannel = canal;
            appUpdateInfo.GitHub.Owner = owner;
            appUpdateInfo.GitHub.Repository = repo;
            appUpdateInfo.SetPlaintextToken(token);

            // Assert
            Assert.AreEqual(version, appUpdateInfo.AppVersion);
            Assert.AreEqual(canal, appUpdateInfo.UpdateChannel);
            Assert.AreEqual(owner, appUpdateInfo.GitHub.Owner);
            Assert.AreEqual(repo, appUpdateInfo.GitHub.Repository);
            Assert.AreEqual(token, appUpdateInfo.GetDecryptedToken());
            Assert.AreEqual("*", appUpdateInfo.AssetPattern); // Default value
        }

        [TestMethod]
        public void AppUpdateInfo_LoadJson_RestauraPropiedadesCorrectamente()
        {
            // Arrange
            var appUpdateInfo = new AppUpdateInfo();
            string json = "{\"appVersion\":\"v2.0.0\",\"assetPattern\":\"*.msi\",\"updateChannel\":\"QA\",\"gitHub\":{\"owner\":\"test-owner\",\"repository\":\"test-repo\"}}";

            // Act
            appUpdateInfo.LoadJson(json);

            // Assert
            Assert.AreEqual("v2.0.0", appUpdateInfo.AppVersion);
            Assert.AreEqual("*.msi", appUpdateInfo.AssetPattern);
            Assert.AreEqual("QA", appUpdateInfo.UpdateChannel);
            Assert.AreEqual("test-owner", appUpdateInfo.GitHub.Owner);
            Assert.AreEqual("test-repo", appUpdateInfo.GitHub.Repository);
        }
    }
}