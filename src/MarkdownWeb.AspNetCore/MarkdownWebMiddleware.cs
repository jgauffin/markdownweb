using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MarkdownWeb.Git;
using MarkdownWeb.Storage;
using MarkdownWeb.Storage.Files;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;

namespace MarkdownWeb.AspNetCore
{
    /// <summary>
    ///     Serves markdown files from a specific url.
    /// </summary>
    public class MarkdownWebMiddleware
    {
        private static IPageRepository _repository;
        private static readonly RouteData EmptyRouteData = new RouteData();
        private static readonly ActionDescriptor EmptyActionDescriptor = new ActionDescriptor();
        private readonly string _docDirectory;
        private readonly IWebHostEnvironment _environment;
        private readonly RequestDelegate _next;
        private readonly MarkdownWebMiddlewareOptions _options;
        private readonly PageService _pageService;
        private readonly string _rootDirectory;
        private readonly IActionResultExecutor<ViewResult> _viewResultExecutor;

        public MarkdownWebMiddleware(
            RequestDelegate next,
            MarkdownWebMiddlewareOptions options,
            IWebHostEnvironment environment,
            IActionResultExecutor<ViewResult> viewResultExecutor)
        {
            _next = next;
            _options = options;
            _environment = environment;
            _viewResultExecutor = viewResultExecutor;
            _rootDirectory = Path.Combine(environment.ContentRootPath, options.DocumentationDirectory);
            _docDirectory = string.IsNullOrEmpty(options.GitSubFolder)
                ? _rootDirectory
                : Path.Combine(_rootDirectory, options.GitSubFolder);

            _pageService = CreatePageService();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments(_options.WebPath))
            {
                await _next(context);
                return;
            }

            var path = context.Request.Path.ToString();
            if (!path.Contains(".") && !path.EndsWith("/"))
            {
                path += "/";
            }

            if (context.Request.Query["image"].Count > 0 || path.EndsWith(".png"))
            {
                await ServeImages(path, context.Request.Query["image"][0], context.Response);
                return;
            }

            if (_options.LayoutPage.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            {
                await RenderHtmlFile(path, context.Response);
            }
            else
            {
                await RenderViewFile(context, path);
            }
        }

        private PageService CreatePageService()
        {
            var pageRepository = string.IsNullOrEmpty(_options.GitRepositoryUrl)
                ? new FileBasedRepository(_options.DocumentationDirectory)
                : CreateRepository();

            var urlConverter = new UrlConverter(_options.WebPath, (IPageSource)pageRepository);
            var pageService = new PageService(pageRepository, urlConverter);
            return pageService;
        }

        private IPageRepository CreateRepository()
        {
            if (_repository != null)
            {
                return _repository;
            }

            var settings = new GitStorageConfiguration
            {
                // directory where we should store the fetched files.
                FetchDirectory = _rootDirectory,

                //Folder in the git repos where the documentation is stored
                DocumentationDirectory = _docDirectory,
                RepositoryUri = new Uri(_options.GitRepositoryUrl),
                UpdateInterval = TimeSpan.FromMinutes(5)
            };

            _repository = new GitPageRepository(settings)
            {
                ErrorLogTask = (level, errMsg, exception) => { _options.ErrorLog?.Invoke(errMsg, exception); },
                DeleteAndFetchOnErrors = true,
                UpdateInBackground = true
            };
            return _repository;
        }

        private GeneratedPage LoadPage(string path)
        {
            var pagesPath = _options.WebPath.Add("/pages/");
            if (pagesPath.Equals(new PathString(path)))
            {
                return _pageService.GenerateIndex(_options.WebPath);
            }

            path = Path.Combine(_options.WebPath, path);
            try
            {
                return _pageService.ParseUrl(path);
            }
            catch (DirectoryNotFoundException ex)
            {
                _options.ErrorLog?.Invoke(path, ex);
                return new GeneratedPage
                {
                    Body = "<h1>Page not found</h1><p>Oops, we could not find the page that you where looking for.</p>"
                };
            }
            catch (FileNotFoundException ex)
            {
                _options.ErrorLog?.Invoke(path, ex);
                return new GeneratedPage
                {
                    Body = "<h1>Page not found</h1><p>Oops, we could not find the page that you where looking for.</p>"
                };
            }
        }

        private async Task RenderHtmlFile(string webPath, HttpResponse response)
        {
            string layout;
            var layoutInfo = _environment.WebRootFileProvider.GetFileInfo(_options.LayoutPage);
            if (!layoutInfo.Exists)
            {
                throw new InvalidOperationException("Failed to find the layout file for markdown web: '" +
                                                    _options.LayoutPage +
                                                    "'. Configure options.LayoutPage so that it points on a HTML file in your wwwroot directory.");
            }

            using (var stream = layoutInfo.CreateReadStream())
            {
                var sr = new StreamReader(stream);
                layout = await sr.ReadToEndAsync();
            }

            var page = LoadPage(webPath);
            response.ContentType = "text/html";
            var html = layout
                .Replace("{Wiki}", page.Body)
                .Replace("{Title}", page.Title)
                .Replace("{TableOfContents}", page.Parts.FirstOrDefault()?.Body ?? "");
            response.ContentType = "text/html";
            await response.WriteAsync(html);
        }

        private async Task RenderViewFile(HttpContext context, string path)
        {
            var page = LoadPage(path);
            var result = new ViewResult
            {
                ViewName = _options.LayoutPage,
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(),
                    new ModelStateDictionary())
                {
                    Model = page
                }
            };
            var routeData = context.GetRouteData() ?? EmptyRouteData;
            var actionContext = new ActionContext(context, routeData, EmptyActionDescriptor);
            await _viewResultExecutor.ExecuteAsync(actionContext, result);
        }

        private async Task ServeImages(string wikiPath, string imageUrl, HttpResponse response)
        {
            if (!imageUrl.StartsWith("/"))
            {
                //TODO: Relative image path.
            }

            var src = imageUrl.TrimStart('/');
            var filename = Path.GetFileName(src);
            new FileExtensionContentTypeProvider().TryGetContentType(filename, out var contentType);
            if (contentType == null)
            {
                contentType = "application/octet-stream";
            }

            var fullPath = Path.Combine(_rootDirectory, src.Replace("/", "\\"));
            response.ContentType = contentType;
            await using (var file = File.OpenRead(fullPath))
            {
                await file.CopyToAsync(response.BodyWriter.AsStream());
            }
        }
    }
}