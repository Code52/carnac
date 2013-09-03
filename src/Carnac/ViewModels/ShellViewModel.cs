using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Carnac.Logic;
using Carnac.Logic.Enums;
using Carnac.Logic.Models;
using Carnac.Logic.Native;
using Carnac.Logic.Settings;
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

        readonly ISettingsProvider settingsProvider;
        readonly IMessageProvider messageProvider;

        readonly TimeSpan fiveseconds = TimeSpan.FromSeconds(5);
        readonly TimeSpan sixseconds = TimeSpan.FromSeconds(6);

        [ImportingConstructor]
        public ShellViewModel(
            ISettingsProvider settingsProvider,
            IScreenManager screenManager,
            ITimerFactory timerFactory,
            IWindowManager windowManager,
            IMessageProvider messageProvider)
        {
            this.settingsProvider = settingsProvider;
            this.messageProvider = messageProvider;

            Keys = new ObservableCollection<Message>();
            Screens = new ObservableCollection<DetailedScreen>(screenManager.GetScreens());

            Settings = settingsProvider.GetSettings<PopupSettings>();

            PlaceScreen();

            var keyShowViewModel = new KeyShowViewModel(Keys, Settings);
            windowManager.ShowWindow(keyShowViewModel);

            timerToken = timerFactory.Start(1000, Cleanup);
            DisplayName = "Carnac";
        }

        public ObservableCollection<Message> Keys { get; private set; }

        public ObservableCollection<DetailedScreen> Screens { get; set; }
        public DetailedScreen SelectedScreen { get; set; }

        public PopupSettings Settings { get; set; }

        public string Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        readonly List<string> authors = new List<string>
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
        readonly List<string> components = new List<string>
                                                       {
                                                         "MahApps.Metro",
                                                         "Analects",
                                                         "Caliburn Micro",
                                                         "NSubstitute",
                                                         "Reactive Extensions",
                                                         "Notify Property Weaver"
                                                     };
        public string Authors
        {
            get { return string.Join(", ", authors); }
        }

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

            settingsProvider.SaveSettings(Settings);
        }

        public void SetDefaultSettings()
        {
            settingsProvider.ResetToDefaults<PopupSettings>();
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