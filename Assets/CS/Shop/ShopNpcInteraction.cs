using UnityEngine;
using UnityEngine.UI;

public class ShopNpcInteraction : MonoBehaviour
{
    [Header("Shop Reference")]
    public ShopUI shopUI;

    [Tooltip("Key to interact with the NPC. Default: E")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Trigger Settings")]
    [Tooltip("Radius of the interaction trigger. Auto-created at runtime.")]
    public float triggerRadius = 2f;

    [Header("Prompt")]
    public GameObject promptUI;
    public Text promptText;

    private bool _playerInRange;
    private bool _initialized;

    private void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);

        EnsureTriggerCollider();

        // 初始关闭商店面板
        if (shopUI != null)
        {
            shopUI.Close();
            _initialized = true;
        }
    }

    private void EnsureTriggerCollider()
    {
        SphereCollider[] existing = GetComponents<SphereCollider>();
        for (int i = 0; i < existing.Length; i++)
        {
            if (existing[i].isTrigger) return;
        }

        SphereCollider trigger = gameObject.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = triggerRadius;
        Debug.Log(string.Format("[ShopNpc] Auto-added trigger SphereCollider (radius={0}). " +
            "Keep the main BoxCollider as non-trigger for physics.", triggerRadius));
    }

    private void Update()
    {
        // 如果 Start 时 shopUI 还没赋值，延迟初始化
        if (!_initialized && shopUI != null)
        {
            shopUI.Close();
            _initialized = true;
        }

        if (_playerInRange && Input.GetKeyDown(interactKey))
        {
            if (shopUI == null)
            {
                Debug.LogError("[ShopNpc] shopUI is null! Please assign the ShopCanvas to the Shop UI field in Inspector.");
                return;
            }
            shopUI.Toggle();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;
            ShowPrompt(true);
            Debug.Log("[ShopNpc] Player entered shop range. Press E to open.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
            ShowPrompt(false);
            if (shopUI != null)
                shopUI.Close();
            Debug.Log("[ShopNpc] Player left shop range.");
        }
    }

    private void ShowPrompt(bool show)
    {
        if (promptUI != null)
            promptUI.SetActive(show);
        if (show && promptText != null)
            promptText.text = string.Format("按 [{0}] 打开商店", interactKey);
    }
}
