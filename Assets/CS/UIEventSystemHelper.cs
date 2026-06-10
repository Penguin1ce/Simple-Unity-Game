using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 全局 EventSystem 工具，确保场景中永远只有一个 EventSystem。
/// 每次调用都会检查并清理多余的 EventSystem。
/// </summary>
public static class UIEventSystemHelper
{
    public static void Ensure()
    {
        EventSystem[] all = UnityEngine.Object.FindObjectsOfType<EventSystem>();

        // 如果有多个，只保留第一个，删除其余的
        if (all.Length > 1)
        {
            for (int i = 1; i < all.Length; i++)
            {
                Debug.Log(string.Format("[UIEventSystemHelper] Removing duplicate EventSystem: {0}", all[i].gameObject.name));
                UnityEngine.Object.Destroy(all[i].gameObject);
            }
            return;
        }

        // 如果一个都没有，创建一个
        if (all.Length == 0)
        {
            GameObject go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
            Debug.Log("[UIEventSystemHelper] Created EventSystem.");
        }
    }
}
