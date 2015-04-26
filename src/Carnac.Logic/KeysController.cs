using System;
using System.Collections.ObjectModel;
using System.Reactive;
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

        public KeysController(ObservableCollection<Message> keys, IMessageProvider messageProvider,  IConcurrencyService concurrencyService)
        {
            this.keys = keys;
            this.messageProvider = messageProvider;
            this.concurrencyService = concurrencyService;
        }

        public void Start()
        {
            var messageStream = messageProvider.GetMessageStream().Publish().RefCount();

            var addMessageStream = messageStream.Select(m => Tuple.Create(m, ActionType.Add));

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
                })
                .Select(m => Tuple.Create(m, ActionType.FadeOut));

            // Finally we just put a one second delay on the messages from the fade out stream and flag to remove.
            var removeMessageStream = fadeOutMessageStream
                .Delay(OneSecond, concurrencyService.Default)
                .Select(m => Tuple.Create(m.Item1, ActionType.Remove));
            
            var actionStream = Observable.Merge(
                removeMessageStream, 
                fadeOutMessageStream, 
                addMessageStream);

            actionSubscription.Disposable = actionStream
                .ObserveOn(concurrencyService.MainThreadScheduler)
                .SubscribeOn(concurrencyService.MainThreadScheduler) // Because we mutate message state we need to do everything on UI thread. 
                                                             // If we introduced a 'Update' action to this feed we could remove mutation from the stream
                .Subscribe(action =>
            {
                switch (action.Item2)
                {
                    case ActionType.Add:
                        keys.Add(action.Item1);
                        break;
                    case ActionType.Remove:
                        keys.Remove(action.Item1);
                        break;
                    case ActionType.FadeOut:
                        action.Item1.IsDeleting = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }, ex =>
            {
                
            });
        }

        public void Dispose()
        {
            actionSubscription.Dispose();
        }
    }
}