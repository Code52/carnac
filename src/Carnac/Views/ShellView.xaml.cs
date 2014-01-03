using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
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

            var exitMenuItem = new MenuItem
            {
                Text = Properties.Resources.ShellView_Exit
            };

            var iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Carnac.icon.embedded.ico");

            var ni = new NotifyIcon
                         {
                             Icon = new Icon(iconStream),
                             ContextMenu = new ContextMenu(new[] { exitMenuItem })
                         };

            exitMenuItem.Click += (sender, args) =>
            {
                ni.Visible = false;
                Application.Current.Shutdown();
            };
            ni.MouseClick += NotifyIconClick;
            ni.Visible = true;
        }

        void NotifyIconClick(object sender, MouseEventArgs mouseEventArgs)
        {
            if (mouseEventArgs.Button == MouseButtons.Right) return;

            Show();
            WindowState = WindowState.Normal;
            Topmost = true;  // When it comes back, make sure it's on top...
            Topmost = false; // and then it doesn't need to be anymore.
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
            base.OnClosing(e);
        }

        private void RadioChecked(object sender, RoutedEventArgs e)
        {
            var dc = DataContext as ShellViewModel;
            if (dc == null)
                return;

            var rb = sender as System.Windows.Controls.RadioButton;
            if (rb == null) 
                return;

            var tag = rb.Tag as DetailedScreen;
            if (tag == null) 
                return;

            dc.SelectedScreen = tag;
        }
    }
}