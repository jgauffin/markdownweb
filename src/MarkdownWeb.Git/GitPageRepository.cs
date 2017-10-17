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

        /// <summary>
        /// Use this to be able to log errors that happen when the repos is fetched in the background.
        /// </summary>
        public Action<string, Exception> ErrorLogTask { get; set; }

        /// <summary>
        /// Invoked once a download have been completed.
        /// </summary>
        public Action DownloadCompleted { get; set; }

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

        /// <summary>
        /// Updates the already cloned git repos, thus it's not required that it runs for the requesting thread.
        /// </summary>
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
                    try
                    {
                        Commands.Pull(_repos, new Signature("origin", "info@coderrapp.com", DateTimeOffset.UtcNow),
                            new PullOptions() {MergeOptions = new MergeOptions {CommitOnSuccess = true,}});
                        File.WriteAllText(cacheFile, "Now");
                        DownloadCompleted?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        ErrorLogTask?.Invoke("Failed to pull origin", ex);
                    }
                });
            }
        }

        public void Dispose()
        {
            _repos.Dispose();
        }
    }
}