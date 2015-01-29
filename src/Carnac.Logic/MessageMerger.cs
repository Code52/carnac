using System;
using System.ComponentModel.Composition;
using Carnac.Logic.Models;

namespace Carnac.Logic
{
    [Export]
    public class MessageMerger : IMessageMerger
    {
        TimeSpan oneSecond;

        public Message MergeIfNeeded(Message acc, Message key)
        {
            if (ShouldCreateNewMessage(acc, key))
                return key;

            return acc.Merge(key);
        }

        bool ShouldCreateNewMessage(Message acc, Message key)
        {
            oneSecond = TimeSpan.FromSeconds(1);
            return acc.ProcessName != key.ProcessName ||
                   key.LastMessage.Subtract(acc.LastMessage) > oneSecond ||
                   acc.IsShortcut ||
                   key.IsShortcut;
        }
    }
}