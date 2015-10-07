# MarkdownWeb

This project is a web page generator that uses Markdown as the format. You can use wiki link syntax
to link between different pages. It also supports editing of pages using a Markdown editor with preview support and syntax highlighting.

If you implement your repository you can also store page revisions (to get a complete wiki functionality).


This is the generator behind the [SharpMessaging](http://sharpmessaging.net/doc) and [Griffin Framework](http://griffinframework.net/doc) homepages.

# Support

* All basic markdown formatters.
* Fenced codeblocks (with language specified as in Github pages)
* Github table format
* Links between markdown pages
* Local images
* Markdown editor
* Save/Edit pages

# Installation

**Basic installation**

    install-package markdownweb
	
**ASP.NET Mvc template**

Generates a controller, a razor view and sample documentation.

    install-package markdownweb.mvc5

	
# Manual installation
	
	
The `PageService` is the main class which takes care of all parsing. It do however need to know how to treat links and 
where it can load/store pages. It do therefore have two dependencies that you need to configure first.

## IPageRepository

The abstraction for the datastorage. The built in storage is using the harddrive for storage. You should specify which directory
the files can be stored in. The structure will mirror the structure you use when you create pages.


## IUrlConverter

Ass the markdown pages can be placed anywhere in your web site structure the library needs to know where the pages is located to make it
url independent. That transaction is handled by the `IUrlConverter` interface.

For instance you might have the pages in `http://yoursite.com/doc/` while all links in the pages are relative to the doc root (`/`). Thus
when someone surfs to `http://yoursite.com/doc/users/create/` the library just want to see it as `/users/create`.

## Complete configuration

```csharp
var repository = new FileBasedRepository(@"C:\Web\MarkdownPages");
var converter = new UrlConverter(VirtualPathUtility.ToAbsolutePath("~/doc/"));
var service = new PageService(repository, converter);
```

Then you just need to pass an url to the service:

```csharp
var htmlPage = service.ParseUrl(Request.Url.AbsolutePath);
```

.. or just markdown:

```csharp
var htmlPage = service.ParseString("/doc/", myMarkdownDocument);
```

