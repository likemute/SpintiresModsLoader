﻿/**
 *  Copyright 2017 by Alexey Andreev <trashtalk.mute@gmail.com>
 *
 * App.cs is part of SpintiresModsLoader.
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
 * create date: 2017-6-21, 16:04
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Win32;
using SevenZip;
using SpintiresModsLoader.Models;
using SpintiresModsLoader.Resources.Utils;
using SpintiresModsLoader.Views.Base;

namespace SpintiresModsLoader
{
    public class App : NotifyClass
    {
        #region Private Variables;
        private bool _loaded;
        private ObservableCollection<Mod> _allModList = new ObservableCollection<Mod>();
        private PropertyChangedListener<Mod> _allModListListener;
        private string _spintiresConfigXmlPath;
        private string _programDataPath;
        private string _modsPath;
        private string _tempPath;
        private FileSystemWatcher _watcher;
        private FileStream _programDataLocker;
        #endregion

        #region Public Variables;
        public bool Loaded
        {
            get => _loaded;
            set
            {
                _loaded = value;
                NotifyPropertyChanged("Loaded");
            }
        }

        public ObservableCollection<Mod> AllModList
        {
            get => _allModList;
            set
            {
                _allModList = value;
                NotifyPropertyChanged("AllModList");
            }
        }

        public PropertyChangedListener<Mod> AllModListListener
        {
            get => _allModListListener;
            set
            {
                _allModListListener = value;
                NotifyPropertyChanged("AllModListListener");
            }
        }

        public string SpintiresConfigXmlPath
        {
            get => _spintiresConfigXmlPath;
            set
            {
                _spintiresConfigXmlPath = value;
                var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SpintiresModsLoader");
                if (key == null) return;
                key.SetValue("xmlconfigPath", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SpinTires"));
                key.Close();
                NotifyPropertyChanged("SpintiresConfigXmlPath");
                NotifyPropertyChanged("SpintiresConfigXmlFound");
                UpdateSpintiresConfigXml();
            }
        }

        public bool SpintiresConfigXmlFound => File.Exists(Path.Combine(SpintiresConfigXmlPath, "Config.xml"));

        public string ProgramDataPath
        {
            get => _programDataPath;
            set
            {
                _programDataPath = value;
                NotifyPropertyChanged("ProgramDataPath");
            }
        }

        public string ModsPath
        {
            get => _modsPath;
            set
            {
                _modsPath = value;
                NotifyPropertyChanged("ModsPath");
            }
        }

        public string TempPath
        {
            get => _tempPath;
            set
            {
                _tempPath = value;
                NotifyPropertyChanged("TempPath");
            }
        }

        public FileSystemWatcher Watcher
        {
            get => _watcher;
            set
            {
                _watcher = value;
                NotifyPropertyChanged("Watcher");
            }
        }

        public FileStream ProgramDataLocker
        {
            get => _programDataLocker;
            set
            {
                _programDataLocker = value;
                NotifyPropertyChanged("ProgramDataLocker");
            }
        }
        #endregion

        #region Private Functions
        private void CreatePaths()
        {
            Watcher.EnableRaisingEvents = false;
            Directory.CreateDirectory(_programDataPath);
            Directory.CreateDirectory(_spintiresConfigXmlPath);
            Directory.CreateDirectory(ModsPath);
            Directory.CreateDirectory(TempPath);
            Watcher.EnableRaisingEvents = true;
        }

        private void UpdateSpintiresConfigXml()
        {
            if (!SpintiresConfigXmlFound) return;
            var xdoc = XDocument.Load(Path.Combine(_spintiresConfigXmlPath, "Config.xml"));
            var mediaNodesToRemove = xdoc.Descendants("MediaPath").Where(g => g.Attribute("ModLine") != null);
            mediaNodesToRemove.Remove();

            foreach (var mod in AllModList)
            {
                if (mod.AddedToGame)
                {
                    var newMediaPath = new XElement("MediaPath");
                    newMediaPath.SetAttributeValue("Path", mod.FilePath);
                    newMediaPath.SetAttributeValue("ModLine", "true");
                    newMediaPath.SetAttributeValue("Hash", mod.FileHash);
                    var configElement = xdoc.Element("Config");
                    configElement?.Descendants("MediaPath").FirstOrDefault(p => p.Attribute("Path")?.Value == "Media")?.AddBeforeSelf(newMediaPath);
                }
            }
            xdoc.Save(Path.Combine(_spintiresConfigXmlPath, "Config.xml"));
        }

        private void WatcherOnEvent(object sender1, FileSystemEventArgs fileSystemEventArgs)
        {
            if (fileSystemEventArgs.Name.IndexOf("Temp\\", StringComparison.Ordinal) == -1 && (fileSystemEventArgs.Name != "Temp" || fileSystemEventArgs.ChangeType != WatcherChangeTypes.Changed))
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    CreatePaths();
                    Refresh();
                });
            }
        }

        private bool ValidateCache()
        {
            try
            {
                var xdoc = XDocument.Load(Path.Combine(ProgramDataPath, "Cache.xml"));
                var mediaNodes = xdoc.Descendants("MediaPath").Where(g => g.Attribute("ModLine") != null).ToDictionary(o => o.Attribute("Path")?.Value, o => o.Attribute("Hash")?.Value);
                var totalFilesCount = 0;
                if (xdoc.Root != null)
                {
                    totalFilesCount = (int)xdoc.Root.Element("TotalFilesCount");
                }
                var files = Directory.GetFiles(ModsPath, "*.zip");
                if (files.Length != totalFilesCount)
                {
                    throw new Exception("Number of files in cache invalid");
                }
                foreach (KeyValuePair<string, string> mediaNode in mediaNodes)
                {
                    if (!files.Contains(Path.GetFileName(mediaNode.Key)))
                    {
                        throw new Exception("File from cache file not found");
                    }
                    using (var md5 = MD5.Create())
                    using (Stream stream = File.OpenRead(Path.GetFileName(mediaNode.Key)))
                    {
                        if (BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty) != mediaNode.Value)
                        {
                            throw new Exception("File hash from cache file not equal");
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void RecreateCache()
        {
            var files = Directory.GetFiles(ModsPath, "*.zip");
            var rootElement = new XElement("Root");
            rootElement.Add(new XElement("TotalFilesCount", files.Length));
            foreach (var fileName in files)
            {
                var filePath = Path.Combine(ModsPath, fileName);
                var mod = ReadModConfigFromFile(filePath);
                if (mod == null) continue;
                var newElement = new XElement("MediaPath");
                newElement.SetAttributeValue("Path", mod.FilePath);
                newElement.SetAttributeValue("Hash", mod.FileHash);
                newElement.SetAttributeValue("Name", mod.Name);
                newElement.SetAttributeValue("Version", mod.Version);
                newElement.SetAttributeValue("Author", mod.Author);
                rootElement.Add(newElement);
            }
            var newDocument = new XDocument(rootElement);
            newDocument.Save(Path.Combine(ProgramDataPath, "Cache.xml"));
        }

        private void ReadCache()
        {
            var cacheDoc = XDocument.Load(Path.Combine(ProgramDataPath, "Cache.xml"));
            var cacheNodes = cacheDoc.Root?.Descendants("MediaPath");
            if (cacheNodes != null)
            {
                var configDoc = XDocument.Load(Path.Combine(SpintiresConfigXmlPath, "Config.xml"));
                var configNodes = configDoc.Descendants("MediaPath").Where(g => g.Attribute("ModLine") != null).ToList();

                foreach (var mediaNode in cacheNodes)
                {
                    var filePath = (string)mediaNode.Attribute("Path");
                    var fileHash = (string)mediaNode.Attribute("Hash");
                    var name = (string)mediaNode.Attribute("Name");
                    var version = (string)mediaNode.Attribute("Version");
                    var author = (string)mediaNode.Attribute("Author");
                    var modToAdd = new Mod
                    {
                        Name = name,
                        Version = version,
                        Author = author,
                        FilePath = filePath,
                        FileHash = fileHash
                    };
                    if (configNodes.FirstOrDefault(p => p.Attribute("Path")?.Value == modToAdd.FilePath) != null)
                    {
                        modToAdd.AddedToGame = true;
                    }
                    AllModList.Add(modToAdd);
                }
            }
        }

        private void AddToCache(string filePath, string fileHash, string name, string version, string author)
        {
            var xdoc = XDocument.Load(Path.Combine(_programDataPath, "Cache.xml"));
            var mediaNodes = xdoc.Root?.Descendants("MediaPath").Where(p => p.Attribute("Path")?.Value == filePath);
            mediaNodes?.Remove();
            var newElement = new XElement("MediaPath");
            newElement.SetAttributeValue("Path", filePath);
            newElement.SetAttributeValue("Hash", fileHash);
            newElement.SetAttributeValue("Name", name);
            newElement.SetAttributeValue("Version", version);
            newElement.SetAttributeValue("Author", author);
            xdoc.Root?.Add(newElement);
            var fileCountXml = xdoc.Root?.Element("TotalFilesCount")?.Value;
            if (fileCountXml != null)
            {
                var fileCount = Int32.Parse(fileCountXml);
                xdoc.Root?.Element("TotalFilesCount")?.SetValue(fileCount + 1);
            }
            xdoc.Save(Path.Combine(_programDataPath, "Cache.xml"));
        }

        private void RemoveFromCache(string filePath)
        {
            var xdoc = XDocument.Load(Path.Combine(ProgramDataPath, "Cache.xml"));
            var mediaNodes = xdoc.Root?.Descendants("MediaPath").Where(p => p.Attribute("Path")?.Value == filePath);
            var fileCountXml = xdoc.Root?.Element("TotalFilesCount")?.Value;
            if (fileCountXml != null)
            {
                var fileCount = Int32.Parse(fileCountXml);
                xdoc.Root?.Element("TotalFilesCount")?.SetValue(fileCount - 1);
            }
            mediaNodes?.Remove();
            xdoc.Save(Path.Combine(ProgramDataPath, "Cache.xml"));
        }

        #endregion

        #region Public Functions
        public App()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SpintiresModsLoader");
            var language = key?.GetValue("language", GetLanguage().TwoLetterISOLanguageName).ToString();
            if (language != null) SetLanguage(new CultureInfo(language));
            ProgramDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SpintiresModsLoader");
            SpintiresConfigXmlPath = key?.GetValue("xmlconfigPath", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SpinTires")).ToString();
            ModsPath = key?.GetValue("modsPath", Path.Combine(ProgramDataPath, "Mods")).ToString();
            TempPath = key?.GetValue("tempPath", Path.Combine(ProgramDataPath, "Temp")).ToString();
            Directory.CreateDirectory(ProgramDataPath);
            Watcher = new FileSystemWatcher
            {
                Path = ProgramDataPath,
                IncludeSubdirectories = true,
                Filter = "*",
                NotifyFilter = NotifyFilters.DirectoryName |
                               NotifyFilters.FileName
            };
            Watcher.Deleted += WatcherOnEvent;
            Watcher.Changed += WatcherOnEvent;
            Watcher.Created += WatcherOnEvent;
            Watcher.Renamed += WatcherOnEvent;
            Watcher.EnableRaisingEvents = true;
            CreatePaths();
            ProgramDataLocker = File.Open(Path.Combine(ProgramDataPath, "run.lock"), FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
            key?.Close();
            AllModList.CollectionChanged += (sender, args) =>
            {
                if (_loaded)
                    UpdateSpintiresConfigXml();
            };
            AllModListListener = PropertyChangedListener.Create(AllModList);
            AllModListListener.PropertyChanged += (sender, args) =>
            {
                if (Loaded)
                    UpdateSpintiresConfigXml();
            };
            Application.Current.Exit += (sender, args) =>
            {
                Watcher.EnableRaisingEvents = false;
                _programDataLocker.Close();
                File.Delete(Path.Combine(_programDataPath, "run.lock"));
            };
            Refresh(true);
        }

        public void Refresh(bool force = false)
        {
            if (!ValidateCache() || force)
            {
                Loaded = false;
                AllModList.Clear();
                if (!ValidateCache())
                {
                    RecreateCache();
                }
                ReadCache();
                _loaded = true;
                UpdateSpintiresConfigXml();
            }
        }

        public Mod ReadModConfigFromFile(string filePath)
        {
            string fileHash;
            using (var md5 = MD5.Create())
            using (Stream stream = File.OpenRead(filePath))
            {
                fileHash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
            }
            using (var extr = new SevenZipExtractor(filePath))
            {
                try
                {
                    var memStream = new MemoryStream();
                    extr.ExtractFile("modinfo.xml", memStream);
                    memStream.Seek(0L, SeekOrigin.Begin);
                    var xmlFileInfo = XmlReader.Create(memStream);
                    xmlFileInfo.ReadToFollowing("name");
                    var name = xmlFileInfo.ReadString();
                    xmlFileInfo.ReadToFollowing("version");
                    var version = xmlFileInfo.ReadString();
                    xmlFileInfo.ReadToNextSibling("author");
                    var author = xmlFileInfo.ReadString();
                    var newMod = new Mod()
                    {
                        Author = author,
                        FileHash = fileHash,
                        FilePath = filePath,
                        Name = name,
                        Version = version
                    };
                    return newMod;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public void AddMod(Mod mod)
        {
            var newPath = mod.FilePath.Replace(TempPath, ModsPath);
            Watcher.EnableRaisingEvents = false;
            File.Move(mod.FilePath, newPath);
            Watcher.EnableRaisingEvents = true;
            mod.FilePath = newPath;
            AddToCache(newPath, mod.FileHash, mod.Name, mod.Version, mod.Author);
            AllModList.Add(mod);
        }

        public void DeleteMod(Mod mod)
        {
            RemoveFromCache(mod.FilePath);
            Watcher.EnableRaisingEvents = false;
            File.Delete(mod.FilePath);
            Watcher.EnableRaisingEvents = true;
            AllModList.Remove(mod);
        }

        public void SetLanguage(CultureInfo value)
        {
            var dict = new ResourceDictionary();
            switch (value.TwoLetterISOLanguageName)
            {
                case "ru":
                    dict.Source = new Uri($"locale/lang.{value.Name}.xaml", UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("locale/lang.xaml", UriKind.Relative);
                    break;
            }
            var oldDict = (from d in Application.Current.Resources.MergedDictionaries
                where d.Source != null && d.Source.OriginalString.StartsWith("locale/lang.")
                select d).First();
            if (oldDict != null)
            {
                int ind = Application.Current.Resources.MergedDictionaries.IndexOf(oldDict);
                Application.Current.Resources.MergedDictionaries.Remove(oldDict);
                Application.Current.Resources.MergedDictionaries.Insert(ind, dict);
            }
            else
            {
                Application.Current.Resources.MergedDictionaries.Add(dict);
            }
            Thread.CurrentThread.CurrentCulture = value;
            Thread.CurrentThread.CurrentUICulture = value;
            var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SpintiresModsLoader");
            if (key == null) return;
            key.SetValue("language", value.TwoLetterISOLanguageName);
            key.Close();
        }

        public CultureInfo GetLanguage()
        {
            return Thread.CurrentThread.CurrentCulture;
        }
        #endregion
    }
}
