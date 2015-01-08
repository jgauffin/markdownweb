Welcome to this generator
============

There are just three things that you need to know about this generator.

1. Always start with a heading (like above, i.e. with ==== on the second line).
2. Links without http:// are [parsed](parsed) as a folder or [document](example).
3. Folders must have a index.md

# Markdown specifics

This generator supports standard markdown with the following extensions

## Links

Links can be refer to parent folders `[Example](../index)` or to subfolder `[Sub folder](parsed)`.

## Tables

Tables as per the [GitHub Markdown specification](https://help.github.com/articles/github-flavored-markdown/).

*(Alignment is not supported yet.)*

NickName | URL
-------- | -----------------------
Jonas | http://blog.gauffin.org


## Code blocks

We recommend that you use fenced code blocks as specified by the [GitHub Markdown specification](https://help.github.com/articles/github-flavored-markdown/).

# Styling

Check the `Content\MarkdownWeb.css` for an example.

Include the stylesheet in your `Layout.cshtml` for the default styling

All pages are wrapped in a `doc-pages` class, which makes it possible to customize
standard styles just for this documentation generator. You can change that name in the `Views\MarkdownDoc\Index.cshtml`.

```csharp
foreach (var name in names)
{
    Console.WriteLine(name);
}
```

## Code styling

We recommend [Prism](http://prismjs.com/download.html) for syntax coloring. 

Code blocks are tagged with `language-XXXX` class name as recommended by the [HTML5 Specification](http://www.w3.org/TR/html5/text-level-semantics.html#the-code-element). We do
also add an attribute called `data-lang`.

## Table styling

We use the boostrap styles `table table-striped table-bordered` on all tables.

