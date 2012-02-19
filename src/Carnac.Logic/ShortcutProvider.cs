using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;
using Carnac.Logic.Models;

namespace Carnac.Logic
{
    [Export(typeof(IShortcutProvider))]
    public class ShortcutProvider : IShortcutProvider
    {
        readonly Dictionary<string, ShortcutCollection> shortcuts = new Dictionary<string, ShortcutCollection>();

        public ShortcutProvider()
        {
            shortcuts.Add("devenv", new ShortcutCollection
                                         {
                                             new KeyShortcut("Run Tests", 
                                                 new KeyPressDefinition(Keys.R, controlPressed: true),
                                                 new KeyPressDefinition(Keys.T, controlPressed: true))
                                         });
        }

        public IEnumerable<KeyShortcut> GetShortcutsMatching(IEnumerable<KeyPress> keys)
        {
            var keyPresses = keys.ToArray();
            var processName = keyPresses.Last().Process.ProcessName;
            if (shortcuts.ContainsKey(processName))
            {
                var collection = shortcuts[processName];

                return collection.GetShortcutsMatching(keyPresses);
            }

            return Enumerable.Empty<KeyShortcut>();
        }
    }
}