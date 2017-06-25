/**
 *  Copyright 2017 by Alexey Andreev <trashtalk.mute@gmail.com>
 *
 * ScrollIntoViewForListBox.cs is part of SpintiresModsLoader.
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
 * create date: 2017-6-21, 2:29
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using SpintiresModsLoader.Models;

namespace SpintiresModsLoader.Utils
{
    public class ScrollIntoViewForListBox : Behavior<ListBox>
    {
        private int _selectedIndex2 = -1;

        /// <summary>
        ///  When Beahvior is attached
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            ListBox listBox = AssociatedObject;
            listBox.SelectionChanged += AssociatedObject_SelectionChanged;
            ((INotifyCollectionChanged)listBox.Items).CollectionChanged += ScrollIntoViewForListBox_CollectionChanged;
        }

        private void ScrollIntoViewForListBox_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ListBox listBox = AssociatedObject;
            if (listBox?.SelectedItem != null)
            {
                listBox.Dispatcher.BeginInvoke(
                    (Action)(() =>
                    {
                        listBox.UpdateLayout();
                        if (listBox.SelectedItem !=
                            null)
                            listBox.ScrollIntoView(
                                listBox.SelectedItem);
                    }));
            }
        }

        /// <summary>
        /// On Selection Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AssociatedObject_SelectionChanged(object sender,
            SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox?.SelectedIndex > -1)
            {
                _selectedIndex2 = listBox.SelectedIndex;
            }
            if (listBox?.SelectedItem == null && listBox.SelectedIndex == -1 && _selectedIndex2>-1)
            {
                if (listBox.Items.Count > 0)
                {
                    int newIndex;
                    if ((_selectedIndex2) >= listBox.Items.Count)
                    {
                        newIndex = _selectedIndex2 - 1;
                    }
                    else
                    {
                        newIndex = _selectedIndex2;
                    }
                    listBox.SelectedIndex = newIndex;
                }
            }

            if (listBox?.SelectedItem != null)
            {
                listBox.Dispatcher.BeginInvoke(
                    (Action)(() =>
                    {
                        listBox.UpdateLayout();
                        if (listBox.SelectedItem !=
                            null)
                            listBox.ScrollIntoView(
                                listBox.SelectedItem);
                    }));
            }
        }
        /// <summary>
        /// When behavior is detached
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            ListBox listBox = AssociatedObject;
            listBox.SelectionChanged -=
                AssociatedObject_SelectionChanged;
            ((INotifyCollectionChanged) listBox.Items).CollectionChanged -= ScrollIntoViewForListBox_CollectionChanged;
        }
    }
}
