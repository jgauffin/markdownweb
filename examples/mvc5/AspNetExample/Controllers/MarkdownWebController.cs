using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using MarkdownWeb;
using MarkdownWeb.Storage;
using MarkdownWeb.Storage.Files;

namespace AspNetExample.Controllers
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
                return ServeImages(folderPath);

            var repository = new FileBasedRepository(folderPath);
            var exists = repository.Exists(path);
            if (Request.QueryString["editor"] != null || !exists)
                return HandlePageEdits(path, exists, repository, baseUrl);

            var urlConverter = new UrlConverter(baseUrl);
            var parser = new PageService(repository, urlConverter);
            var result = parser.ParseUrl(baseUrl + path);
            return View("Index", result);
        }

        private ActionResult HandlePageEdits(string path, bool exists, FileBasedRepository repository, string baseUrl)
        {
            if (Request.HttpMethod == "POST")
            {
                SubmitEditedPage(path, exists, repository);
                return Redirect(Request.Url.AbsolutePath);
            }
            var page = repository.Get(path);
            ViewBag.Text = page == null ? "" : page.Body;
            return View("Editor");
        }

        private ActionResult ServeImages(string folderPath)
        {
            var src = Request.QueryString["image"];
            var mime = MimeMapping.GetMimeMapping(Path.GetFileName(src));
            return File(Path.Combine(folderPath, src.Replace("/", "\\")), mime);
        }

        private void SubmitEditedPage(string wikiPagePath, bool pageExists, IPageRepository repository)
        {
            var title = Request.Form["Title"];
            if (string.IsNullOrEmpty(title))
            {
                var body = Request.Form["Body"];
                var pos = body.IndexOfAny(new[] {'\r', '\n'});
                title = pos == -1 ? "" : body.Substring(0, pos);
            }
            var crudPage = new EditedPage
            {
                Body = Request.Form["Body"],
                Title = title,
                Author = User.Identity.Name
            };
            if (pageExists)
                repository.Update(wikiPagePath, crudPage);
            else
                repository.Create(wikiPagePath, crudPage);
        }
    }
}