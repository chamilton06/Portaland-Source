using System.Collections;
using UnityEngine;

// spins the sun round, fades light + ambient, moon at night
// other scripts can check DayNight.IsNight or listen to NightChanged
public class DayNight : MonoBehaviour
{
    public static DayNight Instance { get; private set; }
    public static bool IsNight => Instance != null && Instance.night;
    public static float Hour => Instance != null ? Instance.timeOfDay : 12f;
    public static event System.Action<bool> NightChanged;   // true = night just started

    public Light sun;                       // your directional light
    public Light moon;                      // optional 2nd directional light, made for you if empty
    public float dayLengthMinutes = 12f;    // real minutes for a full 24h
    [Range(0f, 24f)] public float timeOfDay = 8f;   // start hour
    public float sunYaw = 30f;              // which way the sun travels across the sky
    public KeyCode skipHourKey = KeyCode.F8;   // testing, jumps 1 hour

    public float sunIntensity = 1.1f;
    public Color dayColor = new Color(1f, 0.96f, 0.86f);
    public Color sunsetColor = new Color(1f, 0.55f, 0.3f);
    public float moonIntensity = 0.25f;
    public Color moonColor = new Color(0.55f, 0.65f, 1f);

    // ambient light, sky/horizon/ground, day vs night
    public Color daySky = new Color(0.55f, 0.62f, 0.72f);
    public Color dayEquator = new Color(0.4f, 0.42f, 0.4f);
    public Color dayGround = new Color(0.2f, 0.18f, 0.15f);
    public Color nightSky = new Color(0.06f, 0.08f, 0.16f);
    public Color nightEquator = new Color(0.04f, 0.05f, 0.09f);
    public Color nightGround = new Color(0.02f, 0.02f, 0.03f);

    bool night;

    void Awake()
    {
        Instance = this;
        if (sun == null) sun = RenderSettings.sun;
        if (sun == null)
        {
            Debug.LogError("DayNight: drag the Directional Light into Sun");
            enabled = false;
            return;
        }
        RenderSettings.sun = sun;   // sky + fireflies follow this one
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;

        if (moon == null)
        {
            moon = new GameObject("Moon").AddComponent<Light>();
            moon.type = LightType.Directional;
            moon.shadows = LightShadows.Soft;
            moon.transform.SetParent(transform, false);
        }
        moon.color = moonColor;

        Apply();
        night = SunHeight() < 0.05f;
    }

    void Update()
    {
        timeOfDay += 24f / (dayLengthMinutes * 60f) * Time.deltaTime;
        if (Input.GetKeyDown(skipHourKey)) timeOfDay += 1f;
        if (timeOfDay >= 24f) timeOfDay -= 24f;

        Apply();

        bool nowNight = SunHeight() < 0.05f;
        if (nowNight != night)
        {
            night = nowNight;
            Debug.Log(night ? "night falls" : "sun is up");
            NightChanged?.Invoke(night);
        }
    }

    void Apply()
    {
        // 6 = sunrise, 12 = noon, 18 = sunset, 0 = midnight
        float angle = timeOfDay / 24f * 360f - 90f;
        sun.transform.rotation = Quaternion.Euler(angle, sunYaw, 0f);
        moon.transform.rotation = Quaternion.Euler(angle + 180f, sunYaw, 0f);

        float h = SunHeight();                        // 1 noon, 0 horizon, -1 midnight
        float dayAmount = Mathf.Clamp01((h + 0.1f) * 4f);
        float high = Mathf.Clamp01(h * 2f);           // 0 at sunset, 1 when high up

        sun.intensity = Mathf.Clamp01(h * 4f) * sunIntensity;
        sun.color = Color.Lerp(sunsetColor, dayColor, high);
        sun.enabled = sun.intensity > 0.01f;

        moon.intensity = Mathf.Clamp01(-h * 4f) * moonIntensity;
        moon.enabled = moon.intensity > 0.01f;

        RenderSettings.ambientSkyColor = Color.Lerp(nightSky, daySky, dayAmount);
        RenderSettings.ambientEquatorColor = Color.Lerp(nightEquator, dayEquator, dayAmount);
        RenderSettings.ambientGroundColor = Color.Lerp(nightGround, dayGround, dayAmount);
        if (RenderSettings.fog) RenderSettings.fogColor = Color.Lerp(nightSky, daySky, dayAmount);
    }

    float SunHeight()
    {
        return -sun.transform.forward.y;
    }
}