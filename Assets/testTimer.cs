using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class testTimer : MonoBehaviour
{
    public float elapsedTime = 0f;
    public TextMeshProUGUI timeText;

    void Start()
    {
        timeText.text = "Elapsed Time: " + elapsedTime.ToString("F2") + "s";
    }
    void Update()
    {
        elapsedTime += Time.deltaTime;
        timeText.text = "Elapsed Time: " + elapsedTime.ToString("F2") + "s";
    }
}
