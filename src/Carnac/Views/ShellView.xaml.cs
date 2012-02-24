using System;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using Carnac.Logic.Native;
using Carnac.Utilities;
using Carnac.ViewModels;
using Application = System.Windows.Application;

namespace Carnac.Views
{
    public partial class ShellView
    {
        public ShellView()
        {
            InitializeComponent();

            // Check if there was instance before this. If there was-close the current one.  
            if (ProcessUtilities.ThisProcessIsAlreadyRunning())
            {
                ProcessUtilities.SetFocusToPreviousInstance("Carnac");
                Application.Current.Shutdown();
            }

            var item = new MenuItem
            {
                Text = "Exit"
            };

            item.Click += (sender, args) => Close();

            var iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Carnac.icon.embedded.ico");

            var ni = new NotifyIcon
                         {
                             Icon = new Icon(iconStream),
                             ContextMenu = new ContextMenu(new[] { item })
                         };

            ni.Click += NotifyIconClick;
            ni.Visible = true;
        }

        private void NotifyIconClick(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            this.Topmost = true;  // When it comes back, make sure it's on top...
            this.Topmost = false; // and then it doesn't need to be anymore.
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            System.Windows.Application.Current.Shutdown();
        }

        private void RadioChecked(object sender, RoutedEventArgs e)
        {
            var dc = DataContext as ShellViewModel;
            if (dc == null) return;

            var rb = sender as System.Windows.Controls.RadioButton;
            if (rb == null) return;

            var tag = rb.Tag as DetailedScreen;
            if (tag == null) return;

            dc.SelectedScreen = tag;
        }
    }

    public class PlacementMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if ((bool)values[2] == false || values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
                return new Thickness(0);

            var or = (Thickness)values[0];
            var sc = (DetailedScreen)values[1];

            Thickness th = new Thickness();

            th.Top = or.Top * (sc.RelativeHeight / sc.Height);
            th.Bottom = or.Bottom * (sc.RelativeHeight / sc.Height);
            th.Left = or.Left * (sc.RelativeWidth / sc.Width);
            th.Right = or.Right * (sc.RelativeWidth / sc.Width);

            return th;

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}