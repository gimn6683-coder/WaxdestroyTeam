using UnityEngine;
using UnityEngine.UI;

public class ScreenMessage : MonoBehaviour
{
    static ScreenMessage instance;

    [Header("표시 설정 (건드리지 않아도 됨)")]
    [Tooltip("이미 만든 Text가 있으면 연결. 비우면 화면에 자동으로 만들어 사용")]
    public Text messageText;
    [Tooltip("글꼴. 비우면 한글이 나오는 시스템 글꼴을 자동으로 사용")]
    public Font font;
    public int fontSize = 40;
    public Color textColor = Color.white;
    public Color outlineColor = new Color(0f, 0f, 0f, 0.85f);
    [Tooltip("화면 아래쪽에서 얼마나 위에 띄울지 (1920x1080 기준 픽셀)")]
    public float bottomOffset = 220f;
    [Tooltip("메시지가 머무는 시간(초)")]
    public float showTime = 1.5f;
    [Tooltip("사라지는 데 걸리는 시간(초)")]
    public float fadeTime = 0.4f;

    float timer;

    // 어디서든 ScreenMessage.Show("내용") 으로 호출
    public static void Show(string message)
    {
        if (instance == null)
        {
            instance = FindFirstObjectByType<ScreenMessage>();
            if (instance == null)
                instance = new GameObject("ScreenMessage").AddComponent<ScreenMessage>();
        }
        instance.Display(message);
    }

    void Awake()
    {
        if (instance == null) instance = this;

        if (messageText == null) BuildUI();
        else if (font != null) messageText.font = font;

        SetAlpha(0f);
    }

    void BuildUI()
    {
        GameObject canvasGO = new GameObject("ScreenMessageCanvas");
        canvasGO.transform.SetParent(transform, false);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;   // 다른 UI보다 위에 표시

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject textGO = new GameObject("Message", typeof(RectTransform), typeof(Text));
        textGO.transform.SetParent(canvasGO.transform, false);

        RectTransform rt = textGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, bottomOffset);
        rt.sizeDelta = new Vector2(1400f, 100f);

        messageText = textGO.GetComponent<Text>();
        messageText.raycastTarget = false;
        messageText.alignment = TextAnchor.MiddleCenter;
        messageText.fontSize = fontSize;
        messageText.color = textColor;
        messageText.horizontalOverflow = HorizontalWrapMode.Overflow;

        // 한글이 나오는 시스템 글꼴 (직접 지정한 font가 있으면 그것을 사용)
        messageText.font = font != null
            ? font
            : Font.CreateDynamicFontFromOSFont(
                new[] { "Malgun Gothic", "맑은 고딕", "Apple SD Gothic Neo", "NanumGothic", "Arial" }, fontSize);

        Outline outline = textGO.AddComponent<Outline>();
        outline.effectColor = outlineColor;
        outline.effectDistance = new Vector2(2f, -2f);
    }

    void Display(string message)
    {
        messageText.text = message;
        timer = showTime + fadeTime;
        SetAlpha(1f);
    }

    void Update()
    {
        if (timer <= 0f) return;

        timer -= Time.unscaledDeltaTime;

        if (timer <= 0f) SetAlpha(0f);
        else if (timer < fadeTime) SetAlpha(timer / fadeTime);   // 서서히 사라짐
    }

    void SetAlpha(float a)
    {
        Color c = messageText.color;
        c.a = a;
        messageText.color = c;
    }
}