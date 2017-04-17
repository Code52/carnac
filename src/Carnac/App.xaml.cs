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
        readonly IMessageProvider messageProvider;
        readonly PopupSettings settings;
        KeyShowView keyShowView;
        CarnacTrayIcon trayIcon;
        KeysController carnac;

        public App()
        {
            var keyProvider = new KeyProvider(InterceptKeys.Current, new PasswordModeService(), new DesktopLockEventService());
            settingsProvider = new SettingsProvider(new RoamingAppDataStorage("Carnac"));
            settings = settingsProvider.GetSettings<PopupSettings>();
            messageProvider = new MessageProvider(new ShortcutProvider(), keyProvider, settings);
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
            var keyShowViewModel = new KeyShowViewModel(settings);
            keyShowView = new KeyShowView(keyShowViewModel);
            keyShowView.Show();

            carnac = new KeysController(keyShowViewModel.Messages, messageProvider, new ConcurrencyService());
            carnac.Start();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            trayIcon.Dispose();
            carnac.Dispose();
            ProcessUtilities.DestroyMutex();

            base.OnExit(e);
        }

        void TrayIconOnOpenPreferences()
        {
            var preferencesViewModel = new PreferencesViewModel(settingsProvider, new ScreenManager());
            var preferencesView = new PreferencesView(preferencesViewModel);
            preferencesView.Show();
        }
    }
}
