namespace KUtilitiesCore.GitHubUpdater.Interface
{
    /// <summary>
    /// Interface for parsing version and channel information from tag names.
    /// </summary>
    public interface IVersionParser
    {
        /// <summary>
        /// Parses the version from the tag name.
        /// </summary>
        /// <param name="tagName">The tag name to parse.</param>
        /// <returns>A <see cref="System.Version"/> representing the parsed version.</returns>
        System.Version Parse(string tagName);

        /// <summary>
        /// Gets the channel from the tag name.
        /// </summary>
        /// <param name="tagName">The tag name to parse.</param>
        /// <returns>The channel string (e.g., "qa", "dev").</returns>
        string GetChannel(string tagName);
    }
}
