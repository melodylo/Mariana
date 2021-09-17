// File Name: Converters.cs
// Author: Melody Lo
// Version number: 1.0
// Date published: Sept. 17, 2021
// Project name: Milano – Project Mariana
// Company/Division: KLA BBP-GPG Advanced Tech
// File Description: Converts C# properties to the necessary data type for XAML properties.

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Mariana
{
    /// <summary>
    /// Used for changing the width of an XAML control based on the visibility of adjacent XAML controls.
    /// </summary>
    public class WidthConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean into a numeric string.
        /// </summary>
        /// <param name="value"> The source data being passed to the target. </param>
        /// <param name="targetType"> The type of the target property. </param>
        /// <param name="parameter"> An optional parameter to be used in the converter login. </param>
        /// <param name="culture"> The language of the conversion. </param>
        /// <returns> A numeric string that represents the width of an XAML control </returns>
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool) value) // if adjacent elements are visible
            {
                return "80";
            }
            else
            {
                return "155";
            }
        }

        // string -> boolean
        /// <summary>
        /// Converts a numeric string into a boolean.
        /// </summary>
        /// <param name="value"> The source data being passed to the target. </param>
        /// <param name="targetType"> The type of the target property. </param>
        /// <param name="parameter"> An optional parameter to be used in the converter login. </param>
        /// <param name="culture"> The language of the conversion. </param>
        /// <returns> A boolean that represents whether adjacent XAML controls are visible or not. </returns>
        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string) value == "80")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Used for visualising error-checking in XAML text boxes.
    /// </summary>
    public class BackgroundConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean to a background color. 
        /// </summary>
        /// <param name="value"> The source data being passed to the target. </param>
        /// <param name="targetType"> The type of the target property. </param>
        /// <param name="parameter"> An optional parameter to be used in the converter login. </param>
        /// <param name="culture"> The language of the conversion. </param>
        /// <returns> A background color to visually show whether the entered text in the XAML text box is valid or not. </returns>
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool) value) // if input is valid
            {
                return Brushes.Transparent;
            }
            else
            {
                return new SolidColorBrush(Color.FromRgb(255, 204, 196));
            }
        }

        // background color -> boolean
        /// <summary>
        /// Converts a background color to a boolean.
        /// </summary>
        /// <param name="value"> The source data being passed to the target. </param>
        /// <param name="targetType"> The type of the target property. </param>
        /// <param name="parameter"> An optional parameter to be used in the converter login. </param>
        /// <param name="culture"> The language of the conversion. </param>
        /// <returns> A boolean that represents whether the entered text in the XAML text box is valid or not. </returns>
        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == Brushes.Transparent)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Used for displaying which y-axis each line is plotted on.
    /// </summary>
    public class YAxisConverter : IValueConverter
    {
        /// <summary>
        /// Reduces a string to its first character. 
        /// </summary>
        /// <param name="value"> The source data being passed to the target. </param>
        /// <param name="targetType"> The type of the target property. </param>
        /// <param name="parameter"> An optional parameter to be used in the converter login. </param>
        /// <param name="culture"> The language of the conversion. </param>
        /// <returns> An upper-case character that represents which y-axis a line is plotted on. </returns>
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString()[0].ToString().ToUpper();
        }

        /// <summary>
        /// Converts a character to a full-length string. 
        /// </summary>
        /// <param name="value"> The source data being passed to the target. </param>
        /// <param name="targetType"> The type of the target property. </param>
        /// <param name="parameter"> An optional parameter to be used in the converter login. </param>
        /// <param name="culture"> The language of the conversion. </param>
        /// <returns> A string that represents which y-axis a line is plotted on. </returns>
        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.ToString().ToUpper() == "P")
            {
                return "primary";
            }
            else if (value.ToString().ToUpper() == "S")
            {
                return "secondary";
            }
            else
            {
                return "other";
            }
        }
    }
}
