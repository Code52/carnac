using System.Collections.Generic;
using System.ComponentModel.Composition;
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
                                             new KeyShortcut(new KeyPressDefinition(Keys.R, controlPressed: true),
                                                             new KeyPressDefinition(Keys.T, controlPressed: true))
                                         });
        }

        public bool IsChord(IEnumerable<KeyPress> keys, KeyPress newKeyPress)
        {
            var processName = newKeyPress.Process.ProcessName;
            if (shortcuts.ContainsKey(processName))
            {
                var collection = shortcuts[processName];

                return collection.ContainsShortcut(keys, newKeyPress);
            }

            return false;
        }
    }
}