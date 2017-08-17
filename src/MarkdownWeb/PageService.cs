using System;
using MarkdownWeb.MarkdownService;
using MarkdownWeb.PostFilters;
using MarkdownWeb.PreFilters;
using MarkdownWeb.Storage;

namespace MarkdownWeb
{
    public class PageService : IMarkdownParser
    {
        private readonly IPageRepository _repository;
        private readonly IUrlPathConverter _urlPathConverter;
        private readonly MarkdownParser _markdown;

        /// <summary>
        ///     Create a new instance of <see cref="PageService" />.
        /// </summary>
        /// <param name="repository">Used to fetch pages from a data source of your choosing.</param>
        /// <param name="urlPathConverter">Used to translate web site urls and wiki links.</param>
        public PageService(IPageRepository repository, IUrlPathConverter urlPathConverter)
        {
            _repository = repository;
            _urlPathConverter = urlPathConverter;
            _markdown = new MarkdownParser(_urlPathConverter, _repository);
            PreFilters = new PreFilterCollection();
            PostFilters = new PostFilterCollection();
            Configuration = new PageServiceConfiguration();
        }

        /// <summary>
        ///     Used to modify the text before it's given to the markdown parser.
        /// </summary>
        public PreFilterCollection PreFilters { get; private set; }

        /// <summary>
        ///     Used to process the generated HTML once it's been created by the markdown parser.
        /// </summary>
        public PostFilterCollection PostFilters { get; private set; }

        /// <summary>
        ///     Specifies how the service should behave in different scenarios.
        /// </summary>
        public PageServiceConfiguration Configuration { get; set; }

        string IMarkdownParser.Parse(string currentPagePath, string text)
        {
            var url = _urlPathConverter.ToWebPath(currentPagePath);
            return ParseString(url, text).Body;
        }

        /// <summary>
        ///     Parse a string
        /// </summary>
        /// <param name="websiteUrl">Path to the page in your web site</param>
        /// <param name="document">Markdown document</param>
        /// <returns>Generated page</returns>
        public HtmlPage ParseString(string websiteUrl, string document)
        {
            if (websiteUrl == null) throw new ArgumentNullException("websiteUrl");
            if (document == null) throw new ArgumentNullException("document");

            var wikiPagePath = _urlPathConverter.MapUrlToWikiPath(websiteUrl);
            var page = new HtmlPage();
            var pos = document.IndexOfAny(new[] {'\r', '\n'});
            page.Title = pos != -1 ? document.Substring(0, pos).TrimStart('#', ' ', '\t') : "";

            document = PreFilters.Execute(this, wikiPagePath, document);

            _markdown.DefaultCodeLanguage = Configuration.DefaultCodeLanguage;
            _markdown.MissingPageStyle = Configuration.MissingLinkStyle;
            document = _markdown.Parse(wikiPagePath, document);

            var postContext = new PostFilterContext(document);
            PostFilters.Execute(postContext);
            page.Body = postContext.HtmlToParse;
            page.Parts = postContext.GetParts();
            return page;
        }

        private HtmlPage Parse(string websiteUrl, StoredPage storedPage)
        {
            if (websiteUrl == null) throw new ArgumentNullException("websiteUrl");

            var wikiPagePath = _urlPathConverter.MapUrlToWikiPath(websiteUrl);
            var page = new HtmlPage();
            if (!string.IsNullOrEmpty(storedPage.Title))
            {
                page.Title = storedPage.Title;
            }
            else
            {
                var pos = storedPage.Body.IndexOfAny(new[] {'\r', '\n'});
                page.Title = pos != -1 ? storedPage.Body.Substring(0, pos).TrimStart('#', ' ', '\t') : "";
            }

            page.Body = PreFilters.Execute(this, wikiPagePath, storedPage.Body);
            
            _markdown.DefaultCodeLanguage = Configuration.DefaultCodeLanguage;
            _markdown.MissingPageStyle = Configuration.MissingLinkStyle;
            page.Body = _markdown.Parse(wikiPagePath, page.Body);
            
            var postContext = new PostFilterContext(page.Body);
            PostFilters.Execute(postContext);
            page.Body = postContext.HtmlToParse;
            page.Parts = postContext.GetParts();
            return page;
        }

        public HtmlPage ParseUrl(string url)
        {
            var wikiPagePath = _urlPathConverter.MapUrlToWikiPath(url);
            var page = _repository.Get(wikiPagePath);
            return Parse(url, page);
        }
    }
}