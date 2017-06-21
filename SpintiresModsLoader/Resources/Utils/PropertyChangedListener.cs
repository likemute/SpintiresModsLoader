/**
 *  Copyright 2017 by Alexey Andreev <trashtalk.mute@gmail.com>
 *
 * PropertyChangedListener.cs is part of SpintiresModsLoader.
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
 * create date: 2017-6-20, 23:41
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace SpintiresModsLoader.Resources.Utils
{
    public class PropertyChangedListener<T> : INotifyPropertyChanged where T : INotifyPropertyChanged
    {
        private readonly ObservableCollection<T> _collection;
        private readonly string _propertyName;
        private readonly Dictionary<T, int> _items = new Dictionary<T, int>(new ObjectIdentityComparer());
        public PropertyChangedListener(ObservableCollection<T> collection, string propertyName = "")
        {
            _collection = collection;
            _propertyName = propertyName ?? "";
            AddRange(collection);
            CollectionChangedEventManager.AddHandler(collection, CollectionChanged);
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddRange(e.NewItems.Cast<T>());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveRange(e.OldItems.Cast<T>());
                    break;
                case NotifyCollectionChangedAction.Replace:
                    AddRange(e.NewItems.Cast<T>());
                    RemoveRange(e.OldItems.Cast<T>());
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Reset();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        private void AddRange(IEnumerable<T> newItems)
        {
            foreach (T item in newItems)
            {
                if (_items.ContainsKey(item))
                {
                    _items[item]++;
                }
                else
                {
                    _items.Add(item, 1);
                    PropertyChangedEventManager.AddHandler(item, ChildPropertyChanged, _propertyName);
                }
            }
        }

        private void RemoveRange(IEnumerable<T> oldItems)
        {
            foreach (T item in oldItems)
            {
                _items[item]--;
                if (_items[item] == 0)
                {
                    _items.Remove(item);
                    PropertyChangedEventManager.RemoveHandler(item, ChildPropertyChanged, _propertyName);
                }
            }
        }

        private void Reset()
        {
            foreach (T item in _items.Keys.ToList())
            {
                PropertyChangedEventManager.RemoveHandler(item, ChildPropertyChanged, _propertyName);
                _items.Remove(item);
            }
            AddRange(_collection);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void ChildPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        private class ObjectIdentityComparer : IEqualityComparer<T>
        {
            public bool Equals(T x, T y)
            {
                return ReferenceEquals(x, y);
            }
            public int GetHashCode(T obj)
            {
                return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
            }
        }
    }

    public static class PropertyChangedListener
    {
        public static PropertyChangedListener<T> Create<T>(ObservableCollection<T> collection, string propertyName = "") where T : INotifyPropertyChanged
        {
            return new PropertyChangedListener<T>(collection, propertyName);
        }
    }
}
