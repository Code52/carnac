using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Windows;
using Carnac.Logic;
using Carnac.Logic.KeyMonitor;
using Carnac.Logic.Models;
using Carnac.Utilities;
using Carnac.Views;
using SettingsProviderNet;

namespace Carnac
{
    public partial class App
    {
        CompositionContainer container;
        SettingsProvider settingsProvider;
        CarnacTrayIcon trayIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Check if there was instance before this. If there was-close the current one.  
            if (ProcessUtilities.ThisProcessIsAlreadyRunning())
            {
                ProcessUtilities.SetFocusToPreviousInstance("Carnac");
                Shutdown();
                return;
            }

            trayIcon = new CarnacTrayIcon();
            trayIcon.OpenPreferences += TrayIconOnOpenPreferences;

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            trayIcon.Dispose();
            base.OnExit(e);
        }

        void TrayIconOnOpenPreferences()
        {

        }
    }
}
