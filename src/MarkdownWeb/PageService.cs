using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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

        /// <summary>
        /// Generate index for a page.
        /// </summary>
        /// <param name="websiteUrl">Must start with the sub path that the wiki is located in.</param>
        /// <returns></returns>
        public HtmlPage GenerateIndex(string websiteUrl)
        {
            var wikiPagePath = _urlPathConverter.MapUrlToWikiPath(websiteUrl);
            var pages = _repository.GetAllPages(wikiPagePath.FriendlyWikiUrl);
            var body = new StringBuilder();
            body.AppendLine("<ul class=\"pagelist\">");
            foreach (var subPage in pages)
            {
                GenerateIndex(subPage, body);
            }
            body.AppendLine("</ul>");

            return new HtmlPage
            {
                Body = body.ToString(),
                Title = "Page list",
                WikiPageReference = wikiPagePath
            };
        }

        private void GenerateIndex(PageReferenceWithChildren node, StringBuilder body)
        {
            var summary = GetPageSummary(node);
            if (node.IsDirectory)
            {
                body.Append($"<li class=\"folder\"><a href=\"{summary.Url}\">{summary.Title}");
            }
            else
            {
                body.Append($"<li><a href=\"{summary.Url}\">{summary.Title}");
            }

            if (node.Children.Any())
            {
                body.AppendLine();
                body.AppendLine("<ul class=\"child\">");
                foreach (var subPage in node.Children)
                {
                    GenerateIndex(subPage, body);
                }
                body.AppendLine("</ul>");

            }
            body.AppendLine("</li>");
        }

        public HtmlPage ParseUrl(string url)
        {
            var wikiPagePath = _urlPathConverter.MapUrlToWikiPath(url);
            var page = wikiPagePath == null
                ? null
                : _repository.Get(wikiPagePath);

            if (page != null)
                return Parse(wikiPagePath, page.Body);

            // Failed to find page.
            var allPages = GetPages();

            // Try to find the most specific name first.
            var notFoundPath = _urlPathConverter.RemoveWebRoot(url).Trim('/');
            var pos = 0;
            var pages = new List<PageSummary>();
            while (true)
            {
                var wikiPath = notFoundPath.Substring(pos);
                if (pos == -1)
                {
                    break;
                }

                pages = allPages
                    .Where(x => x.PageReference.WikiUrl.IndexOf(wikiPath, StringComparison.OrdinalIgnoreCase) != -1)
                    .ToList();
                if (pages.Any())
                {
                    break;
                }

                pos = notFoundPath.IndexOf('/', pos + 1);
            }

            if (pages.Count == 1)
            {
                var foundPage = _repository.Get(pages[0].PageReference);
                return Parse(pages[0].PageReference, foundPage.Body);
            }

            if (pages.Any())
            {
                var body = new StringBuilder();
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

            var pagesUrl = _urlPathConverter.ToWebUrl("/pages/");
            return new HtmlPage
            {

                Body =
                    $@"<p>The page specified is missing: {url}</p>
                    <p>Visit our <a href=""{pagesUrl}"">page list</a> to find the correct page.",
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

                var summary = GetPageSummary(reference);
                if (summary != null)
                {
                    pages.Add(summary);
                }
            }

            return pages;
        }

        private PageSummary GetPageSummary(PageReference reference)
        {
            var page = _repository.Get(reference);
            if (page == null)
            {
                return new PageSummary
                {
                    Abstract = "",
                    Title = reference.FriendlyWikiUrl,
                    Url = _urlPathConverter.ToWebUrl(reference.FriendlyWikiUrl),
                    PageReference = reference
                };
            }

            if (string.IsNullOrEmpty(page.Body))
                return null;

            var parsedPage = Parse(reference, page.Body);
            return new PageSummary
            {
                Abstract = parsedPage.Abstract,
                Title = parsedPage.Title,
                Url = _urlPathConverter.ToWebUrl(reference.FriendlyWikiUrl),
                PageReference = reference
            };
        }

        public List<MissingPage> GetMissingPages()
        {
            List<MissingPage> pages = new List<MissingPage>();
            var links = _repository.GetAllPagesAsLinks();
            foreach (var pageLink in links)
            {
                var url = _urlPathConverter.ToWebUrl(pageLink);
                var reference = _urlPathConverter.MapUrlToWikiPath(url);
                var page = _repository.Get(reference);
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