using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Caliburn.Micro;
using Carnac.Logic;
using Carnac.ViewModels;
using Timer = System.Timers.Timer;

namespace Carnac.Views
{
    public partial class KeyShowView : IHandle<LeftChanged>
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

            var timer = new Timer(100);
            timer.Elapsed +=
                (s, x) =>
                {
                    SetWindowPos(hwnd,
                                 HWND.TOPMOST,
                                 0, 0, 0, 0,
                                 (uint)(SWP.NOMOVE | SWP.NOSIZE | SWP.SHOWWINDOW));
                };

            timer.Start();
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int W, int H, uint uFlags);


        /// <summary>
        /// HWND values for hWndInsertAfter
        /// </summary>
        public static class HWND
        {
            public static readonly IntPtr
            NOTOPMOST = new IntPtr(-2),
            BROADCAST = new IntPtr(0xffff),
            TOPMOST = new IntPtr(-1),
            TOP = new IntPtr(0),
            BOTTOM = new IntPtr(1);
        }


        /// <summary>
        /// SetWindowPos Flags
        /// </summary>
        public static class SWP
        {
            public static readonly int
            NOSIZE = 0x0001,
            NOMOVE = 0x0002,
            NOZORDER = 0x0004,
            NOREDRAW = 0x0008,
            NOACTIVATE = 0x0010,
            DRAWFRAME = 0x0020,
            FRAMECHANGED = 0x0020,
            SHOWWINDOW = 0x0040,
            HIDEWINDOW = 0x0080,
            NOCOPYBITS = 0x0100,
            NOOWNERZORDER = 0x0200,
            NOREPOSITION = 0x0200,
            NOSENDCHANGING = 0x0400,
            DEFERERASE = 0x2000,
            ASYNCWINDOWPOS = 0x4000;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            var vm = ((KeyShowViewModel) DataContext);
            Left = vm.Settings.Left;
            AppBootstrapper.Aggregator.Subscribe(this);
            WindowState = WindowState.Maximized;
        }

        public void Handle(LeftChanged message)
        {
            WindowState = WindowState.Normal;
            Left = message.Left;
            WindowState = WindowState.Maximized;
        }
    }

    public class LeftChanged
    {
        public double Left { get; set; }

        public LeftChanged(double left)
        {
            Left = left;
        }
    }
}
