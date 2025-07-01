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
            string version = "2.1.0";
            string canal = "Production";
            string owner = "MiOrg";
            string repo = "MiRepo";
            string token = "TokenSuperSecreto123";

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
        }
    }
}