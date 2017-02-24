using System;
using System.Web;
using MarkdownDeep;
using MarkdownWeb.PostFilters;
using MarkdownWeb.PreFilters;

namespace MarkdownWeb
{
    /// <summary>
    ///     Transforms a markdown script into contents for a HTML page
    /// </summary>
    public class PageParser
    {
        private readonly IPageSource _pageSource;
        private readonly string _rootDirectory;
        private readonly string _rootUri;
        private string _currentUrlPath;
        private Func<string, string> _virtualPathHandler = url => VirtualPathUtility.ToAbsolute(url);

        public PageParser(string rootDirectory, string rootUri)
        {
            _rootDirectory = rootDirectory;
            _rootUri = rootUri.Trim('/');
            _pageSource = new PageSource(rootUri, rootDirectory);
            PreFilters = new PreFilterCollection();
            PostFilters = new PostFilterCollection();
            MissingLinkStyle = "color: red";
            DefaultCodeLanguage = "text";
        }

        public PreFilterCollection PreFilters { get; private set; }
        public PostFilterCollection PostFilters { get; private set; }

        /// <summary>
        ///     Used to convert our local urls to website urls
        /// </summary>
        public Func<string, string> VirtualPathHandler
        {
            get { return _virtualPathHandler; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _virtualPathHandler = value;
            }
        }

        /// <summary>
        ///     Style to set on missing links
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Default is <c>color: red;</c>
        ///     </para>
        /// </remarks>
        public string MissingLinkStyle { get; set; }

        /// <summary>
        ///     Language tag to use per default for code blocks (when no lang has been specified)
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Default is <c>text</c>
        ///     </para>
        /// </remarks>
        public string DefaultCodeLanguage { get; set; }

        public ParsedPage ParseString(string url, string document)
        {
            _currentUrlPath = url.Trim('/');
            var markdown = new Markdown
            {
                PrepareLink = OnHtmlLink,
                PrepareImage = OnImageLink,
                FormatCodeBlock = OnCodeBlock
            };

            var page = new ParsedPage();
            var pos = document.IndexOfAny(new[] {'\r', '\n'});
            page.Title = pos != -1 ? document.Substring(0, pos).TrimStart('#', ' ', '\t') : "";

            document = PreFilters.Execute(this, _currentUrlPath, _rootUri, document);
            document = markdown.Transform(document);
            document = PostFilters.Execute(document);
            page.Body = document;
            return page;
        }


        public ParsedPage ParseUrl(string url)
        {
            _currentUrlPath = url.Trim('/');
            var text = _pageSource.GetContent(url);
            return ParseString(_currentUrlPath, text);
        }

        public string GetSource(string url)
        {
            _currentUrlPath = url.Trim('/');
            return _pageSource.GetContent(url);
        }


        private string OnCodeBlock(Markdown arg1, string arg2)
        {
            return string.Format(
                @"<pre class=""language-{0}""><code data-lang=""{0}"" class=""language-{0}"">{1}</code></pre>",
                DefaultCodeLanguage,
                arg2);
        }

        private bool OnHtmlLink(HtmlTag arg)
        {
            var src = arg.attributes["href"];
            if (src.StartsWith("http"))
                return false;
            if (src.StartsWith("~"))
            {
                src = VirtualPathHandler(src);
                arg.attributes["href"] = src;
                return true;
            }

            if (!src.StartsWith("/") && !src.StartsWith("#"))
            {
                src = _pageSource.GetAbsoluteUrl(_currentUrlPath, src);
                arg.attributes["href"] = VirtualPathHandler("~/" + _rootUri + "/" + src);
            }

            var exists = _pageSource.PageExists(src) || src.StartsWith("#");
            if (!exists)
                arg.attributes["style"] = MissingLinkStyle;

            return true;
        }

        private bool OnImageLink(HtmlTag arg1, bool arg2)
        {
            var src = arg1.attributes["src"];
            if (src.StartsWith("http"))
                return false;

            if (src.StartsWith("~"))
            {
                //var url = string.Format(_rootUri + "?image={0}/{1}", _currentUrlPath, arg1.attributes["src"]);
                arg1.attributes["src"] = VirtualPathHandler(src);
                return true;
            }

            src = _pageSource.GetAbsoluteUrl(_currentUrlPath, src);
            var formattedUrl = string.Format(VirtualPathHandler("~/" + _rootUri) + "?image={0}", src);
            arg1.attributes["src"] = formattedUrl;
            return true;
        }
    }
}