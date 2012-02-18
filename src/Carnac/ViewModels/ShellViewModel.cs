using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using System.ComponentModel.Composition;
using Carnac.Logic;
using Carnac.Logic.KeyMonitor;
using Carnac.Logic.Native;
using Message = Carnac.Models.Message;
using Timer = System.Timers.Timer;

namespace Carnac.ViewModels
{
    [Export(typeof(IShell))]
    public class ShellViewModel :Screen, IShell, IObserver<KeyPress>
    {
        [DllImport("user32.dll")]
        static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [DllImport("User32.dll")]
        static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

        public ObservableCollection<DetailedScreen> Screens { get; set; }

        private IDisposable keySubscription;

        public ShellViewModel()
        {
            DisplayName = "Carnac";

            Keys = new ObservableCollection<Message>();
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

            var timer = new Timer(1000);
            timer.Elapsed += (s, e) => Application.Current.Dispatcher.BeginInvoke((ThreadStart)(Cleanup), DispatcherPriority.Background, null);
            timer.Start();
        }

        private readonly TimeSpan fiveseconds = TimeSpan.FromSeconds(5);
        private readonly TimeSpan sixseconds = TimeSpan.FromSeconds(6);
        public void Cleanup()
        {
            var deleting = Keys.Where(k => DateTime.Now.Subtract(k.LastMessage) > fiveseconds && k.IsDeleting == false).ToList();
            foreach (var y in deleting)
                y.IsDeleting = true;

            var deleted = Keys.Where(k => DateTime.Now.Subtract(k.LastMessage) > sixseconds && k.IsDeleting == true).ToList();
            foreach (var y in deleted)
                Keys.Remove(y);
        }

        public ObservableCollection<Message> Keys { get; private set; }
        public Message CurrentMessage { get; private set; }

        protected override void OnActivate()
        {
            keySubscription = new KeyProvider(InterceptKeys.Current).Subscribe(this);
        }

        protected override void OnDeactivate(bool close)
        {
            keySubscription.Dispose();
        }

        public void OnNext(KeyPress value)
        {
            if (value.InterceptKeyEventArgs.KeyDirection != KeyDirection.Up) return;
            if (Keys.Count > 10)
                Keys.RemoveAt(0);

            Message m;

            if (CurrentMessage == null || CurrentMessage.ProcessName != value.Process.ProcessName || CurrentMessage.LastMessage < DateTime.Now.AddSeconds(-1))
            {
                m = new Message
                        {
                            StartingTime = DateTime.Now,
                            ProcessName = value.Process.ProcessName
                        };
                
                CurrentMessage = m;
                Keys.Add(m);
            }
            else 
                m = CurrentMessage;

            if (value.InterceptKeyEventArgs.AltPressed && value.InterceptKeyEventArgs.ControlPressed)
            {
                m.Text.Add("Ctrl");
                m.Text.Add("Alt");
            }
            else if (value.InterceptKeyEventArgs.AltPressed)
            {
                m.Text.Add("Alt");
            }
            else if (value.InterceptKeyEventArgs.ControlPressed)
            {
                m.Text.Add("Ctrl");
            }
            else
                m.Text.Add(value.InterceptKeyEventArgs.Key.Sanitise());

            m.LastMessage = DateTime.Now;
            m.Count++;
            Console.WriteLine("\n" + m.Count + " - " + m.Text);
        }

        public void OnError(Exception error){}
        public void OnCompleted(){}
    }

}
