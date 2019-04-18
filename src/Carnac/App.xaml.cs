using System;
using System.IO;
using System.Net;
using System.Reactive.Linq;
using System.Windows;
using Carnac.Logic;
using Carnac.Logic.KeyMonitor;
using Carnac.Logic.Models;
using Carnac.UI;
using Carnac.Utilities;
using SettingsProviderNet;
using Squirrel;

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

#if !DEBUG
        readonly string carnacUpdateUrl = "https://github.com/Code52/carnac";
#endif

        public App()
        {
            settingsProvider = new SettingsProvider(new RoamingAppDataStorage("Carnac"));
            settings = settingsProvider.GetSettings<PopupSettings>();
            var keyProvider = new KeyProvider(InterceptKeys.Current, new PasswordModeService(), new DesktopLockEventService(), settingsProvider);
            messageProvider = new MessageProvider(new ShortcutProvider(), keyProvider, settings);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
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

            carnac = new KeysController(keyShowViewModel.Messages, messageProvider, new ConcurrencyService(), settingsProvider);
            carnac.Start();

#if !DEBUG
            if (settings.AutoUpdate)
            {
                Observable
                    .Timer(TimeSpan.FromMinutes(5))
                    .Subscribe(async x =>
                    {
                        try
                        {
                            using (var mgr = UpdateManager.GitHubUpdateManager(carnacUpdateUrl))
                            {
                                await mgr.Result.UpdateApp();
                            }
                        }
                        catch
                        {
                            // Do something useful with the exception
                        }
                    });
            }
#endif

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
