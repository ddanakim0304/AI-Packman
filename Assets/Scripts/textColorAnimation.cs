using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class textColorAnimation : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    public float colorSpeed = 1f;
    private float hue = 0f;

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        // Increment hue value over time
        hue = (hue + colorSpeed * Time.deltaTime) % 1f;
        
        // Convert HSV to RGB (using hue value for rainbow effect)
        Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);
        
        // Apply the color to the text
        textMesh.color = rainbowColor;
    }

}
