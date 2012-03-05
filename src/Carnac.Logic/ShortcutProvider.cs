using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Carnac.Logic.Models;
using YamlDotNet.RepresentationModel;

namespace Carnac.Logic
{
    [Export(typeof(IShortcutProvider))]
    public class ShortcutProvider : IShortcutProvider
    {
                List<ShortcutCollection> shortcuts = new List<ShortcutCollection>();

        public ShortcutProvider()
        {
            string folder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\Keymaps\";
            string filter = "*.yml";
            string[] files = Directory.GetFiles(folder, filter);

            var yaml = new YamlStream();

            foreach (string file in files)
            {
                yaml.Load(File.OpenText(file));
                var root = yaml.Documents[0].RootNode;

                var collection = root as YamlMappingNode;
                if (collection != null)
                {
                    string group = GetValueByKey(collection, "group");
                    string process = GetValueByKey(collection, "process");
                    var shortcutCollection = new ShortcutCollection { Process = process, Group = group };


                    var groupShortcuts = collection.Children.First(n => n.Key.ToString() == "shortcuts").Value as YamlSequenceNode;

                    foreach (YamlMappingNode entry in groupShortcuts.Children)
                    {
                        string name = GetValueByKey(entry, "name");

                        if (entry.Children.First(n => n.Key.ToString() == "keys").Value as YamlSequenceNode == null)
                            continue;

                        var keys = entry.Children.First(n => n.Key.ToString() == "keys").Value as YamlSequenceNode;

                        foreach (var keyCombo in keys.Children)
                        {
                            List<KeyPressDefinition> definitions = new List<KeyPressDefinition>();
                            string[] combos = keyCombo.ToString().Split(',');
                            foreach (string combo in combos)
                            {
                                var definition = GetKeyPressDefintion(combo);
                                if (definition != null)
                                    definitions.Add(definition);
                            }
                            if(definitions.Count > 0)
                                shortcutCollection.Add(new KeyShortcut(name, definitions.ToArray()));
                        }
                    }

                    shortcuts.Add(shortcutCollection);
                }
            }
        }

        private string GetValueByKey(YamlMappingNode node, string name)
        {
            return node.Children.First(n => n.Key.ToString() == name).Value.ToString();
        }

        private KeyPressDefinition GetKeyPressDefintion(string combo)
        {
            combo = combo.ToLower();
            var key = combo.Split('+').Last();
            var keys = ReplaceKey.ToKey(key);
            if (keys != null)
                return
                    new KeyPressDefinition
                        (keys.Value,
                         shiftPressed: combo.Contains("shift"),
                         controlPressed: combo.Contains("ctrl"),
                         altPressed: combo.Contains("alt"),
                         winkeyPressed: combo.Contains("win"));
            return null;
        }

        public IEnumerable<KeyShortcut> GetShortcutsMatching(IEnumerable<KeyPress> keys)
        {
            var keyPresses = keys.ToArray();
            var processName = keyPresses.Last().Process.ProcessName;
            foreach(var shortcut in shortcuts.Where(s => (s.Process == processName) || string.IsNullOrWhiteSpace(s.Process)))
            {
                var match = shortcut.GetShortcutsMatching(keyPresses);
                if (match.Count() > 0) 
                    return match;
            }

            return Enumerable.Empty<KeyShortcut>();
        }

    }
}