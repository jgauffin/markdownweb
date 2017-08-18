using System;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using MarkdownWeb;
using MarkdownWeb.Git;

namespace GitAspNetMvc.Controllers
{
    /// <summary>
    ///     To make this work, add "routes.MapMvcAttributeRoutes();" before the default route in "RouteConfig.cs"
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         All you need to do is to install the nuget packages and copy this controller to your own project. That's it!
    ///     </para>
    /// </remarks>
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
                return ServeImages(Path.Combine(folderPath, "docs"));

            var repository = LoadGitHubRepository(folderPath);

            var urlConverter = new UrlConverter(baseUrl);
            var parser = new PageService(repository, urlConverter);
            var result = parser.ParseUrl(baseUrl + path);
            return View("Index", result);
        }
        
        private ActionResult ServeImages(string folderPath)
        {
            var src = Request.QueryString["image"].TrimStart('/');
            var mime = MimeMapping.GetMimeMapping(Path.GetFileName(src));
            return File(Path.Combine(folderPath, src.Replace("/", "\\")), mime);
        }

        private GitPageRepository LoadGitHubRepository(string folderPath)
        {
            var settings = new GitStorageConfiguration
            {
                // directory where we should store the fetched files.
                FetchDirectory = folderPath,

                //Folder in the git repos where the documentation is stored
                DocumentationDirectory = Path.Combine(folderPath, "docs"),
                RepositoryUri = new Uri("https://github.com/onetrueerror/OneTrueError.Documentation.git"),
            };

            return new GitPageRepository(settings);
        }
    }
}