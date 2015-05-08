using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Carnac.Logic.KeyMonitor;

namespace Carnac.Tests
{
    public class KeyPlayer : List<InterceptKeyEventArgs>, IInterceptKeys
    {
        public IObservable<InterceptKeyEventArgs> GetKeyStream()
        {
            return this.ToObservable();
        }
    }
}