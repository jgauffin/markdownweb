namespace MarkdownWeb
{
    /// <summary>
    /// Used to store information about each page.
    /// </summary>
    public class PageSummary
    {
        public string Abstract { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public PageReference PageReference { get; set; }

        public string[] Tags { get; set; }

        public PageSummary Children { get; set; }

        public override string ToString()
        {
            return $"[{PageReference}] {Title}";
        }
    }
}