using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using Carnac.Logic.KeyMonitor;

namespace Carnac.Tests
{
    public class KeyPlayer : List<InterceptKeyEventArgs>, IInterceptKeys
    {
        readonly Subject<InterceptKeyEventArgs> subject = new Subject<InterceptKeyEventArgs>();

        public void Play()
        {
            foreach (var key in this)
            {
                subject.OnNext(key);
            }
            subject.OnCompleted();
        }

        public void Play(Subject<InterceptKeyEventArgs> interceptKeysSource)
        {
            foreach (var key in this)
            {
                interceptKeysSource.OnNext(key);
            }
        }

        public IObservable<InterceptKeyEventArgs> GetKeyStream()
        {
            return subject;
        }
    }
}