using System;
using System.Text.RegularExpressions;
using KUtilitiesCore.GitHubUpdater.Interface;

namespace KUtilitiesCore.GitHubUpdater.Helpers
{
    /// <summary>
    /// Default implementation of <see cref="IVersionParser"/> using Regex to extract version and channel information.
    /// </summary>
    public class DefaultVersionParser : IVersionParser
    {
        private static readonly Regex VersionRegex = new Regex(@"(\d+\.\d+(?:\.\d+)?(?:\.\d+)?)", RegexOptions.Compiled);

        /// <summary>
        /// Parses the version from the tag name.
        /// </summary>
        /// <param name="tagName">The tag name to parse (e.g., "v1.2.3-qa").</param>
        /// <returns>A <see cref="Version"/> object representing the extracted version.</returns>
        public Version Parse(string tagName)
        {
            if (string.IsNullOrEmpty(tagName)) return new Version(0, 0, 0);

            var match = VersionRegex.Match(tagName);
            if (match.Success)
            {
                var versionStr = match.Groups[1].Value;
                
                if (Version.TryParse(versionStr, out var version))
                {
                    // Normalize: Ensure at least 3 parts for consistency if requested, 
                    // but stay close to how System.Version was parsed.
                    // The requirement says "v1.0" -> 1.0.0, so let's ensure build is at least 0.
                    int major = version.Major;
                    int minor = version.Minor;
                    int build = Math.Max(0, version.Build);
                    int revision = version.Revision;

                    if (revision >= 0)
                        return new Version(major, minor, build, revision);
                    
                    return new Version(major, minor, build);
                }
            }

            return new Version(0, 0, 0);
        }

        /// <summary>
        /// Gets the release channel from the tag name.
        /// </summary>
        /// <param name="tagName">The tag name to parse (e.g., "v1.2.3-qa").</param>
        /// <returns>The extracted channel string (e.g., "qa").</returns>
        public string GetChannel(string tagName)
        {
            if (string.IsNullOrEmpty(tagName)) return string.Empty;

            var match = VersionRegex.Match(tagName);
            if (match.Success)
            {
                var versionStr = match.Groups[1].Value;
                int index = tagName.IndexOf(versionStr) + versionStr.Length;
                if (index < tagName.Length)
                {
                    string remaining = tagName.Substring(index).TrimStart('-', '.');
                    return remaining;
                }
            }

            return string.Empty;
        }
    }
}
