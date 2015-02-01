using System;
using System.Collections.ObjectModel;
using System.Windows;
using Carnac.Logic;
using Carnac.Logic.KeyMonitor;
using Carnac.Logic.Models;
using Carnac.UI;
using Carnac.Utilities;
using SettingsProviderNet;

namespace Carnac
{
    public partial class App
    {
        readonly SettingsProvider settingsProvider;
        readonly KeyProvider keyProvider;
        readonly IMessageProvider messageProvider;
        readonly PopupSettings settings;
        KeyShowView keyShowView;
        CarnacTrayIcon trayIcon;
        KeysController carnac;
        ObservableCollection<Message> keyCollection;
        IDisposable shutdownCarnac;

        public App()
        {
            settingsProvider = new SettingsProvider(new RoamingAppDataStorage("Carnac"));
            keyProvider = new KeyProvider(InterceptKeys.Current, new PasswordModeService(), new DesktopLockEventService());
            settings = settingsProvider.GetSettings<PopupSettings>();
            messageProvider = new MessageProvider(new ShortcutProvider(), settings, new MessageMerger());
        }

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
            trayIcon.ShutdownCarnac += TrayIconOnShutdownCarnac;
            keyCollection = new ObservableCollection<Message>();
            keyShowView = new KeyShowView(new KeyShowViewModel(keyCollection, settings));
            keyShowView.Show();

            carnac = new KeysController(keyCollection, messageProvider, keyProvider, new ConcurrencyService());
            shutdownCarnac = carnac.Start();

            base.OnStartup(e);
        }

        void TrayIconOnShutdownCarnac()
        {
            ProcessUtilities.DestroyMutex();
            trayIcon.Dispose();
            shutdownCarnac.Dispose();
            Shutdown();
        }

        void TrayIconOnOpenPreferences()
        {
            var preferencesViewModel = new PreferencesViewModel(settingsProvider, new ScreenManager());
            var preferencesView = new PreferencesView(preferencesViewModel);
            preferencesView.Show();
        }
    }
}
