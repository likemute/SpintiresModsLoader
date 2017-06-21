/**
 *  Copyright 2017 by Alexey Andreev <trashtalk.mute@gmail.com>
 *
 * MainWindow.xaml.cs is part of SpintiresModsLoader.
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
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SpintiresModsLoader.Models;
using SpintiresModsLoader.Resources.Utils;

namespace SpintiresModsLoader.Views
{
    /// <summary>
    ///     Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private ListViewDragDropManager<Mod> _dragManager;

        /// <summary>
        /// Minimizes the button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Handles the Click event of the CloseButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // This is all that you need to do, in order to use the ListViewDragManager.
            this._dragManager = new ListViewDragDropManager<Mod>(this.ModList);
            this._dragManager.ShowDragAdorner = true;
            this._dragManager.DragAdornerOpacity = 0.35;

            // Hook up events on both ListViews to that we can drag-drop
            // items between them.
            this.ModList.DragEnter += OnListViewDragEnter;
            this.ModList.Drop += OnListViewDrop;
        }

        /// <summary>
        /// Resizes the command can execute.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        private void ResizeCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        /// <summary>
        /// Resizes the command execute.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void ResizeCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.BorderThickness = new Thickness(4);
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.BorderThickness = new Thickness(10);
                this.WindowState = WindowState.Normal;
            }
        }

        /// <summary>
        /// Handles the MouseDown event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void WindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        #region OnListViewDragEnter

        // Handles the DragEnter event for both ListViews.
        void OnListViewDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }

        #endregion // OnListViewDragEnter

        #region OnListViewDrop

        // Handles the Drop event for both ListViews.
        void OnListViewDrop(object sender, DragEventArgs e)
        {
            if (e.Effects == DragDropEffects.None)
                return;
        }

        #endregion // OnListViewDrop
    }
}