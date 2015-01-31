using System.Windows;
using Caliburn.Micro;
using Carnac.Logic;
using Carnac.Logic.KeyMonitor;
using Carnac.Logic.Models;
using Carnac.ViewModels;
using SettingsProviderNet;

namespace Carnac
{
    public class AppBootstrapper : Bootstrapper<IShell>
    {
        CarnacWindowManager windowManager;
        SettingsProvider settingsProvider;
        KeyShowViewModel keyShowViewModel;
        KeysController carnac;
        KeyProvider keyProvider;

        protected override void Configure()
        {
            windowManager = new CarnacWindowManager();
            settingsProvider = new SettingsProvider(new RoamingAppDataStorage("Carnac"));
            keyProvider = new KeyProvider(InterceptKeys.Current, new PasswordModeService(), new DesktopLockEventService());
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            Shell = new ShellViewModel(settingsProvider, new ScreenManager(), windowManager);

            keyShowViewModel = new KeyShowViewModel(Shell.Keys, Shell.Settings);
            windowManager.ShowWindow(keyShowViewModel);

            var messageProvider = new MessageProvider(new ShortcutProvider(), Shell.Settings, new MessageMerger());
            carnac = new KeysController(Shell.Keys, messageProvider, keyProvider);

            var window = windowManager.CreateWindow(Shell);
            if (!settingsProvider.GetSettings<PopupSettings>().SettingsConfigured)
                window.Show();
            carnac.Start();
        }

        public IShell Shell { get; set; }
    }
}
