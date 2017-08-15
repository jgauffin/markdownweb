using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace MarkdownWeb.Git
{
    public class GitStorageConfiguration
    {
        public Uri RepositoryUri { get; set; }
        public Credentials Credentials { get; set; }

        public TimeSpan UpdateInterval { get; set; }

        public string LocalDirectory { get; set; }

    }
}
