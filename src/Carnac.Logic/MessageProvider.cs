using System;
using System.Reactive.Linq;
using Carnac.Logic.Models;

namespace Carnac.Logic
{
    public class MessageProvider : IMessageProvider
    {
        readonly IShortcutProvider shortcutProvider;
        readonly IKeyProvider keyProvider;
        readonly PopupSettings settings;
        readonly IMessageMerger messageMerger;

        public MessageProvider(IShortcutProvider shortcutProvider, IKeyProvider keyProvider, PopupSettings settings, IMessageMerger messageMerger)
        {
            this.shortcutProvider = shortcutProvider;
            this.keyProvider = keyProvider;
            this.messageMerger = messageMerger;
            this.settings = settings;
        }

        //TODO: Kind of an anti pattern (IMO) to pass in an Observable Sequence to a method. Either inject via DI the source of the sequence, or push the values at this object with concrete method calls. -LC
        public IObservable<Message> GetMessageStream()
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
            return keyProvider.GetKeyStream()
                .Scan(new ShortcutAccumulator(), (acc, key) => acc.ProcessKey(shortcutProvider, key))
                .Where(c => c.HasCompletedValue)
                .SelectMany(c => c.GetMessages())
                .Scan(new Message(), (acc, key) => messageMerger.MergeIfNeeded(acc, key))
                .DistinctUntilChanged()
                .Where(m => !settings.DetectShortcutsOnly || m.IsShortcut);
        }
    }
}