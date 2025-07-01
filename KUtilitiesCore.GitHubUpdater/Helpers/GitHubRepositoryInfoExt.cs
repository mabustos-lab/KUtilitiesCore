using KUtilitiesCore.Encryption;
using KUtilitiesCore.Encryption.Extensions;
using KUtilitiesCore.GitHubUpdater.Interface;
using System;
using System.Linq;
using System.Security;

namespace KUtilitiesCore.GitHubUpdater.Helpers
{
    internal static class GitHubRepositoryInfoExt
    {
        public static SecureString GetSecuredToken(
            this IGitHubRepositoryInfo repoInfo,
            IEncryptionService encryptionService) => encryptionService.Decrypt(repoInfo.EncryptedToken).ToSecureString();
    }
}
