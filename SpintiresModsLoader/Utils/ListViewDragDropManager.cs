/**
 *  Copyright 2017 by Alexey Andreev <trashtalk.mute@gmail.com>
 *
 * ListViewDragDropManager.cs is part of SpintiresModsLoader.
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
 * create date: 2017-6-21, 1:43
 */
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace SpintiresModsLoader.Utils
{
    #region ListViewDragDropManager

    /// <summary>
    ///     Manages the dragging and dropping of ListViewItems in a ListView.
    ///     The ItemType type parameter indicates the type of the objects in
    ///     the ListView's items source.  The ListView's ItemsSource must be
    ///     set to an instance of ObservableCollection of ItemType, or an
    ///     Exception will be thrown.
    /// </summary>
    /// <typeparam name="TItemType">The type of the ListBox's items.</typeparam>
    public class ListViewDragDropManager<TItemType> where TItemType : class
    {
        private bool _canInitiateDrag;
        private DragAdorner _dragAdorner;
        private double _dragAdornerOpacity;
        private int _indexToSelect;
        private TItemType _itemUnderDragCursor;
        private ListView _listView;
        private Point _ptMouseDown;
        private bool _showDragAdorner;

        /// <summary>
        ///     Initializes a new instance of ListViewDragManager.
        /// </summary>
        public ListViewDragDropManager()
        {
            _canInitiateDrag = false;
            _dragAdornerOpacity = 0.7;
            _indexToSelect = -1;
            _showDragAdorner = true;
        }

        /// <summary>
        ///     Initializes a new instance of ListViewDragManager.
        /// </summary>
        /// <param name="listView"></param>
        public ListViewDragDropManager(ListView listView)
            : this()
        {
            ListView = listView;
        }

        /// <summary>
        ///     Initializes a new instance of ListViewDragManager.
        /// </summary>
        /// <param name="listView"></param>
        /// <param name="dragAdornerOpacity"></param>
        public ListViewDragDropManager(ListView listView, double dragAdornerOpacity)
            : this(listView)
        {
            DragAdornerOpacity = dragAdornerOpacity;
        }

        /// <summary>
        ///     Initializes a new instance of ListViewDragManager.
        /// </summary>
        /// <param name="listView"></param>
        /// <param name="showDragAdorner"></param>
        public ListViewDragDropManager(ListView listView, bool showDragAdorner)
            : this(listView)
        {
            ShowDragAdorner = showDragAdorner;
        }

        /// <summary>
        ///     Raised when a drop occurs.  By default the dropped item will be moved
        ///     to the target index.  Handle this event if relocating the dropped item
        ///     requires custom behavior.  Note, if this event is handled the default
        ///     item dropping logic will not occur.
        /// </summary>
        public event EventHandler<ProcessDropEventArgs<TItemType>> ProcessDrop;

        /// <summary>
        ///     Gets/sets the opacity of the drag adorner.  This property has no
        ///     effect if ShowDragAdorner is false. The default value is 0.7
        /// </summary>
        public double DragAdornerOpacity
        {
            get => _dragAdornerOpacity;
            set
            {
                if (IsDragInProgress)
                    throw new InvalidOperationException(
                        "Cannot set the DragAdornerOpacity property during a drag operation.");

                if (value < 0.0 || value > 1.0)
                    throw new ArgumentOutOfRangeException("DragAdornerOpacity", value, "Must be between 0 and 1.");

                _dragAdornerOpacity = value;
            }
        }

        /// <summary>
        ///     Returns true if there is currently a drag operation being managed.
        /// </summary>
        public bool IsDragInProgress { get; private set; }

        /// <summary>
        ///     Gets/sets the ListView whose dragging is managed.  This property
        ///     can be set to null, to prevent drag management from occuring.  If
        ///     the ListView's AllowDrop property is false, it will be set to true.
        /// </summary>
        public ListView ListView
        {
            get { return _listView; }
            set
            {
                if (IsDragInProgress)
                    throw new InvalidOperationException("Cannot set the ListView property during a drag operation.");

                if (_listView != null)
                {
                    #region Unhook Events

                    _listView.PreviewMouseLeftButtonDown -= listView_PreviewMouseLeftButtonDown;
                    _listView.PreviewMouseMove -= listView_PreviewMouseMove;
                    _listView.DragOver -= listView_DragOver;
                    _listView.DragLeave -= listView_DragLeave;
                    _listView.DragEnter -= listView_DragEnter;
                    _listView.Drop -= listView_Drop;

                    #endregion // Unhook Events
                }

                _listView = value;

                if (_listView != null)
                {
                    if (!_listView.AllowDrop)
                        _listView.AllowDrop = true;

                    #region Hook Events

                    _listView.PreviewMouseLeftButtonDown += listView_PreviewMouseLeftButtonDown;
                    _listView.PreviewMouseMove += listView_PreviewMouseMove;
                    _listView.DragOver += listView_DragOver;
                    _listView.DragLeave += listView_DragLeave;
                    _listView.DragEnter += listView_DragEnter;
                    _listView.Drop += listView_Drop;

                    #endregion // Hook Events
                }
            }
        }

        /// <summary>
        ///     Gets/sets whether a visual representation of the ListViewItem being dragged
        ///     follows the mouse cursor during a drag operation.  The default value is true.
        /// </summary>
        public bool ShowDragAdorner
        {
            get => _showDragAdorner;
            set
            {
                if (IsDragInProgress)
                    throw new InvalidOperationException(
                        "Cannot set the ShowDragAdorner property during a drag operation.");

                _showDragAdorner = value;
            }
        }

        private bool CanStartDragOperation
        {
            get
            {
                if (Mouse.LeftButton != MouseButtonState.Pressed)
                    return false;

                if (!_canInitiateDrag)
                    return false;

                if (_indexToSelect == -1)
                    return false;

                if (!HasCursorLeftDragThreshold)
                    return false;

                return true;
            }
        }

        private bool HasCursorLeftDragThreshold
        {
            get
            {
                if (_indexToSelect < 0)
                    return false;

                var item = GetListViewItem(_indexToSelect);
                var bounds = VisualTreeHelper.GetDescendantBounds(item);
                var ptInItem = _listView.TranslatePoint(_ptMouseDown, item);

                // In case the cursor is at the very top or bottom of the ListViewItem
                // we want to make the vertical threshold very small so that dragging
                // over an adjacent item does not select it.
                var topOffset = Math.Abs(ptInItem.Y);
                var btmOffset = Math.Abs(bounds.Height - ptInItem.Y);
                var vertOffset = Math.Min(topOffset, btmOffset);

                var width = SystemParameters.MinimumHorizontalDragDistance * 2;
                var height = Math.Min(SystemParameters.MinimumVerticalDragDistance, vertOffset) * 2;
                var szThreshold = new Size(width, height);

                var rect = new Rect(_ptMouseDown, szThreshold);
                rect.Offset(szThreshold.Width / -2, szThreshold.Height / -2);
                var ptInListView = MouseUtilities.GetMousePosition(_listView);
                return !rect.Contains(ptInListView);
            }
        }

        /// <summary>
        ///     Returns the index of the ListViewItem underneath the
        ///     drag cursor, or -1 if the cursor is not over an item.
        /// </summary>
        private int IndexUnderDragCursor
        {
            get
            {
                var index = -1;
                for (var i = 0; i < _listView.Items.Count; ++i)
                {
                    var item = GetListViewItem(i);
                    if (IsMouseOver(item))
                    {
                        index = i;
                        break;
                    }
                }
                return index;
            }
        }

        /// <summary>
        ///     Returns true if the mouse cursor is over a scrollbar in the ListView.
        /// </summary>
        private bool IsMouseOverScrollbar
        {
            get
            {
                var ptMouse = MouseUtilities.GetMousePosition(_listView);
                var res = VisualTreeHelper.HitTest(_listView, ptMouse);
                if (res == null)
                    return false;

                var depObj = res.VisualHit;
                while (depObj != null)
                {
                    if (depObj is ScrollBar)
                        return true;

                    // VisualTreeHelper works with objects of type Visual or Visual3D.
                    // If the current object is not derived from Visual or Visual3D,
                    // then use the LogicalTreeHelper to find the parent element.
                    if (depObj is Visual || depObj is Visual3D)
                        depObj = VisualTreeHelper.GetParent(depObj);
                    else
                        depObj = LogicalTreeHelper.GetParent(depObj);
                }

                return false;
            }
        }

        private TItemType ItemUnderDragCursor
        {
            get => _itemUnderDragCursor;
            set
            {
                if (_itemUnderDragCursor == value)
                    return;

                // The first pass handles the previous item under the cursor.
                // The second pass handles the new one.
                for (var i = 0; i < 2; ++i)
                {
                    if (i == 1)
                        _itemUnderDragCursor = value;

                    if (_itemUnderDragCursor != null)
                    {
                        var listViewItem = GetListViewItem(_itemUnderDragCursor);
                        if (listViewItem != null)
                            ListViewItemDragState.SetIsUnderDragCursor(listViewItem, i == 1);
                    }
                }
            }
        }

        private bool ShowDragAdornerResolved => ShowDragAdorner && DragAdornerOpacity > 0.0;

        private void FinishDragOperation(ListViewItem draggedItem, AdornerLayer adornerLayer)
        {
            // Let the ListViewItem know that it is not being dragged anymore.
            ListViewItemDragState.SetIsBeingDragged(draggedItem, false);

            IsDragInProgress = false;

            if (ItemUnderDragCursor != null)
                ItemUnderDragCursor = null;

            // Remove the drag adorner from the adorner layer.
            if (adornerLayer != null)
            {
                adornerLayer.Remove(_dragAdorner);
                _dragAdorner = null;
            }
        }

        private ListViewItem GetListViewItem(int index)
        {
            if (_listView.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return null;

            return _listView.ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem;
        }

        private ListViewItem GetListViewItem(TItemType dataItem)
        {
            if (_listView.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return null;

            return _listView.ItemContainerGenerator.ContainerFromItem(dataItem) as ListViewItem;
        }

        private AdornerLayer InitializeAdornerLayer(ListViewItem itemToDrag)
        {
            // Create a brush which will paint the ListViewItem onto
            // a visual in the adorner layer.
            var brush = new VisualBrush(itemToDrag);

            // Create an element which displays the source item while it is dragged.
            _dragAdorner = new DragAdorner(_listView, itemToDrag.RenderSize, brush);

            // Set the drag adorner's opacity.		
            _dragAdorner.Opacity = DragAdornerOpacity;

            var layer = AdornerLayer.GetAdornerLayer(_listView);
            layer.Add(_dragAdorner);

            // Save the location of the cursor when the left mouse button was pressed.
            _ptMouseDown = MouseUtilities.GetMousePosition(_listView);

            return layer;
        }

        private void InitializeDragOperation(ListViewItem itemToDrag)
        {
            // Set some flags used during the drag operation.
            IsDragInProgress = true;
            _canInitiateDrag = false;

            // Let the ListViewItem know that it is being dragged.
            ListViewItemDragState.SetIsBeingDragged(itemToDrag, true);
        }

        private bool IsMouseOver(Visual target)
        {
            // We need to use MouseUtilities to figure out the cursor
            // coordinates because, during a drag-drop operation, the WPF
            // mechanisms for getting the coordinates behave strangely.

            var bounds = VisualTreeHelper.GetDescendantBounds(target);
            var mousePos = MouseUtilities.GetMousePosition(target);
            return bounds.Contains(mousePos);
        }

        private void listView_DragEnter(object sender, DragEventArgs e)
        {
            if (_dragAdorner != null && _dragAdorner.Visibility != Visibility.Visible)
            {
                // Update the location of the adorner and then show it.				
                UpdateDragAdornerLocation();
                _dragAdorner.Visibility = Visibility.Visible;
            }
        }

        private void listView_DragLeave(object sender, DragEventArgs e)
        {
            if (!IsMouseOver(_listView))
            {
                if (ItemUnderDragCursor != null)
                    ItemUnderDragCursor = null;

                if (_dragAdorner != null)
                    _dragAdorner.Visibility = Visibility.Collapsed;
            }
        }

        private void listView_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;

            if (ShowDragAdornerResolved)
                UpdateDragAdornerLocation();

            // Update the item which is known to be currently under the drag cursor.
            var index = IndexUnderDragCursor;
            ItemUnderDragCursor = index < 0 ? null : ListView.Items[index] as TItemType;
        }

        private void listView_Drop(object sender, DragEventArgs e)
        {
            if (ItemUnderDragCursor != null)
                ItemUnderDragCursor = null;

            e.Effects = DragDropEffects.None;

            if (!e.Data.GetDataPresent(typeof(TItemType)))
                return;

            // Get the data object which was dropped.
            var data = e.Data.GetData(typeof(TItemType)) as TItemType;
            if (data == null)
                return;

            // Get the ObservableCollection<ItemType> which contains the dropped data object.
            var itemsSource = _listView.ItemsSource as ObservableCollection<TItemType>;
            if (itemsSource == null)
                throw new Exception(
                    "A ListView managed by ListViewDragManager must have its ItemsSource set to an ObservableCollection<ItemType>.");

            var oldIndex = itemsSource.IndexOf(data);
            var newIndex = IndexUnderDragCursor;

            if (newIndex < 0)
                if (itemsSource.Count == 0)
                    newIndex = 0;

                // The drag started somewhere else, but our ListView has items
                // so make the new item the last in the list.
                else if (oldIndex < 0)
                    newIndex = itemsSource.Count;

                // The user is trying to drop an item from our ListView into
                // our ListView, but the mouse is not over an item, so don't
                // let them drop it.
                else
                    return;

            // Dropping an item back onto itself is not considered an actual 'drop'.
            if (oldIndex == newIndex)
                return;

            if (ProcessDrop != null)
            {
                // Let the client code process the drop.
                var args = new ProcessDropEventArgs<TItemType>(itemsSource, data, oldIndex, newIndex, e.AllowedEffects);
                ProcessDrop(this, args);
                e.Effects = args.Effects;
            }
            else
            {
                // Move the dragged data object from it's original index to the
                // new index (according to where the mouse cursor is).  If it was
                // not previously in the ListBox, then insert the item.
                if (oldIndex > -1)
                    itemsSource.Move(oldIndex, newIndex);
                else
                    itemsSource.Insert(newIndex, data);

                // Set the Effects property so that the call to DoDragDrop will return 'Move'.
                e.Effects = DragDropEffects.Move;
            }
        }

        private void listView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseOverScrollbar)
            {
                // 4/13/2007 - Set the flag to false when cursor is over scrollbar.
                _canInitiateDrag = false;
                return;
            }

            var index = IndexUnderDragCursor;
            _canInitiateDrag = index > -1;

            if (_canInitiateDrag)
            {
                // Remember the location and index of the ListViewItem the user clicked on for later.
                _ptMouseDown = MouseUtilities.GetMousePosition(_listView);
                _indexToSelect = index;
            }
            else
            {
                _ptMouseDown = new Point(-10000, -10000);
                _indexToSelect = -1;
            }
        }

        private void listView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!CanStartDragOperation)
                return;

            // Select the item the user clicked on.
            if (_listView.SelectedIndex != _indexToSelect)
                _listView.SelectedIndex = _indexToSelect;

            // If the item at the selected index is null, there's nothing
            // we can do, so just return;
            if (_listView.SelectedItem == null)
                return;

            var itemToDrag = GetListViewItem(_listView.SelectedIndex);
            if (itemToDrag == null)
                return;

            var adornerLayer = ShowDragAdornerResolved ? InitializeAdornerLayer(itemToDrag) : null;

            InitializeDragOperation(itemToDrag);
            PerformDragOperation();
            FinishDragOperation(itemToDrag, adornerLayer);
        }

        private void PerformDragOperation()
        {
            var selectedItem = _listView.SelectedItem as TItemType;
            var allowedEffects = DragDropEffects.Move | DragDropEffects.Move | DragDropEffects.Link;
            if (selectedItem != null && DragDrop.DoDragDrop(_listView, selectedItem, allowedEffects) != DragDropEffects.None)
                _listView.SelectedItem = selectedItem;
        }

        private void UpdateDragAdornerLocation()
        {
            if (_dragAdorner != null)
            {
                var ptCursor = MouseUtilities.GetMousePosition(ListView);

                var left = ptCursor.X - _ptMouseDown.X;

                // 4/13/2007 - Made the top offset relative to the item being dragged.
                var itemBeingDragged = GetListViewItem(_indexToSelect);
                var itemLoc = itemBeingDragged.TranslatePoint(new Point(0, 0), ListView);
                var top = itemLoc.Y + ptCursor.Y - _ptMouseDown.Y;

                _dragAdorner.SetOffsets(left, top);
            }
        }
    }

    #endregion // ListViewDragDropManager

    #region ListViewItemDragState

    /// <summary>
    ///     Exposes attached properties used in conjunction with the ListViewDragDropManager class.
    ///     Those properties can be used to allow triggers to modify the appearance of ListViewItems
    ///     in a ListView during a drag-drop operation.
    /// </summary>
    public static class ListViewItemDragState
    {
        /// <summary>
        ///     Identifies the ListViewItemDragState's IsBeingDragged attached property.
        ///     This field is read-only.
        /// </summary>
        public static readonly DependencyProperty IsBeingDraggedProperty =
            DependencyProperty.RegisterAttached(
                "IsBeingDragged",
                typeof(bool),
                typeof(ListViewItemDragState),
                new UIPropertyMetadata(false));

        /// <summary>
        ///     Identifies the ListViewItemDragState's IsUnderDragCursor attached property.
        ///     This field is read-only.
        /// </summary>
        public static readonly DependencyProperty IsUnderDragCursorProperty =
            DependencyProperty.RegisterAttached(
                "IsUnderDragCursor",
                typeof(bool),
                typeof(ListViewItemDragState),
                new UIPropertyMetadata(false));

        /// <summary>
        ///     Returns true if the specified ListViewItem is being dragged, else false.
        /// </summary>
        /// <param name="item">The ListViewItem to check.</param>
        public static bool GetIsBeingDragged(ListViewItem item)
        {
            return (bool) item.GetValue(IsBeingDraggedProperty);
        }

        /// <summary>
        ///     Returns true if the specified ListViewItem is currently underneath the cursor
        ///     during a drag-drop operation, else false.
        /// </summary>
        /// <param name="item">The ListViewItem to check.</param>
        public static bool GetIsUnderDragCursor(ListViewItem item)
        {
            return (bool) item.GetValue(IsUnderDragCursorProperty);
        }

        /// <summary>
        ///     Sets the IsBeingDragged attached property for the specified ListViewItem.
        /// </summary>
        /// <param name="item">The ListViewItem to set the property on.</param>
        /// <param name="value">Pass true if the element is being dragged, else false.</param>
        internal static void SetIsBeingDragged(ListViewItem item, bool value)
        {
            item.SetValue(IsBeingDraggedProperty, value);
        }

        /// <summary>
        ///     Sets the IsUnderDragCursor attached property for the specified ListViewItem.
        /// </summary>
        /// <param name="item">The ListViewItem to set the property on.</param>
        /// <param name="value">Pass true if the element is underneath the drag cursor, else false.</param>
        internal static void SetIsUnderDragCursor(ListViewItem item, bool value)
        {
            item.SetValue(IsUnderDragCursorProperty, value);
        }
    }

    #endregion // ListViewItemDragState

    #region ProcessDropEventArgs

    /// <summary>
    ///     Event arguments used by the ListViewDragDropManager.ProcessDrop event.
    /// </summary>
    /// <typeparam name="TItemType">The type of data object being dropped.</typeparam>
    public class ProcessDropEventArgs<TItemType> : EventArgs where TItemType : class
    {
        internal ProcessDropEventArgs(
            ObservableCollection<TItemType> itemsSource,
            TItemType dataItem,
            int oldIndex,
            int newIndex,
            DragDropEffects allowedEffects)
        {
            ItemsSource = itemsSource;
            DataItem = dataItem;
            OldIndex = oldIndex;
            NewIndex = newIndex;
            AllowedEffects = allowedEffects;
        }

        /// <summary>
        ///     The drag drop effects allowed to be performed.
        /// </summary>
        public DragDropEffects AllowedEffects { get; } = DragDropEffects.None;

        /// <summary>
        ///     The data object which was dropped.
        /// </summary>
        public TItemType DataItem { get; }

        /// <summary>
        ///     The drag drop effect(s) performed on the dropped item.
        /// </summary>
        public DragDropEffects Effects { get; set; } = DragDropEffects.None;

        /// <summary>
        ///     The items source of the ListView where the drop occurred.
        /// </summary>
        public ObservableCollection<TItemType> ItemsSource { get; }

        /// <summary>
        ///     The target index of the data item being dropped, in the ItemsSource collection.
        /// </summary>
        public int NewIndex { get; }

        /// <summary>
        ///     The current index of the data item being dropped, in the ItemsSource collection.
        /// </summary>
        public int OldIndex { get; }
    }

    #endregion // ProcessDropEventArgs
}