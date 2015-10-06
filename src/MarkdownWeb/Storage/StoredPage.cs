using System;

namespace MarkdownWeb.Storage
{
    public class StoredPage
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public int Revision { get; set; }
    }
}
