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
        private string _folderPath;
        private string _baseUrl;

        public MarkdownWebController()
        {
            _folderPath = HostingEnvironment.MapPath(DIRECTORY);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            _baseUrl = Url.Content("~/" + URL);
        }

        [Route(URL + "{*path}")]
        public ActionResult Index(string path)
        {
            if (path == null)
                path = "";

            if (Request.QueryString["image"] != null)
                return ServeImages(_folderPath);


            var repository = new FileBasedRepository(_folderPath);
            var urlConverter = new UrlConverter(_baseUrl, repository);
            var reference = urlConverter.ToReference(path);
            if (Request.QueryString["editor"] != null || reference == null)
                return HandlePageEdits(path, reference != null, repository, _baseUrl);

            var pageService = new PageService(repository, urlConverter);
            var result = pageService.ParseUrl(_baseUrl + path);
            return View("Index", result);
        }

        public ActionResult Pages()
        {
            var repository = new FileBasedRepository(_folderPath);
            var urlConverter = new UrlConverter(_baseUrl, repository);
            var pageService = new PageService(repository, urlConverter);
            var result = pageService.GetPages();
            return View(result);
        }

        private ActionResult HandlePageEdits(string path, bool exists, FileBasedRepository repository, string baseUrl)
        {
            if (Request.HttpMethod == "POST")
            {
                SubmitEditedPage(path, exists, repository);
                return Redirect(Request.Url.AbsolutePath);
            }

            var urlConverter = new UrlConverter(_baseUrl, repository);
            var reference = urlConverter.ToReference(path);
            var page = reference != null ? repository.Get(reference) : null;
            ViewBag.Text = page == null ? "" : page.Body;
            return View("Editor");
        }


        private ActionResult ServeImages(string folderPath)
        {
            var src = Request.QueryString["image"];
            var mime = MimeMapping.GetMimeMapping(Path.GetFileName(src));
            var fileUrl = src.Replace("/", "\\").TrimStart('\\');
            var path = Path.Combine(folderPath, fileUrl);
            return File(path, mime);
        }

        private void SubmitEditedPage(string wikiPagePath, bool pageExists, IPageRepository repository)
        {
            var title = Request.Form["Title"];
            if (string.IsNullOrEmpty(title))
            {
                var body = Request.Form["Body"];
                var pos = body.IndexOfAny(new[] { '\r', '\n' });
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