using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Carnac.Logic.Models;

namespace Carnac.Logic
{
    public class KeysController : IDisposable
    {
        static readonly TimeSpan FiveSeconds = TimeSpan.FromSeconds(5);
        static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);
        readonly ObservableCollection<Message> keys;
        readonly IMessageProvider messageProvider;
        readonly IConcurrencyService concurrencyService;
        readonly SingleAssignmentDisposable actionSubscription = new SingleAssignmentDisposable();

        public KeysController(ObservableCollection<Message> keys, IMessageProvider messageProvider, IConcurrencyService concurrencyService)
        {
            this.keys = keys;
            this.messageProvider = messageProvider;
            this.concurrencyService = concurrencyService;
        }

        public void Start()
        {
            var messageStream = messageProvider.GetMessageStream().Publish();

            var addMessageSubscription = messageStream
                .ObserveOn(concurrencyService.MainThreadScheduler)
                .Subscribe(newMessage =>
                    {
                        if (newMessage.Previous != null)
                        {
                            keys.Remove(newMessage.Previous);
                        }
                        keys.Add(newMessage);
                    });

            var fadeOutMessageSeq = messageStream
                .Delay(FiveSeconds, concurrencyService.Default)
                .Select(m => m.FadeOut())
                .Publish();

            var fadeOutMessageSubscription = fadeOutMessageSeq
                .Subscribe(msg =>
                {
                    var idx = keys.IndexOf(msg.Previous);
                    if(idx>-1)
                        keys[idx] = msg;
                });

            // Finally we just put a one second delay on the messages from the fade out stream and flag to remove.
            var removeMessageSubscription = fadeOutMessageSeq
                .Delay(OneSecond, concurrencyService.Default)
                .Subscribe(msg => keys.Remove(msg));


            actionSubscription.Disposable = new CompositeDisposable(
                addMessageSubscription,
                fadeOutMessageSubscription,
                removeMessageSubscription,
                fadeOutMessageSeq.Connect(),
                messageStream.Connect());
        }

        public void Dispose()
        {
            actionSubscription.Dispose();
        }
    }
}