using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
using MarkdownWeb.Storage;
using MarkdownWeb.Storage.Files;

namespace MarkdownWeb.Git
{
    /// <summary>
    ///     Makes sure that the latest source code is downloaded.
    /// </summary>
    public class GitPageRepository : IPageRepository, IDisposable, IPageSource
    {
        private readonly GitSettings _config;
        private readonly FileBasedRepository _fileBasedRepository;
        private readonly Repository _repos;
        private readonly object _syncLock = new object();
        private int _isRunning = 0;

        /// <summary>
        /// Creates a new instance of <see cref="GitPageRepository"/>.
        /// </summary>
        /// <param name="config">Configuration options for GIT.</param>
        public GitPageRepository(GitSettings config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            // First validate it.
            if (!Repository.IsValid(config.FetchDirectory) && Directory.Exists(config.FetchDirectory))
            {
                Directory.Delete(config.FetchDirectory, true);
            }

            if (!Directory.Exists(Path.Combine(config.FetchDirectory, ".git")))
            {
                // Next, check if we have a non-empty folder, but no .git.
                // which means that something went wrong. Delete it.
                if (Directory.Exists(config.FetchDirectory) && Directory.GetFiles(config.FetchDirectory).Length > 0)
                {
                    Directory.Delete(config.FetchDirectory, true);
                }

                // Ok, now we can clone everything.
                Repository.Clone(_config.RepositoryUri.ToString(), _config.FetchDirectory);

                File.WriteAllText(CacheFile, "Now");
            }

            _repos = new Repository(config.FetchDirectory);
            _fileBasedRepository = new FileBasedRepository(config.DocumentationDirectory);
        }


        /// <summary>
        ///     Use this to be able to log errors that happen when the repository is fetched in the background.
        /// </summary>
        public Action<LogLevel, string, Exception> ErrorLogTask { get; set; }



        private string CacheFile
        {
            get
            {
                var cacheFile = Path.Combine(_config.FetchDirectory, "stamp.dat");
                return cacheFile;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _repos.Dispose();
        }

        /// <inheritdoc />
        public StoredPage Get(PageReference pageReference)
        {
            EnsureCache();
            return _fileBasedRepository.Get(pageReference);
        }

        /// <inheritdoc />
        public StoredPage Get(PageReference pageReference, int revision)
        {
            EnsureCache();
            return _fileBasedRepository.Get(pageReference);
        }

        /// <inheritdoc />
        public PageMetadata[] GetRevisions(PageReference pageReference)
        {
            EnsureCache();
            return _fileBasedRepository.GetRevisions(pageReference);
        }

        public IEnumerable<string> GetAllPagesAsLinks(Func<string, bool> pathFilter)
        {
            EnsureCache();
            return _fileBasedRepository.GetAllPagesAsLinks(pathFilter);
        }

        public IReadOnlyList<PageReferenceWithChildren> GetAllPages(string wikiPath, Func<string, bool> pathFilter)
        {
            EnsureCache();
            return _fileBasedRepository.GetAllPages(wikiPath, pathFilter);
        }

        /// <inheritdoc />
        public bool Exists(PageReference pageReference)
        {
            EnsureCache();
            return _fileBasedRepository.Exists(pageReference);
        }

        /// <inheritdoc />
        public void Create(string wikiPagePath, EditedPage page)
        {
            throw new NotSupportedException("Changes should be made directly in the git repository.");
        }

        /// <inheritdoc />
        public void Update(string wikiPagePath, EditedPage page)
        {
            throw new NotSupportedException("Changes should be made directly in the git repository.");
        }

        /// <summary>
        ///     Updates the already cloned git repository, thus it's not required that it runs for the requesting thread.
        /// </summary>
        private void EnsureCache()
        {
            if (Interlocked.CompareExchange(ref _isRunning, 1, 0) != 0)
                return;

            lock (_syncLock)
            {
                var cacheFile = CacheFile;
                if (File.Exists(cacheFile) &&
                    DateTime.UtcNow.Subtract(File.GetLastWriteTimeUtc(cacheFile)) < _config.UpdateInterval)
                    return;

                //do it in the background
                if (_config.UpdateInBackground)
                    Task.Run(() => { SyncRepository(cacheFile, true); }
                    ).ContinueWith(x => ErrorLogTask?.Invoke(LogLevel.Debug, "Pull completed", null));
                else
                    SyncRepository(cacheFile, true);
            }
        }

        private void SyncRepository(string cacheFile, bool isFirstAttempt)
        {
            try
            {
                ErrorLogTask?.Invoke(LogLevel.Info, "Attempting to pull form origin " + _config.RepositoryUri, null);
                var user = new Signature("markdownweb", "info@markdownweb.com", DateTimeOffset.UtcNow);
                var options = new PullOptions {MergeOptions = new MergeOptions
                {
                    CommitOnSuccess = true,
                    FailOnConflict = false,
                    MergeFileFavor = MergeFileFavor.Theirs,
                    IgnoreWhitespaceChange = true,
                    FileConflictStrategy = CheckoutFileConflictStrategy.Theirs,
                    
                }};

                try
                {
                    Commands.Pull(_repos, user, options);
                }
                catch (CheckoutConflictException ex)
                {
                    if (!_config.ResetOnConflicts)
                        throw;

                    ErrorLogTask?.Invoke(LogLevel.Warning, "Got conflict, doing a reset hard.", ex);
                    _repos.Reset(ResetMode.Hard);
                    Commands.Pull(_repos, user, options);
                }
                //var originMaster=_repos.Branches["origin/master"];
                //_repos.Merge(originMaster, user, new MergeOptions(){CommitOnSuccess = true})
                File.WriteAllText(cacheFile, "Now");
                _config.DownloadCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                if (_config.DeleteAndFetchOnErrors && isFirstAttempt)
                {
                    ErrorLogTask?.Invoke(LogLevel.Warning, "Failed to pull from origin, deleting and restarting.", ex);
                    Directory.Delete(_config.FetchDirectory, true);
                    SyncRepository(cacheFile, false);
                }
                else
                {
                    ErrorLogTask?.Invoke(LogLevel.Error, "Failed to pull from origin.", ex);
                }
            }
            finally
            {
                Interlocked.Exchange(ref _isRunning, 0);
            }
        }

        public bool PageExists(PageReference page)
        {
            return Exists(page);
        }
    }
}