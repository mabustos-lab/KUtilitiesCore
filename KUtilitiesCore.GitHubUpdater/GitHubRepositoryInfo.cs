using KUtilitiesCore.GitHubUpdater.Interface;
using System;
using System.Linq;
using System.Security;


namespace KUtilitiesCore.GitHubUpdater
{
    public sealed class GitHubRepositoryInfo : IGitHubRepositoryInfo
    {
        public GitHubRepositoryInfo()
        {
            EncryptedToken = string.Empty;
        }
        public string EncryptedToken { get; internal set; }

        public string[] IssueLabels { get; set; } = ["bug", "reported-from-client"];

        public string Owner { get; set; } = string.Empty;

        public string Repository { get; set; } = string.Empty;

        public string API_URL { get; set; } = "https://api.github.com";

      
    }
}