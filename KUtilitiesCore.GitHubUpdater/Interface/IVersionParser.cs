namespace KUtilitiesCore.GitHubUpdater.Interface
{
    public interface IVersionParser
    {
        System.Version Parse(string tagName);
        string GetChannel(string tagName);
    }
}
