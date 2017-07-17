using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Carnac.UI
{
    public class ToSpecialKeyConverter : IValueConverter
    {

        private readonly List<String> specialKeys = new List<String>(){
            "Escape",
            "Alt",
            "Ctrl",
            "Shift",
            "Tab",
            "Delete",
            "Insert",
            "Home",
            "Next",
            "PageUp",
            "PageDown",
            "End",
            "Back",
            "NumLock",
            "PrintScreen",
            "Scroll",
            "Capital",
            "F1",
            "F2",
            "F3",
            "F4",
            "F5",
            "F6",
            "F7",
            "F8",
            "F9",
            "F10",
            "F11",
            "F12",
            "Apps",
            "BrowserHome",
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return specialKeys.Contains(value.ToString()) ? "SPECIAL_KEY" : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
