using UnityEngine;
using UnityEngine.UI;

public class QuickSlotBar : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite slotFrame;
    public Sprite healthIcon;
    public Sprite manacIcon;
    public Sprite attackIcon;

    [Header("Item IDs")]
    public string healthId = "potion_health";
    public string manacId = "potion_mana";
    public string attackId = "potion_attack_boost";

    private Text[] _countTexts = new Text[3];
    private Image[] _icons = new Image[3];
    private string[] _ids;

    private void Start()
    {
        _ids = new string[] { healthId, manacId, attackId };
        BuildBar();

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += Refresh;
        Refresh();
    }

    private void BuildBar()
    {
        RectTransform rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(330, 85);

        Image bg = GetComponent<Image>();
        if (bg == null) bg = gameObject.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.5f);

        HorizontalLayoutGroup hlg = gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 6;
        hlg.padding = new RectOffset(12, 12, 6, 6);
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        Sprite[] icons = { healthIcon, manacIcon, attackIcon };
        string[] keys = { "Z", "X", "C" };

        for (int i = 0; i < 3; i++)
        {
            BuildSlot(i, icons[i], keys[i]);
        }
    }

    private void BuildSlot(int index, Sprite icon, string key)
    {
        GameObject slotGo = new GameObject("Slot_" + key, typeof(RectTransform));
        slotGo.transform.SetParent(transform, false);
        RectTransform slotRt = slotGo.GetComponent<RectTransform>();
        slotRt.sizeDelta = new Vector2(96, 72);

        LayoutElement slotLe = slotGo.AddComponent<LayoutElement>();
        slotLe.preferredWidth = 96; slotLe.preferredHeight = 72;

        if (slotFrame != null)
        {
            Image frameImg = slotGo.AddComponent<Image>();
            frameImg.sprite = slotFrame;
            frameImg.type = Image.Type.Sliced;
        }

        // 图标
        GameObject iconGo = new GameObject("Icon", typeof(RectTransform));
        iconGo.transform.SetParent(slotGo.transform, false);
        RectTransform iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.5f, 1); iconRt.anchorMax = new Vector2(0.5f, 1);
        iconRt.pivot = new Vector2(0.5f, 1);
        iconRt.anchoredPosition = new Vector2(0, -6);
        iconRt.sizeDelta = new Vector2(36, 36);
        Image iconImg = iconGo.AddComponent<Image>();
        iconImg.sprite = icon != null ? icon : null;
        iconImg.preserveAspect = true;
        _icons[index] = iconImg;

        // 快捷键
        GameObject keyGo = new GameObject("KeyHint", typeof(RectTransform));
        keyGo.transform.SetParent(slotGo.transform, false);
        RectTransform keyRt = keyGo.GetComponent<RectTransform>();
        keyRt.anchorMin = new Vector2(0, 1); keyRt.anchorMax = new Vector2(0, 1);
        keyRt.pivot = new Vector2(0, 1);
        keyRt.anchoredPosition = new Vector2(4, -4);
        keyRt.sizeDelta = new Vector2(22, 18);
        Text keyText = keyGo.AddComponent<Text>();
        keyText.text = key;
        keyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        keyText.fontSize = 12;
        keyText.fontStyle = FontStyle.Bold;
        keyText.alignment = TextAnchor.MiddleCenter;
        keyText.color = Color.yellow;

        // 数量
        GameObject countGo = new GameObject("Count", typeof(RectTransform));
        countGo.transform.SetParent(slotGo.transform, false);
        RectTransform countRt = countGo.GetComponent<RectTransform>();
        countRt.anchorMin = new Vector2(0.5f, 0); countRt.anchorMax = new Vector2(0.5f, 0);
        countRt.pivot = new Vector2(0.5f, 0);
        countRt.anchoredPosition = new Vector2(0, 6);
        countRt.sizeDelta = new Vector2(80, 18);
        Text countText = countGo.AddComponent<Text>();
        countText.text = "x0";
        countText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        countText.fontSize = 13;
        countText.alignment = TextAnchor.MiddleCenter;
        countText.color = Color.white;
        _countTexts[index] = countText;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z)) UseSlot(0);
        if (Input.GetKeyDown(KeyCode.X)) UseSlot(1);
        if (Input.GetKeyDown(KeyCode.C)) UseSlot(2);
    }

    private void UseSlot(int index)
    {
        if (_ids == null || index >= _ids.Length) return;
        if (string.IsNullOrEmpty(_ids[index])) return;
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.UsePotion(_ids[index]);
    }

    private void Refresh()
    {
        for (int i = 0; i < 3; i++)
        {
            int count = InventoryManager.Instance != null
                ? InventoryManager.Instance.GetItemCount(_ids[i]) : 0;

            if (_countTexts[i] != null)
                _countTexts[i].text = "x" + count;

            if (_icons[i] != null)
                _icons[i].color = count > 0 ? Color.white : new Color(0.35f, 0.35f, 0.35f, 0.4f);
        }
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= Refresh;
    }
}
