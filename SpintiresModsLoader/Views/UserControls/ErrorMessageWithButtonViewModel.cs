/**
 *  Copyright 2017 by Alexey Andreev <trashtalk.mute@gmail.com>
 *
 * ErrorMessageWithButtonViewModel.cs is part of SpintiresModsLoader.
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
 * create date: 2017-6-15, 14:40
 */
using System.Windows.Input;
using SpintiresModsLoader.Views.Base;

namespace SpintiresModsLoader.Views.UserControls
{
    public class ErrorMessageWithButtonViewModel : BaseViewModel
    {
        private ICommand _closeCommand;
        private string _message;

        public ErrorMessageWithButtonViewModel()
        {
            CloseCommand = new RelayCommand(obj => { });
            if (IsInDesignMode)
                Message = "Message";
        }

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                NotifyPropertyChanged("Message");
            }
        }

        public ICommand CloseCommand
        {
            get => _closeCommand;
            set
            {
                _closeCommand = value;
                NotifyPropertyChanged("CloseCommand");
            }
        }
    }
}