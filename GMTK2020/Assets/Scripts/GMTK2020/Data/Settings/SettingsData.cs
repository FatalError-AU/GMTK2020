using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

namespace GMTK2020.Data.Settings
{
    public static class SettingsData
    {
        private static Type[] _Providers;
        private static readonly Dictionary<Type, object> Values = new Dictionary<Type, object>();

        public static T Get<T>() where T : class => (T) Values[typeof(T)];

        public static void Reset<T>() where T : class
        {
            if (Values.ContainsKey(typeof(T)))
                Values.Remove(typeof(T));

            object inst = CreateInstance(typeof(T));
            Values.Add(typeof(T), inst);
        }

        public static void Flush()
        {
            foreach (Type provider in _Providers)
            {
                if (!Values.ContainsKey(provider))
                    continue;

                PlayerPrefs.SetString("settings:" + provider.FullName, JsonConvert.SerializeObject(Values[provider]));
            }

            PlayerPrefs.Save();
        }

        private static object CreateInstance(Type t)
        {
            ConstructorInfo constructor = t.GetConstructor(new Type[0]);

            if (constructor == null)
                throw new ArgumentException("Error: " + nameof(t) + " does not contain an empty constructor");

            return constructor.Invoke(new object[0]);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            IEnumerable<Type> discoveredProviders =
                    from assembly in AppDomain.CurrentDomain.GetAssemblies()
                    from t in assembly.GetTypes()
                    let attrib = t.GetCustomAttribute<SettingsProviderAttribute>(false)
                    where attrib != null
                    select t;
            _Providers = discoveredProviders.ToArray();

            foreach (Type provider in _Providers)
            {
                object obj = null;

                try
                {
                    if (PlayerPrefs.HasKey("settings:" + provider.FullName))
                        obj = JsonConvert.DeserializeObject(PlayerPrefs.GetString("settings:" + provider.FullName), provider);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.StackTrace);
                }

                if (obj == null)
                    obj = CreateInstance(provider);

                if (obj != null)
                    Values.Add(provider, obj);
            }

            Application.quitting += Flush;
        }
    }
}