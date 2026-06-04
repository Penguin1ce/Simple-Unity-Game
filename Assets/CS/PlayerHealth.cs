using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("References")]
    public MonoBehaviour thirdPersonController;

    /// <summary>
    /// 是否死亡，供其他脚本判断
    /// </summary>
    public bool IsDead => isDead;

    private bool isDead = false;

    private void Awake()
    {
        currentHealth = maxHealth;

        // 自动获取ThirdPersonController
        if (thirdPersonController == null)
        {
            // StarterAssets的ThirdPersonController可能在这个名字下
            thirdPersonController = GetComponentInChildren<UnityEngine.InputSystem.PlayerInput>();
            // 或者直接用类型查找
            if (thirdPersonController == null)
            {
                // 查找任何包含"Controller"或"Control"的组件
                var components = GetComponents<MonoBehaviour>();
                foreach (var comp in components)
                {
                    var typeName = comp.GetType().Name;
                    if (typeName.Contains("Controller") || typeName.Contains("Control"))
                    {
                        thirdPersonController = comp;
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 死亡处理
    /// </summary>
    private void Die()
    {
        isDead = true;
        currentHealth = 0;

        // 打印日志
        Debug.Log("玩家死亡");

        // 禁用ThirdPersonController
        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = false;
        }
    }
}