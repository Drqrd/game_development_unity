using UnityEngine;

namespace IHelper
{
    public static class Vector3
    {
        public static UnityEngine.Vector3 Abs(UnityEngine.Vector3 v)
        {
            return new UnityEngine.Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }
    }
}

