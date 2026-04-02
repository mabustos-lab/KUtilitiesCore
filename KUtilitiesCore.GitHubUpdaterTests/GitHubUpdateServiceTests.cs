using Microsoft.VisualStudio.TestTools.UnitTesting;
using KUtilitiesCore.GitHubUpdater;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetEnv;
using KUtilitiesCore.GitHubUpdater.Helpers;
using KUtilitiesCore.GitHubUpdater.AssetDownloader;


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
            string version = "0.0.0";
            string canal = Environment.GetEnvironmentVariable("UpdateChannel");
            string owner = "mabustos-lab";
            string repo = "Sefoil.Siomax";
            
            string token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            appUpdateInfo.AppVersion = version;
            appUpdateInfo.UpdateChannel = canal;
            appUpdateInfo.GitHub.API_URL = Environment.GetEnvironmentVariable("API_URL"); 
            appUpdateInfo.GitHub.Owner = owner;
            appUpdateInfo.GitHub.Repository = repo;
            appUpdateInfo.SetPlaintextToken(token);
            //appUpdateInfo.SaveChanges();
            var service = new GitHubUpdateService(appUpdateInfo);
            var asset = new GitHubAssetDownloader(appUpdateInfo.GetToken());
            var parser = new DefaultVersionParser();
            var selector = new WildcardAssetSelector();
            var manager = new GitHubUpdateManager(appUpdateInfo, service, asset, parser, selector);
            var release = manager.CheckForUpdatesAsync().GetAwaiter().GetResult();

            //if(release != null)
            //{
            //    manager.DownloadUpdateAsync(release, "C:\\Users\\Mike\\AppData\\Local\\Temp").GetAwaiter().GetResult();                
            //}
            
            Assert.IsNotNull(release);
        }

    }
}