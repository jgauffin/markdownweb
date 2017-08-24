using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LibGit2Sharp;
using MarkdownWeb.Storage;
using MarkdownWeb.Storage.Files;

namespace MarkdownWeb.Git
{
    /// <summary>
    ///     Makes sure that the lastest source code is downloaded.
    /// </summary>
    public class GitPageRepository : IPageRepository, IDisposable
    {
        private readonly GitStorageConfiguration _config;
        private readonly FileBasedRepository _fileBasedRepository;
        private readonly Repository _repos;
        private readonly object _syncLock = new object();

        public GitPageRepository(GitStorageConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            if (!Directory.Exists(Path.Combine(config.FetchDirectory, ".git")))
            {
                Repository.Clone(_config.RepositoryUri.ToString(), _config.FetchDirectory);
                File.WriteAllText(CacheFile, "Now");
            }

            _repos = new Repository(config.FetchDirectory);
            _fileBasedRepository = new FileBasedRepository(config.DocumentationDirectory);
        }

        private string CacheFile
        {
            get
            {
                var cacheFile = Path.Combine(_config.FetchDirectory, "stamp.dat");
                return cacheFile;
            }
        }

        public StoredPage Get(string wikiPagePath)
        {
            EnsureCache();
            return _fileBasedRepository.Get(wikiPagePath);
        }

        public StoredPage Get(string wikiPagePath, int revision)
        {
            EnsureCache();
            return _fileBasedRepository.Get(wikiPagePath);
        }

        public PageMetadata[] GetRevisions(string wikiPagePath)
        {
            EnsureCache();
            return _fileBasedRepository.GetRevisions(wikiPagePath);
        }

        public bool Exists(string wikiPagePath)
        {
            EnsureCache();
            return _fileBasedRepository.Exists(wikiPagePath);
        }

        public void Create(string wikiPagePath, EditedPage page)
        {
            throw new NotSupportedException("Changes should be made directly in the git repository.");
        }

        public void Update(string wikiPagePath, EditedPage page)
        {
            throw new NotSupportedException("Changes should be made directly in the git repository.");
        }

        private void EnsureCache()
        {
            lock (_syncLock)
            {
                var cacheFile = CacheFile;
                if (File.Exists(cacheFile) &&
                    DateTime.UtcNow.Subtract(File.GetLastWriteTimeUtc(cacheFile)) < _config.UpdateInterval)
                    return;

                //do it in the background
                Task.Run(() =>
                {
                    Commands.Fetch(_repos, "origin", new List<string>(), new FetchOptions(), "");
                    File.WriteAllText(cacheFile, "Now");
                });
            }
        }

        public void Dispose()
        {
            _repos.Dispose();
        }
    }
}