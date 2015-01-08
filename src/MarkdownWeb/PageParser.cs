using System.Diagnostics;
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
    public class PageParser
    {
        private readonly string _rootDirectory;
        private readonly string _rootUri;
        private string _currentFilePath;
        private string _currentUrlPath;

        public PageParser(string rootDirectory, string rootUri)
        {
            _rootDirectory = rootDirectory;
            _rootUri = rootUri.Trim('/');
        }

        public ParsedPage ParseString(string str)
        {
            var markdown = new Markdown
            {
                PrepareLink = OnHtmlLink,
                PrepareImage = OnImageLink,
                FormatCodeBlock = OnCodeBlock
            };

            var page = new ParsedPage();

            var pos = str.IndexOfAny(new[] {'\r', '\n'});
            page.Title = pos != -1 ? str.Substring(0, pos).TrimStart('#', ' ', '\t') : "";
            page.Body = markdown.Transform(str);
            return page;
        }

        public ParsedPage ParseUrl(string url)
        {
            url = url == null
                ? ""
                : url.Trim('/');


            if (url.StartsWith(_rootUri))
                url = url.Remove(0, _rootUri.Length).Trim('/');

            var path = GetFullPath(url);

            //direct link
            if (!path.EndsWith("index.md"))
            {
                var pos = url.LastIndexOf('/');
                if (pos == -1)
                    _currentUrlPath = "/";
                else
                    _currentUrlPath = url.Substring(0, pos);
            }
            else
                _currentUrlPath = url;

            _currentFilePath = Path.GetDirectoryName(path);

            var text = File.ReadAllText(path);
            text = PreParseGithubCodeBlocks2(text);
            text = PreParseGithubTables(text);
            return ParseString(text);
        }

        private string GetFullPath(string url)
        {
            if (url.StartsWith("doc/"))
                url = url.Remove(0, 4);
            if (url.StartsWith("/doc/"))
                url = url.Remove(0, 5);

            var fullPath = Path.Combine(_rootDirectory, url.Replace('/', '\\'));
            if (Directory.Exists(fullPath))
                fullPath = Path.Combine(fullPath, "index.md");
            else if (!fullPath.EndsWith(".md"))
                fullPath += ".md";

            return fullPath;
        }

        private string GetLinkPath(string url)
        {
            if (url.StartsWith(_currentUrlPath))
                url = url.Remove(0, _currentUrlPath.Length).TrimStart('/');
            if (url.StartsWith('/' + _currentUrlPath))
                url = url.Remove(0, _currentUrlPath.Length + 1).TrimStart('/');
            if (url.StartsWith("doc/"))
                Debugger.Break();

            var basePath = _currentFilePath;
            if (url.StartsWith("/doc/"))
            {
                url = url.Remove(0, 5);
                basePath = _rootDirectory;
            }

            var fullPath = Path.Combine(basePath, url.Replace('/', '\\'));

            if (Directory.Exists(fullPath))
                fullPath = Path.Combine(fullPath, "index.md");
            else if (!fullPath.EndsWith(".md"))
                fullPath += ".md";

            return fullPath;
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
                src = VirtualPathUtility.ToAbsolute(src);
                arg.attributes["href"] = src;
                return true;
            }

            if (!src.StartsWith("/"))
            {
                var fixedSrc = VirtualPathUtility.ToAbsolute("~/" + _rootUri + "/" + _currentUrlPath + "/" + src);
                arg.attributes["href"] = fixedSrc;
            }

            var path = GetLinkPath(src);
            if (!File.Exists(path))
                arg.attributes["style"] = "color: red";

            return true;
        }

        private bool OnImageLink(HtmlTag arg1, bool arg2)
        {
            if (!arg1.attributes["src"].Contains("/"))
            {
                var url = string.Format(_rootUri + "?image={0}/{1}", _currentUrlPath, arg1.attributes["src"]);
                arg1.attributes["src"] = VirtualPathUtility.ToAbsolute(url);
            }


            return true;
        }

        private string PreParseGithubCodeBlocks(string text)
        {
            var regex = new Regex("(\r\n|\r|\n)(```)(.+)(\r\n)", RegexOptions.Multiline);
            return regex.Replace(text, GithubCodeReplacer).Replace("```", "</code></pre>");
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
                var nlPos = text.IndexOfAny(new[] {'\r', '\n'}, pos);
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