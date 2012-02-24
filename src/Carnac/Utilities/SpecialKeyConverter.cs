using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Carnac.Utilities
{
    [ValueConversion(typeof(string), typeof(string))]
    public class SpecialKeyConverter : IValueConverter
    {
        static readonly HashSet<string> specialKeys = new HashSet<string>(StringComparer.Ordinal)
        {
            "Space", "Cancel", "Escape", "Back", "Tab", "Clear", "Enter", "Return", "Shift", "Ctrl", "Alt",
            "Menu", "Pause", "Print Screen", "Next", "Previous", "Page Up", "Page Down", "End", "Home", 
            "Select", "Print", "Execute", "Snapshot", "Insert", "Delete", "Help", "Num Lock",
            "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9",
            "F10", "F11", "F12", "F13", "F14", "F15", "F16", "F17", "F18", "F19", 
            "F20", "F21", "F22", "F23", "F24"
        };

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(string) && targetType != typeof(object))
                return null;
            var str = value as string;
            if (string.IsNullOrEmpty(str))
                return str;
            if (str == "Win")
                return str;
            if (specialKeys.Contains(str))
                return "Special";
            else
                return "Normal";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
