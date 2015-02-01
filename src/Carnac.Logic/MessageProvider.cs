using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Carnac.Logic.Models;

namespace Carnac.Logic
{
    [Export(typeof(IMessageProvider))]
    public class MessageProvider : IMessageProvider
    {
        readonly IShortcutProvider shortcutProvider;
        readonly PopupSettings settings;
        readonly IMessageMerger messageMerger;

        [ImportingConstructor]
        public MessageProvider(IShortcutProvider shortcutProvider, PopupSettings settings, IMessageMerger messageMerger)
        {
            this.shortcutProvider = shortcutProvider;
            this.messageMerger = messageMerger;
            this.settings = settings;
        }

        public IObservable<Message> GetMessageStream(IObservable<KeyPress> keyStream)
        {
            /*
            shortcut Acc stream:
               - ! before item means HasCompletedValue is false
               - [1 & 2] means multiple messages are returned from get messages (1 and 2 in this case), others are a single message returned

            message merger:
               - * before items indicates the previous message has been modified (key has been merged into acc), otherwise new acc is created

            keystream   :  a---b---ctrl+r----ctrl+r----------ctrl+r----a--------------↓---↓
            shortcut Acc:  a---b---!ctrl+r---ctrl+r,ctrl+r---!ctrl+r---[ctrl+r & a]---↓---↓
            sel many    :  a---b-------------ctrl+r,ctrl+r-------------ctrl+r---a-----↓---↓
            msg merger  :  a---*ab-----------ctrl+r,ctrl+r-------------ctrl+r---a-----↓---*'↓ x2'
            */
            return keyStream
                .Scan(new ShortcutAccumulator(), (acc, key) => acc.ProcessKey(shortcutProvider, key))
                .Where(c => c.HasCompletedValue)
                .SelectMany(c => c.GetMessages())
                .Scan(new Message(), (acc, key) => messageMerger.MergeIfNeeded(acc, key))
                .DistinctUntilChanged()
                .Where(m => !settings.DetectShortcutsOnly || m.IsShortcut);
        }
    }
}