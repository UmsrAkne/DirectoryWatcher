using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Timers;
using DirectoryWatcher.Models;
using Prism.Commands;
using Prism.Mvvm;

namespace DirectoryWatcher.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MainWindowViewModel : BindableBase, IDisposable
    {
        private readonly Timer timer;
        private string title = "Prism Application";
        private ObservableCollection<ExDirectoryInfo> directoryInfos = new ();
        private string directoryPath;
        private bool soundPlayRequested;

        public MainWindowViewModel()
        {
            SoundFilePath = new FileInfo("notification.wav").FullName;

            timer = new Timer();
            timer.Elapsed += async (_, _) =>
            {
                if (soundPlayRequested)
                {
                    soundPlayRequested = false;
                    await Player.PlaySoundAsync(!string.IsNullOrWhiteSpace(SoundFilePath) ? SoundFilePath : "notification.wav");
                }
            };

            timer.Interval = 3000;
            timer.Start();
        }

        public string Title { get => title; set => SetProperty(ref title, value); }

        public string SoundFilePath { get; set; }

        public ObservableCollection<FileSystemWatcher> WatchingDirectories { get; set; } = new ();

        public ObservableCollection<ExDirectoryInfo> DirectoryInfos
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

            var d = new ExDirectoryInfo(new DirectoryInfo(DirectoryPath));
            if (DirectoryInfos.FirstOrDefault(f => f.FullName == d.FullName) != null)
            {
                return;
            }

            DirectoryInfos.Add(d);
            DirectoryPath = string.Empty;

            AddWatchingDirectory(d);
        });

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            timer.Dispose();
        }

        private void AddWatchingDirectory(ExDirectoryInfo d)
        {
            var directories =
                Directory.GetDirectories(d.DirectoryInfo.FullName, "*", SearchOption.AllDirectories)
                    .Select(p => new DirectoryInfo(p)).ToList();

            directories.Add(new DirectoryInfo(d.FullName));

            var additionCount = 0;

            foreach (var di in directories)
            {
                if(WatchingDirectories.All(fw => fw.Path != di.FullName))
                {
                    var fsw = new FileSystemWatcher(di.FullName);
                    fsw.EnableRaisingEvents = true;
                    fsw.Created += (_, _) =>
                    {
                        soundPlayRequested = true;
                    };

                    WatchingDirectories.Add(fsw);
                    d.SubDirectoryCount = additionCount++;
                }
            }
        }
    }
}