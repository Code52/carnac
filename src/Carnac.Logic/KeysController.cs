using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Carnac.Logic.Models;
using SettingsProviderNet;

namespace Carnac.Logic
{
    public class KeysController : IDisposable
    {
        static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);
        readonly TimeSpan fadeOutDelay;
        readonly ObservableCollection<Message> messages;
        readonly IMessageProvider messageProvider;
        readonly IConcurrencyService concurrencyService;
        readonly SingleAssignmentDisposable actionSubscription = new SingleAssignmentDisposable();

        public KeysController(ObservableCollection<Message> messages, IMessageProvider messageProvider, IConcurrencyService concurrencyService, ISettingsProvider settingsProvider)
        {
            this.messages = messages;
            this.messageProvider = messageProvider;
            this.concurrencyService = concurrencyService;

            var settings = settingsProvider.GetSettings<PopupSettings>();
            fadeOutDelay = TimeSpan.FromSeconds(settings.ItemFadeDelay);
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
                            messages.Remove(newMessage.Previous);
                        }
                        messages.Add(newMessage);
                    });

            var fadeOutMessageSeq = messageStream
                .Delay(fadeOutDelay, concurrencyService.Default)
                .Select(m => m.FadeOut())
                .Publish();

            var fadeOutMessageSubscription = fadeOutMessageSeq
                .ObserveOn(concurrencyService.MainThreadScheduler)
                .Subscribe(msg =>
                {
                    var idx = messages.IndexOf(msg.Previous);
                    if(idx>-1)
                        messages[idx] = msg;
                });

            // Finally we just put a one second delay on the messages from the fade out stream and flag to remove.
            var removeMessageSubscription = fadeOutMessageSeq
                .Delay(OneSecond, concurrencyService.Default)
                .ObserveOn(concurrencyService.MainThreadScheduler)
                .Subscribe(msg => messages.Remove(msg));


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