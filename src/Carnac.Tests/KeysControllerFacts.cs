using System;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Forms;
using Carnac.Logic;
using Carnac.Logic.KeyMonitor;
using Carnac.Logic.Models;
using Microsoft.Reactive.Testing;
using NSubstitute;
using Shouldly;
using Xunit;
using Message = Carnac.Logic.Models.Message;

namespace Carnac.Tests
{
    public class KeysControllerFacts
    {
        readonly ObservableCollection<Message> keysCollection = new ObservableCollection<Message>();
        readonly TestScheduler testScheduler;
        readonly KeysController sut;
        readonly Subject<Message> messageStream;

        public KeysControllerFacts()
        {
            testScheduler = new TestScheduler();
            var messageProvider = Substitute.For<IMessageProvider>();
            messageProvider.GetMessageStream(Arg.Any<IObservable<KeyPress>>()).Returns(_ => messageStream);
            var keyProvider = Substitute.For<IKeyProvider>();
            var concurrencyService = Substitute.For<IConcurrencyService>();
            concurrencyService.UiScheduler.Returns(testScheduler);
            concurrencyService.Default.Returns(testScheduler);
            messageStream = new Subject<Message>();
            sut = new KeysController(keysCollection, messageProvider, keyProvider, concurrencyService);
        }

        [Fact]
        public void MessagesAreAddedIntoKeysColletion()
        {
            var message = new Message(A);

            sut.Start();
            ProvideMessage(message);

            keysCollection.ShouldContain(message);
            message.IsDeleting.ShouldBe(false);
        }

        [Fact]
        public void MessagesAreFlaggedAsDeletingAfter5Seconds()
        {
            var message = new Message(A);

            sut.Start();
            ProvideMessage(message);
            testScheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);

            message.IsDeleting.ShouldBe(true);
        }

        [Fact]
        public void MessagesIsRemovedAfter6Seconds()
        {
            var message = new Message(A);

            sut.Start();
            ProvideMessage(message);
            testScheduler.AdvanceBy(TimeSpan.FromSeconds(6).Ticks);
            
            keysCollection.ShouldNotContain(message);
        }

        [Fact]
        public void MessageTimeoutIsStartedAgainIfMessageIsUpdated()
        {
            var message = new Message(A);

            sut.Start();
            ProvideMessage(message);
            testScheduler.AdvanceBy(TimeSpan.FromSeconds(3).Ticks);
            message.Merge(new Message(A));
            testScheduler.AdvanceBy(TimeSpan.FromSeconds(3).Ticks);

            message.IsDeleting.ShouldBe(false);
            keysCollection.ShouldContain(message);
        }

        [Fact]
        public void UnsubscribesFromMessageUpdatesAfterDeleteTimeout()
        {
            var message = new Message(A);

            sut.Start();
            ProvideMessage(message);
            testScheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);
            message.IsDeleting = false;
            message.Merge(new Message(A)); // This will trigger an update of the message
            testScheduler.AdvanceBy(5);
            testScheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);

            // If we didn't unsubscribe to updates we would have triggered another inner sequence which would try to delete again.
            message.IsDeleting.ShouldBe(false);
        }

        void ProvideMessage(Message message)
        {
            testScheduler.AdvanceBy(5); // Need to tick to allows merge to be setup properly
            messageStream.OnNext(message);
            testScheduler.AdvanceBy(10); // Few more ticks to allow the message to be processed
        }

        static KeyPress A
        {
            get
            {
                return new KeyPress(new Processinfo("foo"),
                    new InterceptKeyEventArgs(Keys.A, KeyDirection.Down, false, false, false), false, new[] {"a"});
            }
        }
    }
}
