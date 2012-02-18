using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
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
        IDisposable keySubscription;

        readonly ISettingsService settingsService;

        readonly TimeSpan fiveseconds = TimeSpan.FromSeconds(5);
        readonly TimeSpan sixseconds = TimeSpan.FromSeconds(6);

        [ImportingConstructor]
        public ShellViewModel(ISettingsService settingsService, IScreenManager screenManager)
        {
            this.settingsService = settingsService;

            Keys = new ObservableCollection<Message>();
            Screens = new ObservableCollection<DetailedScreen>(screenManager.GetScreens());

            Settings = settingsService.Get<Settings>("PopupSettings");
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

        public override string DisplayName
        {
            get { return "Carnac"; }
            set { }
        }

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

            var controlPressed = value.InterceptKeyEventArgs.ControlPressed;
            var altPressed = value.InterceptKeyEventArgs.AltPressed;
            var shiftPressed = value.InterceptKeyEventArgs.ShiftPressed;
            if (controlPressed)
                m.Text.Add("Ctrl");
            if (altPressed)
                m.Text.Add("Alt");
            if (shiftPressed)
                m.Text.Add("Shift");
            
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