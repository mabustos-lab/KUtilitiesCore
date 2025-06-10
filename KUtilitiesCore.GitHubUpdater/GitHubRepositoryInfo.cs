using KUtilitiesCore.Encryption;
using KUtilitiesCore.Encryption.Extensions;
using KUtilitiesCore.GitHubUpdater.Interface;
using System;
using System.Linq;


namespace KUtilitiesCore.GitHubUpdater
{
    internal sealed class GitHubRepositoryInfo : IGitHubRepositoryInfo
    {
        public GitHubRepositoryInfo()
        {
            EncryptedToken = string.Empty;
        }
        public string EncryptedToken { get; internal set; }

        public string[] IssueLabels { get; set; } = ["bug", "reported-from-client"];

        public string Owner { get; set; } = string.Empty;

        public string Repository { get; set; } = string.Empty;
    }
}