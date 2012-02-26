using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Caliburn.Micro;
using Carnac.Logic;
using Carnac.Logic.KeyMonitor;

namespace Carnac
{
    public class AppBootstrapper : Bootstrapper<IShell>
    {
        public static EventAggregator Aggregator { get; set; }
        CompositionContainer container;

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

            batch.AddExportedValue<IKeyProvider>(new KeyProvider(InterceptKeys.Current, new PasswordModeService()));
            batch.AddExportedValue<IWindowManager>(new WindowManager());
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            batch.AddExportedValue(container);
            batch.AddExportedValue(catalog);

            container.Compose(batch);
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            string contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
            var exports = container.GetExportedValues<object>(contract);

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
    }
}
