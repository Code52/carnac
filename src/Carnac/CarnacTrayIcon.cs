using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace Carnac
{
    public class CarnacTrayIcon : IDisposable
    {
        readonly NotifyIcon trayIcon;

        public CarnacTrayIcon()
        {
            var exitMenuItem = new MenuItem
            {
                Text = Properties.Resources.ShellView_Exit
            };

            var iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Carnac.icon.embedded.ico");

            trayIcon = new NotifyIcon
            {
                Icon = new Icon(iconStream),
                ContextMenu = new ContextMenu(new[] { exitMenuItem })
            };

            exitMenuItem.Click += (sender, args) =>
            {
                trayIcon.Visible = false;
                Application.Current.Shutdown();
            };
            trayIcon.Click += NotifyIconClick;
            trayIcon.Visible = true;
        }

        public event Action OpenPreferences = () => { }; 

        void NotifyIconClick(object sender, EventArgs mouseEventArgs)
        {
            OpenPreferences();
        }

        public void Dispose()
        {
            trayIcon.Dispose();
        }
    }
}