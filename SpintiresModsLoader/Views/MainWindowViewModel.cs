/**
 *  Copyright 2017 by Alexey Andreev <trashtalk.mute@gmail.com>
 *
 * MainWindowViewModel.cs is part of SpintiresModsLoader.
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
 * create date: 2017-6-15, 14:24
 */
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.Win32;
using SpintiresModsLoader.Models;
using SpintiresModsLoader.Views.Base;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace SpintiresModsLoader.Views
{
    internal class MainWindowViewModel : BaseViewModel
    {
        private bool _settingsOpened;

        private bool _extendedAddModDialogs;

        private List<CultureInfo> _languages;

        private CultureInfo _selectedLanguage;

        /// <summary>
        ///     The _email
        /// </summary>
        private ObservableCollection<Mod> _allModList;

        private Mod _allModsSelectedItem;

        private ListCollectionView _allModsView;

        private AddModWindow _addNewWin;

        private ICommand _runGameCommand;

        public ICommand RunGameCommand
        {
            get => _runGameCommand;
            set
            {
                _runGameCommand = value;
                NotifyPropertyChanged("RunGameCommand");
            }
        }

        private ICommand _donatePaypalCommand;

        public ICommand DonatePaypalCommand
        {
            get => _donatePaypalCommand;
            set
            {
                _donatePaypalCommand = value;
                NotifyPropertyChanged("DonatePaypalCommand");
            }
        }

        private ICommand _donateYandexMoneyCommand;

        public ICommand DonateYandexMoneyCommand
        {
            get => _donateYandexMoneyCommand;
            set
            {
                _donateYandexMoneyCommand = value;
                NotifyPropertyChanged("DonateYandexMoneyCommand");
            }
        }

        private ICommand _toggleModCommand;

        public ICommand ToggleModCommand
        {
            get => _toggleModCommand;
            set
            {
                _toggleModCommand = value;
                NotifyPropertyChanged("ToggleModCommand");
            }
        }

        private ICommand _upActiveModCommand;

        public ICommand UpActiveModCommand
        {
            get => _upActiveModCommand;
            set
            {
                _upActiveModCommand = value;
                NotifyPropertyChanged("UpActiveModCommand");
            }
        }

        private ICommand _downActiveModCommand;

        public ICommand DownActiveModCommand
        {
            get => _downActiveModCommand;
            set
            {
                _downActiveModCommand = value;
                NotifyPropertyChanged("DownActiveModCommand");
            }
        }

        private ICommand _toggleSettingsCommand;

        public ICommand ToggleSettingsCommand
        {
            get => _toggleSettingsCommand;
            set
            {
                _toggleSettingsCommand = value;
                NotifyPropertyChanged("ToggleSettingsCommand");
            }
        }

        private ICommand _addNewModCommand;

        public ICommand AddNewModCommand
        {
            get => _addNewModCommand;
            set
            {
                _addNewModCommand = value;
                NotifyPropertyChanged("AddNewModCommand");
            }
        }

        private ICommand _deleteModCommand;

        public ICommand DeleteModCommand
        {
            get => _deleteModCommand;
            set
            {
                _deleteModCommand = value;
                NotifyPropertyChanged("DeleteModCommand");
            }
        }

        private ICommand _selectSpintiresConfigXmlPathCommand;

        public ICommand SelectSpintiresConfigXmlPathCommand
        {
            get => _selectSpintiresConfigXmlPathCommand;
            set
            {
                _selectSpintiresConfigXmlPathCommand = value;
                NotifyPropertyChanged("SelectSpintiresConfigXmlPathCommand");
            }
        }

        private ICommand _selectAppDataPathCommand;

        public ICommand SelectAppDataPathCommand
        {
            get => _selectAppDataPathCommand;
            set
            {
                _selectAppDataPathCommand = value;
                NotifyPropertyChanged("SelectAppDataPathCommand");
            }
        }

        public MainWindowViewModel()
        {
            if (IsInDesignMode)
            {
                AllModList = new ObservableCollection<Mod>();
                var mod1 = new Mod()
                {
                    Name = "Какой-то мод",
                    Version = "1.0.0",
                    AddedToGame = true
                };
                var mod2 = new Mod()
                {
                    Name = "Еще какой-то мод",
                    Version = "1.0.0"
                };
                var mod3 = new Mod()
                {
                    Name = "И еще один",
                    AddedToGame = true
                };
                var mod4 = new Mod()
                {
                    Name = "Какой-то мод",
                    Version = "1.0.0",
                    AddedToGame = true
                };
                var mod5 = new Mod()
                {
                    Name = "Еще какой-то мод",
                    Version = "1.0.0"
                };
                var mod6 = new Mod()
                {
                    Name = "И еще один",
                    AddedToGame = true
                };
                AllModList.Add(mod1);
                AllModList.Add(mod2);
                AllModList.Add(mod3);
                AllModList.Add(mod4);
                AllModList.Add(mod5);
                AllModList.Add(mod6);
                AllModList.Add(mod2);
                AllModList.Add(mod3);
                AllModList.Add(mod4);
                AllModList.Add(mod5);
                AllModList.Add(mod6);
                AllModList.Add(mod4);
                AllModList.Add(mod5);
                AllModList.Add(mod6);
                _selectedLanguage = new CultureInfo("ru");
                SettingsOpened = false;
            }
            else
            {
                AllModList = GetApp().AllModList;
                _selectedLanguage = GetApp().GetLanguage();
            }

            AllModsView = new ListCollectionView(AllModList);
            Languages = new List<CultureInfo> {new CultureInfo("ru"), new CultureInfo("en")};
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SpintiresModsLoader");
            _extendedAddModDialogs = (key?.GetValue("extendedMode", "False").ToString() == "True");

            ToggleModCommand = new RelayCommand(obj =>
            {
                if (AllModsSelectedItem != null)
                {
                    var mod = AllModsSelectedItem;
                    mod.AddedToGame = !mod.AddedToGame;
                }
                AllModsView.Refresh();
            });
            UpActiveModCommand = new RelayCommand(obj =>
            {
                if (AllModsSelectedItem != null)
                {
                    var index = AllModList.IndexOf(AllModsSelectedItem);
                    if (index > 0)
                        AllModList.Move(index, index - 1);
                }
            });
            DownActiveModCommand = new RelayCommand(obj =>
            {
                if (AllModsSelectedItem != null)
                {
                    var index = AllModList.IndexOf(AllModsSelectedItem);
                    if (index < AllModList.Count - 1)
                        AllModList.Move(index, index + 1);
                }
            });
            AddNewModCommand = new RelayCommand(obj => { OpenAddNewModWindow(); });
            DeleteModCommand = new RelayCommand(obj =>
            {
                if (AllModsSelectedItem == null) return;
                GetApp().DeleteMod(AllModsSelectedItem);
            });
            ToggleSettingsCommand = new RelayCommand(obj =>
            {
                SettingsOpened = !SettingsOpened;
            });

            DonatePaypalCommand = new RelayCommand(obj =>
            {
                Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=QFPDXQMZGMHKA");
            });

            DonateYandexMoneyCommand = new RelayCommand(obj =>
            {
                Process.Start("http://yasobe.ru/na/likemute");
            });

            RunGameCommand = new RelayCommand(obj =>
            {
                Process.Start("steam://rungameid/263280");
            });

            SelectSpintiresConfigXmlPathCommand = new RelayCommand(obj =>
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    fbd.SelectedPath = GetApp().SpintiresConfigXmlPath;
                    var result = fbd.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        GetApp().SpintiresConfigXmlPath = fbd.SelectedPath;
                    }
                }
            });

            SelectAppDataPathCommand = new RelayCommand(obj =>
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    fbd.SelectedPath = GetApp().ProgramDataPath;
                    var result = fbd.ShowDialog();
                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        var regKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SpintiresModsLoader");
                        if (regKey == null) return;
                        regKey.SetValue("appDataPath", fbd.SelectedPath);
                        regKey.Close();
                        MessageBox.Show(Sys.FindResource("PleaseRestartTheApplicationForTheChangesToTakeEffect") as string);
                    }
                }
            });
        }

        public List<CultureInfo> Languages
        {
            get => _languages;
            set
            {
                _languages = value;
                NotifyPropertyChanged("Languages");
            }
        }

        public CultureInfo SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                _selectedLanguage = value;
                GetApp().SetLanguage(value);
                NotifyPropertyChanged("SelectedLanguage");
            }
        }

        public App App => GetApp();

        /// <summary>
        ///     Gets or sets the error text.
        /// </summary>
        /// <value>
        ///     The error text.
        /// </value>
        public ObservableCollection<Mod> AllModList
        {
            get => _allModList;

            set
            {
                _allModList = value;
                NotifyPropertyChanged("AllModList");
            }
        }

        public ListCollectionView AllModsView
        {
            get => _allModsView;

            set
            {
                _allModsView = value;
                NotifyPropertyChanged("AllModsView");
            }
        }

        public Mod AllModsSelectedItem
        {
            get => _allModsSelectedItem;

            set
            {
                _allModsSelectedItem = value;
                NotifyPropertyChanged("AllModsSelectedItem");
            }
        }

        public bool ExtendedAddModDialogs
        {
            get => _extendedAddModDialogs;
            set
            {
                _extendedAddModDialogs = value;
                NotifyPropertyChanged("ExtendedAddModDialogs");
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SpintiresModsLoader");
                if (key == null) return;
                key.SetValue("extendedMode", _extendedAddModDialogs);
                key.Close();
            }
        }

        public bool SettingsOpened
        {
            get => _settingsOpened;
            set
            {
                _settingsOpened = value;
                NotifyPropertyChanged("SettingsOpened");
            }
        }

        public void OpenAddNewModWindow()
        {
            var openFileDialog = new OpenFileDialog {Multiselect = true};
            if (openFileDialog.ShowDialog() == true)
                if (openFileDialog.FileNames.Length>0)
                {
                    _addNewWin = new AddModWindow();
                    ((AddModWindowViewModel) _addNewWin.DataContext).SourceModsFilePaths = openFileDialog.FileNames;
                    _addNewWin.ShowDialog();
                }
        }

        public void CloseAddNewModWindow()
        {
            _addNewWin.Close();
        }
    }
}