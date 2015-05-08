using System;
using Carnac.Logic.Models;

namespace Carnac.Logic
{
    public class MessageMerger : IMessageMerger
    {
        static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);

        public Message MergeIfNeeded(Message previousMessage, Message newMessage)
        {
            return ShouldCreateNewMessage(previousMessage, newMessage)
                ? newMessage
                : previousMessage.Merge(newMessage);
        }

        static bool ShouldCreateNewMessage(Message previous, Message current)
        {
            return previous.ProcessName != current.ProcessName ||
                   current.LastMessage.Subtract(previous.LastMessage) > OneSecond ||
                   !previous.CanBeMerged ||
                   !current.CanBeMerged;
        }
    }
}