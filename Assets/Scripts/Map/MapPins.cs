using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// click the map to drop a pin, right click a pin to remove it
public class MapPins : MonoBehaviour, IPointerClickHandler
{
    public RectTransform mapArea;
    public RectTransform content;                                   // MapContent, to read the zoom
    public Sprite[] pinIcons;                                       // swords, bush, ?, location, skull, chest
    public Button[] pickerButtons;                                  // same order as pinIcons

    public float pinSize = 36f;
    public float zoomGrow = 0.5f;   // 0 = same size at any zoom, 1 = grows fully with the map

    private readonly List<RectTransform> pins = new List<RectTransform>();
    private RectTransform pinLayer;
    private int selected;

    void Awake()
    {
        // own layer so pins sit above the forests
        pinLayer = new GameObject("Pins", typeof(RectTransform)).GetComponent<RectTransform>();
        pinLayer.SetParent(mapArea, false);
        pinLayer.anchorMin = Vector2.zero;
        pinLayer.anchorMax = Vector2.one;
        pinLayer.offsetMin = Vector2.zero;
        pinLayer.offsetMax = Vector2.zero;

        for (int i = 0; i < pickerButtons.Length; i++)
        {
            int index = i;
            pickerButtons[i].onClick.AddListener(() => Select(index));
        }
        Select(0);
    }

    // keep pins the same size whatever the zoom
    void LateUpdate()
    {
        float zoom = content.localScale.x;
        float size = Mathf.Pow(zoom, zoomGrow) / zoom;   // grows a bit when zoomed in, not as much as the trees
        foreach (RectTransform pin in pins)
        {
            pin.localScale = new Vector3(size, size, 1f);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging) return;   // that was a drag, not a click

        // clicked an existing pin
        GameObject hit = eventData.pointerCurrentRaycast.gameObject;
        RectTransform hitPin = hit != null ? hit.transform as RectTransform : null;
        if (hitPin != null && pins.Contains(hitPin))
        {
            if (eventData.button == PointerEventData.InputButton.Right) RemovePin(hitPin);
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left) AddPin(eventData);
    }

    void AddPin(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(mapArea, eventData.position, eventData.pressEventCamera, out Vector2 local)) return;
        if (!mapArea.rect.Contains(local)) return;   // clicked the paper outside the world
        Vector2 point = Rect.PointToNormalized(mapArea.rect, local);

        GameObject go = new GameObject("Pin", typeof(RectTransform), typeof(Image));
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(pinLayer, false);
        rt.anchorMin = point;
        rt.anchorMax = point;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(pinSize, pinSize);

        Image img = go.GetComponent<Image>();
        img.sprite = pinIcons[selected];

        img.preserveAspect = true;   // raycast stays on so right click can find it

        pins.Add(rt);
    }

    void RemovePin(RectTransform pin)
    {
        pins.Remove(pin);
        Destroy(pin.gameObject);
    }

    void Select(int index)
    {
        selected = index;
    }
}