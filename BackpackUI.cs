using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class BackpackUI : MonoBehaviour
{
    [Header("Background Sprites")]
    public Sprite panelBackground;
    public Sprite titleBarSprite;
    public Sprite closeButtonSprite;
    public Sprite slotFrameSprite;
    public Sprite tabActiveSprite;
    public Sprite tabInactiveSprite;

    [Header("Button Sprites")]
    public Sprite useButtonSprite;

    [Header("Default Icons")]
    public Sprite potionDefaultIcon;
    public Sprite questDefaultIcon;

    [Header("Weapon HUD")]
    public Image weaponHudIcon;
    public Text weaponHudName;
    public Text weaponHudAtk;

    private Button _potionTabBtn;
    private Button _questTabBtn;
    private Text _potionTabText;
    private Text _questTabText;
    private Transform _contentContainer;

    private bool _showingPotions = true;
    private bool _uiBuilt;
    private List<GameObject> _activeSlotObjects = new List<GameObject>();

    private void Start()
    {
        EnsureEventSystem();
        EnsureGraphicRaycaster();

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += Refresh;
        if (WeaponManager.Instance != null)
            WeaponManager.Instance.OnWeaponChanged += RefreshWeaponHUD;

        BuildUI();
        gameObject.SetActive(false);

        RefreshWeaponHUD(WeaponManager.Instance != null ? WeaponManager.Instance.CurrentWeaponId : null);
    }

    private void EnsureEventSystem()
    {
        UIEventSystemHelper.Ensure();
    }

    private void EnsureGraphicRaycaster()
    {
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null && parentCanvas.GetComponent<GraphicRaycaster>() == null)
        {
            parentCanvas.gameObject.AddComponent<GraphicRaycaster>();
            Debug.Log("[BackpackUI] Added GraphicRaycaster to parent Canvas.");
        }
    }

    private void BuildUI()
    {
        if (_uiBuilt) return;

        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null)
        {
            rt = gameObject.AddComponent<RectTransform>();
        }
        rt.sizeDelta = new Vector2(600, 450);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        if (panelBackground != null)
        {
            Image bg = GetComponent<Image>();
            if (bg == null) bg = gameObject.AddComponent<Image>();
            bg.sprite = panelBackground;
            bg.type = Image.Type.Sliced;
        }

        BuildTitleBar(rt);
        BuildTabBar(rt);
        BuildContentArea(rt);

        _uiBuilt = true;
    }

    private void BuildTitleBar(RectTransform panelRt)
    {
        GameObject titleBar = CreateUIElement("TitleBar", transform);
        RectTransform rt = titleBar.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(0, 40);

        if (titleBarSprite != null)
        {
            Image img = titleBar.AddComponent<Image>();
            img.sprite = titleBarSprite;
        }

        GameObject titleText = CreateUIElement("TitleText", titleBar.transform);
        RectTransform textRt = titleText.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;
        Text text = titleText.AddComponent<Text>();
        text.text = "背 包";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 22;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        GameObject closeBtn = CreateUIElement("CloseButton", titleBar.transform);
        RectTransform closeRt = closeBtn.GetComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(1, 0.5f);
        closeRt.anchorMax = new Vector2(1, 0.5f);
        closeRt.pivot = new Vector2(1, 0.5f);
        closeRt.anchoredPosition = new Vector2(-8, 0);
        closeRt.sizeDelta = new Vector2(30, 30);

        if (closeButtonSprite != null)
        {
            Image closeImg = closeBtn.AddComponent<Image>();
            closeImg.sprite = closeButtonSprite;
        }
        Button closeBtnComp = closeBtn.AddComponent<Button>();
        closeBtnComp.onClick.AddListener(() => gameObject.SetActive(false));
    }

    private void BuildTabBar(RectTransform panelRt)
    {
        GameObject tabBar = CreateUIElement("TabBar", transform);
        RectTransform rt = tabBar.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -45);
        rt.sizeDelta = new Vector2(-20, 35);

        Image tabBarBg = tabBar.AddComponent<Image>();
        tabBarBg.color = new Color(0, 0, 0, 0.4f);
        tabBarBg.raycastTarget = false;

        HorizontalLayoutGroup hlg = tabBar.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.padding = new RectOffset(10, 10, 2, 2);
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;

        _potionTabBtn = CreateTabButton(tabBar.transform, "消耗品", true);
        _questTabBtn = CreateTabButton(tabBar.transform, "任务道具", false);

        _potionTabBtn.onClick.AddListener(() => SwitchTab(true));
        _questTabBtn.onClick.AddListener(() => SwitchTab(false));

        UpdateTabVisuals();
    }

    private Button CreateTabButton(Transform parent, string label, bool isPotion)
    {
        GameObject go = CreateUIElement("Tab_" + label, parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(120, 30);

        Image tabImg = go.AddComponent<Image>();
        tabImg.color = new Color(0.3f, 0.3f, 0.3f);
        tabImg.raycastTarget = true;

        GameObject textGo = CreateUIElement("Label", go.transform);
        RectTransform textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;
        Text text = textGo.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 18;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        if (isPotion) _potionTabText = text;
        else _questTabText = text;

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = tabImg;
        btn.transition = Selectable.Transition.None;
        return btn;
    }

    private void BuildContentArea(RectTransform panelRt)
    {
        GameObject scrollGo = CreateUIElement("ScrollView", transform);
        RectTransform scrollRt = scrollGo.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(10, 10);
        scrollRt.offsetMax = new Vector2(-10, -85);

        ScrollRect scrollRect = scrollGo.AddComponent<ScrollRect>();

        GameObject viewport = CreateUIElement("Viewport", scrollGo.transform);
        RectTransform viewportRt = viewport.GetComponent<RectTransform>();
        viewportRt.anchorMin = Vector2.zero;
        viewportRt.anchorMax = Vector2.one;
        viewportRt.sizeDelta = Vector2.zero;
        Image viewportImg = viewport.AddComponent<Image>();
        viewportImg.color = new Color(0, 0, 0, 0.3f);
        viewportImg.raycastTarget = false;
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = true;

        GameObject content = CreateUIElement("Content", viewport.transform);
        RectTransform contentRt = content.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0, 1);
        contentRt.anchoredPosition = Vector2.zero;
        contentRt.sizeDelta = new Vector2(0, 0);

        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 5;
        vlg.padding = new RectOffset(5, 5, 5, 5);
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRt;
        scrollRect.content = contentRt;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        _contentContainer = content.transform;
    }

    private void SwitchTab(bool showPotions)
    {
        _showingPotions = showPotions;
        UpdateTabVisuals();
        Refresh();
        Debug.Log(string.Format("[BackpackUI] Tab switched to: {0}", showPotions ? "消耗品" : "任务道具"));
    }

    private void UpdateTabVisuals()
    {
        if (_potionTabBtn != null)
        {
            _potionTabBtn.image.sprite = _showingPotions ? tabActiveSprite : tabInactiveSprite;
            _potionTabBtn.image.color = _showingPotions ? new Color(0.5f, 0.5f, 0.5f) : new Color(0.2f, 0.2f, 0.2f);
            if (_potionTabText != null)
                _potionTabText.color = _showingPotions ? Color.white : Color.gray;
        }
        if (_questTabBtn != null)
        {
            _questTabBtn.image.sprite = !_showingPotions ? tabActiveSprite : tabInactiveSprite;
            _questTabBtn.image.color = !_showingPotions ? new Color(0.5f, 0.5f, 0.5f) : new Color(0.2f, 0.2f, 0.2f);
            if (_questTabText != null)
                _questTabText.color = !_showingPotions ? Color.white : Color.gray;
        }
    }

    private void Refresh()
    {
        if (!_uiBuilt || InventoryManager.Instance == null) return;

        ClearSlots();

        InventorySlot[] slots = InventoryManager.Instance.GetAllSlots();
        ItemCategory targetCategory = _showingPotions ? ItemCategory.Potion : ItemCategory.QuestItem;

        int matchedCount = 0;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty) continue;

            InventoryItemData def = InventoryManager.Instance.GetItemDefinition(slots[i].itemId);
            if (def != null && def.itemCategory != targetCategory) continue;

            CreateSlotUI(_contentContainer, slots[i].itemId, def, slots[i].count);
            matchedCount++;
        }

        if (matchedCount == 0)
        {
            GameObject emptyGo = CreateUIElement("EmptyHint", _contentContainer);
            RectTransform emptyRt = emptyGo.GetComponent<RectTransform>();
            emptyRt.sizeDelta = new Vector2(0, 30);
            Text emptyText = emptyGo.AddComponent<Text>();
            emptyText.text = string.Format("暂无{0}", _showingPotions ? "消耗品" : "任务道具");
            emptyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            emptyText.fontSize = 16;
            emptyText.alignment = TextAnchor.MiddleCenter;
            emptyText.color = Color.gray;
            _activeSlotObjects.Add(emptyGo);
        }
    }

    private void ClearSlots()
    {
        for (int i = _activeSlotObjects.Count - 1; i >= 0; i--)
        {
            if (_activeSlotObjects[i] != null)
                Destroy(_activeSlotObjects[i]);
        }
        _activeSlotObjects.Clear();
    }

    private void CreateSlotUI(Transform parent, string itemId, InventoryItemData def, int count)
    {
        GameObject slotGo = CreateUIElement("Slot_" + itemId, parent);
        RectTransform slotRt = slotGo.GetComponent<RectTransform>();
        slotRt.sizeDelta = new Vector2(0, 48);

        if (slotFrameSprite != null)
        {
            Image slotBg = slotGo.AddComponent<Image>();
            slotBg.sprite = slotFrameSprite;
            slotBg.type = Image.Type.Sliced;
        }

        HorizontalLayoutGroup hlg = slotGo.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8;
        hlg.padding = new RectOffset(8, 8, 4, 4);
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        GameObject iconGo = CreateUIElement("Icon", slotGo.transform);
        RectTransform iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.sizeDelta = new Vector2(32, 32);
        LayoutElement iconLe = iconGo.AddComponent<LayoutElement>();
        iconLe.preferredWidth = 32;
        iconLe.preferredHeight = 32;
        iconLe.minWidth = 32;
        iconLe.minHeight = 32;
        Image iconImg = iconGo.AddComponent<Image>();
        iconImg.sprite = def != null && def.icon != null ? def.icon : GetDefaultIcon(def != null ? def.itemCategory : ItemCategory.Weapon);
        iconImg.preserveAspect = true;

        GameObject infoGo = CreateUIElement("Info", slotGo.transform);
        RectTransform infoRt = infoGo.GetComponent<RectTransform>();
        infoRt.sizeDelta = new Vector2(120, 40);
        VerticalLayoutGroup infoVlg = infoGo.AddComponent<VerticalLayoutGroup>();
        infoVlg.childAlignment = TextAnchor.MiddleLeft;
        infoVlg.childForceExpandWidth = true;
        infoVlg.childForceExpandHeight = false;

        GameObject nameGo = CreateUIElement("Name", infoGo.transform);
        Text nameText = nameGo.AddComponent<Text>();
        nameText.text = def != null ? def.itemName : itemId;
        nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        nameText.fontSize = 16;
        nameText.color = Color.white;

        GameObject countGo = CreateUIElement("Count", infoGo.transform);
        Text countText = countGo.AddComponent<Text>();
        countText.text = "x" + count;
        countText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        countText.fontSize = 14;
        countText.color = new Color(0.8f, 0.8f, 0.8f);

        if (def != null && def.itemCategory == ItemCategory.Potion)
        {
            GameObject btnGo = CreateUIElement("UseBtn", slotGo.transform);
            RectTransform btnRt = btnGo.GetComponent<RectTransform>();
            btnRt.sizeDelta = new Vector2(60, 35);

            if (useButtonSprite != null)
            {
                Image btnImg = btnGo.AddComponent<Image>();
                btnImg.sprite = useButtonSprite;
            }

            Button useBtn = btnGo.AddComponent<Button>();
            string useItemId = itemId;
            useBtn.onClick.AddListener(() => { InventoryManager.Instance.UsePotion(useItemId); });

            GameObject btnTextGo = CreateUIElement("Label", btnGo.transform);
            RectTransform btnTextRt = btnTextGo.GetComponent<RectTransform>();
            btnTextRt.anchorMin = Vector2.zero;
            btnTextRt.anchorMax = Vector2.one;
            btnTextRt.sizeDelta = Vector2.zero;
            Text btnText = btnTextGo.AddComponent<Text>();
            btnText.text = "使用";
            btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnText.fontSize = 14;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = Color.white;
        }

        _activeSlotObjects.Add(slotGo);
    }

    private Sprite GetDefaultIcon(ItemCategory category)
    {
        switch (category)
        {
            case ItemCategory.Potion: return potionDefaultIcon;
            case ItemCategory.QuestItem: return questDefaultIcon;
            default: return null;
        }
    }

    private void RefreshWeaponHUD(string weaponId)
    {
        if (WeaponManager.Instance == null) return;

        InventoryItemData data = WeaponManager.Instance.CurrentWeaponData;
        if (weaponHudIcon != null)
            weaponHudIcon.sprite = data != null ? data.icon : null;
        if (weaponHudName != null)
            weaponHudName.text = data != null ? data.itemName : "";
        if (weaponHudAtk != null)
            weaponHudAtk.text = data != null ? "ATK: " + data.attackPower : "";
    }

    public void Toggle()
    {
        bool newState = !gameObject.activeSelf;
        gameObject.SetActive(newState);
        if (newState) Refresh();
    }

    public void Open()
    {
        gameObject.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= Refresh;
        if (WeaponManager.Instance != null)
            WeaponManager.Instance.OnWeaponChanged -= RefreshWeaponHUD;
    }
}
