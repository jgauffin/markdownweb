using System;

namespace MarkdownWeb.MarkdownService.Extensions
{
    public class PageLink
    {
        public PageLink(string url, string label)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
            Label = label;
        }

        public string Url { get; set; }
        public string Label { get; }
        public bool? IsMissing { get; set; }
        public bool IsWikiLink { get; set; }
        public bool IsLocal { get; set; }
    }
}