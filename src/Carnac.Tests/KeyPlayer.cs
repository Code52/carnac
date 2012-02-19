using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using Carnac.Logic.KeyMonitor;

namespace Carnac.Tests
{
    public class KeyPlayer : List<InterceptKeyEventArgs>, IObservable<InterceptKeyEventArgs>
    {
        readonly Subject<InterceptKeyEventArgs> subject = new Subject<InterceptKeyEventArgs>();

        public IDisposable Subscribe(IObserver<InterceptKeyEventArgs> observer)
        {
            return subject.Subscribe(observer);
        }

        public void Play()
        {
            foreach (var key in this)
            {
                subject.OnNext(key);
            }
            subject.OnCompleted();
        }
    }
}