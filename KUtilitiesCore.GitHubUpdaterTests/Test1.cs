using KUtilitiesCore.GitHubUpdater;

namespace KUtilitiesCore.GitHubUpdaterTests
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void TestMethod1()
        {
            AppUpdateInfo appUpdateInfo = new AppUpdateInfo() 
            { AppVersion= "1.0.0",
             UpdateChannel = "QA"};
            appUpdateInfo.GitHub.Repository = "Repo";
            appUpdateInfo.GitHub.Owner = "Owner";
            appUpdateInfo.SetPlaintextToken("EsteEsMiSecreto");
            appUpdateInfo.SaveChanges();
        }
    }
}
