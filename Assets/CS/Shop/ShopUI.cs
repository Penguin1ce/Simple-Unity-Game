using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopUI : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite panelBackground;
    public Sprite titleBarSprite;
    public Sprite closeButtonSprite;
    public Sprite slotFrameSprite;
    public Sprite buyButtonSprite;

    [Header("Default Icons")]
    public Sprite potionDefaultIcon;
    public Sprite questDefaultIcon;

    [Header("Config")]
    public ShopConfig shopConfig;
    public Text goldText;

    private Transform _contentContainer;
    private bool _uiBuilt;
    private List<GameObject> _activeSlotObjects = new List<GameObject>();

    private void Start()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnGoldChanged += RefreshGold;
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += RefreshItems;

        BuildUI();
        RefreshGold(CurrencyManager.Instance != null ? CurrencyManager.Instance.Gold : 0);
    }

    private void BuildUI()
    {
        if (_uiBuilt) return;

        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null)
        {
            rt = gameObject.AddComponent<RectTransform>();
        }
        rt.sizeDelta = new Vector2(500, 400);
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
        BuildGoldDisplay(rt);
        BuildContentArea(rt);

        _uiBuilt = true;
    }

    private void BuildTitleBar(RectTransform panelRt)
    {
        GameObject titleBar = CreateUIElement("TitleBar", transform);
        RectTransform titleRt = titleBar.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 1);
        titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot = new Vector2(0.5f, 1);
        titleRt.anchoredPosition = Vector2.zero;
        titleRt.sizeDelta = new Vector2(0, 36);

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
        text.text = "商 店";
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
        closeRt.sizeDelta = new Vector2(28, 28);

        if (closeButtonSprite != null)
        {
            Image closeImg = closeBtn.AddComponent<Image>();
            closeImg.sprite = closeButtonSprite;
        }
        Button closeBtnComp = closeBtn.AddComponent<Button>();
        closeBtnComp.onClick.AddListener(() => gameObject.SetActive(false));
    }

    private void BuildGoldDisplay(RectTransform panelRt)
    {
        GameObject goldBar = CreateUIElement("GoldBar", transform);
        RectTransform goldRt = goldBar.GetComponent<RectTransform>();
        goldRt.anchorMin = new Vector2(0, 1);
        goldRt.anchorMax = new Vector2(1, 1);
        goldRt.pivot = new Vector2(0.5f, 1);
        goldRt.anchoredPosition = new Vector2(0, -40);
        goldRt.sizeDelta = new Vector2(-20, 30);

        Image goldBg = goldBar.AddComponent<Image>();
        goldBg.color = new Color(0, 0, 0, 0.4f);
        goldBg.raycastTarget = false;

        GameObject goldLabel = CreateUIElement("GoldLabel", goldBar.transform);
        RectTransform labelRt = goldLabel.GetComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.sizeDelta = Vector2.zero;
        goldText = goldLabel.AddComponent<Text>();
        goldText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        goldText.fontSize = 18;
        goldText.alignment = TextAnchor.MiddleCenter;
        goldText.color = Color.yellow;
    }

    private void BuildContentArea(RectTransform panelRt)
    {
        GameObject scrollGo = CreateUIElement("ScrollView", transform);
        RectTransform scrollRt = scrollGo.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(10, 10);
        scrollRt.offsetMax = new Vector2(-10, -76);

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
        vlg.childAlignment = TextAnchor.UpperLeft;
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

    private void RefreshGold(int gold)
    {
        if (goldText != null)
            goldText.text = string.Format("金币: {0}", gold);
    }

    private void RefreshItems()
    {
        if (!_uiBuilt || !gameObject.activeSelf) return;
        ClearSlots();

        if (shopConfig == null) return;

        for (int i = 0; i < shopConfig.items.Count; i++)
        {
            ShopEntry entry = shopConfig.items[i];
            InventoryItemData def = InventoryManager.Instance != null
                ? InventoryManager.Instance.GetItemDefinition(entry.itemId) : null;

            string itemName = def != null ? def.itemName : entry.itemId;
            Sprite icon = def != null && def.icon != null ? def.icon : potionDefaultIcon;

            Debug.Log(string.Format("[ShopUI] def={0}, itemName='{1}', icon={2}",
                def != null, itemName, icon != null));
            CreateShopSlot(_contentContainer, entry.itemId, itemName, icon, entry.price, true);
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

    private void CreateShopSlot(Transform parent, string itemId, string itemName, Sprite icon, int price, bool showName)
    {
        bool canAfford = CurrencyManager.Instance != null && CurrencyManager.Instance.HasGold(price);

        GameObject slotGo = CreateUIElement("Slot_" + itemId, parent);
        RectTransform slotRt = slotGo.GetComponent<RectTransform>();
        slotRt.sizeDelta = new Vector2(0, 44);

        LayoutElement slotLe = slotGo.AddComponent<LayoutElement>();
        slotLe.preferredHeight = 44;
        slotLe.minHeight = 44;

        if (slotFrameSprite != null)
        {
            Image slotBg = slotGo.AddComponent<Image>();
            slotBg.sprite = slotFrameSprite;
            slotBg.type = Image.Type.Sliced;
        }

        HorizontalLayoutGroup hlg = slotGo.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8;
        hlg.padding = new RectOffset(10, 10, 4, 4);
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        if (icon != null)
        {
            GameObject go = CreateUIElement("Icon", slotGo.transform);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(32, 32);
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.preferredWidth = 32; le.preferredHeight = 32;
            le.minWidth = 32; le.minHeight = 32;
            Image img = go.AddComponent<Image>();
            img.sprite = icon;
            img.preserveAspect = true;
        }

        if (showName)
        {
            GameObject go = CreateUIElement("Name", slotGo.transform);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(90, 28);
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.preferredWidth = 90; le.preferredHeight = 28;
            Text txt = go.AddComponent<Text>();
            txt.text = itemName;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 16;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.color = Color.white;
        }

        GameObject priceGo = CreateUIElement("Price", slotGo.transform);
        RectTransform priceRt = priceGo.GetComponent<RectTransform>();
        priceRt.sizeDelta = new Vector2(70, 28);
        LayoutElement priceLe = priceGo.AddComponent<LayoutElement>();
        priceLe.preferredWidth = 70; priceLe.preferredHeight = 28;
        Text priceText = priceGo.AddComponent<Text>();
        priceText.text = string.Format("{0} 金币", price);
        priceText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        priceText.fontSize = 14;
        priceText.alignment = TextAnchor.MiddleLeft;
        priceText.color = canAfford ? Color.yellow : Color.gray;

        GameObject spacer = CreateUIElement("Spacer", slotGo.transform);
        LayoutElement spacerLe = spacer.AddComponent<LayoutElement>();
        spacerLe.flexibleWidth = 1;

        GameObject btnGo = CreateUIElement("BuyBtn", slotGo.transform);
        RectTransform btnRt = btnGo.GetComponent<RectTransform>();
        btnRt.sizeDelta = new Vector2(70, 32);
        LayoutElement btnLe = btnGo.AddComponent<LayoutElement>();
        btnLe.preferredWidth = 70; btnLe.preferredHeight = 32;

        if (buyButtonSprite != null)
        {
            Image btnImg = btnGo.AddComponent<Image>();
            btnImg.sprite = buyButtonSprite;
        }
        else
        {
            Image btnImg = btnGo.AddComponent<Image>();
            btnImg.color = canAfford ? new Color(0.3f, 0.7f, 0.3f) : new Color(0.3f, 0.3f, 0.3f);
        }

        Button buyBtn = btnGo.AddComponent<Button>();
        buyBtn.transition = Selectable.Transition.None;
        buyBtn.interactable = canAfford;
        string capturedId = itemId;
        int capturedPrice = price;
        buyBtn.onClick.AddListener(() =>
        {
            Debug.Log(string.Format("[ShopUI] Buy clicked: {0} for {1}G", capturedId, capturedPrice));
            if (CurrencyManager.Instance == null)
            {
                Debug.LogError("[ShopUI] CurrencyManager.Instance is null!");
                return;
            }
            if (!CurrencyManager.Instance.SpendGold(capturedPrice))
            {
                Debug.LogWarning(string.Format("[ShopUI] SpendGold failed for {0}G", capturedPrice));
                return;
            }
            if (InventoryManager.Instance == null)
            {
                Debug.LogError("[ShopUI] InventoryManager.Instance is null!");
                return;
            }
            InventoryManager.Instance.AddItem(capturedId, 1);
            Debug.Log(string.Format("[ShopUI] Added {0} to inventory, current count: {1}",
                capturedId, InventoryManager.Instance.GetItemCount(capturedId)));
        });

        GameObject btnTextGo = CreateUIElement("Label", btnGo.transform);
        RectTransform btnTextRt = btnTextGo.GetComponent<RectTransform>();
        btnTextRt.anchorMin = Vector2.zero; btnTextRt.anchorMax = Vector2.one;
        btnTextRt.sizeDelta = Vector2.zero;
        Text btnText = btnTextGo.AddComponent<Text>();
        btnText.text = canAfford ? "购买" : "金币不足";
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = 14;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;

        _activeSlotObjects.Add(slotGo);
        Debug.Log(string.Format("[ShopUI] Created slot: {0}, icon={1}, name={2}, price={3}G",
            itemId, icon != null, showName, price));
    }

    public void Toggle()
    {
        bool newState = !gameObject.activeSelf;
        gameObject.SetActive(newState);
        if (newState)
        {
            ShowCursor();
            EnsureEventSystem();
            RefreshItems();
            RefreshGold(CurrencyManager.Instance != null ? CurrencyManager.Instance.Gold : 0);
        }
        else
        {
            RestoreCursor();
        }
    }

    public void Open()
    {
        gameObject.SetActive(true);
        ShowCursor();
        EnsureEventSystem();
        RefreshItems();
        RefreshGold(CurrencyManager.Instance != null ? CurrencyManager.Instance.Gold : 0);
    }

    public void Close()
    {
        gameObject.SetActive(false);
        RestoreCursor();
    }

    private void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void RestoreCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void EnsureEventSystem()
    {
        UIEventSystemHelper.Ensure();

        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null && parentCanvas.GetComponent<GraphicRaycaster>() == null)
            parentCanvas.gameObject.AddComponent<GraphicRaycaster>();
    }

    private GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnGoldChanged -= RefreshGold;
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshItems;
    }
}