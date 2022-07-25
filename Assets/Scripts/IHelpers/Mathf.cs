using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace IHelper
{
    public struct Mathf
    {
        public static bool IsEven(int num)
        {
            return num % 2 == 0;
        }

        public static bool IsOdd(int num)
        {
            return !IsEven(num);
        }
    }
}


