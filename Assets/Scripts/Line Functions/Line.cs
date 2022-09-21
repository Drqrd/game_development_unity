using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LineFunctions {
    public class Line : MonoBehaviour
    {
        [Tooltip("Defines the coefficients of the line, from 0 and up. 0 is considered the y intecept.")]
        [SerializeField] private float[] coefficients;

        public float Evaluate(float x, int order = 0, float value = 0f) {
            value += order == 0 ? coefficients[order] : Mathf.Pow(x, order) * coefficients[order];
            
            if (order < coefficients.Length - 1) {
                return Evaluate(x, order + 1, value);
            }
            else return value;
        }
    }
}
