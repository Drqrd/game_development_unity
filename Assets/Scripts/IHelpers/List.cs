using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IHelper
{
    public struct List<T> 
    {
        public static void Log(System.Collections.Generic.List<T> l)
        {
            foreach(T entry in l)
            {
                Debug.Log(entry);
            }
        }
    }
}