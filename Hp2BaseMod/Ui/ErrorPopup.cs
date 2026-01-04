using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ErrorPopup : MonoBehaviour
{
    private static ErrorPopup _instance;
    private GameObject _canvasGO;
    private GameObject _windowGO;
    private Text _text;
    private ScrollRect _scrollRect;
    private Image _cornerImage;
    public Texture2D cornerTexture;
    // Assign this in inspector or via code 
    public static void Show(string message)
    {
        if (_instance == null)
        {
            var go = new GameObject("ErrorPopup");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<ErrorPopup>();
            _instance.CreateUI();
        }
        _instance.SetMessage(message);
        _instance._canvasGO.SetActive(true);
    }

    public static void SetCornerTexture(Texture2D texture)
    {
        if (_instance != null && _instance._cornerImage != null)
        {
            _instance._cornerImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }

    private void Update()
    {
        if (_canvasGO != null && _canvasGO.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            _canvasGO.SetActive(false);
        }
    }
    private void CreateUI()
    { // ---------------------- // Canvas // ---------------------- 
        _canvasGO = new GameObject("ErrorCanvas");
        _canvasGO.transform.SetParent(transform);
        var canvas = _canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        _canvasGO.AddComponent<CanvasScaler>();
        _canvasGO.AddComponent<GraphicRaycaster>();
        // Fullscreen blocker with darker overlay 
        var blocker = new GameObject("Blocker");
        blocker.transform.SetParent(_canvasGO.transform);
        var blockerImg = blocker.AddComponent<Image>();
        blockerImg.color = new Color(0, 0, 0, 0.75f);
        var blockerRT = blocker.GetComponent<RectTransform>();
        blockerRT.anchorMin = Vector2.zero;
        blockerRT.anchorMax = Vector2.one;
        blockerRT.offsetMin = Vector2.zero;
        blockerRT.offsetMax = Vector2.zero;
        // ---------------------- // Window Shadow // ---------------------- 
        var shadowGO = new GameObject("Shadow");
        shadowGO.transform.SetParent(_canvasGO.transform);
        var shadowImg = shadowGO.AddComponent<Image>();
        shadowImg.color = new Color(0, 0, 0, 0.6f);
        var shadowRT = shadowGO.GetComponent<RectTransform>();
        shadowRT.sizeDelta = new Vector2(1130, 650);
        shadowRT.anchorMin = new Vector2(0.5f, 0.5f);
        shadowRT.anchorMax = new Vector2(0.5f, 0.5f);
        shadowRT.pivot = new Vector2(0.5f, 0.5f);
        shadowRT.anchoredPosition = new Vector2(5, -5);
        // ---------------------- // Window // ---------------------- 
        _windowGO = new GameObject("Window");
        _windowGO.transform.SetParent(_canvasGO.transform);
        var windowImg = _windowGO.AddComponent<Image>();
        windowImg.color = new Color(0.15f, 0.12f, 0.15f, 1f);
        // Dark purple-gray 
        var windowRT = _windowGO.GetComponent<RectTransform>();
        windowRT.sizeDelta = new Vector2(1120, 640);
        windowRT.anchorMin = new Vector2(0.5f, 0.5f);
        windowRT.anchorMax = new Vector2(0.5f, 0.5f);
        windowRT.pivot = new Vector2(0.5f, 0.5f);
        windowRT.anchoredPosition = Vector2.zero;
        // Window border 
        var borderGO = new GameObject("Border");
        borderGO.transform.SetParent(_windowGO.transform);
        var borderOutline = borderGO.AddComponent<Outline>();
        borderOutline.effectColor = new Color(1f, 0.4f, 0.7f, 1f);
        // Pink accent 
        borderOutline.effectDistance = new Vector2(3, -3);
        var borderImg = borderGO.AddComponent<Image>();
        borderImg.color = Color.clear;
        var borderRT = borderGO.GetComponent<RectTransform>();
        borderRT.anchorMin = Vector2.zero;
        borderRT.anchorMax = Vector2.one;
        borderRT.offsetMin = Vector2.zero;
        borderRT.offsetMax = Vector2.zero;
        // ---------------------- // Title Bar // ---------------------- 
        var titleBarGO = new GameObject("TitleBar");
        titleBarGO.transform.SetParent(_windowGO.transform);
        var titleBarImg = titleBarGO.AddComponent<Image>();
        titleBarImg.color = new Color(0.9f, 0.3f, 0.6f, 1f);
        // Pink theme 
        var titleBarRT = titleBarGO.GetComponent<RectTransform>();
        titleBarRT.anchorMin = new Vector2(0, 1);
        titleBarRT.anchorMax = new Vector2(1, 1);
        titleBarRT.pivot = new Vector2(0.5f, 1);
        titleBarRT.anchoredPosition = Vector2.zero;
        titleBarRT.sizeDelta = new Vector2(0, 60);
        // Title Text 
        var titleTextGO = new GameObject("TitleText");
        titleTextGO.transform.SetParent(titleBarGO.transform);
        var titleText = titleTextGO.AddComponent<Text>();
        titleText.text = "⚠ ERROR";
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 24;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleLeft;
        var titleTextRT = titleTextGO.GetComponent<RectTransform>();
        titleTextRT.anchorMin = new Vector2(0, 0);
        titleTextRT.anchorMax = new Vector2(1, 1);
        titleTextRT.offsetMin = new Vector2(20, 0);
        titleTextRT.offsetMax = new Vector2(-20, 0);
        // Title shadow 
        var titleShadow = titleTextGO.AddComponent<Shadow>();
        titleShadow.effectColor = new Color(0, 0, 0, 0.5f);
        titleShadow.effectDistance = new Vector2(2, -2);
        // ---------------------- // Content Background // ---------------------- 
        var contentBgGO = new GameObject("ContentBackground");
        contentBgGO.transform.SetParent(_windowGO.transform);
        var contentBgImg = contentBgGO.AddComponent<Image>();
        contentBgImg.color = new Color(0.08f, 0.08f, 0.1f, 1f);
        // Darker background 
        var contentBgRT = contentBgGO.GetComponent<RectTransform>();
        contentBgRT.anchorMin = new Vector2(0.05f, 0.2f);
        contentBgRT.anchorMax = new Vector2(0.95f, 0.88f);
        contentBgRT.offsetMin = Vector2.zero;
        contentBgRT.offsetMax = Vector2.zero;
        // Content border 
        var contentBorder = contentBgGO.AddComponent<Outline>();
        contentBorder.effectColor = new Color(0.3f, 0.3f, 0.35f, 1f);
        contentBorder.effectDistance = new Vector2(1, -1);
        // ---------------------- // Scrollable Text // ---------------------- 

        var scrollGO = new GameObject("Scroll");
        var scrollImg = scrollGO.AddComponent<Image>();
        scrollImg.color = Color.clear;
        scrollGO.transform.SetParent(_windowGO.transform);
        _scrollRect = scrollGO.AddComponent<ScrollRect>();
        var scrollRT = scrollGO.GetComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0.08f, 0.23f);
        scrollRT.anchorMax = new Vector2(0.92f, 0.85f);
        scrollRT.offsetMin = Vector2.zero; scrollRT.offsetMax = Vector2.zero;

        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollGO.transform);
        var vpRT = viewport.AddComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero; vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = Vector2.zero; vpRT.offsetMax = Vector2.zero;

        viewport.AddComponent<RectMask2D>();
        _scrollRect.viewport = vpRT;

        _scrollRect.viewport = vpRT;

        var content = new GameObject("Content");
        content.transform.SetParent(viewport.transform);
        var contentRT = content.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.anchoredPosition = Vector2.zero;
        contentRT.sizeDelta = new Vector2(0, 500);
        // Give it initial height 
        _text = content.AddComponent<Text>();
        _text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        _text.fontSize = 16;
        _text.color = Color.white;
        _text.alignment = TextAnchor.UpperLeft;
        _text.horizontalOverflow = HorizontalWrapMode.Wrap;
        _text.verticalOverflow = VerticalWrapMode.Overflow;
        // Add ContentSizeFitter directly to content with text 
        var fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        _scrollRect.content = contentRT;
        _scrollRect.horizontal = false;
        _scrollRect.vertical = true;
        // ---------------------- // Buttons // ---------------------- 
        CreateButton("COPY", new Vector2(-130, 60), new Color(0.35f, 0.35f, 0.55f, 1f), () => GUIUtility.systemCopyBuffer = _text.text);
        CreateButton("OK", new Vector2(130, 60), new Color(0.9f, 0.3f, 0.6f, 1f), () => _canvasGO.SetActive(false));
        // ---------------------- // Corner Image (Top Right) - Created last so it's in front // ---------------------- 
        var cornerImageGO = new GameObject("CornerImage");
        cornerImageGO.transform.SetParent(_windowGO.transform);
        _cornerImage = cornerImageGO.AddComponent<Image>();
        _cornerImage.color = Color.white;
        var cornerRT = cornerImageGO.GetComponent<RectTransform>();
        cornerRT.anchorMin = new Vector2(1, 1);
        cornerRT.anchorMax = new Vector2(1, 1);
        cornerRT.pivot = new Vector2(0.5f, 0.5f);
        // Center pivot so it overflows evenly 
        cornerRT.anchoredPosition = new Vector2(40, 40);
        // Move it 40px outside top and right 
        cornerRT.sizeDelta = new Vector2(180, 180);
        // Set corner texture if available 
        if (cornerTexture != null)
        {
            SetCornerTexture(cornerTexture);
        }
    }
    private void CreateButton(string text, Vector2 anchoredPos, Color color, UnityEngine.Events.UnityAction onClick)
    {
        var btnGO = new GameObject(text);
        btnGO.transform.SetParent(_windowGO.transform);
        var rt = btnGO.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(180, 55);
        rt.anchorMin = new Vector2(0.5f, 0);
        rt.anchorMax = new Vector2(0.5f, 0);
        rt.anchoredPosition = anchoredPos;
        var img = btnGO.AddComponent<Image>();
        img.color = color;
        // Button border 
        var outline = btnGO.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.3f);
        outline.effectDistance = new Vector2(2, -2);
        var btn = btnGO.AddComponent<Button>();
        btn.onClick.AddListener(onClick);
        // Button highlight 
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        colors.selectedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
        btn.colors = colors;
        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(btnGO.transform);
        var txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;
        var txt = txtGO.AddComponent<Text>();
        txt.text = text;
        txt.color = Color.white;
        txt.fontSize = 16;
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        // Text shadow 
        var txtShadow = txtGO.AddComponent<Shadow>();
        txtShadow.effectColor = new Color(0, 0, 0, 0.5f);
        txtShadow.effectDistance = new Vector2(1, -1);
    }
    private void SetMessage(string message)
    {
        _text.text = message;
        // Force canvas and layout updates 
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
        Canvas.ForceUpdateCanvases();
        _scrollRect.verticalNormalizedPosition = 1f;
        // scroll to top 
    }
}