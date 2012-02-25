using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;
using Carnac.Logic.Models;
using Carnac.Logic.Internal;
using System.IO;
using System.Diagnostics;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace Carnac.Logic
{
    [Export(typeof(IShortcutProvider))]
    public class FileShortcutProvider : IShortcutProvider
    {
        List<ShortcutCollection> shortcuts = new List<ShortcutCollection>();

        public FileShortcutProvider()
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
                        List<KeyPressDefinition> definitions = new List<KeyPressDefinition>();

                        if (entry.Children.First(n => n.Key.ToString() == "keys").Value as YamlSequenceNode == null)
                            continue;

                        var keys = entry.Children.First(n => n.Key.ToString() == "keys").Value as YamlSequenceNode;

                        foreach (var keyCombo in keys.Children)
                        {
                            string combo = keyCombo.ToString();
                            var definition = GetKeyPressDefintion(combo);
                            definitions.Add(definition);
                        }

                        shortcutCollection.Add(new KeyShortcut(name, definitions.ToArray()));
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
            var key = combo.Split('+').Last();
            var keys = ReplaceKey.ToKey(key);
            if (keys != null)
                return
                    new KeyPressDefinition
                        (keys.Value,
                         shiftPressed: combo.Contains("Shift"),
                         controlPressed: combo.Contains("Ctrl"),
                         altPressed: combo.Contains("Alt"));
            return null;
        }

        public IEnumerable<KeyShortcut> GetShortcutsMatching(IEnumerable<KeyPress> keys)
        {
            var keyPresses = keys.ToArray();
            var processName = keyPresses.Last().Process.ProcessName;
            var shortcut = shortcuts.FirstOrDefault(s => s.Process == processName);

            if (shortcut != null)
            {
                return shortcut.GetShortcutsMatching(keyPresses);
            }

            return Enumerable.Empty<KeyShortcut>();
        }
    }
}