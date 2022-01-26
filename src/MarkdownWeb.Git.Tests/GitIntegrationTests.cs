using System;
using System.IO;
using Xunit;

namespace MarkdownWeb.Git.Tests
{
    public class GitIntegrationTests
    {
        public GitIntegrationTests()
        {
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
            _config = new GitSettings
            {
                FetchDirectory = _path,
                RepositoryUri = new Uri("https://github.com/coderrapp/codeRR.Documentation.git")
            };
        }

        private readonly string _path = @"C:\Temp\MarkdownSharp\Git\";
        private readonly GitSettings _config;

        [Fact]
        public void Should_merge_if_not_exist()
        {
            _config.UpdateInBackground = false;
            var repos = new GitPageRepository(_config);
            repos.Get(new PageReference("/", "/index.md"));
        }
    }
}