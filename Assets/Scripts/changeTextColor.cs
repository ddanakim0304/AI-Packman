using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Add this

public class ButtonTextColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler // Add interfaces
{
    public TextMeshProUGUI buttonText;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color clickedColor = Color.red;

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        buttonText.color = normalColor;
        button.onClick.AddListener(() => OnClick());
    }

    // Implement interface methods
    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonText.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonText.color = normalColor;
    }

    public void OnClick()
    {
        buttonText.color = clickedColor;
    }
}