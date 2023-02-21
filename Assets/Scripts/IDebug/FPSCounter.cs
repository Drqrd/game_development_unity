using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    private int avgFrameRate;

    // Update is called once per frame
    void Update()
    {
        float current = 0;
        current = Time.frameCount / Time.time;
        avgFrameRate = (int)current;
        text.text = $"{avgFrameRate.ToString()} FPS";
    }
}
