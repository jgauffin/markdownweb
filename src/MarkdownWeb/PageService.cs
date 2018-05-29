using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MarkdownWeb.Helpers;
using MarkdownWeb.MarkdownService;
using MarkdownWeb.MarkdownService.Extensions;
using MarkdownWeb.PostFilters;
using MarkdownWeb.PreFilters;
using MarkdownWeb.Storage;

namespace MarkdownWeb
{
    public class PageService : IMarkdownParser
    {
        private readonly MarkdownParser _markdown;
        private readonly IPageRepository _repository;
        private readonly IUrlPathConverter _urlPathConverter;

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
        ///     Specifies how the service should behave in different scenarios.
        /// </summary>
        public PageServiceConfiguration Configuration { get; set; }

        /// <summary>
        ///     Used to process the generated HTML once it's been created by the markdown parser.
        /// </summary>
        public PostFilterCollection PostFilters { get; private set; }

        /// <summary>
        ///     Used to modify the text before it's given to the markdown parser.
        /// </summary>
        public PreFilterCollection PreFilters { get; private set; }

        string IMarkdownParser.Parse(PageReference pageToRender, string text)
        {
            return ParseString(pageToRender, text).Body;
        }

        public HtmlPage ParseString(string websiteUrl, string document)
        {
            var reference = _urlPathConverter.MapUrlToWikiPath(websiteUrl);
            return ParseString(reference, document);
        }

        /// <summary>
        ///     Parse a string
        /// </summary>
        /// <param name="pageReference">Path to the page in your web site</param>
        /// <param name="document">Markdown document</param>
        /// <returns>Generated page</returns>
        public HtmlPage ParseString(PageReference pageReference, string document)
        {
            if (pageReference == null) throw new ArgumentNullException("pageReference");
            if (document == null) throw new ArgumentNullException("document");
            return Parse(pageReference, document);
        }

        public HtmlPage ParseUrl(string url)
        {
            var wikiPagePath = _urlPathConverter.MapUrlToWikiPath(url);
            var page = wikiPagePath == null
                ? null
                : _repository.Get(wikiPagePath.ToString());

            if (page != null)
                return Parse(wikiPagePath, page.Body);

            var wikiPath = _urlPathConverter.RemoveWebRoot(url);
            var allPages = GetPages();
            var pages = allPages
                .Where(x => x.PageReference.RealWikiPath.StartsWith(wikiPath))
                .ToList();
            if (pages.Any())
            {
                var body = new StringBuilder();
                body.AppendLine("<h1>Page list</h1>");
                body.AppendLine("<ul class=\"pagelist\">");
                foreach (var subPage in pages)
                {
                    body.AppendLine($"<li><a href=\"{subPage.Url}\">{subPage.Title}</li>");
                }
                body.AppendLine("</ul>");

                return new HtmlPage
                {
                    Body = body.ToString(),
                    Title = "Page list",
                    WikiPageReference = wikiPagePath
                };
            }

            var pagesUrl=_urlPathConverter.ToWebUrl("/pages/");
            return new HtmlPage
            {
                
                Body =
                    $@"<h1>Missing page</h1>
                    <p>The page specified is missing: {url}</p>
                    <p>Try our page list to find the correct page: <a href=""{pagesUrl}"">Page list</a>",
                Title = "Missing page",
                WikiPageReference = wikiPagePath
            };


        }

        public List<PageSummary> GetPages()
        {
            List<PageSummary> pages = new List<PageSummary>();
            var links = _repository.GetAllPagesAsLinks();
            foreach (var pageLink in links)
            {
                var webUrl = _urlPathConverter.ToWebUrl(pageLink);
                var reference = _urlPathConverter.MapUrlToWikiPath(webUrl);
                var page = _repository.Get(pageLink);
                if (string.IsNullOrEmpty(page.Body))
                    continue;

                var parsedPage = Parse(reference, page.Body);
                if (webUrl.EndsWith("index.md"))
                    webUrl = webUrl.Remove(webUrl.Length - 8);

                pages.Add(new PageSummary
                {
                    Abstract = parsedPage.Abstract,
                    Title = parsedPage.Title,
                    Url = webUrl,
                    PageReference = reference
                });
            }

            return pages;
        }

        public List<MissingPage> GetMissingPages()
        {
            List<MissingPage> pages = new List<MissingPage>();
            var links = _repository.GetAllPagesAsLinks();
            foreach (var pageLink in links)
            {
                var url = _urlPathConverter.ToWebUrl(pageLink);
                var reference = _urlPathConverter.MapUrlToWikiPath(url);
                var page = _repository.Get(pageLink);
                var parsedPage = Parse(reference, page.Body);

                foreach (var link in parsedPage.Links)
                {
                    if (link.IsMissing == true)
                    {
                        var existingPage = pages.FirstOrDefault(x => x.Url == link.Url);
                        if (existingPage != null)
                            existingPage.References.Add(pageLink);
                        else
                            pages.Add(new MissingPage { Url = link.Url, References = new List<string> { pageLink } });
                    }
                }
            }

            return pages;
        }

        private HtmlPage Parse(PageReference pageReference, string markdown)
        {
            if (pageReference == null) throw new ArgumentNullException(nameof(pageReference));
            if (markdown == null) throw new ArgumentNullException(nameof(markdown));

            var page = PreProcessMarkdown(markdown);
            page.Body = PreFilters.Execute(this, pageReference, page.Body);
            _markdown.DefaultCodeLanguage = Configuration.DefaultCodeLanguage;
            _markdown.MissingPageStyle = Configuration.MissingLinkStyle;

            var links = new List<PageLink>();
            var context = new MarkdownParserContext(_repository, links)
            {
                RequestedPage = pageReference,
                UrlPathConverter = _urlPathConverter
            };
            page.Body = _markdown.Parse(context, page.Body);

            var postContext = new PostFilterContext(page.Body);
            PostFilters.Execute(postContext);
            page.Body = postContext.HtmlToParse;
            page.Parts = postContext.GetParts();
            page.WikiPageReference = pageReference;
            page.Links = links;
            return page;
        }

        private HtmlPage PreProcessMarkdown(string markdown)
        {
            var textReader = new StringReader(markdown);
            textReader.SkipEmptyLines();
            var heading = textReader.ReadLine();
            if (heading == null)
                return null;

            var isHeading = heading.StartsWith("#");
            heading = heading.TrimStart('#');

            if (textReader.Peek() == '=')
            {
                isHeading = true;
                textReader.ReadLine();
            }

            var articleBody = new StringBuilder();
            var articleAbstract = "";
            if (isHeading)
            {
                textReader.SkipEmptyLines();
                articleAbstract = textReader.ReadUntilEmptyLine();
            }
            else
            {
                articleBody.AppendLine(heading);
            }

            var headers = new Dictionary<string, string>();
            while (true)
            {
                var line = textReader.ReadLine();
                if (line == null)
                    break;

                if (!line.StartsWith("$") || !line.Contains(":"))
                {
                    articleBody.AppendLine(line);
                    continue;
                }

                var pos = line.IndexOf(':');
                var key = line.Substring(1, pos);
                var value = line.Substring(pos + 1);
                headers.Add(key, value);
            }

            return new HtmlPage
            {
                Body = $"{articleAbstract}\r\n\r\n{articleBody}",
                Title = heading,
                Abstract = articleAbstract
            };
        }
    }
}