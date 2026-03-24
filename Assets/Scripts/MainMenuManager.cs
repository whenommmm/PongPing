using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    private const string PrefDifficulty = "Difficulty";
    private const string PrefHighScore  = "HighScore";

    private int selectedDifficulty = 1;
    private Image easyImg, mediumImg, hardImg;

    public Sprite backgroundSprite;
    public TMP_FontAsset arcadeFont; 

    private Sprite _roundedSprite;
    private const int RoundRadius = 10; 

    private static readonly Color BgColor = new Color(0.153f, 0.188f, 0.235f); 
    private static readonly Color CardColor = new Color(0.94f, 0.93f, 0.88f, 0.98f); 
    private static readonly Color AccentColor = new Color(0.153f, 0.188f, 0.235f); 
    
    private static readonly Color PlayBtnColor = new Color(0.851f, 0.478f, 0.169f); 
    private static readonly Color ButtonColor = new Color(0.7f, 0.7f, 0.75f); 
    private static readonly Color EasyColor = new Color(0.2f, 0.8f, 0.2f);
    private static readonly Color MediumColor = PlayBtnColor; 
    private static readonly Color HardColor = new Color(0.9f, 0.15f, 0.15f); 
    private static readonly Color TextColor = Color.black; 
    private static readonly Color SubTextColor = Color.black; 
    private static readonly Color DividerColor = new Color(0f, 0f, 0f, 0.2f); 

    void Awake() => BuildUI();

    private void BuildUI()
    {
        _roundedSprite = MakeRoundedSprite(128, 128, RoundRadius);

        Camera.main.backgroundColor = BgColor;
        Camera.main.clearFlags = CameraClearFlags.SolidColor;

        // setup the canvas for urp post processing
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject go = new GameObject("Canvas");
            canvas = go.AddComponent<Canvas>();
            
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.planeDistance = 5f;

            CanvasScaler cs = go.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920, 1080);
            go.AddComponent<GraphicRaycaster>();
        }

        RectTransform root = canvas.GetComponent<RectTransform>();

        // background image setup
        Image bgImg = MakeImage(root, "Background", backgroundSprite != null ? Color.white : BgColor, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        if (backgroundSprite != null)
        {
            bgImg.sprite = backgroundSprite;
        }

        // draw the center card
        RectTransform card = MakePanel(canvas.transform, "Card", new Vector2(580f, 740f), CardColor);

        GameObject container = new GameObject("Container");
        container.transform.SetParent(canvas.transform, false);
        RectTransform ct = container.AddComponent<RectTransform>();
        ct.anchorMin = new Vector2(0.5f, 0.5f);
        ct.anchorMax = new Vector2(0.5f, 0.5f);
        ct.pivot = new Vector2(0.5f, 0.5f);
        ct.sizeDelta = new Vector2(500f, 710f);
        ct.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup vlg = container.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.spacing = 13f;
        vlg.padding = new RectOffset(0, 0, 28, 28);
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;

        MakeLabel(ct, "TitleText", "PONG PING", 82, FontStyles.Bold, AccentColor, 108f);

        // load and show high score
        int hs = PlayerPrefs.GetInt(PrefHighScore, 0);
        string hsDisplay = hs > 0 ? "[ Best Score: " + hs + " ]" : "[ No score yet ]";
        MakeLabel(ct, "HighScoreText", hsDisplay, 21, FontStyles.Italic, SubTextColor, 34f);

        MakeDivider(ct, DividerColor);

        MakeLabel(ct, "DiffLabel", "--- SELECT DIFFICULTY ---", 16, FontStyles.Bold, SubTextColor, 26f);

        selectedDifficulty = PlayerPrefs.GetInt(PrefDifficulty, 1);
        MakeDifficultyRow(ct);

        MakeButton(ct, "PlayButton", "PLAY", PlayBtnColor, TextColor, 82f, OnPlayPressed);
        MakeButton(ct, "QuitButton", "QUIT", ButtonColor, SubTextColor, 48f, OnQuitPressed);

        MakeDivider(ct, DividerColor);

        // show controls at bottom
        MakeLabel(ct, "ControlsText", "W / UP : MOVE UP\n\nS / DOWN : MOVE DOWN", 14, FontStyles.Normal, SubTextColor, 50f);
    }

    private void MakeDifficultyRow(RectTransform parent)
    {
        GameObject row = new GameObject("DifficultyRow");
        row.transform.SetParent(parent, false);
        RectTransform rt = row.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0f, 60f);

        HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.spacing = 16f;
        hlg.padding = new RectOffset(4, 4, 0, 0);
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true;

        Button easy = MakeSmallButton(rt, "EasyBtn", "EASY", out easyImg);
        Button medium = MakeSmallButton(rt, "MediumBtn", "MEDIUM", out mediumImg);
        Button hard = MakeSmallButton(rt, "HardBtn", "HARD", out hardImg);

        easy.onClick.AddListener(() => SelectDifficulty(0));
        medium.onClick.AddListener(() => SelectDifficulty(1));
        hard.onClick.AddListener(() => SelectDifficulty(2));

        RefreshDifficultyButtons();
    }

    private void SelectDifficulty(int level)
    {
        // save their choice
        selectedDifficulty = level;
        PlayerPrefs.SetInt(PrefDifficulty, level);
        PlayerPrefs.Save();
        RefreshDifficultyButtons();
    }

    private void RefreshDifficultyButtons()
    {
        // update button highlight colors
        if (easyImg != null) easyImg.color = selectedDifficulty == 0 ? EasyColor : ButtonColor;
        if (mediumImg != null) mediumImg.color = selectedDifficulty == 1 ? MediumColor : ButtonColor;
        if (hardImg != null) hardImg.color = selectedDifficulty == 2 ? HardColor : ButtonColor;
    }

    private void OnPlayPressed() => SceneManager.LoadScene(1);

    private void OnQuitPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private RectTransform MakePanel(Transform parent, string name, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;

        Image img = go.AddComponent<Image>();
        img.sprite = _roundedSprite;
        img.type = Image.Type.Sliced;
        img.color = color;
        return rt;
    }

    private TextMeshProUGUI MakeLabel(RectTransform parent, string name, string text, float fontSize, FontStyles style, Color color, float height)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, height);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        if (arcadeFont != null) tmp.font = arcadeFont;
        return tmp;
    }

    private Button MakeButton(RectTransform parent, string name, string label, Color bgCol, Color textCol, float height, UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, height);

        Image img = go.AddComponent<Image>();
        img.sprite = _roundedSprite;
        img.type = Image.Type.Sliced;
        img.color = Color.white; 

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = bgCol;
        cb.highlightedColor = Color.Lerp(bgCol, Color.white, 0.18f);
        cb.pressedColor = Color.Lerp(bgCol, Color.black, 0.18f);
        cb.selectedColor = bgCol;
        btn.colors = cb;
        btn.onClick.AddListener(action);

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        RectTransform trt = textGo.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = height * 0.36f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = textCol;
        tmp.alignment = TextAlignmentOptions.Center;
        if (arcadeFont != null) tmp.font = arcadeFont;

        return btn;
    }

    private Button MakeSmallButton(RectTransform parent, string name, string label, out Image imgOut)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 60f);

        imgOut = go.AddComponent<Image>();
        imgOut.sprite = _roundedSprite;
        imgOut.type = Image.Type.Sliced;
        imgOut.color = ButtonColor;

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = imgOut;

        ColorBlock cb = btn.colors;
        cb.highlightedColor = Color.Lerp(ButtonColor, Color.white, 0.15f);
        cb.pressedColor = Color.Lerp(ButtonColor, Color.black, 0.15f);
        btn.colors = cb;

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        RectTransform trt = textGo.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 20f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = TextColor;
        tmp.alignment = TextAlignmentOptions.Center;
        if (arcadeFont != null) tmp.font = arcadeFont;

        return btn;
    }

    private Image MakeImage(RectTransform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
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

    // draw the curved image behind the buttons
    private Sprite MakeRoundedSprite(int w, int h, int r)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.ARGB32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[w * h];
        Color white = Color.white;
        Color clear = new Color(0f, 0f, 0f, 0f);

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                pixels[y * w + x] = InsideRoundedRect(x, y, w, h, r) ? white : clear;

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(r, r, r, r));
    }

    private bool InsideRoundedRect(int x, int y, int w, int h, int r)
    {
        if (x < r && y < r) return CircleDist(x, y, r, r) <= r;
        if (x > w - r - 1 && y < r) return CircleDist(x, y, w - r - 1, r) <= r;
        if (x < r && y > h - r - 1) return CircleDist(x, y, r, h - r - 1) <= r;
        if (x > w - r - 1 && y > h - r - 1) return CircleDist(x, y, w - r - 1, h - r - 1) <= r;
        return true;
    }

    private float CircleDist(int x, int y, int cx, int cy)
    {
        float dx = x - cx, dy = y - cy;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
}
