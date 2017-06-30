/**
 *  Copyright 2017 by Alexey Andreev <trashtalk.mute@gmail.com>
 *
 * ModPrepareViewModel.cs is part of SpintiresModsLoader.
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
 * create date: 2017-6-15, 14:17
 */
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using SpintiresModsLoader.Models;
using SpintiresModsLoader.Views.Base;

namespace SpintiresModsLoader.Views.UserControls
{
    public class ModPrepareViewModel : BaseViewModel
    {
        private ICommand _addNewModCommand;

        private string _inputAuthor;

        private string _inputName;

        private string _inputVersion;

        private bool _inputAddDoPrepend;

        private bool _modFileExist;

        private ModPossibleRootFolder _selectedRootFolder;

        private string _sourceModFilePath;

        public ModPrepareViewModel()
        {
            _addNewModCommand = new RelayCommand(obj => { });
            RootFolders.CollectionChanged += (sender, args) =>
            {
                NotifyPropertyChanged("SelectFolderVisible");
            };
            if (IsInDesignMode)
            {
                ModFileExist = true;
                InputName = Sys.FindResource("ModTitle") as string;
                InputVersion = "1.0.0";
                InputAuthor = "likemute";
                RootFolders.Add(new ModPossibleRootFolder()
                {
                    FullFolder = "\\lala\\fafa",
                    Folder = "fafa",
                    Weight = 10
                });
                RootFolders.Add(new ModPossibleRootFolder()
                {
                    FullFolder = "\\lala\\fafa2",
                    Folder = "fafa2",
                    Weight = 10
                });
            }
        }

        public ICommand AddNewModCommand
        {
            get => _addNewModCommand;
            set
            {
                _addNewModCommand = value;
                NotifyPropertyChanged("AddNewModCommand");
            }
        }

        public bool ModFileExist
        {
            get => _modFileExist;
            set
            {
                _modFileExist = value;
                NotifyPropertyChanged("ModFileExist");
                NotifyPropertyChanged("InputEditable");
                NotifyPropertyChanged("InputLabelVisible");
            }
        }

        public ObservableCollection<ModPossibleRootFolder> RootFolders { get; } =
            new ObservableCollection<ModPossibleRootFolder>();

        public Visibility SelectFolderVisible => RootFolders.Count > 1 ? Visibility.Visible : Visibility.Hidden;

        public bool InputEditable => !_modFileExist;

        public Visibility InputLabelVisible => InputEditable ? Visibility.Visible : Visibility.Collapsed;

        public string InputName
        {
            get => _inputName;
            set
            {
                _inputName = value;
                NotifyPropertyChanged("InputName");
            }
        }

        public string InputVersion
        {
            get => _inputVersion;
            set
            {
                _inputVersion = value;
                NotifyPropertyChanged("InputVersion");
            }
        }

        public string InputAuthor
        {
            get => _inputAuthor;
            set
            {
                _inputAuthor = value;
                NotifyPropertyChanged("InputAuthor");
            }
        }

        public string SourceModFilePath
        {
            get => _sourceModFilePath;
            set
            {
                _sourceModFilePath = value;
                NotifyPropertyChanged("SourceModFilePath");
            }
        }

        public bool InputAddDoPrepend
        {
            get => _inputAddDoPrepend;
            set
            {
                _inputAddDoPrepend = value;
                NotifyPropertyChanged("InputAddDoPrepend");
            }
        }

        public ModPossibleRootFolder SelectedRootFolder
        {
            get => _selectedRootFolder;
            set
            {
                _selectedRootFolder = value;
                if (_selectedRootFolder != null)
                {
                    var modfiles = Directory.GetFiles(SelectedRootFolder.FullFolder, "modinfo.xml",
                        SearchOption.TopDirectoryOnly);
                    if (modfiles.Length > 0)
                    {
                        var xmlFileInfo = XmlReader.Create(modfiles[0]);
                        xmlFileInfo.ReadToFollowing("name");
                        InputName = xmlFileInfo.ReadString();
                        xmlFileInfo.ReadToFollowing("version");
                        InputVersion = xmlFileInfo.ReadString();
                        xmlFileInfo.ReadToNextSibling("author");
                        InputAuthor = xmlFileInfo.ReadString();
                        xmlFileInfo.ReadToNextSibling("DoPrepend");
                        InputAddDoPrepend = xmlFileInfo.ReadString() == "true";
                        ModFileExist = true;
                    }
                    else
                    {
                        ModFileExist = false;
                        InputName = Path.GetFileNameWithoutExtension(SourceModFilePath);
                        InputVersion = "";
                        InputAuthor = Environment.UserName;
                    }
                }
                else
                {
                    ModFileExist = false;
                    InputName = "";
                    InputVersion = "";
                    InputAuthor = "";
                }
                NotifyPropertyChanged("SelectedRootFolder");
            }
        }
    }
}