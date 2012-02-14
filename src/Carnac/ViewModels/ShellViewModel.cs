using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Caliburn.Micro;
using Carnac.KeyMonitor;
using System.ComponentModel.Composition;

namespace Carnac.ViewModels
{
    [Export(typeof(IShell))]
    public class ShellViewModel :Screen, IShell, IObserver<InterceptKeyEventArgs>
    {
        [DllImport("user32.dll")]
        static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [DllImport("User32.dll")]
        static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);
        
        [DllImport("User32.dll")]
        static extern int GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private ObservableCollection<DetailedScreen> _screens;

        public ObservableCollection<DetailedScreen> Screens
        {
            get
            {
                return _screens;
            }
            set { _screens = value; }
        }

        private IDisposable keySubscription;

        private Dictionary<int, Process> _processes;

        public ShellViewModel()
        {
            _processes = new Dictionary<int, Process>();

            Keys = new ObservableCollection<string>();
            Screens = new ObservableCollection<DetailedScreen>();

            int index = 1;
            var d = new DISPLAY_DEVICE();
            d.cb = Marshal.SizeOf(d);
            try
            {
                for (uint id = 0; EnumDisplayDevices(null, id, ref d, 0); id++)
                {
                    d.cb = Marshal.SizeOf(d);

                    var x = new DISPLAY_DEVICE();
                    x.cb = Marshal.SizeOf(x);

                    //Get the actual monitor
                    EnumDisplayDevices(d.DeviceName, 0, ref x, 0);

                    if (string.IsNullOrEmpty(x.DeviceName) || string.IsNullOrEmpty(x.DeviceString))
                        continue;


                    var screen = new DetailedScreen { FriendlyName = x.DeviceString, Index = index++ };

                    var mode = new DEVMODE();
                    mode.dmSize = (ushort)Marshal.SizeOf(mode);
                    if (EnumDisplaySettings(d.DeviceName, -1, ref mode))
                    {
                        screen.Width = (int)mode.dmPelsWidth;
                        screen.Height = (int)mode.dmPelsHeight;
                        screen.Top = (int)mode.dmPosition.y;

                    }

                    Screens.Add(screen);
                }
            }
            catch (Exception ex)
            {
                //log this
            }

            var maxWidth = Screens.OrderByDescending(s => s.Width).FirstOrDefault().Width;
            foreach (var s in Screens)
            {
                s.RelativeWidth = 200 * (s.Width / maxWidth);
                s.RelativeHeight = s.RelativeWidth * (s.Height / s.Width);
                s.Top *= (s.RelativeHeight / s.Height);
            }

            WindowManager manager = new WindowManager();
            manager.ShowWindow(new KeyShowViewModel(Keys));
        }

        public ObservableCollection<string> Keys { get; private set; }

        protected override void OnActivate()
        {
            keySubscription = InterceptKeys.Current.Subscribe(this);
        }

        protected override void OnDeactivate(bool close)
        {
            keySubscription.Dispose();
        }

        public void OnNext(InterceptKeyEventArgs value)
        {
            Process process;

            int handle = 0;
            handle = GetForegroundWindow();

            if (!_processes.ContainsKey(handle))
            {
                uint processID = 0;
                uint threadID = GetWindowThreadProcessId(new IntPtr(handle), out processID);
                var p = Process.GetProcessById(Convert.ToInt32(processID));
                _processes.Add(handle, p);
                process = p;
            }
            else process = _processes[handle];


            if (value.KeyDirection != KeyDirection.Up) return;
            if (Keys.Count > 10)
                Keys.RemoveAt(0);

            if (value.AltPressed && value.ControlPressed)
                Keys.Add(string.Format("Ctrl + Alt + {0}", value.Key));
            else if (value.AltPressed)
                Keys.Add(string.Format("Alt + {0}", value.Key));
            else if (value.ControlPressed)
                Keys.Add(string.Format("Ctrl + {0}", value.Key));
            else
                Keys.Add(string.Format("{0} - {1}", process.ProcessName, value.Key.ToString()));
        }
        public void OnError(Exception error){}
        public void OnCompleted(){}
    }
}
