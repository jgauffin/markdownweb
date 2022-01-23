using System;
using Microsoft.AspNetCore.Builder;

namespace MarkdownWeb.AspNetCore
{
    public static class AppBuilderExtensions
    {
        public static IApplicationBuilder UseMarkdownWeb(this IApplicationBuilder builder,
            Action<MarkdownWebMiddlewareOptions> options = null)
        {
            var o = new MarkdownWebMiddlewareOptions();
            options?.Invoke(o);
            builder.UseMiddleware<MarkdownWebMiddleware>(o);
            return builder;
        }
    }
}