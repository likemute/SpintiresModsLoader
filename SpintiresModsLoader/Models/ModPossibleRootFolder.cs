/**
 *  Copyright 2017 by Alexey Andreev <trashtalk.mute@gmail.com>
 *
 * ModPossibleRootFolder.cs is part of SpintiresModsLoader.
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
 * create date: 2017-6-15, 14:26
 */
namespace SpintiresModsLoader.Models
{
    public class ModPossibleRootFolder : BaseModel
    {
        private string _folder;
        private string _fullFolder;
        private int _weight;

        public string Folder
        {
            get => _folder;
            set
            {
                _folder = value;
                NotifyPropertyChanged("Folder");
            }
        }

        public string FullFolder
        {
            get => _fullFolder;
            set
            {
                _fullFolder = value;
                NotifyPropertyChanged("FullFolder");
            }
        }

        public int Weight
        {
            get => _weight;
            set
            {
                _weight = value;
                NotifyPropertyChanged("Weight");
            }
        }
    }
}