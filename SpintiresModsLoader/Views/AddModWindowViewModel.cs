/**
 *  Copyright 2017 by Alexey Andreev <trashtalk.mute@gmail.com>
 *
 * AddModWindowViewModel.cs is part of SpintiresModsLoader.
 * 
 * SpintiresModsLoader is free software: you can redistribute 
 * it and/or modify it under the terms of the GNU General Public 
 * License as published by the Free Software Foundation, either 
 * version 3 of the License, or (at your option) any later version.
 * 
 * Some open source application is distributed in the hope that it will 
 * be useful, but WITHOUT ANY WARRANTY; without even the implied warranty 
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
 *
 * @license GPL-3.0+ <http://spdx.org/licenses/GPL-3.0+>
 *
 * author: likemute (Alexey Andreev) <trashtalk.mute@gmail.com>
 * github: https://github.com/likemute
 * create date: 2017-6-15, 14:23
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using SevenZip;
using SpintiresModsLoader.Models;
using SpintiresModsLoader.Views.Base;
using SpintiresModsLoader.Views.UserControls;

namespace SpintiresModsLoader.Views
{
    public class AddModWindowViewModel : BaseViewModel
    {
        private UserControl _currentPage;

        private string[] _sourceModsFilePaths;

        private int _sourceModFileIndex = -1;

        private string _tmpDir;

        private readonly ObservableCollection<ModPossibleRootFolder> _rootFolders = new ObservableCollection<ModPossibleRootFolder>();

        public UserControl CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                NotifyPropertyChanged("CurrentPage");
            }
        }

        public string[] SourceModsFilePaths
        {
            get => _sourceModsFilePaths;
            set
            {
                _sourceModsFilePaths = value;
                NotifyPropertyChanged("SourceModsFilePaths");
                _sourceModFileIndex = -1;
                NotifyPropertyChanged("SourceModFilePath");
            }
        }

        public string SourceModFilePath
        {
            get
            {
                if (_sourceModFileIndex > -1 && SourceModsFilePaths.Length > _sourceModFileIndex)
                {
                    return SourceModsFilePaths[_sourceModFileIndex];
                }
                return null;
            }
        }

        public ICommand CloseAddWindowEvent { get; set; }

        public AddModWindowViewModel()
        {
            PropertyChanged += AddModWindowViewModel_PropertyChanged;
            CurrentPage = new ModUnpack();
            CloseAddWindowEvent = new RelayCommand(obj =>
            {
                var di = new DirectoryInfo(GetApp().TempPath);
                foreach (var file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (var dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
            });
        }

        private void ProcessQueue()
        {
            _sourceModFileIndex = _sourceModFileIndex + 1;
            if (string.IsNullOrEmpty(SourceModFilePath))
            {
                ((MainWindowViewModel)Sys.MainWindow.DataContext).CloseAddNewModWindow();
                return;
            }
            NotifyPropertyChanged("SourceModFilePath");
        }

        private void AddModWindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "SourceModFilePath") return;
            if (string.IsNullOrEmpty(SourceModFilePath))
            {
                ProcessQueue();
                return;
            }
            CurrentPage = new ModUnpack();
            ((ModUnpackViewModel)CurrentPage.DataContext).Title = Path.GetFileName(SourceModFilePath);
            Thread unpackThread = new Thread(() => {
                using (var extr = new SevenZipExtractor(SourceModFilePath))
                {
                    if (extr.Format == InArchiveFormat.Zip)
                    {
                        var newMod = GetApp().ReadModConfigFromFile(SourceModFilePath);
                        if (newMod != null)
                        {
                            var existingMod = GetApp().AllModList.FirstOrDefault(p => p.Name == newMod.Name);
                            var newFilename = Path.Combine(GetApp().TempPath, string.Concat(Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8), ".zip"));
                            File.Copy(SourceModFilePath, newFilename);
                            newMod.FilePath = newFilename;
                            if (existingMod != null)
                            {
                                var currentVersion = new Version(existingMod.Version);
                                var newVersion = new Version(newMod.Version);
                                if (newVersion.CompareTo(currentVersion) < 0)
                                {
                                    Sys.Dispatcher.Invoke(delegate
                                    {
                                        CurrentPage = new ErrorMessageWithButton();
                                        ((ErrorMessageWithButtonViewModel)CurrentPage.DataContext).Message = Sys.FindResource("ModAlreadyAddedWithGreaterVersion") as string;
                                        ((ErrorMessageWithButtonViewModel)CurrentPage.DataContext).CloseCommand =
                                            new RelayCommand(obj =>
                                            {
                                                ProcessQueue();
                                            });
                                    });
                                    return;
                                }
                                Sys.Dispatcher.Invoke(delegate
                                {
                                    GetApp().DeleteMod(existingMod);
                                });
                            }
                            Sys.Dispatcher.Invoke(delegate
                            {
                                GetApp().AddMod(newMod);
                                ProcessQueue();
                            });
                            return;
                        }
                    }
                }
                try
                {
                    _tmpDir = Path.Combine(GetApp().TempPath, Path.GetRandomFileName());
                    Directory.CreateDirectory(_tmpDir);
                    using (var extr = new SevenZipExtractor(SourceModFilePath))
                    {
                        extr.Extracting += (o, args) =>
                        {
                            Sys.Dispatcher.Invoke(delegate
                            {
                                ((ModUnpackViewModel)CurrentPage.DataContext).Value = args.PercentDone;
                            });
                        };
                        extr.ExtractArchive(_tmpDir);
                    }
                    Sys.Dispatcher.Invoke(delegate // <--- HERE
                    {
                        _rootFolders.Clear();
                    });
                    var newRootFolders = SearchRootModFolder(_tmpDir);
                    foreach (ModPossibleRootFolder rootFolder in newRootFolders)
                    {
                        Sys.Dispatcher.Invoke(delegate // <--- HERE
                        {
                            _rootFolders.Add(rootFolder);
                        });
                    }
                    if (_rootFolders.Count != 0)
                    {
                        Sys.Dispatcher.Invoke(PrepareMod);
                        return;
                    }
                    var levelfiles = Directory.GetFiles(_tmpDir, "level_*", SearchOption.TopDirectoryOnly);
                    if (Directory.GetDirectories(_tmpDir).Length == 0 && levelfiles.Length > 1)
                    {
                        Directory.CreateDirectory(Path.Combine(_tmpDir, "levels"));
                        foreach (string levelfile in levelfiles)
                            File.Copy(levelfile, levelfile.Replace(_tmpDir, Path.Combine(_tmpDir, "levels")), true);
                        Sys.Dispatcher.Invoke(delegate // <--- HERE
                        {
                            _rootFolders.Add(new ModPossibleRootFolder()
                            {
                                FullFolder = _tmpDir,
                                Folder = "",
                                Weight = 10
                            });
                        });
                        Sys.Dispatcher.Invoke(PrepareMod);
                        return;
                    }
                    Thread.CurrentThread.Abort();
                }
                catch (ThreadAbortException)
                {
                    Directory.Delete(_tmpDir, true);
                    _tmpDir = null;
                    Sys.Dispatcher.Invoke(delegate
                    {
                        _rootFolders.Clear();
                        ProcessQueue();
                    });
                    
                }
            });
            Sys.Dispatcher.Invoke(delegate
            {
                ((ModUnpackViewModel)CurrentPage.DataContext).CancelCommand = new RelayCommand(obj => {
                    unpackThread.Abort();
                    ProcessQueue();
                });
            });
            unpackThread.Start();
        }

        private void PrepareMod()
        {
            CurrentPage = new ModPrepare();
            var modPrepareVm = ((ModPrepareViewModel)CurrentPage.DataContext);
            modPrepareVm.SourceModFilePath = SourceModFilePath;
            ModPossibleRootFolder selected = null;
            foreach (var rootFolder in _rootFolders)
            {
                if (selected == null || selected.Weight < rootFolder.Weight)
                {
                    selected = rootFolder;
                }
                modPrepareVm.RootFolders.Add(rootFolder);
            }
            modPrepareVm.SelectedRootFolder = selected;
            if (((MainWindowViewModel)Sys.MainWindow.DataContext).ExtendedAddModDialogs)
            {
                modPrepareVm.AddNewModCommand = new RelayCommand(obj =>
                {
                    if (modPrepareVm.SelectedRootFolder == null) return;
                    if (!modPrepareVm.ModFileExist)
                    {
                        new XDocument(
                                new XElement("modInfo",
                                    new XElement("name", modPrepareVm.InputName),
                                    new XElement("version", modPrepareVm.InputVersion),
                                    new XElement("author", modPrepareVm.InputAuthor),
                                    new XElement("versionDate", DateTime.UtcNow.ToString("s") + "Z")
                                )
                            )
                            .Save(Path.Combine(modPrepareVm.SelectedRootFolder.FullFolder, "modinfo.xml"));
                    }
                    PackMod(modPrepareVm.SelectedRootFolder, modPrepareVm.InputName);
                });
            }
            else
            {
                if (modPrepareVm.SelectedRootFolder == null)
                {
                    ProcessQueue();
                    return;
                }
                if (!modPrepareVm.ModFileExist)
                {
                    new XDocument(
                            new XElement("modInfo",
                                new XElement("name", modPrepareVm.InputName),
                                new XElement("version", modPrepareVm.InputVersion),
                                new XElement("author", modPrepareVm.InputAuthor),
                                new XElement("versionDate", DateTime.UtcNow.ToString("s") + "Z")
                            )
                        )
                        .Save(Path.Combine(modPrepareVm.SelectedRootFolder.FullFolder, "modinfo.xml"));
                }
                PackMod(modPrepareVm.SelectedRootFolder, modPrepareVm.InputName);
            }
        }

        private void PackMod(ModPossibleRootFolder rootFolder, string title)
        {
            CurrentPage = new ModPack();
            ModPackViewModel modPackVm = ((ModPackViewModel) CurrentPage.DataContext);
            modPackVm.Title = title;
            Thread packThread = new Thread(() =>
            {
                var newFileName = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8);
                var newFilePath = Path.Combine(GetApp().TempPath, String.Concat(newFileName, ".zip"));
                try
                {
                    var textureCacheExists = false;
                    if (Directory.Exists(Path.Combine(rootFolder.FullFolder, "TextureCache")))
                    {
                        SevenZipCompressor cmpTс = new SevenZipCompressor();
                        cmpTс.ArchiveFormat = OutArchiveFormat.Zip;
                        cmpTс.CompressionLevel = CompressionLevel.Low;
                        cmpTс.CompressDirectory(Path.Combine(rootFolder.FullFolder, "TextureCache"), Path.Combine(GetApp().TempPath, String.Concat(newFileName, "-tc.zip")));
                        Directory.Delete(Path.Combine(rootFolder.FullFolder, "TextureCache"), true);
                        textureCacheExists = true;
                    }
                    var meshCacheExists = false;
                    if (Directory.Exists(Path.Combine(rootFolder.FullFolder, "MeshCache")))
                    {
                        SevenZipCompressor cmpMс = new SevenZipCompressor();
                        cmpMс.ArchiveFormat = OutArchiveFormat.Zip;
                        cmpMс.CompressionLevel = CompressionLevel.Low;
                        cmpMс.CompressDirectory(Path.Combine(rootFolder.FullFolder, "MeshCache"), Path.Combine(GetApp().TempPath, String.Concat(newFileName, "-mc.zip")));
                        Directory.Delete(Path.Combine(rootFolder.FullFolder, "MeshCache"), true);
                        meshCacheExists = true;
                    }
                    SevenZipCompressor cmp = new SevenZipCompressor();
                    cmp.Compressing += (o, args) =>
                    {
                        Sys.Dispatcher.Invoke(delegate
                        {
                            ((ModPackViewModel)CurrentPage.DataContext).Value = args.PercentDone;
                        });
                    };
                    cmp.ArchiveFormat = OutArchiveFormat.Zip;
                    cmp.CompressionLevel = CompressionLevel.Low;
                    cmp.CompressDirectory(rootFolder.FullFolder, Path.Combine(GetApp().TempPath, String.Concat(newFileName, ".zip")));
                    Directory.Delete(_tmpDir, true);
                    Sys.Dispatcher.Invoke(delegate
                    {
                        var newMod = GetApp().ReadModConfigFromFile(Path.Combine(GetApp().TempPath, String.Concat(newFileName, ".zip")));
                        if (newMod == null)
                        {
                            ProcessQueue();
                            return;
                        }
                        newMod.TextureCache = textureCacheExists;
                        newMod.MeshCache = meshCacheExists;
                        newMod.AddedToGame = true;
                        GetApp().AddMod(newMod);
                        ProcessQueue();
                    });
                }
                catch (ThreadAbortException){}
            });
            packThread.Start();
        }

        private IEnumerable<ModPossibleRootFolder> SearchRootModFolder(string root)
        {
            List<string> list = Directory.GetDirectories(root, "*", SearchOption.AllDirectories).ToList();
            list.Add(root);
            List<ModPossibleRootFolder> weightList = new List<ModPossibleRootFolder>();
            foreach (string folder in list)
            {
                int weight = 0;
                // ReSharper disable once AssignNullToNotNullAttribute
                string folderName = new DirectoryInfo(Path.GetFileName(folder)).Name;
                if (folderName.ToLower() == "media")
                {
                    weight = weight + (30);
                }
                var dirNames2 = new Regex("classes|levels", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                string[] subFolders2 = Directory.EnumerateDirectories(folder).Where(dir => dirNames2.IsMatch(new DirectoryInfo(dir).Name.ToLower())).ToArray();
                weight = weight + (subFolders2.Length * 30);
                var dirNames = new Regex("_templates|billboards|meshcache|meshes|texturecache|textures|scripts|sounds|strings", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                string[] subFolders = Directory.EnumerateDirectories(folder).Where(dir => dirNames.IsMatch(new DirectoryInfo(dir).Name.ToLower())).ToArray();
                weight = weight + (subFolders.Length * 10);
                string[] files = Directory.GetFiles(folder, "media.xml", SearchOption.TopDirectoryOnly);
                weight = weight + (files.Length * 20);
                string[] modfiles = Directory.GetFiles(folder, "modinfo.xml", SearchOption.TopDirectoryOnly);
                weight = weight + (modfiles.Length * 100);
                if (weight > 0)
                {
                    weightList.Add(new ModPossibleRootFolder()
                    {
                        Weight = weight,
                        Folder = folder.Replace(root, ""),
                        FullFolder = folder
                    });
                }
            }
            return weightList;
        }
    }
}
