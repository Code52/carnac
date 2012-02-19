using System;
using System.ComponentModel.Composition;
using System.Reactive.Subjects;
using Carnac.Logic.Models;

namespace Carnac.Logic
{
    [Export(typeof(IMessageProvider))]
    public class MessageProvider : IMessageProvider, IObserver<KeyPress>
    {
        readonly Subject<Message> subject = new Subject<Message>();
        private readonly IKeyProvider keyProvider;
        private readonly IShortcutProvider shortcutProvider;
        private IDisposable keyStream;

        [ImportingConstructor]
        public MessageProvider(IKeyProvider keyProvider, IShortcutProvider shortcutProvider)
        {
            this.keyProvider = keyProvider;
            this.shortcutProvider = shortcutProvider;
            keyStream = keyProvider.Subscribe(this);
        }

        public Message CurrentMessage { get; private set; }

        public IDisposable Subscribe(IObserver<Message> observer)
        {
            return subject.Subscribe(observer);
        }

        public void OnNext(KeyPress value)
        {
            Message m;

            if (CurrentMessage == null || CurrentMessage.ProcessName != value.Process.ProcessName ||
                CurrentMessage.LastMessage < DateTime.Now.AddSeconds(-1) ||
                value.IsShortcut)
            {
                m = new Message
                {
                    StartingTime = DateTime.Now,
                    ProcessName = value.Process.ProcessName
                };

                CurrentMessage = m;
                subject.OnNext(m);
            }
            else
                m = CurrentMessage;

            m.AddKey(value);

            m.LastMessage = DateTime.Now;
            m.Count++;

            if (value.IsShortcut)
                CurrentMessage = null;
        }

        public void OnError(Exception error)
        {

        }

        public void OnCompleted()
        {

        }
    }

    public interface IMessageProvider : IObservable<Message>
    {
    }

    public interface IShortcutProvider
    {
    }

    [Export(typeof(IShortcutProvider))]
    public class ShortcutProvider : IShortcutProvider
    {
    }
}