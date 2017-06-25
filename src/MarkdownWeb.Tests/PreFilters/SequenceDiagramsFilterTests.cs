using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MarkdownWeb.PreFilters;
using Xunit;

namespace MarkdownWeb.Tests.PreFilters
{
    public class SequenceDiagramsFilterTests
    {
        [Fact]
        public void should_Be_able_to_parse_a_block_that_ends_the_Document()
        {
            var markdown = @"# Hello world

This is a test

```sequence
A->B: Hejsan
B->A: Oooopsie
```";
            var context = new PreFilterContext(new FakeService()) {TextToParse = markdown};

            var sut = new SequenceDiagramsFilter();
            var result = sut.Parse(context);

            var id = sut.Ids.First();
            result.Should().Be($@"# Hello world

This is a test

<div class=""sequencediagram"" id=""{id}""></div>
<script> 
    var d = Diagram.parse('A->B: Hejsan
B->A: Oooopsie');
    var options = {{ theme: 'Simple'}};
    d.drawSVG('{id}', options);
</script>");
        }

        [Fact]
        public void should_Be_able_to_parse_multiple_blocks_in_a_document()
        {
            var markdown = @"# Hello world

This is a test

```sequence
A->B: Hejsan
B->A: Oooopsie
```

Here comes the next one:

```sequence
Client->Server: Authenticate
Server->Client: Challenge(nonce)
Client->Server: Handshake
```";
            var context = new PreFilterContext(new FakeService()) { TextToParse = markdown };

            var sut = new SequenceDiagramsFilter();
            var result = sut.Parse(context);

            result.Should().Be($@"# Hello world

This is a test

<div class=""sequencediagram"" id=""{sut.Ids[0]}""></div>
<script> 
    var d = Diagram.parse('A->B: Hejsan
B->A: Oooopsie');
    var options = {{ theme: 'Simple'}};
    d.drawSVG('{sut.Ids[0]}', options);
</script>

Here comes the next one:

<div class=""sequencediagram"" id=""{sut.Ids[1]}""></div>
<script> 
    var d = Diagram.parse('Client->Server: Authenticate
Server->Client: Challenge(nonce)
Client->Server: Handshake');
    var options = {{ theme: 'Simple'}};
    d.drawSVG('{sut.Ids[1]}', options);
</script>");
        }

        [Fact]
        public void should_Be_able_to_parse_a_block_in_the_middle_of_the_document()
        {
            var markdown = @"# Hello world

This is a test

```sequence
A->B: Hejsan
B->A: Oooopsie
```

Some other paragraph.";
            var context = new PreFilterContext(new FakeService()) { TextToParse = markdown };

            var sut = new SequenceDiagramsFilter();
            var result = sut.Parse(context);

            var id = sut.Ids.First();
            result.Should().Be($@"# Hello world

This is a test

<div class=""sequencediagram"" id=""{id}""></div>
<script> 
    var d = Diagram.parse('A->B: Hejsan
B->A: Oooopsie');
    var options = {{ theme: 'Simple'}};
    d.drawSVG('{id}', options);
</script>

Some other paragraph.");
        }

        [Fact]
        public void should_Be_able_to_parse_a_block_in_the_start_of_the_document()
        {
            var markdown = @"```sequence
A->B: Hejsan
B->A: Oooopsie
```

Some other paragraph.";
            var context = new PreFilterContext(new FakeService()) { TextToParse = markdown };

            var sut = new SequenceDiagramsFilter();
            var result = sut.Parse(context);

            var id = sut.Ids.First();
            var expected = $@"<div class=""sequencediagram"" id=""{id}""></div>
<script> 
    var d = Diagram.parse('A->B: Hejsan
B->A: Oooopsie');
    var options = {{ theme: 'Simple'}};
    d.drawSVG('{id}', options);
</script>

Some other paragraph.";
            result.Should().Be(expected);
        }
    }
}
