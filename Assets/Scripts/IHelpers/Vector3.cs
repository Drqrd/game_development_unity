using UnityEngine;

namespace IHelper
{
    public struct Vector3
    {
        public static UnityEngine.Vector3 Abs(UnityEngine.Vector3 v)
        {
            return new UnityEngine.Vector3(UnityEngine.Mathf.Abs(v.x), UnityEngine.Mathf.Abs(v.y), UnityEngine.Mathf.Abs(v.z));
        }

        public static UnityEngine.Vector3 Flatten(UnityEngine.Vector3 v, int axis = 2) {
            switch (axis) {
                    case 0:
                        return new UnityEngine.Vector3(0f, v.y, v.z);
                    case 1:
                        return new UnityEngine.Vector3(v.x, 0f, v.z);
                    case 2:
                        return new UnityEngine.Vector3(v.x, v.y, 0f);
                    default:
                        Debug.LogError("Error flattening Vector3. Axis can only be 0,1,2 (x,y,z)");
                        return v;
            }
        }
    }
}

