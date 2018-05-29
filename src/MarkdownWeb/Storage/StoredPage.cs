using System;

namespace MarkdownWeb.Storage
{
    public class StoredPage
    {
        public string Name { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
