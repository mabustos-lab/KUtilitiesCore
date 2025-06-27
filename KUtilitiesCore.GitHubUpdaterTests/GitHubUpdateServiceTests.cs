using Microsoft.VisualStudio.TestTools.UnitTesting;
using KUtilitiesCore.GitHubUpdater;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetEnv;


namespace KUtilitiesCore.GitHubUpdater.Tests
{
    [TestClass()]
    public class GitHubUpdateServiceTests
    {
        [TestMethod()]
        public void GitHubUpdateServiceTest()
        {
            // Excluir la prueba si se ejecuta en GitHub Actions (variable de entorno 'GITHUB_ACTIONS' == 'true')
            if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
            {
                Assert.Inconclusive("Esta prueba se omite en GitHub Actions.");
            }
            // Cargamos los secretos de .env
            Env.TraversePath().Load();

            AppUpdateInfo appUpdateInfo = new AppUpdateInfo();
            string version = "1.0.0";
            string canal = "dev";
            string owner = "SiomaxPMX";
            string repo = "Sefoil.Siomax";
            
            string token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            appUpdateInfo.AppVersion = version;
            appUpdateInfo.UpdateChannel = canal;
            appUpdateInfo.GitHub.Owner = owner;
            appUpdateInfo.GitHub.Repository = repo;
            appUpdateInfo.SetPlaintextToken(token);
            //appUpdateInfo.SaveChanges();
            GitHubUpdateService service = new GitHubUpdateService(appUpdateInfo);
            var release =service.GetLatestReleaseAsync().GetAwaiter().GetResult();
            Assert.IsNotNull(release);
        }

        [TestMethod()]
        public void GetLatestReleaseAsyncTest()
        {

        }
    }
}