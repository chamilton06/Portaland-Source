using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// gold menu text that glows when hovered or picked with arrow keys
[RequireComponent(typeof(Button))]
public class MenuButtonGlow : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IDeselectHandler
{
    public Image glow;                                          // soft gold strip behind the text
    public TextMeshProUGUI label;
    public Color normalText = new Color(0.72f, 0.6f, 0.38f);    // dim gold
    public Color hoverText = new Color(1f, 0.9f, 0.65f);        // bright gold
    public float fadeSpeed = 6f;

    private Button button;
    private float target;     // 0 = off, 1 = lit
    private float current;

    void Awake()
    {
        button = GetComponent<Button>();
        if (label == null) label = GetComponentInChildren<TextMeshProUGUI>();
        SetLit(0f);
    }

    // hovering selects it, so only one button is lit at a time
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.interactable) button.Select();
    }

    public void OnSelect(BaseEventData eventData) { target = 1f; }
    public void OnDeselect(BaseEventData eventData) { target = 0f; }

    void Update()
    {
        current = Mathf.MoveTowards(current, button.interactable ? target : 0f, fadeSpeed * Time.unscaledDeltaTime);
        SetLit(current);
    }

    void SetLit(float t)
    {
        if (glow != null)
        {
            Color c = glow.color;
            c.a = t;
            glow.color = c;
        }

        // greyed out ones (continue with no save) stay dark
        Color baseColor = button.interactable ? normalText : normalText * 0.45f;
        label.color = Color.Lerp(baseColor, hoverText, t);
    }
}
