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

            var _ni = new NotifyIcon()
                          {
                              Icon = new Icon(@"..\..\icon.ico"),

                              ContextMenu =  new ContextMenu(new[] { item })
                          };

            _ni.Click += NotifyIcon_Click;
            _ni.Visible = true;
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        private NotifyIcon _ni;

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        protected override void OnStateChanged(System.EventArgs e)
        {
            base.OnStateChanged(e);

            if(this.WindowState == WindowState.Minimized)
                Hide();
        }
    }
}