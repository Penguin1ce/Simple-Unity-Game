using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Hit Settings")]
    [Tooltip("受击动画时长（秒），在此时间内玩家无法操作")]
    public float hitStunDuration = 0.5f;

    [Header("Movement Lock")]
    [Tooltip("拖入玩家的移动控制脚本，受击时自动禁用")]
    public MonoBehaviour movementController;

    [Header("References")]
    public MonoBehaviour thirdPersonController;

    /// <summary>
    /// 是否死亡
    /// </summary>
    public bool IsDead => isDead;

    /// <summary>
    /// 是否正在受击硬直中
    /// </summary>
    public bool IsInHitStun { get; private set; }

    private bool isDead = false;
    private Animator animator;
    private CharacterController characterController;
    private PlayerAttack playerAttack;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator == null) animator = GetComponentInParent<Animator>();
        if (animator == null)
            Debug.LogError("PlayerHealth: Animator component not found!", this);
        else
            Debug.Log($"PlayerHealth: Animator found on {animator.gameObject.name}", this);

        playerAttack = GetComponent<PlayerAttack>();

        // 自动查找移动控制器
        if (movementController == null)
        {
            movementController = GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (movementController == null)
                movementController = GetComponent<StarterAssets.StarterAssetsInputs>();
            if (movementController == null && thirdPersonController != null)
                movementController = thirdPersonController;
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
        else
        {
            // ★ 攻击霸体：正在连招时不受击退、不播受击动画
            bool hasSuperArmor = playerAttack != null && playerAttack.IsAttacking;

            if (!hasSuperArmor && animator != null)
            {
                animator.ResetTrigger("Attack1");
                animator.ResetTrigger("Attack2");
                animator.ResetTrigger("Attack3");
                animator.SetTrigger("TrigHit");
                StartCoroutine(FinishHitAnimation());
            }

            // 攻击时不锁移动，受击才锁
            if (!hasSuperArmor)
                DisablePlayerControlTemporarily();
        }
    }

    /// <summary>
    /// 禁用玩家控制（防滑步），并在受击动画结束后恢复
    /// 优先走 PlayerAttack 的引用计数锁，避免与攻击锁冲突
    /// </summary>
    private void DisablePlayerControlTemporarily()
    {
        IsInHitStun = true;
        if (playerAttack != null)
        {
            playerAttack.LockMovement(hitStunDuration);
            StartCoroutine(EndHitStun(hitStunDuration));
        }
        else if (movementController != null)
        {
            movementController.enabled = false;
            StartCoroutine(RestorePlayerControlAfterHit());
        }
    }

    private System.Collections.IEnumerator EndHitStun(float delay)
    {
        yield return new WaitForSeconds(delay);
        IsInHitStun = false;
    }

    /// <summary>
    /// 受击动画播完后通过 ResetCombo 回到 Idle（仅当玩家没在连招时）
    /// </summary>
    private System.Collections.IEnumerator FinishHitAnimation()
    {
        yield return new WaitForSeconds(hitStunDuration);
        bool isAttacking = playerAttack != null && playerAttack.IsAttacking;
        if (animator != null && !isDead && !isAttacking)
            animator.SetTrigger("ResetCombo");
    }

    /// <summary>
    /// 受击后恢复玩家控制的协程（仅在无 PlayerAttack 时使用）
    /// </summary>
    private System.Collections.IEnumerator RestorePlayerControlAfterHit()
    {
        yield return new WaitForSeconds(hitStunDuration);

        IsInHitStun = false;
        if (isDead) yield break;

        if (movementController != null)
            movementController.enabled = true;
    }

    /// <summary>
    /// 禁用玩家控制（死亡时永久禁用）
    /// </summary>
    private void DisablePlayerControl()
    {
        if (movementController != null)
            movementController.enabled = false;
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