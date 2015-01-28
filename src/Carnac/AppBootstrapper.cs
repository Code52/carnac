using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Carnac.Logic;
using Carnac.Logic.KeyMonitor;
using Carnac.Logic.Models;
using SettingsProviderNet;

namespace Carnac
{
    public class AppBootstrapper : Bootstrapper<IShell>
    {
        public static EventAggregator Aggregator { get; set; }
        CompositionContainer container;
        CarnacWindowManager windowManager;
        SettingsProvider settingsProvider;

        /// <summary>
        /// By default, we are configured to use MEF
        /// </summary>
        protected override void Configure()
        {
            Aggregator = new EventAggregator();

            var catalog = new AggregateCatalog(
                new AssemblyCatalog(typeof(AppBootstrapper).Assembly),
                new AssemblyCatalog(typeof(IScreenManager).Assembly));

            container = new CompositionContainer(catalog);

            var batch = new CompositionBatch();

            batch.AddExportedValue<IKeyProvider>(new KeyProvider(InterceptKeys.Current, new PasswordModeService(), new DesktopLockEventService()));
            windowManager = new CarnacWindowManager();
            batch.AddExportedValue<IWindowManager>(windowManager);
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            settingsProvider = new SettingsProvider(new RoamingAppDataStorage("Carnac"));
            batch.AddExportedValue<ISettingsProvider>(settingsProvider);
            batch.AddExportedValue(container);
            batch.AddExportedValue(catalog);

            container.Compose(batch);
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            string contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
            var exports = container.GetExportedValues<object>(contract).ToArray();

            if (exports.Any())
                return exports.First();

            throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
        }

        protected override void BuildUp(object instance)
        {
            container.SatisfyImportsOnce(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            Shell = (IShell) IoC.GetInstance(typeof (IShell), null);
            var window = windowManager.CreateWindow(Shell);
            if (!settingsProvider.GetSettings<PopupSettings>().SettingsConfigured)
                window.Show();
        }

        public IShell Shell { get; set; }
    }
}
