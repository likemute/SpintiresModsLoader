/**
 *  Copyright 2017 by Alexey Andreev <trashtalk.mute@gmail.com>
 *
 * Sys.xaml.cs is part of SpintiresModsLoader.
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
 * create date: 2017-6-13, 15:29
 */
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;
using System.Windows;
using SpintiresModsLoader.Views;
using CompressionMode = System.IO.Compression.CompressionMode;

namespace SpintiresModsLoader
{
    /// <summary>
    ///     Логика взаимодействия для Sys.xaml
    /// </summary>
    /// <seealso cref="System.Windows.Application" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class Sys
    {
        public App App;

        /// <summary>
        ///     Raises the <see cref="E:System.Windows.Application.Startup" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs" /> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            App = new App();
            MainWindow = new MainWindow();
            MainWindow.Show();
        }

        /// <summary>
        ///     Application Entry Point.
        /// </summary>
        [STAThread]
        [DebuggerNonUserCode]
        [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
        public static void Main()
        {
            var sysObject = new Sys();
            sysObject.InitializeComponent();
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                try
                {
                    var culture = "";
                    var embeddedAssembly = new AssemblyName(args.Name);
                    if (!string.IsNullOrEmpty(embeddedAssembly.CultureName) &&
                        embeddedAssembly.CultureName != "neutral") culture = embeddedAssembly.CultureName + ".";
                    var resourceName = "SpintiresModsLoader.Resources.dll." + culture + embeddedAssembly.Name + ".dll.gz";
                    var streamRes = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
                    if (streamRes != null)
                        using (var stream = new GZipStream(streamRes, CompressionMode.Decompress, false))
                        using (var outstream = new MemoryStream())
                        {
                            CopyTo(stream, outstream);
                            stream.Dispose();
                            streamRes.Dispose();
                            return Assembly.Load(outstream.GetBuffer());
                        }
                    return null;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    return null;
                }
            };
            sysObject.Run();
        }

        /// <summary>
        ///     Copies to.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        private static void CopyTo(Stream source, Stream destination)
        {
            var buffer = new byte[2048];
            int bytesRead;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                destination.Write(buffer, 0, bytesRead);
        }
    }
}