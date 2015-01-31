using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Caliburn.Micro;
using Carnac.Logic;
using Carnac.Logic.Enums;
using Carnac.Logic.Models;
using Carnac.Logic.Native;
using SettingsProviderNet;
using Message = Carnac.Logic.Models.Message;

namespace Carnac.ViewModels
{
    [Export(typeof(IShell))]
    public class ShellViewModel : Screen, IShell
    {
        readonly ISettingsProvider settingsProvider;

        [ImportingConstructor]
        public ShellViewModel(
            ISettingsProvider settingsProvider,
            IScreenManager screenManager,
            IWindowManager windowManager)
        {
            this.settingsProvider = settingsProvider;

            Keys = new ObservableCollection<Message>();
            Screens = new ObservableCollection<DetailedScreen>(screenManager.GetScreens());

            Settings = settingsProvider.GetSettings<PopupSettings>();

            PlaceScreen();

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
                                                         "Fody",
                                                         "Caliburn Micro",
                                                         "NSubstitute",
                                                         "Reactive Extensions"
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
                Process.Start("http://code52.org/carnac/");
            }
            catch //I forget what exceptions can be raised if the browser is crashed?
            {

            }
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

            Settings.SettingsConfigured = true;
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