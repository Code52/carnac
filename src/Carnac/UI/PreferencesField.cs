using System.Windows;
using System.Windows.Controls;

namespace Carnac.UI
{
    public class PreferencesField : ContentControl
    {
        static PreferencesField()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PreferencesField), new FrameworkPropertyMetadata(typeof(PreferencesField)));
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header", typeof (string), typeof (PreferencesField), new PropertyMetadata(default(string)));

        public string Header
        {
            get { return (string) GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty SecondaryControlProperty = DependencyProperty.Register(
            "SecondaryControl", typeof (object), typeof (PreferencesField), new PropertyMetadata(default(object)));

        public object SecondaryControl
        {
            get { return GetValue(SecondaryControlProperty); }
            set { SetValue(SecondaryControlProperty, value); }
        }
    }
}
