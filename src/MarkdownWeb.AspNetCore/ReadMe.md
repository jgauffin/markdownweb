MarkdownWeb for ASP.NET Core
===============================

Installation:

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseRouting();

    //
    // Markdown web
    //
    app.UseMarkdownWeb(options =>
    {
        // Url to serve Markdown web from.
        // In this case: http//yourserver.com/doc/
        options.Path = new PathString("/doc");

        // Where the markdown files are located on disk,
        // either relative to the content root, or an absolute path.
        options.DocumentationDirectory = "wwwroot\markdownfiles";
    });


    app.UseStaticFiles();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapGet("/", async context =>
        {
            await context.Response.WriteAsync("Hello World!");
        });
    });
}
```