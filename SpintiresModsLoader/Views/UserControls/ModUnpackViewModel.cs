/**
 *  Copyright 2017 by Alexey Andreev <trashtalk.mute@gmail.com>
 *
 * ModUnpackViewModel.cs is part of SpintiresModsLoader.
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
using System.Windows.Input;
using SpintiresModsLoader.Views.Base;

namespace SpintiresModsLoader.Views.UserControls
{
    public class ModUnpackViewModel : BaseViewModel
    {
        private ICommand _cancelCommand;

        private string _title;
        private int _value;

        public ModUnpackViewModel()
        {
            CancelCommand = new RelayCommand(obj => { });
            if (IsInDesignMode)
            {
                Title = "archive.zip";
                Value = 40;
            }
        }

        public int Value
        {
            get => _value;
            set
            {
                _value = value;
                NotifyPropertyChanged("Value");
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                NotifyPropertyChanged("Title");
            }
        }

        public ICommand CancelCommand
        {
            get => _cancelCommand;
            set
            {
                _cancelCommand = value;
                NotifyPropertyChanged("CancelCommand");
            }
        }
    }
}