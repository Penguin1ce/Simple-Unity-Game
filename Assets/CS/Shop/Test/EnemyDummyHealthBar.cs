using UnityEngine;
using UnityEngine.UI;

public class EnemyDummyHealthBar : MonoBehaviour
{
    public EnemyDummy enemy;
    public Vector3 offset = new Vector3(0, 1.6f, 0);
    public float barWidth = 2.4f;
    public float barHeight = 0.18f;
    public float scaleMultiplier = 0.01f;

    public Color fullColor = new Color(0.85f, 0.15f, 0.15f);
    public Color lowColor = new Color(0.5f, 0.05f, 0.05f);
    public Color bgColor = new Color(0, 0, 0, 0.5f);

    private Image _fillImage;
    private Transform _barRoot;
    private float _maxHP;

    private void Start()
    {
        if (enemy == null) enemy = GetComponentInParent<EnemyDummy>();
        if (enemy == null) { Destroy(this); return; }

        _maxHP = enemy.maxHealth;
        BuildBar();
    }

    private void BuildBar()
    {
        GameObject root = new GameObject("EnemyBar");
        root.transform.SetParent(transform, false);
        root.transform.localPosition = offset;
        float s = scaleMultiplier;
        root.transform.localScale = new Vector3(s, s, s);

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;
        canvas.worldCamera = Camera.main;

        RectTransform crt = root.GetComponent<RectTransform>();
        crt.sizeDelta = new Vector2(barWidth / s, barHeight / s);

        // 背景
        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(root.transform, false);
        RectTransform brt = bg.AddComponent<RectTransform>();
        brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
        brt.sizeDelta = Vector2.zero;
        bg.AddComponent<Image>().color = bgColor;

        // 填充
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(root.transform, false);
        RectTransform frt = fill.AddComponent<RectTransform>();
        frt.anchorMin = Vector2.zero;
        frt.anchorMax = Vector2.one;
        frt.offsetMin = new Vector2(1, 1);
        frt.offsetMax = new Vector2(-1, -1);
        frt.pivot = new Vector2(0, 0.5f);
        _fillImage = fill.AddComponent<Image>();
        _fillImage.color = fullColor;

        _barRoot = root.transform;
    }

    private void LateUpdate()
    {
        if (enemy == null || _fillImage == null || _barRoot == null) return;

        float ratio = Mathf.Clamp01((float)enemy.CurrentHealth / _maxHP);

        _fillImage.rectTransform.anchorMax = new Vector2(ratio, 1);
        _fillImage.color = Color.Lerp(lowColor, fullColor, ratio);

        Transform cam = Camera.main != null ? Camera.main.transform : null;
        if (cam != null)
            _barRoot.LookAt(_barRoot.position + cam.forward);
    }
}
