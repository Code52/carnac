using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Carnac.Logic.Models;

namespace Carnac.Logic
{
    public class ShortcutCollection : Collection<KeyShortcut>
    {
        public IEnumerable<KeyShortcut> GetShortcutsMatching(IEnumerable<KeyPress> keys)
        {
            return this.Where(s => s.StartsWith(keys));
        }
    }
}