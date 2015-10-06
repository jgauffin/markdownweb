using System;

namespace MarkdownWeb
{
    /// <summary>
    ///     Contains parts for the generated page that doesn't belong to the actual document. For instance a Table Of Contents
    /// </summary>
    public class HtmlPart
    {
        /// <summary>
        ///     Returned if the requested part do not exist.
        /// </summary>
        public static readonly HtmlPart Empty = new HtmlPart("");

        public HtmlPart(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            Name = name;
        }

        /// <summary>
        ///     Name of this part, for instance "TableOfContents"
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     HTML body for the part. Must be enclosed in a single div.
        /// </summary>
        public string Body { get; set; }
    }
}