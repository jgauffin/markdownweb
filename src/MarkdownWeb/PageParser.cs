using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using MarkdownDeep;

namespace MarkdownWeb
{
    /// <summary>
    ///     This code is not beatiful. In fact, it's UGLY. Oooh so ugly.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If you want to make it more beatiful, go ahead. Please do. I beg you.
    ///     </para>
    ///     <para>
    ///         It's a quick hack, but it works and that is currently enough for me.
    ///     </para>
    /// </remarks>
    /// <summary>
    ///     This code is not beatiful. In fact, it's UGLY. Oooh so ugly.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If you want to make it more beatiful, go ahead. Please do. I beg you.
    ///     </para>
    ///     <para>
    ///         It's a quick hack, but it works and that is currently enough for me.
    ///     </para>
    /// </remarks>
    public class PageParser
    {
        private readonly string _rootDirectory;
        private readonly string _rootUri;
        private string _currentUrlPath;
        private IPageSource _pageSource;
        private Func<string, string> _virtualPathHandler = url => VirtualPathUtility.ToAbsolute(url);

        public PageParser(string rootDirectory, string rootUri)
        {
            _rootDirectory = rootDirectory;
            _rootUri = rootUri.Trim('/');
            _pageSource = new PageSource(rootUri, rootDirectory);
        }

        /// <summary>
        /// Used to convert our local urls to website urls
        /// </summary>
        public Func<string, string> VirtualPathHandler
        {
            get { return _virtualPathHandler; }
            set { _virtualPathHandler = value; }
        }

        public ParsedPage ParseString(string url, string document)
        {
            _currentUrlPath = url.Trim('/');
            var markdown = new Markdown
            {
                PrepareLink = OnHtmlLink,
                PrepareImage = OnImageLink,
                FormatCodeBlock = OnCodeBlock
            };

            var source = new PageSource(_rootUri, _rootDirectory);
            var page = new ParsedPage();
            document = PreParseGithubCodeBlocks2(document);
            document = PreParseGithubTables(document);
            var pos = document.IndexOfAny(new[] { '\r', '\n' });
            page.Title = pos != -1 ? document.Substring(0, pos).TrimStart('#', ' ', '\t') : "";
            page.Body = markdown.Transform(document) + "<br>" + _rootUri + "<br>" + source.GetAbsoluteUrl(url, "Applikationer") + "<br>" + url;
            return page;
        }

        public ParsedPage ParseUrl(string url)
        {
            _currentUrlPath = url.Trim('/');
            var text = _pageSource.GetContent(url);
            return ParseString(_currentUrlPath, text);
        }


        private string GithubCodeReplacer(Match match)
        {
            var code = match.Groups[3].Value;
            code = code.Replace("<", "&lt;").Replace(">", "&gt;");
            return string.Format(@"<pre><code data-lang=""{0}"" class=""language-{0}"">", code);
        }

        private string OnCodeBlock(Markdown arg1, string arg2)
        {
            return @"<pre class=""language-text""><code data-lang=""text"" class=""language-text"">" + arg2 +
                   "</code></pre>";
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

            if (!src.StartsWith("/"))
            {
                src = _pageSource.GetAbsoluteUrl(_currentUrlPath, src);
                arg.attributes["href"] = VirtualPathHandler("~/" + _rootUri + "/" + src);
            }

            var exists = _pageSource.PageExists(src);
            if (!exists)
                arg.attributes["style"] = "color: red";

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

        private string PreParseGithubCodeBlocks2(string text)
        {
            var sb = new StringBuilder();
            var lastPos = 0;
            while (true)
            {
                var pos = text.IndexOf("\r\n```", lastPos);
                if (pos == -1)
                    break;

                sb.Append(text.Substring(lastPos, pos - lastPos));
                sb.AppendLine();
                pos += 5;
                var nlPos = text.IndexOfAny(new[] { '\r', '\n' }, pos);
                var codeLang = text.Substring(pos, nlPos - pos);


                lastPos = text.IndexOf("\r\n```\r\n", pos + 1);
                if (lastPos == -1)
                    lastPos = text.Length - 1;

                sb.AppendFormat(@"<pre><code data-lang=""{0}"" class=""language-{0}"">", codeLang);
                var code = text.Substring(nlPos + 2, lastPos - nlPos - 2);
                sb.Append(code.Replace(">", "&gt;").Replace("<", "&lt;"));
                sb.Append("</code></pre>\r\n");
                lastPos += 5;
                if (lastPos > text.Length)
                    break;
            }

            if (lastPos < text.Length - 1)
                sb.AppendLine(text.Substring(lastPos));

            return sb.ToString();
        }

        private string PreParseGithubTables(string text)
        {
            var reader = new StringReader(text);
            var line = "";
            var sb = new StringBuilder();
            while ((line = reader.ReadLine()) != null)
            {
                if (!line.Contains(" | "))
                {
                    sb.AppendLine(line);
                    continue;
                }

                var firstLine = line;
                line = reader.ReadLine();
                if (string.IsNullOrEmpty(line))
                {
                    sb.AppendLine(firstLine);
                    continue;
                }

                if (!line.All(x => x == ' ' || x == '|' || x == '-'))
                {
                    sb.AppendLine(line);
                    continue;
                }

                //got a table.
                sb.AppendLine(@"<table class=""table table-striped table-bordered""><thead><tr><th>");
                sb.AppendLine(firstLine.Replace(" | ", "</th><th>"));
                sb.AppendLine("</tr></thead><tbody>");
                while (true)
                {
                    line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        sb.AppendLine("</tbody></table>");
                        sb.AppendLine("<br>\r\n");
                        break;
                    }

                    sb.Append("<tr><td>");
                    sb.Append(line.Replace(" | ", "</td><td>"));
                    sb.AppendLine("</td></tr>");
                }
            }

            return sb.ToString();
        }
    }
}