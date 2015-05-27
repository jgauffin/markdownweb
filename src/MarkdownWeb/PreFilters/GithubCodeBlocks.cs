using System.Text;

namespace MarkdownWeb.PreFilters
{
    /// <summary>
    ///     Converts github code blocks to HTML (with <c>data-lang</c> attribute and <code>language-xxx</code> as class name)
    /// </summary>
    public class GithubCodeBlocks : IPreFilter
    {
        /// <summary>
        ///     Parse text
        /// </summary>
        /// <param name="filterContext"></param>
        /// <returns>Text with the modifications done by this script</returns>
        public string Parse(PreFilterContext filterContext)
        {
            var text = filterContext.TextToParse;
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
                code = ProcessCode(code);
                sb.Append(code);
                sb.Append("</code></pre>\r\n");
                lastPos += 5;
                if (lastPos > text.Length)
                    break;
            }

            if (lastPos < text.Length - 1)
                sb.AppendLine(text.Substring(lastPos));

            return sb.ToString();
        }

        /// <summary>
        ///     Can be used to apply styles etc to the source code
        /// </summary>
        /// <param name="sourceCode">The source code within the code block</param>
        /// <returns>Modified source code</returns>
        /// <remarks>
        ///     <para>
        ///         Default implementation converts <code><![CDATA[>]]></code> to <code><![CDATA[&lt;]]></code>
        ///     </para>
        /// </remarks>
        public virtual string ProcessCode(string sourceCode)
        {
            return sourceCode.Replace(">", "&gt;").Replace("<", "&lt;");
        }
    }
}