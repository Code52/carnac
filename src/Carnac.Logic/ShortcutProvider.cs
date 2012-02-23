using System;
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
        readonly ISettingsService settings;

        private string resharperShortcuts =
            @"R# context actions|Alt+Enter
#Paste multiple	
Code cleanup|Ctrl+E,C
Silent code cleanup|Ctrl+E,F
Symbol code completion|Ctrl+Space
Smart code completion|Ctrl+Alt+Space
Import symbol completion|Shift+Alt+Space
Complete statement|Ctrl+Shift+Enter
Parameter information|Ctrl+Shift+Space
Quick documentation|Ctrl+Shift+F1
Insert live template|Ctrl+E,L
Surround with template|Ctrl+E,U
Generate code|Alt+Insert
Create file from template|Ctrl+Alt+Insert
Move code up|Ctrl+Shift+Alt+Up
Move code down|Ctrl+Shift+Alt+Down
Move code left|Ctrl+Shift+Alt+Left
Move code right|Ctrl+Shift+Alt+Right
Extend selection|Ctrl+Alt+Right
Shrink selection|Ctrl+Alt+Left
Duplicate a line or selection|Ctrl+D
Comment with line comment|Ctrl+Alt+/
Comment with block comment|Ctrl+Shift+/
Inspect this|Ctrl+Shift+Alt+A
Inspection Results window|Ctrl+Alt+V
Turn code analysis on/off|Ctrl+Shift+Alt+8
#Navigation and Search
Find Results window|Ctrl+Alt+F12
Hierarchies window|Ctrl+Alt+H
View type hierarchy|Ctrl+E,H
File structure|Ctrl+Alt+F
To-do items|Ctrl+Alt+D
Browse stack trace|Ctrl+E,T
Locate in Solution Explorer|Shift+Alt+L
View recent files|Ctrl+,
View recent edits|Ctrl+Shift+,
Go to previous edit|Ctrl+Shift+Backspace
Go to related files|Ctrl+Alt+F7
View bookmarks|Ctrl+`
Go to bookmark|Ctrl+[numeric+key]
Set/remove bookmark	Ctrl+Shift+[numeric+key]
Go to type|Ctrl+T
Go to file|Ctrl+Shift+T
Go to file member|Alt+\
Go to symbol|Shift+Alt+T
Navigate to|Alt+`
Go to type of symbol|Ctrl+Shift+F11
Go to declaration|F12
Go to implementation|Ctrl+F12
Go to base symbols|Alt+Home
Go to derived symbols|Alt+End
Go to usage|Shift+Alt+F12
Go to next member/tag|Alt+Down
Go to previous member/tag|Alt+Up
Go to next highlight (error, warning or suggestion)|Alt+Pgdn
Go to previous highlight (error, warning or suggestion)|Alt+Pgup
Go to next error|Shift+Alt+Pgdn
Go to next error in solution|Shift+Alt+Pgdn
Go to previous error|Shift+Alt+Pgup
Go to previous error in solution|Shift+Alt+Pgup
Go to containing declaration|Ctrl+[
Find usages|Shift+F12
Find usages (advanced)|Ctrl+Shift+Alt+F12
Highlight usages in file|Shift+Alt+F11
Go to previous usage|Ctrl+Alt+Pgup
Go to next usage|Ctrl+Alt+Pgdn
#Refactorings
Refactor this|Ctrl+Shift+R
Rename|Ctrl+R,R
Move|Ctrl+R,O
Safe delete|Ctrl+R,D
Safe delete|Alt+Del
Extract method|Ctrl+R,M
Introduce variable|Ctrl+R,V
Introduce field|Ctrl+R,F
Introduce parameter|Ctrl+R,P
Inline variable/method/field|Ctrl+R,I
Encapsulate field|Ctrl+R,E
Change signature|Ctrl+R,S
#Other
Unit Test Explorer|Ctrl+Alt+U
Unit Test Sessions|Ctrl+Alt+T
Close recent tool|Ctrl+Shift+F4
Activate recent tool|Ctrl+Alt+Backspace";

        [ImportingConstructor]
        public ShortcutProvider(ISettingsService settingsService)
        {
            settings = settingsService;

            var shortcutCollection = new ShortcutCollection();
            var parsedShortcuts = resharperShortcuts.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries)
                .Where(s => !s.StartsWith("#"))
                .Select(s =>
                            {
                                var strings = s.Split('|');
                                if (strings.Length != 2)
                                    return null;
                                return new
                                           {
                                               Label = strings[0],
                                               ShortcutKeys = strings[1].Split(',')
                                           };
                            })
                .Where(s => s != null)
                .Select(s => new KeyShortcut(s.Label,
                                             s.ShortcutKeys
                                                 .Select(k =>
                                                             {
                                                                 var key = k.Split('+').Last();
                                                                 var keys = ReplaceKey.ToKey(key);
                                                                 if (keys != null)
                                                                     return
                                                                         new KeyPressDefinition
                                                                             (keys.Value,
                                                                              shiftPressed:k.Contains("Shift"),
                                                                              controlPressed:k.Contains("Ctrl"),
                                                                              altPressed:k.Contains("Alt"));
                                                                 return null;
                                                             })
                                                             .Where(k=>k != null)
                                                 .ToArray())

                );

            foreach (var parsedShortcut in parsedShortcuts)
            {
                shortcutCollection.Add(parsedShortcut);
            }

            shortcuts.Add("devenv", shortcutCollection);
        }

        public IEnumerable<KeyShortcut> GetShortcutsMatching(IEnumerable<KeyPress> keys)
        {
            if (settings.ContainsKey("DetectShortcuts") && !settings.Get<bool>("DetectShortcuts"))
                return Enumerable.Empty<KeyShortcut>();

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