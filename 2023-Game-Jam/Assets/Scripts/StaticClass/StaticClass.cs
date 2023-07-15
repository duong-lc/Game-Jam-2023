using System;
using System.Collections.Generic;
using Core.Logging;
using UnityEngine;

namespace StaticClass
{

    public static class CheckLayerMask
    {
        public static bool IsInLayerMask(GameObject obj, LayerMask layerMask)
        {
            return (layerMask.value & (1 << obj.layer)) > 0;
        }
    }

    public static class EnumExtension
    {
        public static T Previous<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) - 1;
            return (0 == j) ? Arr[j] : Arr[^1];
        }

        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }

        public static T Last<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            return Arr[^1];
        }
    }

    public static class DictionaryExtension
    {
        public static Dictionary<T, S> AddRange<T, S>(this Dictionary<T, S> source, Dictionary<T, S> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("Collection is null");
            }

            foreach (var item in collection)
            {
                if(!source.ContainsKey(item.Key)){ 
                    source.Add(item.Key, item.Value);
                }
                else
                {
                    // handle duplicate key issue here
#if UNITY_EDITOR
                    NCLogger.Log($"item.Key {item.Key} & item.Value {item.Value} is duplicated", LogLevel.ERROR);
#endif
                }  
            }

            return source;
        }
    }

}