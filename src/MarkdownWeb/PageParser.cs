using System;
using System.IO;
using System.Linq;
using System.Security.Policy;
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
            document = ConvertUncPaths(document);
            document = PreParseGithubCodeBlocks2(document);
            document = PreParseGithubTables(document);
            var pos = document.IndexOfAny(new[] { '\r', '\n' });
            page.Title = pos != -1 ? document.Substring(0, pos).TrimStart('#', ' ', '\t') : "";
            page.Body = markdown.Transform(document);
            page.Body = AnchorHeadings(page.Body);
            page.Body = ConvertUrlsToLinks(page.Body);
            return page;
        }

        //credit: http://stackoverflow.com/questions/22693604/c-sharp-regex-to-parse-html-string-and-add-ids-into-each-header-tag
        private string AnchorHeadings(string body)
        {
            return Regex.Replace(body, @"<h(?<number>[1-6])>(?<innerText>[^<]*)</h[1-6]>",
                x =>
                    string.Format("<h{2} id=\"{0}\">{1}</h{2}>",
                        new String(x.Groups["innerText"].Value.Where(char.IsLetterOrDigit).ToArray()), 
                        x.Groups["innerText"],
                        x.Groups["number"]));
        }

        public ParsedPage ParseUrl(string url)
        {
            _currentUrlPath = url.Trim('/');
            var text = _pageSource.GetContent(url);
            return ParseString(_currentUrlPath, text);
        }

        //credit: http://rickyrosario.com/blog/converting-a-url-into-a-link-in-csharp-using-regular-expressions/
        private string ConvertUrlsToLinks(string msg)
        {
         
            string regex = @"((www\.|(http|https|ftp|news|file)+\:\/\/)[&#95;.a-z0-9-]+\.[a-z0-9\/&#95;:@=.+?,##%&~-]*[^.|\'|\# |!|\(|?|,|\s|>|<|;|\)])";
            Regex r = new Regex(regex, RegexOptions.IgnoreCase);
            return r.Replace(msg, match =>
            {
                if (msg[match.Index - 1] == '"')
                    return match.Value;

                return string.Format(@"<a href=""{0}"">{0}", match.Value);
            }).Replace("href=\"www", "href=\"http://www");
        }

        private string ConvertUncPaths(string msg)
        {
            var regex = @"(\\\\[A-Z_a-z0-9$\\.\-]+)[\s|\n]";
            Regex r = new Regex(regex, RegexOptions.IgnoreCase);
            return r.Replace(msg, "<a href=\"file://$1\" target=\"_blank\">\\$1</a>");
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

            if (!src.StartsWith("/") && !src.StartsWith("#"))
            {
                src = _pageSource.GetAbsoluteUrl(_currentUrlPath, src);
                arg.attributes["href"] = VirtualPathHandler("~/" + _rootUri + "/" + src);
            }

            var exists = _pageSource.PageExists(src) || src.StartsWith("#");
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
                sb.AppendLine("</th></tr></thead><tbody>");
                while (true)
                {
                    line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        sb.AppendLine("</tbody></table>");
                        sb.AppendLine("<br>\r\n");
                        break;
                    }
                    line = line.Trim('|');
                    sb.Append("<tr>");

                    var oldPos = 0;
                    while (true)
                    {
                        var pos = line.IndexOf('|', oldPos);
                        if (pos == -1)
                        {
                            pos = line.Length;
                        }

                        var cell = line.Substring(oldPos, pos - oldPos);
                        sb.Append("<td> ");
                        var data = ParseString(_currentUrlPath, cell).Body.Replace("<p>","").Replace("</p>", "");
                        sb.Append(data);
                        sb.Append(" </td>");
                        oldPos = pos + 1;
                        if (oldPos >= line.Length)
                        {
                            sb.AppendLine("</tr>");
                            break;
                        }
                    }
                }
            }

            return sb.ToString();
        }
    }
}