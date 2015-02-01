using System;
using System.ComponentModel.Composition;
using Carnac.Logic.Models;

namespace Carnac.Logic
{
    [Export(typeof(IMessageMerger))]
    public class MessageMerger : IMessageMerger
    {
        static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);

        public Message MergeIfNeeded(Message acc, Message key)
        {
            return ShouldCreateNewMessage(acc, key) ? key : acc.Merge(key);
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