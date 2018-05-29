namespace MarkdownWeb
{
    public class PageSummary
    {
        public string Abstract { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public PageReference PageReference { get; set; }

        public override string ToString()
        {
            return $"[{PageReference}] {Title}";
        }
    }
}