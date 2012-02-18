using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Analects.SettingsService;
using Caliburn.Micro;
using Carnac.Logic;
using Carnac.Logic.KeyMonitor;
using Carnac.Logic.Native;
using Carnac.Models;
using Message = Carnac.Models.Message;
using Timer = System.Timers.Timer;

namespace Carnac.ViewModels
{
    [Export(typeof (IShell))]
    public class ShellViewModel : Screen, IShell, IObserver<KeyPress>
    {
        [DllImport("user32.dll")]
        private static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [DllImport("User32.dll")]
        private static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

        IDisposable keySubscription;

        readonly ISettingsService settingsService;

        readonly TimeSpan fiveseconds = TimeSpan.FromSeconds(5);
        readonly TimeSpan sixseconds = TimeSpan.FromSeconds(6);

        [ImportingConstructor]
        public ShellViewModel(ISettingsService settingsService)
        {
            DisplayName = "Carnac";

            this.settingsService = settingsService;

            Settings = settingsService.Get<Settings>("PopupSettings");

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

                    var screen = new DetailedScreen {FriendlyName = x.DeviceString, Index = index++};

                    var mode = new DEVMODE();
                    mode.dmSize = (ushort) Marshal.SizeOf(mode);
                    if (EnumDisplaySettings(d.DeviceName, -1, ref mode))
                    {
                        screen.Width = (int) mode.dmPelsWidth;
                        screen.Height = (int) mode.dmPelsHeight;
                        screen.Top = mode.dmPosition.y;
                    }

                    Screens.Add(screen);
                }
            }
            catch (Exception)
            {
                //log this
            }

            var biggestScreen = Screens.OrderByDescending(s => s.Width).FirstOrDefault();
            if (biggestScreen != null)
            {
                var maxWidth = biggestScreen.Width;
                foreach (var s in Screens)
                {
                    s.RelativeWidth = 200*(s.Width/maxWidth);
                    s.RelativeHeight = s.RelativeWidth*(s.Height/s.Width);
                    s.Top *= (s.RelativeHeight/s.Height);
                }
            }

            if (Settings == null)
            {
                Settings = new Settings();
                SetDefaultSettings();
            }

            var manager = new WindowManager();
            manager.ShowWindow(new KeyShowViewModel(Keys, Settings));

            var timer = new Timer(1000);
            timer.Elapsed +=
                (s, e) =>
                    {
                        if (Application.Current == null || Application.Current.Dispatcher == null) return;

                        Application.Current.Dispatcher.BeginInvoke((ThreadStart) (Cleanup),
                                                                   DispatcherPriority.Background, null);
                    };

            timer.Start();
        }

        public ObservableCollection<Message> Keys { get; private set; }

        public Message CurrentMessage { get; private set; }

        public ObservableCollection<DetailedScreen> Screens { get; set; }

        public Settings Settings { get; set; }

        public void Cleanup()
        {
            var deleting =
                Keys.Where(k => DateTime.Now.Subtract(k.LastMessage) > fiveseconds && k.IsDeleting == false).ToList();
            foreach (var y in deleting)
                y.IsDeleting = true;

            var deleted =
                Keys.Where(k => DateTime.Now.Subtract(k.LastMessage) > sixseconds && k.IsDeleting).ToList();
            foreach (var y in deleted)
                Keys.Remove(y);
        }

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

            if (CurrentMessage == null || CurrentMessage.ProcessName != value.Process.ProcessName ||
                CurrentMessage.LastMessage < DateTime.Now.AddSeconds(-1))
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
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }

        public void SaveSettingsGeneral()
        {
            SaveSettings();
        }

        public void SaveSettings()
        {
            // TODO: @tobin - this looks important
            //Settings.Screen = SelectedScreen.Index;
            //if (SelectedScreen.Placement1) Settings.Placement = 1;
            //else if (SelectedScreen.Placement2) Settings.Placement = 2;
            //else if (SelectedScreen.Placement3) Settings.Placement = 3;
            //else if (SelectedScreen.Placement4) Settings.Placement = 4;
            //else Settings.Placement = 0;

            //PlaceScreen();

            settingsService.Set("PopupSettings", Settings);
            settingsService.Save();
        }

        public void SetDefaultSettings()
        {
            Settings.FontSize = 40;
            Settings.FontColor = "White";
            Settings.ItemBackgroundColor = "Black";
            Settings.ItemOpacity = 0.5;
            Settings.ItemMaxWidth = 250;

            SaveSettings();
        }
    }
}