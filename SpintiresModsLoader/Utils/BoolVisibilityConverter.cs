/**
 *  Copyright 2017 by Alexey Andreev <trashtalk.mute@gmail.com>
 *
 * BoolVisibilityConverter.cs is part of SpintiresModsLoader.
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
 * create date: 2017-6-20, 14:59
 */
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SpintiresModsLoader.Utils
{
    [ValueConversion(typeof(object), typeof(Visibility))]
    public class BoolVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string parameterString = parameter as string;
            string[] parameters = parameterString?.Split('|');
            var invert = parameters?.Length > 0 && parameters[0] == "true"; 

            var hidden = parameters?.Length > 1 && parameters[1] == "collapse"
                ? Visibility.Collapsed
                : Visibility.Hidden;


            if (value == null)
            {
                return invert ? Visibility.Visible : hidden;
            }
            if (value is bool)
            {
                if (invert)
                {
                    return (((bool)value)) ? hidden : Visibility.Visible;
                }
                return (((bool)value)) ? Visibility.Visible : hidden;
            }
            return invert ? hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("not implemented");
        }
    }
}
