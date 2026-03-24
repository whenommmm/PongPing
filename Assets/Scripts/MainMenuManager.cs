using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drop this on any GameObject in the MainMenu scene.
/// Builds and styles the entire UI at runtime — no manual Canvas setup needed.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    // ── PlayerPrefs keys ───────────────────────────────────────────────────────
    private const string PrefDifficulty = "Difficulty";
    private const string PrefHighScore  = "HighScore";

    private int   selectedDifficulty = 1;
    private Image easyImg, mediumImg, hardImg;

    [Header("Background")]
    [Tooltip("Assign your background sprite here.")]
    public Sprite backgroundSprite;

    [Header("Font")]
    [Tooltip("Assign your imported Press Start 2P (or any TMP font asset) here.")]
    public TMP_FontAsset arcadeFont; // leave empty to use TMP default

    // ── Shared rounded sprite (white, recoloured per element) ──────────────────
    private Sprite _roundedSprite;
    private const int RoundRadius = 10; // pixels in the 128×128 source texture

    // ── Palette — Custom Warm/Dark Theme ───────────────────────────────────────
    private static readonly Color BgColor      = new Color(0.153f, 0.188f, 0.235f); // #27303C
    private static readonly Color CardColor    = new Color(0.94f, 0.93f, 0.88f, 0.98f); // dull off-white cream
    private static readonly Color AccentColor  = new Color(0.0f, 0.90f, 1.0f); // keeping cyan for title
    private static readonly Color PlayBtnColor = new Color(0.851f, 0.478f, 0.169f); // #D97A2B
    private static readonly Color ButtonColor  = new Color(0.7f, 0.7f, 0.75f); // warm grey for unselected buttons
    private static readonly Color SelectedColor= new Color(0.9f, 0.15f, 0.15f); // red highlight
    private static readonly Color TextColor    = Color.black; // black text for buttons
    private static readonly Color SubTextColor = Color.black; // black text for score/controls
    private static readonly Color DividerColor = new Color(0f, 0f, 0f, 0.2f); // subtle dark dividers

    // ─────────────────────────────────────────────────────────────────────────
    void Awake() => BuildUI();

    // ─────────────────────────────────────────────────────────────────────────
    //  UI Builder
    // ─────────────────────────────────────────────────────────────────────────

    private void BuildUI()
    {
        // Generate the rounded rect sprite once — reused for every button & card
        _roundedSprite = MakeRoundedSprite(128, 128, RoundRadius);

        // Camera background
        Camera.main.backgroundColor = BgColor;
        Camera.main.clearFlags      = CameraClearFlags.SolidColor;

        // Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject go = new GameObject("Canvas");
            canvas = go.AddComponent<Canvas>();
            // Change to ScreenSpaceCamera so URP Post-Processing Bloom affects the UI!
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.planeDistance = 5f;

            CanvasScaler cs   = go.AddComponent<CanvasScaler>();
            cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920, 1080);
            go.AddComponent<GraphicRaycaster>();
        }

        RectTransform root = canvas.GetComponent<RectTransform>();

        // Full-screen background colour or image
        Image bgImg = MakeImage(root, "Background", backgroundSprite != null ? Color.white : BgColor, Vector2.zero, Vector2.one,
                  Vector2.zero, Vector2.zero);
        if (backgroundSprite != null)
        {
            bgImg.sprite = backgroundSprite;
        }

        // ── Dark card panel (gives the content depth) ─────────────────────────
        RectTransform card = MakePanel(canvas.transform, "Card",
                                       new Vector2(580f, 740f), CardColor);

        // ── Content container centred inside the card ─────────────────────────
        GameObject container = new GameObject("Container");
        container.transform.SetParent(canvas.transform, false);
        RectTransform ct = container.AddComponent<RectTransform>();
        ct.anchorMin = new Vector2(0.5f, 0.5f);
        ct.anchorMax = new Vector2(0.5f, 0.5f);
        ct.pivot     = new Vector2(0.5f, 0.5f);
        ct.sizeDelta = new Vector2(500f, 710f);
        ct.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup vlg = container.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment        = TextAnchor.MiddleCenter;
        vlg.spacing               = 13f;
        vlg.padding               = new RectOffset(0, 0, 28, 28);
        vlg.childControlWidth     = true;
        vlg.childControlHeight    = false;
        vlg.childForceExpandWidth = true;

        // ── Title ─────────────────────────────────────────────────────────────
        MakeLabel(ct, "TitleText", "PONG PING", 82, FontStyles.Bold, AccentColor, 108f);

        // ── High score ────────────────────────────────────────────────────────
        int hs = PlayerPrefs.GetInt(PrefHighScore, 0);
        string hsDisplay = hs > 0 ? "[ Best Score: " + hs + " ]" : "[ No score yet ]";
        MakeLabel(ct, "HighScoreText", hsDisplay, 21, FontStyles.Italic, SubTextColor, 34f);

        // ── Divider ───────────────────────────────────────────────────────────
        MakeDivider(ct, DividerColor);

        // ── Difficulty label ──────────────────────────────────────────────────
        MakeLabel(ct, "DiffLabel", "--- SELECT DIFFICULTY ---", 16,
                  FontStyles.Bold, SubTextColor, 26f);

        // ── Difficulty row ────────────────────────────────────────────────────
        selectedDifficulty = PlayerPrefs.GetInt(PrefDifficulty, 1);
        MakeDifficultyRow(ct);

        // ── PLAY button ───────────────────────────────────────────────────────
        MakeButton(ct, "PlayButton", "PLAY", PlayBtnColor, TextColor, 82f, OnPlayPressed);

        // ── QUIT button ───────────────────────────────────────────────────────
        MakeButton(ct, "QuitButton", "QUIT", ButtonColor, SubTextColor, 48f, OnQuitPressed);

        // ── Divider ───────────────────────────────────────────────────────────
        MakeDivider(ct, DividerColor);

        // ── Controls ──────────────────────────────────────────────────────────
        MakeLabel(ct, "ControlsText",
                  "W / UP   Move Up          S / DOWN   Move Down",
                  16, FontStyles.Normal, SubTextColor, 28f);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Difficulty Row
    // ─────────────────────────────────────────────────────────────────────────

    private void MakeDifficultyRow(RectTransform parent)
    {
        GameObject row = new GameObject("DifficultyRow");
        row.transform.SetParent(parent, false);
        RectTransform rt = row.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0f, 60f);

        HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment        = TextAnchor.MiddleCenter;
        hlg.spacing               = 16f;
        hlg.padding               = new RectOffset(4, 4, 0, 0);
        hlg.childControlWidth     = true;
        hlg.childControlHeight    = true;
        hlg.childForceExpandWidth = true;

        Button easy   = MakeSmallButton(rt, "EasyBtn",   "EASY",   out easyImg);
        Button medium = MakeSmallButton(rt, "MediumBtn", "MEDIUM", out mediumImg);
        Button hard   = MakeSmallButton(rt, "HardBtn",   "HARD",   out hardImg);

        easy.onClick.AddListener(()   => SelectDifficulty(0));
        medium.onClick.AddListener(() => SelectDifficulty(1));
        hard.onClick.AddListener(()   => SelectDifficulty(2));

        RefreshDifficultyButtons();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Difficulty Logic
    // ─────────────────────────────────────────────────────────────────────────

    private void SelectDifficulty(int level)
    {
        selectedDifficulty = level;
        PlayerPrefs.SetInt(PrefDifficulty, level);
        PlayerPrefs.Save();
        RefreshDifficultyButtons();
    }

    private void RefreshDifficultyButtons()
    {
        if (easyImg   != null) easyImg.color   = selectedDifficulty == 0 ? SelectedColor : ButtonColor;
        if (mediumImg != null) mediumImg.color = selectedDifficulty == 1 ? SelectedColor : ButtonColor;
        if (hardImg   != null) hardImg.color   = selectedDifficulty == 2 ? SelectedColor : ButtonColor;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Button Callbacks
    // ─────────────────────────────────────────────────────────────────────────

    private void OnPlayPressed()  => SceneManager.LoadScene(1);

    private void OnQuitPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  UI Helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// Creates a centred panel with the rounded card sprite.
    private RectTransform MakePanel(Transform parent, string name, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;

        Image img  = go.AddComponent<Image>();
        img.sprite = _roundedSprite;
        img.type   = Image.Type.Sliced;
        img.color  = color;
        return rt;
    }

    private TextMeshProUGUI MakeLabel(RectTransform parent, string name, string text,
                                      float fontSize, FontStyles style, Color color, float height)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, height);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.fontStyle = style;
        tmp.color     = color;
        tmp.alignment = TextAlignmentOptions.Center;
        if (arcadeFont != null) tmp.font = arcadeFont;
        return tmp;
    }

    private Button MakeButton(RectTransform parent, string name, string label,
                               Color bgCol, Color textCol, float height,
                               UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, height);

        Image img  = go.AddComponent<Image>();
        img.sprite = _roundedSprite;
        img.type   = Image.Type.Sliced;
        img.color  = bgCol;

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor      = bgCol;
        cb.highlightedColor = Color.Lerp(bgCol, Color.white, 0.18f);
        cb.pressedColor     = Color.Lerp(bgCol, Color.black, 0.18f);
        cb.selectedColor    = bgCol;
        btn.colors          = cb;
        btn.onClick.AddListener(action);

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        RectTransform trt = textGo.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = height * 0.36f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = textCol;
        tmp.alignment = TextAlignmentOptions.Center;
        if (arcadeFont != null) tmp.font = arcadeFont;

        return btn;
    }

    private Button MakeSmallButton(RectTransform parent, string name, string label, out Image imgOut)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 60f);

        imgOut        = go.AddComponent<Image>();
        imgOut.sprite = _roundedSprite;
        imgOut.type   = Image.Type.Sliced;
        imgOut.color  = ButtonColor;

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = imgOut;

        ColorBlock cb = btn.colors;
        cb.highlightedColor = Color.Lerp(ButtonColor, Color.white, 0.15f);
        cb.pressedColor     = Color.Lerp(ButtonColor, Color.black, 0.15f);
        btn.colors          = cb;

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        RectTransform trt = textGo.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 20f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = TextColor;
        tmp.alignment = TextAlignmentOptions.Center;
        if (arcadeFont != null) tmp.font = arcadeFont;

        return btn;
    }

    private Image MakeImage(RectTransform parent, string name, Color color,
                             Vector2 anchorMin, Vector2 anchorMax,
                             Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
        Image img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    private void MakeDivider(RectTransform parent, Color color)
    {
        GameObject go = new GameObject("Divider");
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 2f);
        go.AddComponent<Image>().color = color;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Rounded Rectangle Sprite Generator
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates a white rounded-rect Sprite usable with Image.Type.Sliced.
    /// Recolour it via Image.color.
    /// </summary>
    private Sprite MakeRoundedSprite(int w, int h, int r)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.ARGB32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode   = TextureWrapMode.Clamp;

        Color[] pixels = new Color[w * h];
        Color white = Color.white;
        Color clear = new Color(0f, 0f, 0f, 0f);

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                pixels[y * w + x] = InsideRoundedRect(x, y, w, h, r) ? white : clear;

        tex.SetPixels(pixels);
        tex.Apply();

        // Border offsets for 9-slicing — keeps corners intact during resize
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f),
                             100f, 0, SpriteMeshType.FullRect,
                             new Vector4(r, r, r, r));
    }

    private bool InsideRoundedRect(int x, int y, int w, int h, int r)
    {
        // Bottom-left corner
        if (x < r     && y < r)     return CircleDist(x, y, r,     r)     <= r;
        // Bottom-right
        if (x > w-r-1 && y < r)     return CircleDist(x, y, w-r-1, r)     <= r;
        // Top-left
        if (x < r     && y > h-r-1) return CircleDist(x, y, r,     h-r-1) <= r;
        // Top-right
        if (x > w-r-1 && y > h-r-1) return CircleDist(x, y, w-r-1, h-r-1) <= r;
        return true;
    }

    private float CircleDist(int x, int y, int cx, int cy)
    {
        float dx = x - cx, dy = y - cy;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
}
