using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Analects.SettingsService;
using Caliburn.Micro;
using Carnac.Enum;
using Carnac.Logic;
using Carnac.Logic.Native;
using Carnac.Models;
using Carnac.Utilities;
using Message = Carnac.Logic.Models.Message;
using System.Reflection;
using System.Collections.Generic;

namespace Carnac.ViewModels
{
    [Export(typeof(IShell))]
    public class ShellViewModel : Screen, IShell, IObserver<Message>
    {
        IDisposable keySubscription;
        readonly IDisposable timerToken;

        readonly ISettingsService settingsService;
        private readonly IMessageProvider messageProvider;

        readonly TimeSpan fiveseconds = TimeSpan.FromSeconds(5);
        readonly TimeSpan sixseconds = TimeSpan.FromSeconds(6);

        [ImportingConstructor]
        public ShellViewModel(
            ISettingsService settingsService,
            IScreenManager screenManager,
            ITimerFactory timerFactory,
            IWindowManager windowManager,
            IMessageProvider messageProvider)
        {
            this.settingsService = settingsService;
            this.messageProvider = messageProvider;

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
                System.Diagnostics.Process.Start("http://code52.org/carnac/");
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
            keySubscription = messageProvider.Subscribe(this);
        }

        protected override void OnDeactivate(bool close)
        {
            keySubscription.Dispose();
            timerToken.Dispose();
        }

        public void OnNext(Message value)
        {
            if (Keys.Count > 10)
                Keys.RemoveAt(0);

            Keys.Add(value);
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
            if (Screens.Count < 1) 
                return;

            if (SelectedScreen == null)
                SelectedScreen = Screens.First();

            Settings.Screen = SelectedScreen.Index;

               if (SelectedScreen.NotificationPlacementTopLeft)
                Settings.Placement = NotificationPlacement.TopLeft;
            else if (SelectedScreen.NotificationPlacementBottomLeft)
                Settings.Placement = NotificationPlacement.BottomLeft;
            else if (SelectedScreen.NotificationPlacementTopRight)
                Settings.Placement = NotificationPlacement.TopRight;
            else if (SelectedScreen.NotificationPlacementBottomRight)
                Settings.Placement = NotificationPlacement.BottomRight;
            else 
				Settings.Placement = NotificationPlacement.BottomLeft;

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
            if (Screens == null) 
                return;

            SelectedScreen = Screens.FirstOrDefault(s => s.Index == Settings.Screen);

            if (SelectedScreen == null) 
                return;

            switch (Settings.Placement)
            {
                case NotificationPlacement.TopLeft:
                    SelectedScreen.NotificationPlacementTopLeft = true;
                    break;
                case NotificationPlacement.BottomLeft:
                    SelectedScreen.NotificationPlacementBottomLeft = true;
                    break;
                case NotificationPlacement.TopRight:
                    SelectedScreen.NotificationPlacementTopRight = true;
                    break;
                case NotificationPlacement.BottomRight:
                    SelectedScreen.NotificationPlacementBottomRight = true;
                    break;
                default:
                    SelectedScreen.NotificationPlacementBottomLeft = true;
                    break;
            }

            Settings.Left = SelectedScreen.Left;
        }
    }
}