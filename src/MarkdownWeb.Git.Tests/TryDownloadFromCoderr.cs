﻿using System;
using System.IO;
using System.Threading;
using FluentAssertions;
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
        public void Should_be_able_to_download_once()
        {
            var waiter = new ManualResetEvent(false);
            var pageReference = new PageReference("/", "/index.md");
            var settings = new GitSettings
            {
                FetchDirectory = _path,
                DocumentationDirectory = Path.Combine(_path, "docs"),
                RepositoryUri = new Uri("https://github.com/coderrapp/coderr.Documentation.git"),
                DownloadCompleted = () => waiter.Set()
            };

            using (var git = new GitPageRepository(settings))
            {
                git.Exists(pageReference);
                waiter.WaitOne(5000).Should().BeTrue();
            }

        }

        [Fact]
        public void Should_be_able_To_download_another_time_without_getting_sync_errors()
        {
            var waiter = new ManualResetEvent(false);
            var pageReference = new PageReference("/", "/index.md");
            var settings = new GitSettings
            {
                FetchDirectory = _path,
                DocumentationDirectory = Path.Combine(_path, "docs"),
                RepositoryUri = new Uri("https://github.com/coderrapp/coderr.Documentation.git"),
                UpdateInterval = TimeSpan.FromMilliseconds(0),
                DownloadCompleted = () => waiter.Set()
            };

            using (var git = new GitPageRepository(settings))
            {
                git.ErrorLogTask = (level, s, exception) => Console.WriteLine(exception);
                
                git.Exists(pageReference);
                waiter.WaitOne(5000).Should().BeTrue();
                waiter.Reset();
                git.Exists(pageReference);
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
