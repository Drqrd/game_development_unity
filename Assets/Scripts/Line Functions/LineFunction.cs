using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

namespace LineFunctions {
    public class LineFunction : MonoBehaviour
    {
        [Tooltip("Actual functions")]
        [SerializeField] private Line[] lines;
        [Tooltip("Transition points define when the next function is used to evaluate the point, x <= transition point.")]
        [SerializeField] private float[] transitionPoints;
        [Tooltip("Value to test")]
        [SerializeField] private float testValue;

        private void Start() {
            Validate();

            // Adds infinity as the last check, which will always pass
            List<float> newTp = transitionPoints.ToList();
            newTp.Add(float.PositiveInfinity);
            transitionPoints = newTp.ToArray();
        }

        private void Validate() {
            if (transitionPoints.Length != lines.Length - 1) {
                Debug.LogError("WARNING: For every extra line defined, there needs to be a transition point.");
            }
        }

        public float Evaluate(float x, int transitionCheck = 0) {
            if (transitionPoints.Length == 0) {
                return lines[0].Evaluate(x);
            }
            else if ( x <= transitionPoints[transitionCheck]) {
                return lines[transitionCheck].Evaluate(x);
            }
            else {
                return Evaluate(x, transitionCheck  + 1);
            }
        }

        public void TestValue() {
            Debug.Log($"X: {testValue}, Y: {Evaluate(testValue)}");
        }
    }
}
