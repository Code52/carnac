using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Carnac.Logic;
using Carnac.Logic.Models;

namespace Carnac
{
    public class KeysController : IDisposable
    {
        readonly ObservableCollection<Message> keys;
        readonly IMessageProvider messageProvider;
        readonly IKeyProvider keyProvider;
        readonly TimeSpan fiveseconds = TimeSpan.FromSeconds(5);
        readonly TimeSpan onesecond = TimeSpan.FromSeconds(7);
        IDisposable actionSubscription;
        readonly IConcurrencyService concurrencyService;

        public KeysController(ObservableCollection<Message> keys, IMessageProvider messageProvider, IKeyProvider keyProvider)
        {
            this.keys = keys;
            this.messageProvider = messageProvider;
            this.keyProvider = keyProvider;
            concurrencyService = new ConcurrencyService();
        }

        public void Start()
        {
            var messageStream = messageProvider.GetMessageStream(keyProvider.GetKeyStream()).Publish().RefCount();

            var addMessageStream = messageStream.Select(m => Tuple.Create(m, ActionType.Add));
            var fadeOutMessageStream = messageStream.Delay(fiveseconds).Select(m => Tuple.Create(m, ActionType.FadeOut));
            var removeMessageStream = fadeOutMessageStream.Delay(onesecond).Select(m => Tuple.Create(m.Item1, ActionType.Remove));

            var actionStream = addMessageStream.Merge(fadeOutMessageStream).Merge(removeMessageStream);
            actionSubscription = actionStream
                .ObserveOn(concurrencyService.UiScheduler)
                .SubscribeOn(concurrencyService.UiScheduler) // Because we mutate message state we need to do everything on UI thread. 
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
            });

        }

        public void Dispose()
        {
            actionSubscription.Dispose();
        }
    }
}