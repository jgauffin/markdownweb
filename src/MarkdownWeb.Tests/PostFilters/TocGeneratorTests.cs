using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MarkdownWeb.PostFilters;
using Xunit;

namespace MarkdownWeb.Tests.PostFilters
{
   public  class TocGeneratorTests
    {
       [Fact]
       public void Test()
       {
           var html = @"<h1 id=""title"">Title</h1>
text

<h2 id=""subtitle"">Subtitle</h2>

sometext

<h1 id=""newHeading"">new h1</h1>

lalala";
           var ctx = new PostFilterContext(html);

           var sut = new TableOfContents();
           sut.Parse(ctx);

           ctx.GetParts().Should().NotBeEmpty();
           var part = ctx.GetParts()[0];
           part.Name.Should().Be("TableOfContents");
           part.Body.Should().Be(@"<div class=""toc"">
<ul>
<li class=""heading1""><a href=""#title"">Title</a></li>
<li class=""heading2""><a href=""#subtitle"">Subtitle</a></li>
<li class=""heading1""><a href=""#newHeading"">new h1</a></li>
</ul>
</div>
");
       }
    }
}
