using UnityEngine;

namespace IHelper
{
    public struct Vector3
    {
        public static UnityEngine.Vector3 Abs(UnityEngine.Vector3 v)
        {
            return new UnityEngine.Vector3(UnityEngine.Mathf.Abs(v.x), UnityEngine.Mathf.Abs(v.y), UnityEngine.Mathf.Abs(v.z));
        }
    }
}

