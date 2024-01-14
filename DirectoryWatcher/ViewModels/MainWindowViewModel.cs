using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Prism.Commands;
using Prism.Mvvm;

namespace DirectoryWatcher.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MainWindowViewModel : BindableBase
    {
        private string title = "Prism Application";
        private ObservableCollection<DirectoryInfo> directoryInfos = new ();
        private string directoryPath;

        public string Title { get => title; set => SetProperty(ref title, value); }

        public ObservableCollection<DirectoryInfo> DirectoryInfos
        {
            get => directoryInfos;
            set => SetProperty(ref directoryInfos, value);
        }

        public string DirectoryPath
        {
            get => directoryPath;
            set => SetProperty(ref directoryPath, value);
        }

        public DelegateCommand AddDirectoryInfoCommand => new (() =>
        {
            if (string.IsNullOrWhiteSpace(DirectoryPath))
            {
                return;
            }

            // 入力されている文字列がパスとして適切かを確認する。
            if (!Path.IsPathRooted(DirectoryPath) && !DirectoryPath.Contains(".."))
            {
                return;
            }

            var d = new DirectoryInfo(DirectoryPath);
            if (DirectoryInfos.FirstOrDefault(f => f.FullName == d.FullName) != null)
            {
                return;
            }

            DirectoryInfos.Add(d);
            DirectoryPath = string.Empty;
        });
    }
}