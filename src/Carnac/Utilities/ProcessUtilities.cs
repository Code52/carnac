using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace Carnac.Utilities
{
    public static class ProcessUtilities
    {
        static Mutex mutex;

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        static extern IntPtr GetLastActivePopup(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool IsWindowEnabled(IntPtr hWnd);

        /// Determine if the current process is already running
        public static bool ThisProcessIsAlreadyRunning()
        {
            bool createdNew;
            var fullName = Application.Current.GetType().Assembly.FullName;
            mutex = new Mutex(false, fullName, out createdNew);
            return !createdNew;
        }

        public static void DestroyMutex()
        {
            if (mutex == null) return;
            mutex.Dispose();
            mutex = null;
        }

        /// Set focus to the previous instance of the specified program.
        public static void SetFocusToPreviousInstance(string windowCaptionPart)
        {
            var hWnd = IntPtr.Zero;
            foreach (var process in Process.GetProcesses())
            {
                if (process.MainWindowTitle.Contains(windowCaptionPart))
                {
                    hWnd = process.MainWindowHandle;
                }
            }
            // Look for previous instance of this program.
            //IntPtr  = FindWindow(null, windowCaption);
            // If a previous instance of this program was found...
            if (hWnd != null)
            {
                // Is it displaying a popup window?
                var hPopupWnd = GetLastActivePopup(hWnd);
                // If so, set focus to the popup window. Otherwise set focus
                // to the program's main window.
                if (hPopupWnd != null && IsWindowEnabled(hPopupWnd))
                {
                    hWnd = hPopupWnd;
                }

                SetForegroundWindow(hWnd);
                // If program is minimized, restore it.
                if (IsIconic(hWnd))
                {
                    ShowWindow(hWnd, SW_RESTORE);
                }
            }
        }
    }
}
