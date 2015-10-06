using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MarkdownWeb.Storage.Files;
using Xunit;

namespace MarkdownWeb.Tests
{
    public class GithubIssues
    {

        [Fact]
        public void Issue1_dont_screw_up_the_page_when_using_tables()
        {
            var markdown = @"
###Liste des paquets Nuget pour IvFragment Secondaire N3.

Nom | Version | Descriptions
--- | ------- | ------------
N3_IVFragment_Secondaire_MVC5 | 1.2.1 | Utilitaires pour les fragments secondaire



###N3 IvFragment Secondaire MVC5

>Ce paquet Nuget intégrera les fichiers utilitaires requis pour aider à convertir une section d'application .NET MVC en fragment secondaire.
Quelques modifications seront exécutées dans le fichier de config pour installer le filtre N3 pour le fragment secondaire.

![image](https://cloud.githubusercontent.com/assets/10774173/5977134/0b3bfc66-a863-11e4-805b-a55f218f3b41.png)


The result code is like below; it is missing a th closing tag and merge the rest of the document in the table.
";
            var pathConverter = new UrlConverter("/");
            var repository = new FileBasedRepository(Environment.CurrentDirectory);

            var sut = new PageService(repository, pathConverter);
            var actual = sut.ParseString("", markdown);

            actual.Body.Should().Contain("<h3 id=\"N3IvFragmentSecondaireMVC5\">N3 IvFragment Secondaire MVC5</h3>");
            var posTable = actual.Body.IndexOf("</table>");
            var h3 = actual.Body.IndexOf("<h3 id=\"N3IvFragmentSecondaireMVC5");
            posTable.Should().BeLessThan(h3);
        }
    }
}
