using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// All UI is built in code (Canvas + legacy UI Text/Buttons) so the project
/// needs no prefabs, no scenes wired by hand, and no font assets — it uses
/// Unity's built-in font. Screens: Title, Level Select, HUD, Win.
/// </summary>
public class GameUI : MonoBehaviour
{
    private static Font _font;

    public static Font UiFont
    {
        get
        {
            if (_font == null)
            {
                try { _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
                catch { _font = null; }
                if (_font == null)
                {
                    try { _font = Resources.GetBuiltinResource<Font>("Arial.ttf"); }
                    catch { _font = null; }
                }
            }
            return _font;
        }
    }

    private GameObject _hud;
    private GameObject _titlePanel;
    private GameObject _selectPanel;
    private GameObject _winPanel;
    private Text _levelLabel;
    private Text _stateLabel;
    private Text _deathsLabel;
    private Text _hintLabel;
    private Text _toastLabel;
    private Text _winStats;
    private Button[] _levelButtons;
    private float _toastTimer;
    private bool _built;

    public void Init(Action onStart, Action onOpenSelect, Action<int> onPickLevel,
                     Action onBackToTitle, Action onPlayAgain, string[] levelNames)
    {
        if (_built) return;
        _built = true;

        if (FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        var canvasGo = new GameObject("Canvas");
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(960f, 540f);
        canvasGo.AddComponent<GraphicRaycaster>();
        Transform root = canvasGo.transform;

        BuildHud(root);
        BuildTitle(root, onStart, onOpenSelect);
        BuildSelect(root, onPickLevel, onBackToTitle, levelNames);
        BuildWin(root, onPlayAgain, onBackToTitle);
        BuildToast(root);
    }

    // ---------- screens ----------

    public void ShowTitle()
    {
        _titlePanel.SetActive(true);
        _selectPanel.SetActive(false);
        _winPanel.SetActive(false);
    }

    public void ShowSelect(int unlocked)
    {
        _titlePanel.SetActive(false);
        _selectPanel.SetActive(true);
        _winPanel.SetActive(false);
        for (int i = 0; i < _levelButtons.Length; i++)
        {
            _levelButtons[i].interactable = i <= unlocked;
        }
    }

    public void ShowWin(int deaths)
    {
        _titlePanel.SetActive(false);
        _selectPanel.SetActive(false);
        _winPanel.SetActive(true);
        _winStats.text = "All 8 levels cleared.\nDeaths: " + deaths;
    }

    public void HideAll()
    {
        _titlePanel.SetActive(false);
        _selectPanel.SetActive(false);
        _winPanel.SetActive(false);
    }

    // ---------- HUD ----------

    public void SetHudVisible(bool visible)
    {
        if (_hud != null) _hud.SetActive(visible);
    }

    public void SetLevelText(string text) { _levelLabel.text = text; }
    public void SetHint(string text) { _hintLabel.text = text; }
    public void SetDeathsText(int deaths) { _deathsLabel.text = "Deaths: " + deaths; }

    public void SetStateText(WorldState state)
    {
        _stateLabel.text = state == WorldState.A ? "STATE A" : "STATE B";
        _stateLabel.color = state == WorldState.A ? Palette.OnlyA : Palette.OnlyB;
    }

    public void ShowToast(string text)
    {
        _toastLabel.text = text;
        _toastLabel.gameObject.SetActive(true);
        _toastTimer = 1.1f;
    }

    private void Update()
    {
        if (_toastTimer > 0f)
        {
            _toastTimer -= Time.deltaTime;
            if (_toastTimer <= 0f && _toastLabel != null)
            {
                _toastLabel.gameObject.SetActive(false);
            }
        }
    }

    // ---------- construction ----------

    private void BuildHud(Transform root)
    {
        _hud = new GameObject("HUD");
        _hud.transform.SetParent(root, false);
        Stretch(_hud.GetComponent<RectTransform>() ?? _hud.AddComponent<RectTransform>());

        _levelLabel = MakeText(_hud.transform, "", 24, TextAnchor.UpperLeft, Color.white);
        Place(_levelLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -10f), new Vector2(500f, 34f));

        _deathsLabel = MakeText(_hud.transform, "", 24, TextAnchor.UpperCenter, Color.white);
        Place(_deathsLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(300f, 34f));

        _stateLabel = MakeText(_hud.transform, "", 24, TextAnchor.UpperRight, Color.white);
        Place(_stateLabel.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-16f, -10f), new Vector2(300f, 34f));

        _hintLabel = MakeText(_hud.transform, "", 22, TextAnchor.LowerCenter, new Color(1f, 1f, 1f, 0.85f));
        Place(_hintLabel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(900f, 32f));
    }

    private void BuildTitle(Transform root, Action onStart, Action onOpenSelect)
    {
        _titlePanel = MakePanel(root, "TitlePanel");

        Text title = MakeText(_titlePanel.transform, "SHIFT", 110, TextAnchor.MiddleCenter, Color.white);
        title.fontStyle = FontStyle.Bold;
        Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(700f, 150f));

        Text sub = MakeText(_titlePanel.transform, "One key flips the whole world.", 28, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.85f));
        Place(sub.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(700f, 44f));

        MakeButton(_titlePanel.transform, "Start", new Vector2(0f, -50f), new Vector2(280f, 58f), onStart);
        MakeButton(_titlePanel.transform, "Level Select", new Vector2(0f, -125f), new Vector2(280f, 58f), onOpenSelect);

        Text controls = MakeText(_titlePanel.transform,
            "Move: A/D or Arrow Keys    Jump: Space / W    Flip: SHIFT    Restart: R",
            20, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.65f));
        Place(controls.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -200f), new Vector2(900f, 34f));
    }

    private void BuildSelect(Transform root, Action<int> onPickLevel, Action onBackToTitle, string[] levelNames)
    {
        _selectPanel = MakePanel(root, "SelectPanel");

        Text title = MakeText(_selectPanel.transform, "Select Level", 44, TextAnchor.MiddleCenter, Color.white);
        Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(600f, 60f));

        _levelButtons = new Button[levelNames.Length];
        for (int i = 0; i < levelNames.Length; i++)
        {
            int idx = i;
            float x = (i % 4 - 1.5f) * 190f;
            float y = i < 4 ? 50f : -50f;
            _levelButtons[i] = MakeButton(_selectPanel.transform,
                (i + 1) + "\n" + levelNames[i],
                new Vector2(x, y), new Vector2(175f, 76f),
                () => onPickLevel(idx));
        }

        MakeButton(_selectPanel.transform, "Back", new Vector2(0f, -170f), new Vector2(220f, 52f), onBackToTitle);
    }

    private void BuildWin(Transform root, Action onPlayAgain, Action onBackToTitle)
    {
        _winPanel = MakePanel(root, "WinPanel");

        Text title = MakeText(_winPanel.transform, "YOU WIN!", 72, TextAnchor.MiddleCenter, Palette.Goal);
        title.fontStyle = FontStyle.Bold;
        Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 110f), new Vector2(700f, 100f));

        _winStats = MakeText(_winPanel.transform, "", 28, TextAnchor.MiddleCenter, Color.white);
        Place(_winStats.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(700f, 90f));

        MakeButton(_winPanel.transform, "Play Again", new Vector2(0f, -80f), new Vector2(280f, 58f), onPlayAgain);
        MakeButton(_winPanel.transform, "Title Screen", new Vector2(0f, -155f), new Vector2(280f, 58f), onBackToTitle);
    }

    private void BuildToast(Transform root)
    {
        _toastLabel = MakeText(root, "", 44, TextAnchor.MiddleCenter, Color.white);
        _toastLabel.fontStyle = FontStyle.Bold;
        Place(_toastLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(800f, 70f));
        _toastLabel.gameObject.SetActive(false);
    }

    // ---------- helpers ----------

    private GameObject MakePanel(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        Stretch(rt);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.02f, 0.02f, 0.06f, 0.78f);
        return go;
    }

    private Text MakeText(Transform parent, string content, int size, TextAnchor anchor, Color color)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var t = go.AddComponent<Text>();
        t.font = UiFont;
        t.text = content;
        t.fontSize = size;
        t.alignment = anchor;
        t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    private Button MakeButton(Transform parent, string label, Vector2 anchoredPos, Vector2 size, Action onClick)
    {
        var go = new GameObject("Button");
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var img = go.AddComponent<Image>();
        img.color = new Color(0.16f, 0.20f, 0.34f, 0.95f);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        Place(go.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPos, size);

        Text t = MakeText(go.transform, label, 24, TextAnchor.MiddleCenter, Color.white);
        Stretch(t.rectTransform);
        if (onClick != null) btn.onClick.AddListener(() => onClick());
        return btn;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 anchoredPos, Vector2 size)
    {
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
    }
}
