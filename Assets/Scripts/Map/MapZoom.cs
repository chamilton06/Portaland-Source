using UnityEngine;
using UnityEngine.EventSystems;

// scroll to zoom toward the mouse, drag to move the map
public class MapZoom : MonoBehaviour, IScrollHandler, IDragHandler
{
    public RectTransform content;        // holds MapArea, this is what zooms and moves
    public RectTransform playerIcon;     // centred on open + kept the same size
    public float startZoom = 2f;
    public float minZoom = 1f;
    public float maxZoom = 5f;
    public float zoomStep = 0.15f;       // per scroll notch

    private RectTransform view;
    private float zoom = 1f;
    private bool centreNextFrame;

    void Awake()
    {
        view = (RectTransform)transform;
    }

    // every time the map opens
    void OnEnable()
    {
        zoom = startZoom;
        content.anchoredPosition = Vector2.zero;
        Apply();
        centreNextFrame = true;   // wait a frame so MapUI has placed the arrow
    }

    void LateUpdate()
    {
        if (!centreNextFrame || playerIcon == null) return;
        centreNextFrame = false;

        Vector2 local = content.InverseTransformPoint(playerIcon.position);
        content.anchoredPosition = -local * zoom;
        Clamp();
    }

    public void OnScroll(PointerEventData eventData)
    {
        // keep the point under the mouse in place while zooming
        RectTransformUtility.ScreenPointToLocalPointInRectangle(content, eventData.position, eventData.enterEventCamera, out Vector2 before);
        zoom = Mathf.Clamp(zoom * (1f + eventData.scrollDelta.y * zoomStep), minZoom, maxZoom);
        Apply();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(content, eventData.position, eventData.enterEventCamera, out Vector2 after);
        content.anchoredPosition += (after - before) * zoom;
        Clamp();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;   // right click is for pins later
        content.anchoredPosition += eventData.delta / view.lossyScale.x;
        Clamp();
    }

    void Apply()
    {
        content.localScale = new Vector3(zoom, zoom, 1f);
        if (playerIcon != null) playerIcon.localScale = new Vector3(1f / zoom, 1f / zoom, 1f);   // arrow stays readable
    }

    // dont let the map slide off the edge
    void Clamp()
    {
        Vector2 limit = view.rect.size * (zoom - 1f) * 0.5f;
        Vector2 pos = content.anchoredPosition;
        content.anchoredPosition = new Vector2(Mathf.Clamp(pos.x, -limit.x, limit.x), Mathf.Clamp(pos.y, -limit.y, limit.y));
    }
}