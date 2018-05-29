using System;
using System.Collections.Generic;
using MarkdownWeb.MarkdownService.Extensions;
using MarkdownWeb.Storage;

namespace MarkdownWeb.MarkdownService
{
    public class MarkdownParserContext
    {
        private readonly IPageRepository _repository;
        private readonly List<PageLink> _links;

        public MarkdownParserContext(IPageRepository repository, List<PageLink> links)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _links = links;
        }

        /// <summary>
        ///     Reference to the page requested by the browser.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Do note that pages can have two types of paths: <c>/some/path/</c> or <c>/some/path.md</c>. The first can
        ///         either be same as the second or /some/path/index.md. Therefore the
        ///         url might have the incorrect number of segments compared to the actual document. We need to handle that when
        ///         linking.
        ///     </para>
        /// </remarks>
        public PageReference RequestedPage { get; set; }


        public IUrlPathConverter UrlPathConverter { get; set; }

        public void AddLink(PageLink pageLink)
        {
            if (pageLink == null) throw new ArgumentNullException(nameof(pageLink));
            _links.Add(pageLink);
        }

        public bool PageExists(string wikiPageUrl)
        {
            if (_repository != null)
                return _repository.Exists(wikiPageUrl);

            return false;
        }
    }
}