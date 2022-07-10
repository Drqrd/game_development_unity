using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IDebug 
{
    public static class Tally
    {
        public static void LogCount<T>(T[] iterable, bool condition = true, string title = "")
        {
            int count = 0;
            foreach(T element in iterable) if (condition) count++;
            Debug.Log($"--- COUNT - {count} - {title}");
        }

        public static void LogCount<TKey,TValue>(Dictionary<TKey,TValue> iterable, bool condition = true, string title = "")
        {
            int count = 0;
            foreach (KeyValuePair<TKey,TValue> element in iterable)  if(condition) count++;
            Debug.Log($"--- COUNT - {count} - {title}");
        }
    }
}
