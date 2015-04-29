using System;
using System.Diagnostics;
using Carnac.Logic.Models;

namespace Carnac.Logic
{
    public class MessageMerger : IMessageMerger
    {
        static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);

        public Message MergeIfNeeded(Message accumulatingMessage, Message newMessage)
        {
            return ShouldCreateNewMessage(accumulatingMessage, newMessage)
                ? newMessage
                : accumulatingMessage.Merge(newMessage);
        }

        static bool ShouldCreateNewMessage(Message previous, Message current)
        {
            var should = previous.ProcessName != current.ProcessName ||
                   current.LastMessage.Subtract(previous.LastMessage) > OneSecond ||
                   !previous.CanBeMerged ||
                   !current.CanBeMerged;

            var x = string.Format("ProcessNameDiffer:{0}, Over1Second:{1}, previousCantBeMerged:{2}, currentCantBeMerged:{3}",
                previous.ProcessName != current.ProcessName,
                current.LastMessage.Subtract(previous.LastMessage) > OneSecond,
                !previous.CanBeMerged,
                !current.CanBeMerged);
            Trace.WriteLine(x);
            return should;
        }
    }
}