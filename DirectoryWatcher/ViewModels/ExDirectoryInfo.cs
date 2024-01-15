using System.IO;

namespace DirectoryWatcher.ViewModels
{
    public class ExDirectoryInfo
    {
        public ExDirectoryInfo(DirectoryInfo d)
        {
            DirectoryInfo = d;
        }

        public DirectoryInfo DirectoryInfo { get; init; }

        public int SubDirectoryCount { get; set; }

        public string Fullname => DirectoryInfo != null ? DirectoryInfo.FullName : string.Empty;
    }
}