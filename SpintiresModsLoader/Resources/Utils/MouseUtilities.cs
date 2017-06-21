/**
 *  Copyright 2017 by Alexey Andreev <trashtalk.mute@gmail.com>
 *
 * MouseUtilities.cs is part of SpintiresModsLoader.
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
 * create date: 2017-6-21, 1:52
 */
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace SpintiresModsLoader.Resources.Utils
{
    /// <summary>
    ///     Provides access to the mouse location by calling unmanaged code.
    /// </summary>
    /// <remarks>
    ///     This class was written by Dan Crevier (Microsoft).
    ///     http://blogs.msdn.com/llobo/archive/2006/09/06/Scrolling-Scrollviewer-on-Mouse-Drag-at-the-boundaries.aspx
    /// </remarks>
    public class MouseUtilities
    {
        /// <summary>
        ///     Returns the mouse cursor location.  This method is necessary during
        ///     a drag-drop operation because the WPF mechanisms for retrieving the
        ///     cursor coordinates are unreliable.
        /// </summary>
        /// <param name="relativeTo">The Visual to which the mouse coordinates will be relative.</param>
        public static Point GetMousePosition(Visual relativeTo)
        {
            var mouse = new Win32Point();
            GetCursorPos(ref mouse);

            // Using PointFromScreen instead of Dan Crevier's code (commented out below)
            // is a bug fix created by William J. Roberts.  Read his comments about the fix
            // here: http://www.codeproject.com/useritems/ListViewDragDropManager.asp?msg=1911611#xx1911611xx
            return relativeTo.PointFromScreen(new Point(mouse.X, mouse.Y));

            #region Commented Out

            //System.Windows.Interop.HwndSource presentationSource =
            //    (System.Windows.Interop.HwndSource)PresentationSource.FromVisual( relativeTo );
            //ScreenToClient( presentationSource.Handle, ref mouse );
            //GeneralTransform transform = relativeTo.TransformToAncestor( presentationSource.RootVisual );
            //Point offset = transform.Transform( new Point( 0, 0 ) );
            //return new Point( mouse.X - offset.X, mouse.Y - offset.Y );

            #endregion // Commented Out
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref Win32Point pt);

        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(IntPtr hwnd, ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        private struct Win32Point
        {
            public readonly int X;
            public readonly int Y;
        }
    }
}