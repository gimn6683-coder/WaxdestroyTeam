using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Health))]
public class HealthBar : MonoBehaviour
{
    [Header("위치 / 크기")]
    [Tooltip("오브젝트 위치에서 얼마나 위에 띄울지")]
    public Vector3 offset = new Vector3(0f, 2f, 0f);
    [Tooltip("바 크기 (월드 단위). 이미지를 쓰면 가로(X)만 정하면 세로는 이미지 비율대로 자동 계산")]
    public Vector2 size = new Vector2(1.5f, 0.2f);

    [Header("이미지 (비워 두면 단색 사용)")]
    [Tooltip("바 뒤의 틀/빈 바 이미지")]
    public Sprite backSprite;
    [Tooltip("체력에 따라 줄어드는 이미지")]
    public Sprite fillSprite;
    [Tooltip("틀 안쪽 여백(월드 단위, X=좌우, Y=위아래)")]
    public Vector2 fillInset = Vector2.zero;

    [Header("단색 (이미지가 없을 때)")]
    public Color backColor = new Color(0f, 0f, 0f, 0.6f);
    public Color fillColor = Color.red;


    [Header("닳는 연출 (잔상 바)")]
    [Tooltip("체력이 깎일 때 뒤에서 천천히 따라 줄어드는 잔상 바 사용")]
    public bool useTrail = true;
    [Tooltip("잔상에 쓸 이미지. 비우면 Fill Sprite를 그대로 사용")]
    public Sprite trailSprite;
    [Tooltip("잔상 색. 이미지에는 곱해져서 적용됨")]
    public Color trailColor = new Color(1f, 0.85f, 0.3f, 1f);
    [Tooltip("체력이 깎인 뒤 잔상이 줄기 시작하기까지 대기 시간(초)")]
    public float trailDelay = 0.4f;
    [Tooltip("잔상이 줄어드는 속도 (초당 체력 비율). 0.6이면 가득 찬 바가 약 1.7초에 사라짐")]
    public float trailSpeed = 0.6f;

    private Health health;
    private Transform barRoot;
    private Camera cam;


    private RectTransform fillRect;
    private Image fillImage;
    private bool fillFilled;      // 이미지를 가로로 잘라내는 방식인지
    private RectTransform trailRect;
    private Image trailImage;
    private bool trailFilled;

    private float targetRatio;    // 현재 체력 비율
    private float trailRatio;     // 잔상 바가 보여 주는 비율
    private float delayTimer;

    void Start()
    {
        health = GetComponent<Health>();
        CreateBar();
        health.OnHpChanged += UpdateBar;
        UpdateBar(health.CurrentHp, health.maxHp);
    }

    void CreateBar()
    {
        // 이미지가 있으면 가로 길이에 맞춰 세로를 이미지 비율로 계산
        Sprite refSprite = backSprite != null ? backSprite : fillSprite;
        if (refSprite != null)
            size.y = size.x * refSprite.rect.height / refSprite.rect.width;

        GameObject root = new GameObject(name + "_HpBar");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        root.GetComponent<RectTransform>().sizeDelta = size;
        barRoot = root.transform;

        // 1) 맨 뒤: 배경(빈 바)
        CreateImage(root.transform, "Back", backSprite, backColor, Vector2.zero);


        if (useTrail)
        {
            Sprite ts = trailSprite != null ? trailSprite : fillSprite;
            GameObject trail = CreateImage(root.transform, "Trail", ts, trailColor, fillInset);
            trailRect = trail.GetComponent<RectTransform>();
            trailImage = trail.GetComponent<Image>();
            trailImage.color = trailColor;          // 이미지에도 색을 곱함
            trailFilled = MakeFilledIfSprite(trailImage, ts);
        }

        // 3) 맨 앞: 앞쪽 바
        GameObject fill = CreateImage(root.transform, "Fill", fillSprite, fillColor, fillInset);
        fillRect = fill.GetComponent<RectTransform>();
        fillImage = fill.GetComponent<Image>();
        fillFilled = MakeFilledIfSprite(fillImage, fillSprite);
    }

    GameObject CreateImage(Transform parent, string objName, Sprite sprite, Color color, Vector2 inset)
    {
        GameObject go = new GameObject(objName, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = inset;
        rt.offsetMax = -inset;

        Image img = go.GetComponent<Image>();
        img.raycastTarget = false;
        if (sprite != null) { img.sprite = sprite; img.color = Color.white; }
        else img.color = color;
        return go;
    }

    bool MakeFilledIfSprite(Image img, Sprite sprite)
    {
        if (sprite == null) return false;
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Horizontal;
        img.fillOrigin = (int)Image.OriginHorizontal.Left;
        return true;
    }


    void SetBar(Image img, RectTransform rt, bool filled, float ratio)
    {
        if (filled) img.fillAmount = ratio;
        else rt.anchorMax = new Vector2(ratio, 1f);
    }
    void UpdateBar(int current, int max)
    {
        targetRatio = max > 0 ? (float)current / max : 0f;
        SetBar(fillImage, fillRect, fillFilled, targetRatio);

        if (trailImage == null) return;

        if (targetRatio >= trailRatio) trailRatio = targetRatio;  // 회복이면 잔상도 바로 맞춤
        else delayTimer = trailDelay;                             // 깎였으면 잠시 대기 후 줄어듦
        SetBar(trailImage, trailRect, trailFilled, trailRatio);
    }


    void Update()
    {
        if (trailImage == null || trailRatio <= targetRatio) return;

        if (delayTimer > 0f) { delayTimer -= Time.deltaTime; return; }

        trailRatio = Mathf.MoveTowards(trailRatio, targetRatio, trailSpeed * Time.deltaTime);
        SetBar(trailImage, trailRect, trailFilled, trailRatio);
    }

    void LateUpdate()
    {
        if (barRoot == null) return;

        barRoot.position = transform.position + offset;

        if (cam == null) cam = Camera.main;
        if (cam != null) barRoot.rotation = cam.transform.rotation; // 항상 카메라를 향함
    }

    void OnDestroy()
    {
        if (health != null) health.OnHpChanged -= UpdateBar;
        if (barRoot != null) Destroy(barRoot.gameObject);
    }
}