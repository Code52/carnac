using System.Windows;
using Carnac.Logic;
using Carnac.Logic.KeyMonitor;
using Carnac.Logic.Models;
using Carnac.UI;
using Carnac.Utilities;
using Carnac.ViewModels;
using Carnac.Views;
using SettingsProviderNet;

namespace Carnac
{
    public partial class App
    {
        readonly SettingsProvider settingsProvider;
        readonly KeyProvider keyProvider;
        readonly IMessageProvider messageProvider;
        readonly PopupSettings settings;
        PreferencesView preferencesView;
        KeyShowView keyShowView;
        CarnacTrayIcon trayIcon;
        KeysController carnac;

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
            var preferencesViewModel = new PreferencesViewModel(settingsProvider, new ScreenManager());
            preferencesView = new PreferencesView(preferencesViewModel);
            keyShowView = new KeyShowView(new KeyShowViewModel(preferencesViewModel.Keys, settings));
            keyShowView.Show();

            carnac = new KeysController(preferencesViewModel.Keys, messageProvider, keyProvider);
            carnac.Start();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            trayIcon.Dispose();
            carnac.Dispose();

            base.OnExit(e);
        }

        void TrayIconOnOpenPreferences()
        {
            preferencesView.Show();
        }
    }
}
