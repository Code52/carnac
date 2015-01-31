using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
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
        IObservable<Message> messageStream;

        public KeysControllerFacts()
        {
            testScheduler = new TestScheduler();
            var messageProvider = Substitute.For<IMessageProvider>();
            messageProvider.GetMessageStream(Arg.Any<IObservable<KeyPress>>()).Returns(_ => messageStream);
            var keyProvider = Substitute.For<IKeyProvider>();
            var concurrencyService = Substitute.For<IConcurrencyService>();
            concurrencyService.UiScheduler.Returns(testScheduler);
            concurrencyService.Default.Returns(testScheduler);
            sut = new KeysController(keysCollection, messageProvider, keyProvider, concurrencyService);
        }

        [Fact]
        public void MessagesAreAddedIntoKeysColletion()
        {
            var message = new Message(A);
            messageStream = Observable.Return(message);

            sut.Start();
            testScheduler.AdvanceBy(5); // Push it a long a few ticks to allow query to execute

            keysCollection.ShouldContain(message);
            message.IsDeleting.ShouldBe(false);
        }

        [Fact]
        public void MessagesAreFlaggedAsDeletingAfter5Seconds()
        {
            var message = new Message(A);
            messageStream = Observable.Return(message);

            sut.Start();
            testScheduler.AdvanceBy(5); // Push it a long a few ticks to allow query to execute
            testScheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);

            message.IsDeleting.ShouldBe(true);
        }

        [Fact]
        public void MessagesIsRemovedAfter6Seconds()
        {
            var message = new Message(A);
            messageStream = Observable.Return(message);

            sut.Start();
            testScheduler.AdvanceBy(5); // Push it a long a few ticks to allow query to execute
            testScheduler.AdvanceBy(TimeSpan.FromSeconds(6).Ticks);
            
            keysCollection.ShouldNotContain(message);
        }

        [Fact]
        public void MessageTimeoutIsStartedAgainIfMessageIsUpdated()
        {
            var message = new Message(A);
            messageStream = Observable.Return(message);

            sut.Start();
            testScheduler.AdvanceBy(5); // Push it a long a few ticks to allow query to execute
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
            messageStream = Observable.Return(message);

            sut.Start();
            testScheduler.AdvanceBy(5);
            testScheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);
            message.IsDeleting = false;
            message.Merge(new Message(A)); // This will trigger an update of the message
            testScheduler.AdvanceBy(5);
            testScheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);

            // If we didn't unsubscribe to updates we would have triggered another inner sequence which would try to delete again.
            message.IsDeleting.ShouldBe(false);
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
