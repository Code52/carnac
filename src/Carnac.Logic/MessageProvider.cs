using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Subjects;
using Carnac.Logic.Models;
using Message = Carnac.Logic.Models.Message;

namespace Carnac.Logic
{
    [Export(typeof(IMessageProvider))]
    public class MessageProvider : IMessageProvider, IObserver<KeyPress>
    {
        readonly Subject<Message> subject = new Subject<Message>();
        private readonly IShortcutProvider shortcutProvider;
        private IDisposable keyStream;

        [ImportingConstructor]
        public MessageProvider(IKeyProvider keyProvider, IShortcutProvider shortcutProvider)
        {
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
            Message message;

            if (ShouldCreateNewMessage(value) && !IsChord(value))
            {
                message = new Message
                {
                    StartingTime = DateTime.Now,
                    ProcessName = value.Process.ProcessName
                };

                CurrentMessage = message;
                subject.OnNext(message);
            }
            else
                message = CurrentMessage;

            message.AddKey(value);

            message.LastMessage = DateTime.Now;
            message.Count++;
        }

        private bool ShouldCreateNewMessage(KeyPress value)
        {
            return
                CurrentMessage == null ||
                IsDifferentProcess(value) ||
                IsOlderThanASecond()  ||
                LastKeyPressWasShortcut();
        }

        private bool LastKeyPressWasShortcut()
        {
            return CurrentMessage.Keys.Last().IsShortcut;
        }

        private bool IsChord(KeyPress value)
        {
            if (CurrentMessage == null)
                return false;
            var isChord = shortcutProvider.IsChord(CurrentMessage.Keys, value);
            return isChord;
        }

        private bool IsOlderThanASecond()
        {
            return CurrentMessage.LastMessage < DateTime.Now.AddSeconds(-1);
        }

        private bool IsDifferentProcess(KeyPress value)
        {
            return CurrentMessage.ProcessName != value.Process.ProcessName;
        }

        public void OnError(Exception error)
        {

        }

        public void OnCompleted()
        {

        }
    }
}