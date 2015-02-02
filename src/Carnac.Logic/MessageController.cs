using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Carnac.Logic.Models;

namespace Carnac.Logic
{
    public class MessageController : IDisposable
    {
        static readonly TimeSpan FiveSeconds = TimeSpan.FromSeconds(5);
        static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);

        readonly SerialDisposable stopCarnacDisposable = new SerialDisposable();
        readonly ObservableCollection<Message> messages;
        readonly IMessageProvider messageProvider;
        readonly IKeyProvider keyProvider;
        readonly IConcurrencyService concurrencyService;

        public MessageController(ObservableCollection<Message> messages, IMessageProvider messageProvider, IKeyProvider keyProvider, IConcurrencyService concurrencyService)
        {
            this.messages = messages;
            this.messageProvider = messageProvider;
            this.keyProvider = keyProvider;
            this.concurrencyService = concurrencyService;
        }

        public void Start()
        {
            var keyStream = keyProvider.GetKeyStream();
            var messageStream = messageProvider.GetMessageStream(keyStream).Publish();

            var addMessageSubscription = messageStream
                .ObserveOn(concurrencyService.MainThreadScheduler)
                .SubscribeOn(concurrencyService.MainThreadScheduler)
                .Subscribe(message => messages.Add(message));

            /*
            Fade out is a rolling query.

            In the below marble diagram each - represents one second
            a--------b---------a----*ab----
            -----a|
                     -----b|
                               ---------ab|
            -----a--------b-------------ab

            The inner sequence you an see happening after each press waits 5 seconds before releasing the message and completing the inner stream (take(1)).
            */
            var fadeOutMessageStream = messageStream
                .SelectMany(message =>
                {
                    /*
                    Inner sequence diagram (x is an update, @ is the start of an observable.Timer(), o is a timer firing)

                    x---x----x-----
                    @---|
                        @----|
                             @-----o|
                    ---------------x|
                    */
                    return message.Updated
                        .StartWith(Unit.Default)
                        .Select(_ => Observable.Timer(FiveSeconds, concurrencyService.Default))
                        .Switch()
                        .Select(_ => message)
                        .Take(1);
                });
            var fadeOutMessageSubscription = fadeOutMessageStream
                .ObserveOn(concurrencyService.MainThreadScheduler)
                .SubscribeOn(concurrencyService.MainThreadScheduler)
                .Subscribe(m => m.IsDeleting = true);

            // Finally we just put a one second delay on the messages from the fade out stream and flag to remove.
            var removeMessageSubscription = fadeOutMessageStream
                .Delay(OneSecond, concurrencyService.Default)
                .ObserveOn(concurrencyService.MainThreadScheduler)
                .SubscribeOn(concurrencyService.MainThreadScheduler)
                .Subscribe(m => messages.Remove(m));

            stopCarnacDisposable.Disposable = new CompositeDisposable(
                messageStream.Connect(), 
                addMessageSubscription, 
                fadeOutMessageSubscription,
                removeMessageSubscription);
        }

        public void Dispose()
        {
            stopCarnacDisposable.Dispose();
        }
    }
}