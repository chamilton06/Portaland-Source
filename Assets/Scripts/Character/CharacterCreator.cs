using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// The character creation screen. New Game on the main menu opens it; Begin saves the look and
// loads the game. It builds its own camera, lights, stand and menu when the scene starts, so
// the scene itself only needs this one object.
public class CharacterCreator : MonoBehaviour
{
    public GameObject malePrefab;
    public GameObject femalePrefab;
    public Font font;
    [Tooltip("Scene loaded by Begin.")]
    public string gameScene = "Main Game";
    [Tooltip("Scene loaded by Back.")]
    public string menuScene = "MainMenu";

    // Colour choices for the swatches.
    private static readonly Color[] SkinTints = { Color.white, new Color(1f, 0.93f, 0.86f), new Color(0.95f, 0.85f, 0.78f), new Color(0.85f, 0.75f, 0.68f), new Color(0.9f, 0.9f, 0.95f), new Color(0.78f, 0.72f, 0.7f) };
    private static readonly Color[] EyeColours = { new Color(0.35f, 0.22f, 0.12f), new Color(0.2f, 0.35f, 0.6f), new Color(0.25f, 0.45f, 0.25f), new Color(0.5f, 0.5f, 0.5f), new Color(0.65f, 0.4f, 0.1f), new Color(0.7f, 0.05f, 0.05f) };
    private static readonly Color[] HairColours = { new Color(0.08f, 0.07f, 0.07f), new Color(0.25f, 0.17f, 0.11f), new Color(0.45f, 0.3f, 0.18f), new Color(0.75f, 0.6f, 0.38f), new Color(0.55f, 0.18f, 0.08f), new Color(0.6f, 0.6f, 0.62f), Color.white, new Color(0.35f, 0.05f, 0.08f) };
    private static readonly Color[] OutfitColours = { Color.white, new Color(0.45f, 0.45f, 0.48f), new Color(0.6f, 0.15f, 0.12f), new Color(0.3f, 0.42f, 0.28f), new Color(0.3f, 0.33f, 0.5f), new Color(0.55f, 0.45f, 0.3f), new Color(0.28f, 0.22f, 0.3f), new Color(0.2f, 0.2f, 0.2f) };

    private CharacterLook look;
    private CharacterAppearance male, female;
    private Transform turntable;
    private float spin = -25f;   // Start at a three-quarter view.
    private float lastDrag = -10f;
    private readonly List<Action> refreshers = new List<Action>();
    private GameObject beardRow;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
        look = CharacterLook.LoadSaved() ?? new CharacterLook();

        BuildStage();
        BuildMenu();
        Refresh();
    }

    // ------------------------------------------------------------------------
    // The character on its stand
    // ------------------------------------------------------------------------

    private void BuildStage()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.13f, 0.12f, 0.13f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.04f, 0.03f, 0.035f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 4f;
        RenderSettings.fogEndDistance = 12f;

        Camera camera = new GameObject("Camera").AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.04f, 0.03f, 0.035f);
        camera.fieldOfView = 30f;
        // Shifted sideways so the character stands on the right of the screen, clear of the menu.
        // Looking towards -z, the screen's right is -x, so the camera sits at +x.
        camera.transform.position = new Vector3(1.0f, 1.25f, 5.4f);
        camera.transform.LookAt(new Vector3(1.0f, 0.95f, 0f));
        camera.gameObject.AddComponent<AudioListener>();

        Light key = new GameObject("Key Light").AddComponent<Light>();
        key.type = LightType.Directional;
        key.color = new Color(1f, 0.88f, 0.75f);
        key.intensity = 1.1f;
        key.shadows = LightShadows.Soft;
        key.transform.rotation = Quaternion.Euler(35f, -150f, 0f);

        Light rim = new GameObject("Rim Light").AddComponent<Light>();
        rim.type = LightType.Point;
        rim.color = new Color(0.8f, 0.12f, 0.1f);
        rim.intensity = 3f;
        rim.range = 5f;
        rim.transform.position = new Vector3(0.6f, 2.0f, -1.4f);

        GameObject stand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stand.name = "Stand";
        stand.transform.localScale = new Vector3(1.6f, 0.06f, 1.6f);
        stand.transform.position = new Vector3(0f, -0.06f, 0f);
        stand.GetComponent<Renderer>().material.color = new Color(0.16f, 0.15f, 0.15f);

        turntable = new GameObject("Turntable").transform;
        turntable.rotation = Quaternion.Euler(0f, spin, 0f);
        male = Spawn(malePrefab);
        female = Spawn(femalePrefab);
    }

    private CharacterAppearance Spawn(GameObject prefab)
    {
        if (prefab == null) return null;
        GameObject model = Instantiate(prefab, turntable, false);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        return model.GetComponent<CharacterAppearance>();
    }

    private void Update()
    {
        // Drag to turn the character; it turns slowly by itself otherwise.
        bool overMenu = Input.mousePosition.x < Screen.width * 0.36f;
        if (Input.GetMouseButton(0) && !overMenu)
        {
            spin -= Input.GetAxis("Mouse X") * 6f;
            lastDrag = Time.time;
        }
        else if (Time.time - lastDrag > 2f) spin += 12f * Time.deltaTime;
        turntable.rotation = Quaternion.Euler(0f, spin, 0f);

        // Keep the idle animation playing (the controller also handles falling).
        foreach (CharacterAppearance model in new[] { male, female })
            if (model != null && model.gameObject.activeSelf)
                model.GetComponent<Animator>().SetBool("Grounded", true);
    }

    // Keep the menu and preview in sync after a choice changes.
    // Buttons change the look data; CharacterAppearance shows it on the model.
    private void Refresh()
    {
        if (male != null) male.gameObject.SetActive(look.body == BodyType.Male);
        if (female != null) female.gameObject.SetActive(look.body == BodyType.Female);
        CharacterAppearance shown = look.body == BodyType.Male ? male : female;
        if (shown != null) shown.Apply(look);
        turntable.localScale = new Vector3(look.build, look.height, look.build);
        if (beardRow != null) beardRow.SetActive(look.body == BodyType.Male);
        foreach (Action refresh in refreshers) refresh();
    }

    // ------------------------------------------------------------------------
    // The menu
    // ------------------------------------------------------------------------

    private static readonly Color PanelColour = new Color(0.05f, 0.045f, 0.045f, 0.88f);
    private static readonly Color TextColour = new Color(0.9f, 0.86f, 0.76f);
    private static readonly Color GoldColour = new Color(0.78f, 0.64f, 0.38f);
    private RectTransform list;      // The page rows are being added to.
    private RectTransform[] pages;
    private Button[] tabs;

    private void BuildMenu()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        Canvas canvas = new GameObject("Menu", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 1f;

        RectTransform panel = Box("Panel", canvas.transform, PanelColour);
        Anchor(panel, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(60f, 60f), new Vector2(680f, -60f));
        Text title = Label(panel, "CREATE YOUR CHARACTER", 38, GoldColour, TextAnchor.MiddleCenter);
        Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -90f), new Vector2(-20f, -20f));
        RectTransform line = Box("Line", panel, new Color(GoldColour.r, GoldColour.g, GoldColour.b, 0.5f));
        Anchor(line, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(40f, -96f), new Vector2(-40f, -94f));

        // Two pages, like the Elden Ring creator: Origin and Appearance.
        RectTransform originPage = Page(panel);
        RectTransform appearancePage = Page(panel);
        Button originTab = MakeButton(panel, "ORIGIN", 22, () => ShowPage(0));
        Anchor((RectTransform)originTab.transform, new Vector2(0f, 1f), new Vector2(0.5f, 1f), new Vector2(30f, -150f), new Vector2(-4f, -108f));
        Button appearanceTab = MakeButton(panel, "APPEARANCE", 22, () => ShowPage(1));
        Anchor((RectTransform)appearanceTab.transform, new Vector2(0.5f, 1f), new Vector2(1f, 1f), new Vector2(4f, -150f), new Vector2(-30f, -108f));
        pages = new[] { originPage, appearancePage };
        tabs = new[] { originTab, appearanceTab };

        list = originPage;
        Heading("Name");
        NameField();
        Heading("Origin");
        Choice("Class", () => look.startingClass.ToString(), d => look.startingClass = Step(look.startingClass, d));
        Info(() =>
        {
            Origins.Stats stats = Origins.For(look.startingClass);
            return stats.description + "\nHealth " + stats.health + "    Stamina " + stats.stamina;
        });
        Choice("Keepsake", () => Origins.Describe(look.keepsake), d => look.keepsake = Step(look.keepsake, d));
        Heading("Build");
        Choice("Height", () => SizeName(look.height, "Short", "Tall"), d => look.height = StepSize(look.height, d));
        Choice("Build", () => SizeName(look.build, "Slim", "Broad"), d => look.build = StepSize(look.build, d));

        list = appearancePage;
        Heading("Body");
        Choice("Body", () => look.body.ToString(), d => look.body = Step(look.body, d));
        Choice("Skin", () => look.skin.ToString(), d => look.skin = Step(look.skin, d));
        Swatches("Skin tint", SkinTints, c => look.skinTint = c, () => look.skinTint);
        Swatches("Eyes", EyeColours, c => look.eyeColour = c, () => look.eyeColour);
        Heading("Hair");
        Choice("Style", () => Nice(look.hair.ToString()), d => look.hair = Step(look.hair, d));
        Swatches("Colour", HairColours, c => look.hairColour = c, () => look.hairColour);
        beardRow = Choice("Beard", () => look.beard ? "Yes" : "No", d => look.beard = !look.beard);
        Heading("Outfit");
        Choice("Top", () => look.top.ToString(), d => look.top = Step(look.top, d));
        Choice("Legs", () => look.legs.ToString(), d => look.legs = Step(look.legs, d));
        Choice("Boots", () => look.feet.ToString(), d => look.feet = Step(look.feet, d));
        Choice("Hood", () => look.hood ? "Up" : "Down", d => look.hood = !look.hood);
        Choice("Shoulder pads", () => look.shoulderPads ? "On" : "Off", d => look.shoulderPads = !look.shoulderPads);
        Choice("Scheme", () => (look.outfitScheme + 1).ToString(), d => look.outfitScheme = 1 - look.outfitScheme);
        Swatches("Colour", OutfitColours, c => look.outfitColour = c, () => look.outfitColour);
        ShowPage(0);

        Button begin = MakeButton(panel, "BEGIN", 30, Begin);
        Anchor((RectTransform)begin.transform, new Vector2(0.5f, 0f), new Vector2(1f, 0f), new Vector2(10f, 30f), new Vector2(-30f, 95f));
        Button random = MakeButton(panel, "Randomise", 24, Randomise);
        Anchor((RectTransform)random.transform, new Vector2(0f, 0f), new Vector2(0.5f, 0f), new Vector2(30f, 30f), new Vector2(-10f, 95f));
        Button back = MakeButton(canvas.transform, "Back", 22, Back);
        Anchor((RectTransform)back.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-200f, -100f), new Vector2(-60f, -50f));

        Text hint = Label(canvas.transform, "Drag to turn", 20, new Color(TextColour.r, TextColour.g, TextColour.b, 0.5f), TextAnchor.MiddleCenter);
        Anchor(hint.rectTransform, new Vector2(0.62f, 0f), new Vector2(0.92f, 0f), new Vector2(0f, 40f), new Vector2(0f, 80f));
    }

    private void Heading(string text)
    {
        Text heading = Label(Row(32f), text.ToUpper(), 20, GoldColour, TextAnchor.LowerLeft);
        Anchor(heading.rectTransform, Vector2.zero, Vector2.one, new Vector2(4f, 0f), Vector2.zero);
    }

    // A row with "< value >". Either arrow calls change with -1 or +1.
    // Reuse one row for different options. The callbacks read or change that option.
    // Store a refresher so the text also updates after randomising the appearance.
    private GameObject Choice(string name, Func<string> value, Action<int> change)
    {
        RectTransform row = Row(34f);
        Text label = Label(row, name, 21, TextColour, TextAnchor.MiddleLeft);
        Anchor(label.rectTransform, Vector2.zero, new Vector2(0.45f, 1f), new Vector2(10f, 0f), Vector2.zero);
        Button left = MakeButton(row, "<", 24, () => { change(-1); Refresh(); }, true);
        Anchor((RectTransform)left.transform, new Vector2(0.45f, 0f), new Vector2(0.45f, 1f), new Vector2(0f, 2f), new Vector2(40f, -2f));
        Button right = MakeButton(row, ">", 24, () => { change(1); Refresh(); }, true);
        Anchor((RectTransform)right.transform, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-40f, 2f), new Vector2(0f, -2f));
        Text shown = Label(row, "", 21, TextColour, TextAnchor.MiddleCenter);
        Anchor(shown.rectTransform, new Vector2(0.45f, 0f), new Vector2(1f, 1f), new Vector2(40f, 0f), new Vector2(-40f, 0f));
        refreshers.Add(() => shown.text = value());
        return row.gameObject;
    }

    private void Swatches(string name, Color[] colours, Action<Color> pick, Func<Color> current)
    {
        RectTransform row = Row(34f);
        Text label = Label(row, name, 21, TextColour, TextAnchor.MiddleLeft);
        Anchor(label.rectTransform, Vector2.zero, new Vector2(0.45f, 1f), new Vector2(10f, 0f), Vector2.zero);
        float width = 1f / colours.Length;
        for (int i = 0; i < colours.Length; i++)
        {
            Color colour = colours[i];
            RectTransform frame = Box("Swatch", row, Color.clear);
            Anchor(frame, new Vector2(Mathf.Lerp(0.45f, 1f, i * width), 0f), new Vector2(Mathf.Lerp(0.45f, 1f, (i + 1) * width), 1f), new Vector2(3f, 4f), new Vector2(-3f, -4f));
            Image border = frame.GetComponent<Image>();
            RectTransform fill = Box("Fill", frame, colour);
            Anchor(fill, Vector2.zero, Vector2.one, new Vector2(3f, 3f), new Vector2(-3f, -3f));
            Button button = fill.gameObject.AddComponent<Button>();
            button.onClick.AddListener(() => { pick(colour); Refresh(); });
            refreshers.Add(() => border.color = current() == colour ? GoldColour : Color.clear);
        }
    }

    private RectTransform Page(Transform panel)
    {
        RectTransform page = new GameObject("Page", typeof(RectTransform), typeof(VerticalLayoutGroup)).GetComponent<RectTransform>();
        page.SetParent(panel, false);
        Anchor(page, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(30f, 125f), new Vector2(-30f, -160f));
        VerticalLayoutGroup layout = page.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 2f;
        layout.childControlHeight = false;
        layout.childForceExpandHeight = false;
        return page;
    }

    private void ShowPage(int index)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].gameObject.SetActive(i == index);
            tabs[i].GetComponent<Image>().color = i == index ? new Color(0.3f, 0.24f, 0.16f, 0.95f) : new Color(0.14f, 0.12f, 0.11f, 0.95f);
        }
    }

    // A text box for the character's name.
    private void NameField()
    {
        RectTransform row = Row(40f);
        RectTransform box = Box("Name", row, new Color(0.1f, 0.09f, 0.085f, 1f));
        Anchor(box, Vector2.zero, Vector2.one, new Vector2(6f, 2f), new Vector2(-6f, -2f));
        InputField field = box.gameObject.AddComponent<InputField>();
        Text text = Label(box, "", 24, TextColour, TextAnchor.MiddleCenter);
        text.raycastTarget = true;
        text.supportRichText = false;
        Anchor(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(10f, 0f), new Vector2(-10f, 0f));
        Text placeholder = Label(box, "Type a name", 22, new Color(TextColour.r, TextColour.g, TextColour.b, 0.35f), TextAnchor.MiddleCenter);
        Anchor(placeholder.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        field.textComponent = text;
        field.placeholder = placeholder;
        field.characterLimit = 16;
        field.text = look.characterName;
        field.onValueChanged.AddListener(value => look.characterName = value);
    }

    // A line or two of description that updates with the choices.
    private void Info(Func<string> value)
    {
        Text info = Label(Row(54f), "", 18, new Color(TextColour.r, TextColour.g, TextColour.b, 0.75f), TextAnchor.UpperLeft);
        Anchor(info.rectTransform, Vector2.zero, Vector2.one, new Vector2(14f, 0f), new Vector2(-6f, 0f));
        info.horizontalOverflow = HorizontalWrapMode.Wrap;
        refreshers.Add(() => info.text = value());
    }

    // Height and build go in small steps around normal.
    private static readonly float[] Sizes = { 0.94f, 0.97f, 1f, 1.03f, 1.06f };

    private static float StepSize(float value, int direction)
    {
        int index = System.Array.FindIndex(Sizes, v => Mathf.Approximately(v, value));
        if (index < 0) index = 2;   // Not one of the steps: treat it as average.
        return Sizes[Mathf.Clamp(index + direction, 0, Sizes.Length - 1)];
    }

    private static string SizeName(float value, string small, string big)
    {
        if (value < 0.955f) return "Very " + small.ToLower();
        if (value < 0.985f) return small;
        if (value < 1.015f) return "Average";
        if (value < 1.045f) return big;
        return "Very " + big.ToLower();
    }

    private RectTransform Row(float height)
    {
        RectTransform row = new GameObject("Row", typeof(RectTransform)).GetComponent<RectTransform>();
        row.SetParent(list, false);
        row.sizeDelta = new Vector2(0f, height);
        return row;
    }

    private Button MakeButton(Transform parent, string text, int size, Action onClick, bool plainFont = false)
    {
        RectTransform box = Box(text, parent, new Color(0.14f, 0.12f, 0.11f, 0.95f));
        Button button = box.gameObject.AddComponent<Button>();
        ColorBlock colours = button.colors;
        colours.highlightedColor = new Color(1.6f, 1.4f, 1.2f);
        colours.pressedColor = new Color(0.7f, 0.6f, 0.5f);
        button.colors = colours;
        button.onClick.AddListener(() => onClick());
        Text label = Label(box, text, size, TextColour, TextAnchor.MiddleCenter);
        if (plainFont) label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Anchor(label.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        return button;
    }

    private RectTransform Box(string name, Transform parent, Color colour)
    {
        RectTransform box = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        box.SetParent(parent, false);
        box.GetComponent<Image>().color = colour;
        return box;
    }

    private Text Label(Transform parent, string text, int size, Color colour, TextAnchor alignment)
    {
        Text label = new GameObject("Text", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
        label.transform.SetParent(parent, false);
        label.font = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = text;
        label.fontSize = size;
        label.color = colour;
        label.alignment = alignment;
        label.raycastTarget = false;
        return label;
    }

    private static void Anchor(RectTransform rect, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    // ------------------------------------------------------------------------
    // Choices
    // ------------------------------------------------------------------------

    private static T Step<T>(T value, int direction) where T : Enum
    {
        T[] values = (T[])Enum.GetValues(typeof(T));
        int index = Array.IndexOf(values, value);
        return values[(index + direction + values.Length) % values.Length];
    }

    // "SimpleParted" reads as "Simple Parted".
    private static string Nice(string name) => System.Text.RegularExpressions.Regex.Replace(name, "(?<=[a-z])([A-Z])", " $1");

    private void Randomise()
    {
        T Any<T>() where T : Enum { T[] v = (T[])Enum.GetValues(typeof(T)); return v[UnityEngine.Random.Range(0, v.Length)]; }
        Color Pick(Color[] c) => c[UnityEngine.Random.Range(0, c.Length)];
        // Keep the name, origin and body type the player chose; only the looks are random.
        CharacterLook kept = look;
        look = new CharacterLook
        {
            characterName = kept.characterName,
            startingClass = kept.startingClass,
            keepsake = kept.keepsake,
            height = kept.height,
            build = kept.build,
            body = kept.body,
            skin = Any<SkinTone>(),
            skinTint = Pick(SkinTints),
            eyeColour = Pick(EyeColours),
            hair = Any<Hairstyle>(),
            hairColour = Pick(HairColours),
            beard = UnityEngine.Random.value < 0.4f,
            top = Any<OutfitStyle>(),
            legs = Any<OutfitStyle>(),
            feet = Any<OutfitStyle>(),
            hood = UnityEngine.Random.value < 0.4f,
            shoulderPads = UnityEngine.Random.value < 0.5f,
            outfitScheme = UnityEngine.Random.Range(0, 2),
            outfitColour = Pick(OutfitColours),
        };
        Refresh();
    }

    private void Begin()
    {
        look.startingItemsGiven = false;   // A new game: the class and keepsake items get handed out again.
        look.Save();
        SceneManager.LoadScene(gameScene);
    }

    private void Back() => SceneManager.LoadScene(menuScene);
}
