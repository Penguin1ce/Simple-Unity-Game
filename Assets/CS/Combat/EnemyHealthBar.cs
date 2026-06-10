using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyHealthBar : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.3f, 0f);
    [SerializeField] private Vector2 barSize = new Vector2(120f, 14f);
    [SerializeField] private Vector3 barScale = new Vector3(0.01f, 0.01f, 0.01f);

    [Header("Colors")]
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.75f);
    [SerializeField] private Color fillColor = new Color(0.85f, 0.15f, 0.15f, 1f);

    private EnemyHealth enemyHealth;
    private Transform barRoot;
    private Image fillImage;
    private Camera mainCamera;
    private bool isSubscribed;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        BuildBar();
        RefreshBar();
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void Start()
    {
        RefreshBar();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void LateUpdate()
    {
        FaceCamera();
    }

    private void BuildBar()
    {
        if (barRoot != null)
        {
            Destroy(barRoot.gameObject);
        }

        GameObject root = new GameObject("EnemyHealthBar");
        root.transform.SetParent(transform, false);
        root.transform.localPosition = worldOffset;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = barScale;

        barRoot = root.transform;

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 500;

        RectTransform canvasRect = root.GetComponent<RectTransform>();
        canvasRect.sizeDelta = barSize;

        GameObject background = new GameObject("Background", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(root.transform, false);
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        Image backgroundImage = background.GetComponent<Image>();
        backgroundImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        backgroundImage.type = Image.Type.Sliced;
        backgroundImage.color = backgroundColor;

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(background.transform, false);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(2f, 2f);
        fillRect.offsetMax = new Vector2(-2f, -2f);

        fillImage = fill.GetComponent<Image>();
        fillImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0;
        fillImage.color = fillColor;

        mainCamera = Camera.main;
    }

    private void Subscribe()
    {
        if (enemyHealth == null || isSubscribed)
        {
            return;
        }

        enemyHealth.HealthChanged += HandleHealthChanged;
        enemyHealth.Died += HandleDied;
        isSubscribed = true;
        RefreshBar();
    }

    private void Unsubscribe()
    {
        if (!isSubscribed || enemyHealth == null)
        {
            return;
        }

        enemyHealth.HealthChanged -= HandleHealthChanged;
        enemyHealth.Died -= HandleDied;
        isSubscribed = false;
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        UpdateFill(currentHealth, maxHealth);
    }

    private void HandleDied()
    {
        if (barRoot != null)
        {
            barRoot.gameObject.SetActive(false);
        }
    }

    private void RefreshBar()
    {
        if (enemyHealth == null)
        {
            return;
        }

        UpdateFill(enemyHealth.currentHealth, enemyHealth.maxHealth);
    }

    private void UpdateFill(float currentHealth, float maxHealth)
    {
        if (fillImage == null)
        {
            return;
        }

        fillImage.fillAmount = maxHealth <= 0f ? 0f : Mathf.Clamp01(currentHealth / maxHealth);
    }

    private void FaceCamera()
    {
        if (barRoot == null)
        {
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            return;
        }

        Vector3 lookDirection = barRoot.position - mainCamera.transform.position;
        if (lookDirection.sqrMagnitude > 0.0001f)
        {
            barRoot.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        }
    }
}