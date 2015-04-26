using System;
using System.Collections.ObjectModel;
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
        const int MessageAOnNextTick = 100;

        readonly ObservableCollection<Message> keysCollection = new ObservableCollection<Message>();
        readonly TestScheduler testScheduler;
        readonly Message messageA = new Message(A);
        readonly KeysController sut;

        public KeysControllerFacts()
        {
            testScheduler = new TestScheduler();
            var messageSequence = testScheduler.CreateColdObservable(
                ReactiveTest.OnNext(MessageAOnNextTick, messageA)
                );

            sut = CreateKeysController(messageSequence);
        }

        KeysController CreateKeysController(IObservable<Message> messageStream)
        {
            var messageProvider = Substitute.For<IMessageProvider>();
            messageProvider.GetMessageStream(Arg.Any<IObservable<KeyPress>>()).Returns(_ => messageStream);
            var keyProvider = Substitute.For<IKeyProvider>();
            var concurrencyService = Substitute.For<IConcurrencyService>();
            concurrencyService.MainThreadScheduler.Returns(testScheduler);
            concurrencyService.Default.Returns(testScheduler);
            return new KeysController(keysCollection, messageProvider, keyProvider, concurrencyService);
        }

        [Fact]
        public void MessagesAreAddedIntoKeysColletion()
        {
            sut.Start();
            testScheduler.AdvanceTo(MessageAOnNextTick);
            testScheduler.AdvanceBy(2);//ObserveOn+SubscribeOn cost

            keysCollection.ShouldContain(messageA);
            messageA.IsDeleting.ShouldBe(false);
        }

        [Fact]
        public void MessagesAreFlaggedAsDeletingAfter5Seconds()
        {
            sut.Start();
            testScheduler.AdvanceBy(MessageAOnNextTick + 2);
            testScheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);

            messageA.IsDeleting.ShouldBe(true);
        }

        [Fact]
        public void MessagesIsRemovedAfter6Seconds()
        {
            sut.Start();
            testScheduler.AdvanceBy(MessageAOnNextTick + 2);
            testScheduler.AdvanceBy(TimeSpan.FromSeconds(6).Ticks);

            keysCollection.ShouldNotContain(messageA);
        }

        [Fact]
        public void MessageTimeoutIsStartedAgainIfMessageIsUpdated()
        {
            sut.Start();
            testScheduler.AdvanceBy(TimeSpan.FromSeconds(3).Ticks);
            messageA.Merge(new Message(A));                             //Hmmm, can we change this to an immutable style? -LC
            testScheduler.AdvanceBy(TimeSpan.FromSeconds(3).Ticks);

            messageA.IsDeleting.ShouldBe(false);
            keysCollection.ShouldContain(messageA);
        }

        [Fact]
        public void UnsubscribesFromMessageUpdatesAfterDeleteTimeout()
        {
            sut.Start();
            testScheduler.AdvanceBy(MessageAOnNextTick + 2);
            testScheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);
            messageA.IsDeleting = false;
            messageA.Merge(new Message(A)); // This will trigger an update of the message
            testScheduler.AdvanceBy(5);
            testScheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);

            // If we didn't unsubscribe to updates we would have triggered another inner sequence which would try to delete again.
            messageA.IsDeleting.ShouldBe(false);
        }

        static KeyPress A
        {
            get
            {
                return new KeyPress(new ProcessInfo("foo"),
                    new InterceptKeyEventArgs(Keys.A, KeyDirection.Down, false, false, false), false, new[] { "a" });
            }
        }
    }
}
