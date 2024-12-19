using TMPro;
using UnityEngine;

public class animateFont : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private float originalSize;
    public float sizeVariation = 10f; // How much the size varies
    public float animationSpeed = 2f; // Controls animation speed

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        originalSize = textMesh.fontSize;
    }

    void Update()
    {
        // Use sine wave to create smooth size animation
        float newSize = originalSize + (Mathf.Sin(Time.time * animationSpeed) * sizeVariation);
        textMesh.fontSize = newSize;
    }
}