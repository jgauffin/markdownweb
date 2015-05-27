using System.Text.RegularExpressions;

namespace MarkdownWeb.PostFilters
{
    /// <summary>
    ///     Takes all types of regular text links and make them clickable.
    /// </summary>
    //credit: http://rickyrosario.com/blog/converting-a-url-into-a-link-in-csharp-using-regular-expressions/
    public class ConvertUrlsToLinks : IPostFilter
    {
        public string Parse(PostFilterContext context)
        {
            var msg = context.HtmlToParse;
            var regex =
                @"((www\.|(http|https|ftp|news|file)+\:\/\/)[&#95;.a-z0-9-]+\.[a-z0-9\/&#95;:@=.+?,##%&~-]*[^.|\'|\# |!|\(|?|,|\s|>|<|;|\)])";
            var r = new Regex(regex, RegexOptions.IgnoreCase);
            return r.Replace(msg, match =>
            {
                if (msg[match.Index - 1] == '"')
                    return match.Value;

                return string.Format(@"<a href=""{0}"">{0}", match.Value);
            }).Replace("href=\"www", "href=\"http://www");
        }
    }
}