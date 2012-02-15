using System;
using System.Windows.Interop;
using Carnac.Logic;

namespace Carnac.Views
{
    public partial class KeyShowView
    {
        public KeyShowView()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var hwnd = new WindowInteropHelper(this).Handle;
            Win32Methods.SetWindowExTransparent(hwnd);
        }
    }
}
