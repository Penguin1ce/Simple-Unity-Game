using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Hit Settings")]
    [Tooltip("受击动画时长（秒），在此时间内玩家无法操作")]
    public float hitStunDuration = 0.5f;

    [Header("References")]
    public MonoBehaviour thirdPersonController;

    /// <summary>
    /// 是否死亡，供其他脚本判断
    /// </summary>
    public bool IsDead => isDead;

    public event Action<float, float> HealthChanged;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private bool isDead = false;
    private Animator animator;
    private MonoBehaviour playerInputComponent;
    private CharacterController characterController;

    private void Awake()
    {
        if (currentHealth <= 0f || currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        }

        NotifyHealthChanged();
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth <= 0f ? maxHealth : currentHealth, 0f, maxHealth);
    }

    private void Start()
    {
        // 获取CharacterController组件
        characterController = GetComponent<CharacterController>();
        // 获取Animator组件 - 优先本物体，然后是子物体和父物体
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        if (animator == null)
        {
            animator = GetComponentInParent<Animator>();
        }

        if (animator == null)
        {
            Debug.LogWarning("PlayerHealth: Animator component not found (Cube player is fine)", this);
        }
        else
        {
            Debug.Log($"PlayerHealth: Animator found on {animator.gameObject.name}", this);
        }

        // 获取玩家输入控制组件
        playerInputComponent = GetComponent<UnityEngine.InputSystem.PlayerInput>();

        // 如果PlayerInput没找到，尝试获取StarterAssetsInputs
        if (playerInputComponent == null)
        {
            var inputs = GetComponent<StarterAssets.StarterAssetsInputs>();
            if (inputs != null)
            {
                playerInputComponent = inputs;
            }
        }

        // 如果都没找到，尝试查找ThirdPersonController
        if (playerInputComponent == null && thirdPersonController == null)
        {
            var components = GetComponents<MonoBehaviour>();
            foreach (var comp in components)
            {
                if (comp == null) continue;
                var typeName = comp.GetType().Name;
                if (typeName.Contains("ThirdPersonController"))
                {
                    playerInputComponent = comp;
                    break;
                }
            }
        }

        // 使用外部引用作为备选
        if (playerInputComponent == null && thirdPersonController != null)
        {
            playerInputComponent = thirdPersonController;
        }
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0f, currentHealth - damage);
        NotifyHealthChanged();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // 玩家没死，触发受击动画
            if (animator != null)
            {
                Debug.Log("PlayerHealth: 触发 TrigHit 动画触发器");
                animator.SetTrigger("TrigHit");
            }

            // 防滑步：禁用玩家控制，并在受击动画结束后恢复
            DisablePlayerControlTemporarily();
        }
    }

    /// <summary>
    /// 回复生命值
    /// </summary>
    /// <param name="amount">回复量</param>
    public void Heal(float amount)
    {
        if (isDead || amount <= 0f)
        {
            return;
        }

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        NotifyHealthChanged();
    }

    /// <summary>
    /// 禁用玩家控制（防滑步），并在受击动画结束后恢复
    /// </summary>
    private void DisablePlayerControlTemporarily()
    {
        if (playerInputComponent != null)
        {
            Debug.Log($"PlayerHealth: 禁用玩家控制 - {playerInputComponent.GetType().Name}，将在 {hitStunDuration} 秒后恢复");
            playerInputComponent.enabled = false;

            // 启动协程在受击动画结束后恢复控制
            StartCoroutine(RestorePlayerControlAfterHit());
        }
        else
        {
            Debug.LogWarning("PlayerHealth: playerInputComponent 为 null，无法禁用玩家控制", this);
        }
    }

    /// <summary>
    /// 受击后恢复玩家控制的协程
    /// </summary>
    private System.Collections.IEnumerator RestorePlayerControlAfterHit()
    {
        // 等待受击动画时长
        yield return new WaitForSeconds(hitStunDuration);

        // 如果玩家已死亡，不恢复控制
        if (isDead)
        {
            Debug.Log("PlayerHealth: 玩家已死亡，不恢复控制");
            yield break;
        }

        // 恢复玩家控制
        if (playerInputComponent != null)
        {
            Debug.Log($"PlayerHealth: 恢复玩家控制 - {playerInputComponent.GetType().Name}");
            playerInputComponent.enabled = true;
        }
    }

    /// <summary>
    /// 禁用玩家控制（防滑步）
    /// </summary>
    private void DisablePlayerControl()
    {
        if (playerInputComponent != null)
        {
            Debug.Log($"PlayerHealth: 禁用玩家控制 - {playerInputComponent.GetType().Name}");
            playerInputComponent.enabled = false;
        }
        else
        {
            Debug.LogWarning("PlayerHealth: playerInputComponent 为 null，无法禁用玩家控制", this);
        }
    }

    /// <summary>
    /// 死亡处理
    /// </summary>
    private void Die()
    {
        isDead = true;
        currentHealth = 0;
        NotifyHealthChanged();

        // 打印日志
        Debug.Log("玩家死亡");

        // 步骤1：禁用CharacterController组件
        if (characterController != null)
        {
            characterController.enabled = false;
            Debug.Log("PlayerHealth: CharacterController已禁用");
        }

        // 禁用ThirdPersonController脚本
        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = false;
            Debug.Log($"PlayerHealth: ThirdPersonController已禁用");
        }

        // 步骤2：播放死亡动画（Animator触发"TrigDead"）
        if (animator != null)
        {
            Debug.Log("PlayerHealth: 触发 TrigDead 动画触发器");
            animator.SetTrigger("TrigDead");
        }
        else
        {
            Debug.LogError("PlayerHealth: Die() 被调用，但 animator 为 null！", this);
        }

        // 防滑步：禁用玩家控制
        DisablePlayerControl();

        // 步骤3：1.5秒内缩小角色并输出游戏结束
        StartCoroutine(ShrinkAndGameOver());
    }

    private void NotifyHealthChanged()
    {
        if (HealthChanged != null)
        {
            HealthChanged(currentHealth, maxHealth);
        }
    }

    /// <summary>
    /// 1.5秒内将角色Scale从1缩小到0，然后输出游戏结束
    /// </summary>
    private System.Collections.IEnumerator ShrinkAndGameOver()
    {
        float shrinkDuration = 1.5f;
        float elapsedTime = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = Vector3.zero;

        while (elapsedTime < shrinkDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / shrinkDuration);
            transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
            yield return null;
        }

        // 确保Scale最终为0
        transform.localScale = Vector3.zero;
        Debug.Log("游戏结束");
    }
}