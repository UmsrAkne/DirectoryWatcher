using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
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
        private FileSystemWatcher selectedItem;

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

        public FileSystemWatcher SelectedItem
        {
            get => selectedItem;
            set => SetProperty(ref selectedItem, value);
        }

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

        public DelegateCommand DeleteWatchingDirectoryCommand => new DelegateCommand(() =>
        {
            if (SelectedItem == null)
            {
                return;
            }

            SelectedItem.Created -= RequestSound;
            WatchingDirectories.Remove(SelectedItem);
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
                    fsw.Created += RequestSound;

                    WatchingDirectories.Add(fsw);
                    d.SubDirectoryCount = additionCount++;
                }
            }
        }

        private void RequestSound(object sender, FileSystemEventArgs e)
        {
            if (Directory.Exists(e.FullPath))
            {
                // 作成されたのがディレクトリだった場合は音は鳴らさない。
                // 新しいディレクトリは監視ディレクトリに加える。
                // コレクションの操作には dispatcher を経由しないとダメ。直接操作しようとすると落ちる。
                var dispatcher = Application.Current.Dispatcher;

                dispatcher.Invoke(() =>
                {
                    AddWatchingDirectory(new ExDirectoryInfo(new DirectoryInfo(e.FullPath)));
                });
            }
            else
            {
                soundPlayRequested = true;
            }
        }
    }
}