/**
 *  Copyright 2017 by Alexey Andreev <trashtalk.mute@gmail.com>
 *
 * Mod.cs is part of SpintiresModsLoader.
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
    public class Mod : BaseModel
    {
        private string _author;
        private string _fileHash;
        private string _filePath;
        private string _name;
        private string _version;
        private bool _addDoPrepend;
        private bool _addedToGame;
        private bool _textureCache;
        private bool _meshCache;

        public string Name
        {
            get => _name;

            set
            {
                _name = value;
                NotifyPropertyChanged("Name");
            }
        }

        public string Version
        {
            get => _version;

            set
            {
                _version = value;
                NotifyPropertyChanged("Version");
            }
        }

        public string Author
        {
            get => _author;

            set
            {
                _author = value;
                NotifyPropertyChanged("Author");
            }
        }

        public string FilePath
        {
            get => _filePath;

            set
            {
                _filePath = value;
                NotifyPropertyChanged("FilePath");
            }
        }

        public string FileHash
        {
            get => _fileHash;

            set
            {
                _fileHash = value;
                NotifyPropertyChanged("FileHash");
            }
        }

        public bool AddDoPrepend
        {
            get => _addDoPrepend;
            set
            {
                _addDoPrepend = value;
                NotifyPropertyChanged("AddDoPrepend");
            }
        }

        public bool TextureCache
        {
            get => _textureCache;
            set
            {
                _textureCache = value;
                NotifyPropertyChanged("TextureCash");
            }
        }

        public bool MeshCache
        {
            get => _meshCache;
            set
            {
                _meshCache = value;
                NotifyPropertyChanged("MeshCash");
            }
        }

        public bool AddedToGame
        {
            get => _addedToGame;
            set
            {
                _addedToGame = value;
                NotifyPropertyChanged("AddedToGame");
            }
        }
    }
}