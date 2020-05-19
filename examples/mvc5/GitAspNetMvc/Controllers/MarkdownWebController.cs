using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using GitAspNetMvc.Models;
using MarkdownWeb;
using MarkdownWeb.Git;
using MarkdownWeb.Storage.Files;

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
        public const string DIRECTORY = @"C:\src\1tcompany\coderr\OSS\Coderr.Documentation\";
        private string _folderPath;
        private string _baseUrl;

        public MarkdownWebController()
        {
            _folderPath = DIRECTORY[1] == ':' ? DIRECTORY : HostingEnvironment.MapPath(DIRECTORY);
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            _baseUrl = Url.Content("~/" + URL);
        }

        [Route(URL + "{*path}")]
        public ActionResult Index(string path)
        {
            if (path == null)
                path = "";

            if (Request.QueryString["image"] != null)
                return ServeImages(Path.Combine(_folderPath, "docs"));

            var repository = LoadGitHubRepository(_folderPath);

            var urlConverter = new UrlConverter(_baseUrl, repository);
            var parser = new PageService(repository, urlConverter);
            var result = parser.ParseUrl(_baseUrl + path);
            return View("Index", result);
        }

        private ActionResult ServeImages(string folderPath)
        {
            var src = Request.QueryString["image"].TrimStart('/');
            var mime = MimeMapping.GetMimeMapping(Path.GetFileName(src));
            return File(Path.Combine(folderPath, src.Replace("/", "\\")), mime);
        }

        [Route(URL + "pages")]
        public ActionResult Pages()
        {
            var repository = LoadGitHubRepository(_folderPath);
            var urlConverter = new UrlConverter(_baseUrl, repository);
            var pageService = new PageService(repository, urlConverter);
            var result = pageService.GetPages();
            PageTreeGenerator generator = new PageTreeGenerator();
            var root = generator.Generate(result, URL);

            return View(new PageListViewModel(root));
        }

        [Route(URL + "pages/missing")]
        public ActionResult MissingPages()
        {
            var repository = LoadGitHubRepository(_folderPath);
            var urlConverter = new UrlConverter(_baseUrl, repository);
            var pageService = new PageService(repository, urlConverter);
            var result = pageService.GetMissingPages();
            return View(result);
        }

        private GitPageRepository LoadGitHubRepository(string folderPath)
        {
            var settings = new GitStorageConfiguration
            {
                // directory where we should store the fetched files.
                FetchDirectory = folderPath,

                //Folder in the git repos where the documentation is stored
                DocumentationDirectory = Path.Combine(folderPath, "docs"),
                RepositoryUri = new Uri("https://github.com/coderrio/coderr.Documentation.git"),
            };

            return new GitPageRepository(settings);
        }
    }
}