using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GMTK2020.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public class FriendlyNameAttribute : Attribute
    {
        public readonly string Name;

        public FriendlyNameAttribute(string name) => Name = name;

        public static IEnumerable<string> GetNames<T>(bool requireFriendlyName = true, Dictionary<int, T> map = null) where T : Enum
        {
            T[] items = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
            List<string> values = new List<string>();

            foreach (T item in items)
            {
                MemberInfo info = item.GetType().GetMember(item.ToString()).SingleOrDefault();
                if (info == null)
                    continue;

                FriendlyNameAttribute fn = (FriendlyNameAttribute) info.GetCustomAttributes(typeof(FriendlyNameAttribute)).SingleOrDefault();

                string name;

                if (fn == null)
                {
                    if (requireFriendlyName)
                        continue;

                    name = info.Name;
                }
                else
                    name = fn.Name;

                map?.Add(values.Count, item);
                values.Add(name);
            }


            return values;
        }
    }
}