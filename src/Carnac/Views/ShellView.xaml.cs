using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace Carnac.Views
{
    public partial class ShellView
    {
        public ShellView()
        {
            InitializeComponent();

            var item = new MenuItem
            {
                Text = "Exit"
            };

            item.Click += (sender, args) => this.Close();

            var ni = new NotifyIcon
                         {
                              Icon = new Icon(@"..\..\icon.ico"),
                              ContextMenu =  new ContextMenu(new[] { item })
                          };

            ni.Click += NotifyIconClick;
            ni.Visible = true;
        }

        private void NotifyIconClick(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if(WindowState == WindowState.Minimized)
                Hide();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            System.Windows.Application.Current.Shutdown();
        }
    }
}