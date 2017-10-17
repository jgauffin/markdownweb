using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Xunit;

namespace MarkdownWeb.Git.Tests
{
    public class TryDownloadFromCoderr : IDisposable
    {
        private string _path;

        public TryDownloadFromCoderr()
        {
            _path = Path.Combine(Path.GetDirectoryName(Path.GetTempPath()), Path.GetFileNameWithoutExtension(Path.GetTempFileName()));
            Directory.CreateDirectory(_path);

        }

        [Fact]
        public void should_be_able_to_download_once()
        {
            var waiter = new ManualResetEvent(false);
            var settings = new GitStorageConfiguration
            {
                FetchDirectory = _path,
                DocumentationDirectory = Path.Combine(_path, "docs"),
                RepositoryUri = new Uri("https://github.com/coderrapp/coderr.Documentation.git"),
            };

            using (var git = new GitPageRepository(settings))
            {
                git.DownloadCompleted = () => waiter.Set();
                git.Exists("/");
                waiter.WaitOne(5000).Should().BeTrue();
            }

        }

        [Fact]
        public void should_be_able_To_download_another_time_without_getting_sync_errors()
        {
            var waiter = new ManualResetEvent(false);
            var settings = new GitStorageConfiguration
            {
                FetchDirectory = _path,
                DocumentationDirectory = Path.Combine(_path, "docs"),
                RepositoryUri = new Uri("https://github.com/coderrapp/coderr.Documentation.git"),
                UpdateInterval = TimeSpan.FromMilliseconds(0)
            };

            using (var git = new GitPageRepository(settings))
            {
                git.ErrorLogTask = (s, exception) => Console.WriteLine(exception);
                git.DownloadCompleted = () => waiter.Set();
                git.Exists("/");
                waiter.WaitOne(5000).Should().BeTrue();
                waiter.Reset();
                git.Exists("/");
                waiter.WaitOne(5000).Should().BeTrue();
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
