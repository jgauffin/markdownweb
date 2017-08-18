using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Xunit;

namespace MarkdownWeb.Git.Tests
{
    public class TryDownloadFromOneTrueError : IDisposable
    {
        private string _path;

        public TryDownloadFromOneTrueError()
        {
            _path = Path.Combine(Path.GetDirectoryName(Path.GetTempPath()), Path.GetFileNameWithoutExtension(Path.GetTempFileName()));
            Directory.CreateDirectory(_path);

        }
        [Fact]
        public void Test()
        {
            var settings = new GitStorageConfiguration
            {
                FetchDirectory = _path,
                DocumentationDirectory = Path.Combine(_path, "docs"),
                RepositoryUri = new Uri("https://github.com/onetrueerror/OneTrueError.Documentation.git"),
            };

            using (var git = new GitPageRepository(settings))
            {
                git.Exists("/");
            }
                
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(_path, true);
            }
            catch (Exception)
            {
                //some files are not clean up, since they are not released.
                //dunno how to fix.
            }
            
        }
    }
}
