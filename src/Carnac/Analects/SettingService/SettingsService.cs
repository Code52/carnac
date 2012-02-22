using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Formatters.Binary;

namespace Analects.SettingsService
{
    public class SettingsService : ISettingsService
    {
        private IsolatedStorageScope scope = IsolatedStorageScope.Assembly | IsolatedStorageScope.User | IsolatedStorageScope.Roaming;

        private const string filename = "settings.bin";
        private Dictionary<string, object> storage = new Dictionary<string, object>();

        public SettingsService()
        {
            storage = new Dictionary<string, object>();

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(scope, null, null))
            {
                string[] filenames = isoStore.GetFileNames(filename);
                if (filename.Length > 0)
                {
                    LoadStorage(isoStore);
                }
            }
        }

        public bool ContainsKey(string key)
        {
            return storage.ContainsKey(key);
        }

        public T Get<T>(string key)
        {
            if (storage.ContainsKey(key))
                return (T)storage[key];
            return default(T);
        }

        public void Set<T>(string key, T value)
        {
            if (!storage.ContainsKey(key))
                storage.Add(key, value);
            else
                storage[key] = value;

        }

        private void LoadStorage(IsolatedStorageFile isoStore)
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();

                using (var stream = new IsolatedStorageFileStream(filename, FileMode.Open, isoStore))
                    storage = (Dictionary<string, object>)formatter.Deserialize(stream);
            }
            catch //(Exception e)
            {
                // TODO Log Error
            }
        }

        public void Save()
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(scope, null, null))
            using (var stream = new IsolatedStorageFileStream(filename, FileMode.Create, isoStore))
                formatter.Serialize(stream, storage);
        }
    }
}