using System;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using Carnac.KeyMonitor;
using System.ComponentModel.Composition;

namespace Carnac.ViewModels
{
    [Export(typeof(IShell))]
    public class ShellViewModel :Screen, IShell, IObserver<InterceptKeyEventArgs>
    {
        private IDisposable keySubscription;

        public ShellViewModel()
        {
            Keys = new ObservableCollection<string>();
        }

        public ObservableCollection<string> Keys { get; private set; }

        protected override void OnActivate()
        {
            keySubscription = InterceptKeys.Current.Subscribe(this);
        }

        protected override void OnDeactivate(bool close)
        {
            keySubscription.Dispose();
        }

        public void OnNext(InterceptKeyEventArgs value)
        {
            if (value.KeyDirection != KeyDirection.Up) return;
            if (Keys.Count > 3)
                Keys.RemoveAt(0);

            if (value.AltPressed && value.ControlPressed)
                Keys.Add(string.Format("Ctrl + Alt + {0}", value.Key));
            else if (value.AltPressed)
                Keys.Add(string.Format("Alt + {0}", value.Key));
            else if (value.ControlPressed)
                Keys.Add(string.Format("Ctrl + {0}", value.Key));
            else
                Keys.Add(value.Key.ToString());
        }
        public void OnError(Exception error){}
        public void OnCompleted(){}
    }
}
