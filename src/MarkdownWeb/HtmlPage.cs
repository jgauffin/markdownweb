using System;
using System.Collections.Generic;
using System.Linq;
using MarkdownWeb.MarkdownService.Extensions;

namespace MarkdownWeb
{
    /// <summary>
    ///     Generated page
    /// </summary>
    public class HtmlPage
    {
        private List<HtmlPart> _parts = new List<HtmlPart>();

        /// <summary>
        ///     Title is either stored in the data source or being represented as the first line in the markdown page.
        /// </summary>
        public string Title { get; set; }

        public string Abstract { get; set; }

        public string[] Keywords { get; set; }

        public IEnumerable<PageLink> Links { get; set; }

        /// <summary>
        ///     Generated HTML body.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Path to the document (including "[pagename].md")
        /// </summary>
        public PageReference WikiPageReference { get; set; }

        /// <summary>
        ///     Parts generated for the markdown page.
        /// </summary>
        public IReadOnlyList<HtmlPart> Parts
        {
            get { return _parts; }
            set { _parts = new List<HtmlPart>(value); }
        }

        /// <summary>
        /// Get a part or <c>HtmlPart.Empty</c> if the part do not exist.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public HtmlPart GetPartOrDefault(string name)
        {
            return _parts.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ?? HtmlPart.Empty;
        }

        /// <summary>
        ///     Add a generated part
        /// </summary>
        /// <param name="part">HTML part</param>
        public void AddPart(HtmlPart part)
        {
            if (part == null) throw new ArgumentNullException("part");
            _parts.Add(part);
        }
    }
}