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
                ShutdownCarnac();
            };
            trayIcon.MouseClick += NotifyIconClick;
            trayIcon.Visible = true;
        }

        public event Action OpenPreferences = () => { }; 
        public event Action ShutdownCarnac = () => { }; 

        void NotifyIconClick(object sender, MouseEventArgs mouseEventArgs)
        {
            if (mouseEventArgs.Button == MouseButtons.Left)
                OpenPreferences();
        }

        public void Dispose()
        {
            trayIcon.Dispose();
        }
    }
}