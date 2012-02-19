using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Analects.SettingsService;
using Caliburn.Micro;
using Carnac.Logic;
using Carnac.Logic.KeyMonitor;
using Carnac.Logic.Native;
using Carnac.Models;
using Carnac.Utilities;
using Message = Carnac.Models.Message;
using System.Reflection;
using System.Collections.Generic;

namespace Carnac.ViewModels
{
    [Export(typeof(IShell))]
    public class ShellViewModel : Screen, IShell, IObserver<KeyPress>
    {
        IDisposable keySubscription;
        readonly IDisposable timerToken;

        readonly ISettingsService settingsService;
        private readonly IKeyProvider keyProvider;

        readonly TimeSpan fiveseconds = TimeSpan.FromSeconds(5);
        readonly TimeSpan sixseconds = TimeSpan.FromSeconds(6);

        [ImportingConstructor]
        public ShellViewModel(
            ISettingsService settingsService,
            IScreenManager screenManager,
            ITimerFactory timerFactory,
            IWindowManager windowManager,
            IKeyProvider keyProvider)
        {
            this.settingsService = settingsService;
            this.keyProvider = keyProvider;

            Keys = new ObservableCollection<Message>();
            Screens = new ObservableCollection<DetailedScreen>(screenManager.GetScreens());

            Settings = settingsService.Get<Settings>("PopupSettings");
            if (Settings == null)
            {
                Settings = new Settings();
                SetDefaultSettings();
            }

            PlaceScreen();

            windowManager.ShowWindow(new KeyShowViewModel(Keys, Settings));

            timerToken = timerFactory.Start(1000, Cleanup);
        }

        public ObservableCollection<Message> Keys { get; private set; }

        public Message CurrentMessage { get; private set; }

        public ObservableCollection<DetailedScreen> Screens { get; set; }
        public DetailedScreen SelectedScreen { get; set; }

        public Settings Settings { get; set; }

        public override string DisplayName
        {
            get { return "Carnac"; }
            set { }
        }

        public string Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        private readonly List<string> authors = new List<string>
                                                    {
                                                         "Brendan Forster",
                                                         "Alex Friedman",
                                                         "Jon Galloway",
                                                         "Jake Ginnivan",
                                                         "Paul Jenkins",
                                                         "Dmitry Pursanov",
                                                         "Chris Sainty",
                                                         "Andrew Tobin"
                                                     };
        public string Authors
        {
            get { return string.Join(", ", authors); }
        }

        private readonly List<string> components = new List<string>
                                                       {
                                                         "MahApps.Metro",
                                                         "Analects",
                                                         "Caliburn Micro",
                                                         "NSubstitute",
                                                         "Reactive Extensions",
                                                         "Notify Property Weaver"
                                                     };
        public string Components
        {
            get { return string.Join(", ", components); }
        }

        public void Visit()
        {
            try
            {
                System.Diagnostics.Process.Start("http://code52.org/carnac.html");
            }
            catch //I forget what exceptions can be raised if the browser is crashed?
            {

            }
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
            keySubscription = keyProvider.Subscribe(this);
        }

        protected override void OnDeactivate(bool close)
        {
            keySubscription.Dispose();
            timerToken.Dispose();
        }

        public void OnNext(KeyPress value)
        {
            if (Keys.Count > 10)
                Keys.RemoveAt(0);

            Message m;

            if (CurrentMessage == null || CurrentMessage.ProcessName != value.Process.ProcessName ||
                CurrentMessage.LastMessage < DateTime.Now.AddSeconds(-1) ||
                value.IsShortcut)
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

            foreach (var input in value.Input)
            {
                m.AddText(input);
            }

            m.LastMessage = DateTime.Now;
            m.Count++;

            if (value.IsShortcut)
                CurrentMessage = null;
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
            if (Screens.Count < 1) return;

            if (SelectedScreen == null)
                SelectedScreen = Screens.First();

            Settings.Screen = SelectedScreen.Index;

            if (SelectedScreen.Placement1)
                Settings.Placement = 1;
            else if (SelectedScreen.Placement2)
                Settings.Placement = 2;
            else if (SelectedScreen.Placement3)
                Settings.Placement = 3;
            else if (SelectedScreen.Placement4)
                Settings.Placement = 4;
            else Settings.Placement = 2;

            PlaceScreen();

            settingsService.Set("PopupSettings", Settings);
            settingsService.Save();
        }

        public void SetDefaultSettings()
        {
            Settings.FontSize = 40;
            Settings.FontColor = "White";
            Settings.ItemBackgroundColor = "Black";
            Settings.ItemOpacity = 0.5;
            Settings.ItemMaxWidth = 350;

            SaveSettings();
        }
        private void PlaceScreen()
        {
            if (Screens == null) return;

            SelectedScreen = Screens.FirstOrDefault(s => s.Index == Settings.Screen);

            if (SelectedScreen == null) return;

            if (Settings.Placement == 1)
                SelectedScreen.Placement1 = true;
            else if (Settings.Placement == 2)
                SelectedScreen.Placement2 = true;
            else if (Settings.Placement == 3)
                SelectedScreen.Placement3 = true;
            else if (Settings.Placement == 4)
                SelectedScreen.Placement4 = true;
            else SelectedScreen.Placement2 = true;

            Settings.Left = SelectedScreen.Left;
        }
    }
}