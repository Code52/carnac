using System;
using System.Collections.Generic;
using Carnac.Logic.KeyMonitor;

namespace Carnac.Tests
{
    public class KeyPlayer : List<InterceptKeyEventArgs>, IObservable<InterceptKeyEventArgs>
    {
        private IObserver<InterceptKeyEventArgs> subscriber;

        public IDisposable Subscribe(IObserver<InterceptKeyEventArgs> observer)
        {
            subscriber = observer;

            return new DoNothing();
        }

        internal class DoNothing : IDisposable
        {
            public void Dispose()
            {
                
            }
        }

        public void Play()
        {
            foreach (var key in this)
            {
                subscriber.OnNext(key);
            }
            subscriber.OnCompleted();
        }
    }
}