namespace MarkdownWeb
{
    public interface IPageSource
    {
        string GetAbsoluteUrl(string currentPageUrl, string linkedUrl);
        bool PageExists(string currentPageUrl, string linkedUrl);
        bool PageExists(string url);
        string GetContent(string url);
    }
}