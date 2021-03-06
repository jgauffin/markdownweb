﻿using System;
using System.Collections.Generic;

namespace MarkdownWeb.PostFilters
{
    /// <summary>
    ///     Context passed to the post process filters.
    /// </summary>
    public class PostFilterContext
    {
        private readonly List<HtmlPart> _parts = new List<HtmlPart>();

        public PostFilterContext(string html)
        {
            if (html == null) throw new ArgumentNullException("html");
            HtmlToParse = html;
        }

        /// <summary>
        ///     Page has gone through the pre processors and the markdown parser.
        /// </summary>
        public string HtmlToParse { get; set; }

        /// <summary>
        ///     Get all parts that was generated by the post processors.
        /// </summary>
        /// <returns>A list of parts (or an empty list)</returns>
        public IReadOnlyList<HtmlPart> GetParts()
        {
            return _parts;
        }

        /// <summary>
        ///     Add a new part to the document.
        /// </summary>
        /// <param name="name">Name of the part. PascalCase.</param>
        /// <param name="html">HTML. Must always be enclosed in a single div</param>
        public void AddPart(string name, string html)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (html == null) throw new ArgumentNullException("html");
            _parts.Add(new HtmlPart(name) {Body = html});
        }
    }
}