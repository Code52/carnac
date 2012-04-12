using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Carnac.Logic.Settings
{
    // ReSharper disable InconsistentNaming
    public interface ISettingsProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="freshCopy">If true, does not fetch from cache (useful for isolated editing)</param>
        /// <returns></returns>
        T GetSettings<T>(bool freshCopy = false) where T : new();
        void SaveSettings<T>(T settings);
        IEnumerable<SettingsProvider.SettingDescriptor> ReadSettingMetadata<T>();
        IEnumerable<SettingsProvider.SettingDescriptor> ReadSettingMetadata(Type settingsType);
        T ResetToDefaults<T>() where T : new();
    }

    public interface ISettingsStorage
    {
        string SerializeList(List<string> listOfItems);
        List<string> DeserializeList(string serializedList);
        void Save(string key, Dictionary<string, string> settings);
        Dictionary<string, string> Load(string key);
    }

    public class SettingsProvider : ISettingsProvider
    {
        const string NotConvertableMessage = "Settings provider only supports types that Convert.ChangeType supports. See http://msdn.microsoft.com/en-us/library/dtb69x08.aspx";
        readonly ISettingsStorage settingsRepository;
        readonly Dictionary<Type, object> cache = new Dictionary<Type, object>();

        public SettingsProvider(ISettingsStorage settingsRepository = null)
        {
            this.settingsRepository = settingsRepository ?? new IsolatedStorageSettingsStore();
        }

        public T GetSettings<T>(bool fresh = false) where T : new()
        {
            var type = typeof (T);
            if (!fresh && cache.ContainsKey(type))
                return (T)cache[type];

            var settingsLookup = settingsRepository.Load(GetKey<T>());
            var settings = new T();
            var settingMetadata = ReadSettingMetadata<T>();

            foreach (var setting in settingMetadata)
            {
                // Write over it using the stored value if exists
                var key = GetKey<T>(setting);
                var value = settingsLookup.ContainsKey(key) 
                    ? ConvertValue(settingsLookup[key], setting) 
                    : GetDefaultValue(setting);
                setting.Write(settings, value);
            }

            cache[typeof(T)] = settings;

            return settings;
        }

        object GetDefaultValue(SettingDescriptor setting)
        {
            return setting.DefaultValue ?? ConvertValue(null, setting);
        }

        static string GetKey<T>()
        {
            return typeof(T).Name;
        }

        object ConvertValue(string storedValue, SettingDescriptor setting)
        {
            return ConvertValue(storedValue, setting.UnderlyingType);
        }

        object ConvertValue(string storedValue, Type underlyingType)
        {
            if (underlyingType == typeof(string)) return storedValue;
            var isList = IsList(underlyingType);
            if (isList && string.IsNullOrEmpty(storedValue)) return CreateListInstance(underlyingType);
            if (underlyingType != typeof(string) && string.IsNullOrEmpty(storedValue)) return null;
            if (underlyingType.IsEnum) return Enum.Parse(underlyingType, storedValue, false);
            if (underlyingType == typeof(Guid)) return Guid.Parse(storedValue);
            if (isList) return ReadList(storedValue, underlyingType);

            object converted;
            try
            {
                converted = Convert.ChangeType(storedValue, underlyingType, CultureInfo.InvariantCulture);
            }
            catch (InvalidCastException ex)
            {
                throw new NotSupportedException(NotConvertableMessage, ex);
            }
            catch (FormatException ex)
            {
                throw new NotSupportedException(NotConvertableMessage, ex);
            }

            return converted;
        }

        private object ReadList(string storedValue, Type propertyType)
        {
            var listItemType = propertyType.GetGenericArguments()[0];
            var list = CreateListInstance(propertyType);
            var listInterface = (IList)list;

            var valueList = settingsRepository.DeserializeList(storedValue);

            foreach (var value in valueList)
            {
                listInterface.Add(ConvertValue(value, listItemType));
            }

            return list;
        }

        private static object CreateListInstance(Type propertyType)
        {
            return Activator.CreateInstance(propertyType.IsClass ? propertyType : typeof(List<>).MakeGenericType(propertyType.GetGenericArguments()[0]));
        }

        private static bool IsList(Type propertyType)
        {
            return
                typeof(IList).IsAssignableFrom(propertyType) ||
                (propertyType.IsGenericType && typeof(IList<>) == propertyType.GetGenericTypeDefinition());
        }

        public void SaveSettings<T>(T settingsToSave)
        {
            cache[typeof (T)] = settingsToSave;

            var settings = new Dictionary<string, string>();
            var settingsMetadata = ReadSettingMetadata<T>();

            foreach (var setting in settingsMetadata)
            {
                var value = setting.ReadValue(settingsToSave) ?? setting.DefaultValue;
                if (value == null && setting.UnderlyingType.IsEnum)
                    value = EnumHelper.GetValues(setting.UnderlyingType).First();
                if (IsList(setting.UnderlyingType) && value != null)
                    settings[GetKey<T>(setting)] = WriteList(value);
                else
                    settings[GetKey<T>(setting)] = Convert.ToString(value ?? string.Empty, CultureInfo.InvariantCulture);
            }
            settingsRepository.Save(GetKey<T>(), settings);
        }

        private string WriteList(object value)
        {
            var list = (
                from object item in (IList)value
                select Convert.ToString(item ?? string.Empty, CultureInfo.CurrentCulture))
                .ToList();

            return settingsRepository.SerializeList(list);
        }

        internal static string GetKey<T>(SettingDescriptor setting)
        {
            var settingsType = typeof(T);

            return string.Format("{0}.{1}", settingsType.FullName, setting.Property.Name);
        }

        public IEnumerable<SettingDescriptor> ReadSettingMetadata<T>()
        {
            return ReadSettingMetadata(typeof(T));
        }

        public IEnumerable<SettingDescriptor> ReadSettingMetadata(Type settingsType)
        {
            return settingsType.GetProperties()
                .Where(x => x.CanRead && x.CanWrite)
                .Select(x => new SettingDescriptor(x))
                .ToArray();
        }

        public T ResetToDefaults<T>() where T : new()
        {
            settingsRepository.Save(GetKey<T>(), new Dictionary<string, string>());

            var type = typeof (T);
            if (cache.ContainsKey(type))
            {
                var cachedCopy = cache[type];
                var settingMetadata = ReadSettingMetadata<T>();

                foreach (var setting in settingMetadata)
                {
                    setting.Write(cachedCopy, GetDefaultValue(setting));
                }

                return (T)cachedCopy;
            }

            return GetSettings<T>();
        }

        public class SettingDescriptor : INotifyPropertyChanged
        {
            readonly PropertyInfo property;

            public SettingDescriptor(PropertyInfo property)
            {
                this.property = property;
                DisplayName = property.Name;

                ReadAttribute<DefaultValueAttribute>(d => DefaultValue = d.Value);
                ReadAttribute<DescriptionAttribute>(d => Description = d.Description);
                ReadAttribute<DisplayNameAttribute>(d => DisplayName = d.DisplayName);
            }

            void ReadAttribute<TAttribute>(Action<TAttribute> callback)
            {
                var instances = property.GetCustomAttributes(typeof(TAttribute), true).OfType<TAttribute>();
                foreach (var instance in instances)
                {
                    callback(instance);
                }
            }

            public PropertyInfo Property
            {
                get { return property; }
            }

            public object DefaultValue { get; private set; }

            public string Description { get; private set; }

            public string DisplayName { get; private set; }

            public void Write(object settings, object value)
            {
                property.SetValue(settings, value, null);
            }

            /// <summary>
            /// If the property type is nullable, returns the type. i.e int? returns int
            /// </summary>
            public Type UnderlyingType
            {
                get
                {
                    if (Property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        return property.PropertyType.GetGenericArguments()[0];
                    return property.PropertyType;
                }
            }

            public object ReadValue(object settings)
            {
                return property.GetValue(settings, null);
            }

#pragma warning disable 67
            public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
        }
    }

    public static class EnumHelper
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return GetValues(typeof(T))
                .OfType<T>();
        }

        public static IEnumerable<object> GetValues(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException("enumType must be an Enum type", "enumType");

            return enumType
                .GetFields()
                .Where(field => field.IsLiteral)
                .Select(field => field.GetValue(enumType));
        }
    }

    // ReSharper restore InconsistentNaming
}
