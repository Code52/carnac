using System;
using Carnac.Logic.Models;

namespace Carnac.Logic
{
    public class MessageMerger : IMessageMerger
    {
        static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);

        public Message MergeIfNeeded(Message accumulatingMessage, Message newMessage)
        {
            return ShouldCreateNewMessage(accumulatingMessage, newMessage) ? newMessage : accumulatingMessage.Merge(newMessage);
        }

        static bool ShouldCreateNewMessage(Message acc, Message key)
        {
            return acc.ProcessName != key.ProcessName ||
                   key.LastMessage.Subtract(acc.LastMessage) > OneSecond ||
                   !acc.CanBeMerged ||
                   !key.CanBeMerged;
        }
    }
}