using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Light Attack Settings")]
    public float lightAttackRange = 1.5f;
    public float lightAttackDistance = 2f;
    [Tooltip("轻击动画必须播放到多少百分比（0~1）才能接下一段，默认0.85表示85%")]
    public float minAnimationPercent = 0.85f;
    [Tooltip("连招时间窗口（秒），上段动画播完后多久内按左键可以接下一段")]
    public float comboWindow = 0.5f;
    [Tooltip("轻击伤害倍率")]
    public float lightDamageMultiplier = 1f;

    [Header("Heavy Attack Settings")]
    public float heavyAttackRange = 2f;
    public float heavyAttackDistance = 2.5f;
    public float heavyAttackCooldown = 1.2f;
    [Tooltip("重击伤害结算延迟（秒），配合动画挥击时机")]
    public float heavyAttackDelay = 1.0f;
    [Tooltip("重击伤害倍率")]
    public float heavyDamageMultiplier = 2f;

    [Header("Base Damage")]
    [Tooltip("Base damage when no weapon is equipped.")]
    public int baseDamage = 10;

    [Header("Movement Lock")]
    [Tooltip("拖入玩家的移动控制脚本（ThirdPersonController 或 StarterAssetsInputs），攻击时自动禁用")]
    public MonoBehaviour movementController;
    [Tooltip("轻击时禁止移动的时间（秒）")]
    public float lightAttackLockTime = 0.7f;
    [Tooltip("重击时禁止移动的时间（秒）")]
    public float heavyAttackLockTime = 1.2f;

    [Header("References")]
    public Animator animator;

    /// <summary>
    /// 是否正在连招中（给 PlayerHealth 判断用）
    /// </summary>
    public bool IsAttacking => comboStep > 0;

    /// <summary>
    /// 当前实际伤害 = 基础伤害 + 武器攻击力
    /// </summary>
    public int CurrentDamage
    {
        get
        {
            int total = baseDamage;
            if (WeaponManager.Instance != null && WeaponManager.Instance.CurrentWeaponData != null)
                total += WeaponManager.Instance.CurrentWeaponData.attackPower;
            return total;
        }
    }

    // 连招状态
    private int comboStep;              // 0=空闲, 1=Attack1, 2=Attack2, 3=Attack3
    private float lastLightAttackTime;  // 上次轻击的时间，硬冷却防连按
    private float lastHeavyAttackTime;
    private float animationFinishedTime;
    private bool isWaitingNextCombo;
    private int movementLockCount;

    // 动画名称 Hash（性能优化）
    private static readonly int Attack1Hash = Animator.StringToHash("Attack1");
    private static readonly int Attack2Hash = Animator.StringToHash("Attack2");
    private static readonly int Attack3Hash = Animator.StringToHash("Attack3");

    private PlayerHealth playerHealth;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
            playerHealth = GetComponentInParent<PlayerHealth>();
    }

    private void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        // ★ 每帧检测：如果正在连招中，监控动画是否播完
        if (comboStep > 0)
        {
            CheckAttackAnimationState();
        }

        // 连招超时重置：动画播完后超过 comboWindow 没按 → 回 Idle
        if (isWaitingNextCombo && Time.time - animationFinishedTime > comboWindow)
        {
            ResetCombo();
        }

        // 左键：轻击连招
        if (Input.GetMouseButtonDown(0))
        {
            TryLightAttack();
        }

        // 右键：重击
        if (Input.GetMouseButtonDown(1))
        {
            TryHeavyAttack();
        }
    }

    /// <summary>
    /// 每帧检查轻击动画是否已播完，播完则标记等待下一段输入
    /// </summary>
    private void CheckAttackAnimationState()
    {
        if (animator == null) return;
        if (isWaitingNextCombo) return; // 已经在等了，不用重复检测

        if (IsAttackAnimationPlaying()) return; // 还在播

        // 不在攻击动画中 → 动画已播完
        isWaitingNextCombo = true;
        animationFinishedTime = Time.time;
        Debug.Log(string.Format("[PlayerAttack] Attack{0} animation finished, waiting {1}s for combo input", comboStep, comboWindow));
    }

    #region Light Attack (Combo)

    /// <summary>
    /// 轻击连招：左键 → Attack1 →（播完）→ 左键 → Attack2 →（播完）→ 左键 → Attack3
    /// 动画不播完无法接下一段，防止动作重叠
    /// </summary>
    private void TryLightAttack()
    {
        if (playerHealth != null && playerHealth.IsDead) return;

        // ★ 硬冷却：时间不到不许下一段，防止连按绕过动画检测
        float minComboGap = lightAttackLockTime * 0.7f;
        if (comboStep > 0 && Time.time - lastLightAttackTime < minComboGap)
            return;

        // 如果正在播放攻击动画且还没到允许输入的百分比 → 忽略
        if (!isWaitingNextCombo && IsAttackAnimationPlaying())
            return;

        lastLightAttackTime = Time.time;

        // 连招递进
        comboStep++;

        if (comboStep > 3)
            comboStep = 1;

        // 触发对应的动画
        if (animator != null)
        {
            switch (comboStep)
            {
                case 1: animator.SetTrigger("Attack1"); break;
                case 2: animator.SetTrigger("Attack2"); break;
                case 3: animator.SetTrigger("Attack3"); break;
            }
        }

        isWaitingNextCombo = false;

        // 锁定移动
        LockMovement(lightAttackLockTime);

        // 执行攻击判定
        int damage = Mathf.RoundToInt(CurrentDamage * lightDamageMultiplier);
        PerformAttack(damage, lightAttackRange, lightAttackDistance);

        Debug.Log(string.Format("[PlayerAttack] Light Attack {0}, damage: {1}", comboStep, damage));
    }

    /// <summary>
    /// 检查是否正在播放轻击动画（且没到允许连招的百分比）
    /// 纯查询，无副作用
    /// </summary>
    private bool IsAttackAnimationPlaying()
    {
        if (animator == null) return false;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        int currentHash = stateInfo.shortNameHash;

        if (currentHash == Attack1Hash || currentHash == Attack2Hash || currentHash == Attack3Hash)
            return stateInfo.normalizedTime < minAnimationPercent;

        // 也检查过渡中的下一个状态
        AnimatorStateInfo nextInfo = animator.GetNextAnimatorStateInfo(0);
        int nextHash = nextInfo.shortNameHash;
        if (nextHash == Attack1Hash || nextHash == Attack2Hash || nextHash == Attack3Hash)
            return nextInfo.normalizedTime < minAnimationPercent;

        return false;
    }

    /// <summary>
    /// 被打断（受击时 PlayerHealth 调用），强制结束所有攻击状态
    /// </summary>
    public void InterruptAttack()
    {
        comboStep = 0;
        isWaitingNextCombo = false;
        UnlockMovement();

        if (animator != null)
        {
            animator.SetTrigger("ResetCombo");
            animator.ResetTrigger("Attack1");
            animator.ResetTrigger("Attack2");
            animator.ResetTrigger("Attack3");
        }
    }

    /// <summary>
    /// 重置连招，回到 Idle
    /// </summary>
    private void ResetCombo()
    {
        if (comboStep > 0)
        {
            comboStep = 0;
            isWaitingNextCombo = false;
            UnlockMovement();

            if (animator != null)
                animator.SetTrigger("ResetCombo");

            Debug.Log("[PlayerAttack] Combo reset");
        }
    }

    /// <summary>
    /// 锁定移动（攻击/受击期间不能移动），使用引用计数保证多个锁定不冲突
    /// </summary>
    public void LockMovement(float duration)
    {
        if (movementController == null) return;

        movementLockCount++;
        if (movementLockCount == 1)
            movementController.enabled = false;

        StartCoroutine(UnlockMovementAfter(duration));
    }

    /// <summary>
    /// 立即解锁移动（连招重置时用）
    /// </summary>
    private void UnlockMovement()
    {
        movementLockCount = 0;
        if (movementController != null)
            movementController.enabled = true;
    }

    private System.Collections.IEnumerator UnlockMovementAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        movementLockCount--;
        if (movementLockCount <= 0)
        {
            movementLockCount = 0;
            if (movementController != null)
                movementController.enabled = true;
        }
    }

    #endregion

    #region Heavy Attack

    /// <summary>
    /// 重击：右键，伤害高、范围大、冷却长
    /// </summary>
    private void TryHeavyAttack()
    {
        if (playerHealth != null && playerHealth.IsDead) return;
        if (Time.time - lastHeavyAttackTime < heavyAttackCooldown) return;

        // 打断轻击连招
        ResetCombo();
        comboStep = 0;

        lastHeavyAttackTime = Time.time;

        // 锁定移动
        LockMovement(heavyAttackLockTime);

        if (animator != null)
            animator.SetTrigger("Attack");

        int damage = Mathf.RoundToInt(CurrentDamage * heavyDamageMultiplier);
        PerformAttack(damage, heavyAttackRange, heavyAttackDistance, heavyAttackDelay);

        Debug.Log(string.Format("[PlayerAttack] Heavy Attack, damage: {0}", damage));
    }

    #endregion

    #region Attack Detection

    private void PerformAttack(int damage, float range, float distance, float delay = 0.5f)
    {
        Vector3 attackCenter = transform.position + transform.forward * distance;
        Collider[] hitColliders = Physics.OverlapSphere(attackCenter, range);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                EnemyHealth enemyHealth = hitCollider.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    StartCoroutine(DelayDealDamage(enemyHealth, hitCollider.transform, damage, delay));
                }
            }
        }
    }

    private System.Collections.IEnumerator DelayDealDamage(EnemyHealth enemyHealth, Transform enemyTransform, int damage, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (enemyHealth == null || enemyTransform == null)
            yield break;

        Vector3 attackCenter = transform.position + transform.forward * lightAttackDistance;
        float distanceToEnemy = Vector3.Distance(attackCenter, enemyTransform.position);

        if (distanceToEnemy <= lightAttackRange)
            enemyHealth.TakeDamage(damage);
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        if (gameObject == null) return;

        Vector3 lightCenter = transform.position + transform.forward * lightAttackDistance;
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawSphere(lightCenter, lightAttackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(lightCenter, lightAttackRange);

        Vector3 heavyCenter = transform.position + transform.forward * heavyAttackDistance;
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawSphere(heavyCenter, heavyAttackRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(heavyCenter, heavyAttackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, lightCenter);
    }
}
