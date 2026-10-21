using UnityEngine;
using UnityEngine.UI;

public class MapUI : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.M;
    public GameObject mapRoot;                  // tan paper panel
    public RectTransform mapArea;               // the world area on the paper
    public RectTransform playerIcon;            // child of mapArea
    public Transform player;

    public Terrain terrain;                     // bounds from terrain
    public Vector2 worldMin = new Vector2(0f, 0f);      // or set corners by hand
    public Vector2 worldMax = new Vector2(100f, 100f);

    private bool markersBuilt = false;

    void Awake()
    {
        if (terrain != null)
        {
            Vector3 origin = terrain.transform.position;
            Vector3 size = terrain.terrainData.size;
            worldMin = new Vector2(origin.x, origin.z);
            worldMax = new Vector2(origin.x + size.x, origin.z + size.z);
        }

        // stop the map stretching
        AspectRatioFitter fitter = mapArea.GetComponent<AspectRatioFitter>();
        if (fitter != null)
        {
            Vector2 worldSize = worldMax - worldMin;
            fitter.aspectRatio = worldSize.x / worldSize.y;
        }

        if (mapRoot == gameObject)
        {
            Debug.LogError("MapUI must sit on an object that stays active, not on mapRoot itself (M would stop working).");
        }
        mapRoot.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            SetOpen(!mapRoot.activeSelf);
        }

        if (mapRoot.activeSelf && player != null)
        {
            PlaceOnMap(playerIcon, player.position);
            playerIcon.localEulerAngles = new Vector3(0f, 0f, -player.eulerAngles.y);
            playerIcon.SetAsLastSibling();      // keep on top of markers
        }
    }

    public void SetOpen(bool open)
    {
        if (open && !markersBuilt)
        {
            BuildMarkers();
        }
        mapRoot.SetActive(open);
    }

    Vector2 WorldToMap(Vector3 worldPos)
    {
        return new Vector2(
            Mathf.InverseLerp(worldMin.x, worldMax.x, worldPos.x),
            Mathf.InverseLerp(worldMin.y, worldMax.y, worldPos.z));
    }

    void PlaceOnMap(RectTransform element, Vector3 worldPos)
    {
        Vector2 p = WorldToMap(worldPos);
        element.anchorMin = p;
        element.anchorMax = p;
        element.anchoredPosition = Vector2.zero;
    }

    void BuildMarkers()
    {
        markersBuilt = true;

        MapMarker[] markers = FindObjectsByType<MapMarker>(FindObjectsSortMode.None);
        foreach (MapMarker marker in markers)
        {
            foreach (Renderer rend in marker.GetComponentsInChildren<Renderer>())
            {
                Bounds b = rend.bounds;

                GameObject shape = new GameObject(rend.name + " (map)", typeof(RectTransform), typeof(Image));
                RectTransform rt = shape.GetComponent<RectTransform>();
                rt.SetParent(mapArea, false);

                // stretch over its footprint
                rt.anchorMin = WorldToMap(b.min);
                rt.anchorMax = WorldToMap(b.max);
                rt.offsetMin = new Vector2(-1f, -1f);   // so thin poles show
                rt.offsetMax = new Vector2(1f, 1f);

                Image img = shape.GetComponent<Image>();
                img.color = marker.color;
                img.raycastTarget = false;
            }
        }
    }
}
