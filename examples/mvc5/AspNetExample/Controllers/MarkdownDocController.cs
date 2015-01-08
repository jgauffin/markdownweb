using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using MarkdownWeb;

namespace AspNetExample.Controllers
{
    /// <summary>
    ///     To make this work, add "routes.MapMvcAttributeRoutes();" before the default route in "RouteConfig.cs"
    /// </summary>
    public class MarkdownWebController : Controller
    {
        public const string URL = "doc/";
        public const string DIRECTORY = "~/App_Data/Markdown/";

        [Route(URL + "{*path}")]
        public ActionResult Index(string path)
        {
            if (path == null)
                path = "";

            var folderPath = HostingEnvironment.MapPath(DIRECTORY);
            var baseUrl = Url.Content("~/" + URL);

            if (Request.QueryString["image"] != null)
            {
                var src = Request.QueryString["image"];
                var mime = MimeMapping.GetMimeMapping(Path.GetFileName(src));
                return File(Path.Combine(folderPath, src.Replace("/", "\\")), mime);
            }

            var parser = new PageParser(folderPath, baseUrl);
            var result = parser.ParseUrl(baseUrl + path);
            return View("Index", result);
        }
    }
}