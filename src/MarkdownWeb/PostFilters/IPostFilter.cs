using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownWeb.PostFilters
{
    /// <summary>
    /// Invoked after the markdown parser have converted the content to HTML
    /// </summary>
    public interface IPostFilter
    {
        string Parse(PostFilterContext context);
    }
}
