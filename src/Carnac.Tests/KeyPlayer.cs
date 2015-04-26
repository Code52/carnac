using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Carnac.Logic.KeyMonitor;

namespace Carnac.Tests
{
    public class KeyPlayer : List<InterceptKeyEventArgs>, IInterceptKeys
    {
        public void Play(Subject<InterceptKeyEventArgs> interceptKeysSource)
        {
            foreach (var key in this)
            {
                interceptKeysSource.OnNext(key);
            }
        }

        public IObservable<InterceptKeyEventArgs> GetKeyStream()
        {
            return this.ToObservable();
        }
    }
}