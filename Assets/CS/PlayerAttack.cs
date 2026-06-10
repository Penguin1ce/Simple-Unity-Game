using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public float attackDistance = 2f;
    public float attackCooldown = 0.5f;
    public int damage = 20;

    [Header("References")]
    public Animator animator;

    private float lastAttackTime;
    private PlayerHealth playerHealth;
    private int attackBonus;

    private void Awake()
    {
        // 自动获取PlayerHealth组件
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = GetComponentInParent<PlayerHealth>();
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 鼠标解锁时（商店/背包打开中）不攻击
            if (Cursor.lockState != CursorLockMode.Locked) return;

            TryAttack();
        }
    }

    public int CurrentDamage => damage + attackBonus;

    public void ApplyTemporaryAttackBoost(int amount, float duration)
    {
        if (amount <= 0 || duration <= 0f)
        {
            return;
        }

        attackBonus += amount;
        StartCoroutine(RemoveAttackBoostAfterDelay(amount, duration));
    }

    /// <summary>
    /// 尝试攻击
    /// </summary>
    private void TryAttack()
    {
        // 检查冷却时间
        if (Time.time - lastAttackTime < attackCooldown)
        {
            return;
        }

        // 死亡后无法攻击
        if (playerHealth != null && playerHealth.IsDead)
        {
            return;
        }

        // 记录攻击时间
        lastAttackTime = Time.time;

        // 触发攻击动画
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // 执行攻击检测
        PerformAttack();
    }

    /// <summary>
    /// 执行攻击检测和伤害
    /// </summary>
    private void PerformAttack()
    {
        // 计算攻击检测点（自身前方一定距离）
        Vector3 attackCenter = transform.position + transform.forward * attackDistance;

        // 使用 OverlapSphere 检测敌人
        Collider[] hitColliders = Physics.OverlapSphere(attackCenter, attackRange);

        foreach (var hitCollider in hitColliders)
        {
            // 检查是否是敌人
            if (hitCollider.CompareTag("Enemy"))
            {
                // 获取敌人血量组件
                EnemyHealth enemyHealth = hitCollider.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    // 延迟0.5秒后检查是否仍在攻击范围内再造成伤害
                    StartCoroutine(DelayDealDamage(enemyHealth, hitCollider.transform));
                }
            }
        }
    }

    /// <summary>
    /// 延迟造成伤害的协程
    /// </summary>
    private System.Collections.IEnumerator DelayDealDamage(EnemyHealth enemyHealth, Transform enemyTransform)
    {
        yield return new WaitForSeconds(0.5f);

        // 检查敌人是否已销毁
        if (enemyHealth == null || enemyTransform == null)
        {
            yield break;
        }

        // 计算当前的攻击检测点
        Vector3 attackCenter = transform.position + transform.forward * attackDistance;
        float distanceToEnemy = Vector3.Distance(attackCenter, enemyTransform.position);

        // 检查敌人是否仍在攻击范围内
        if (distanceToEnemy <= attackRange)
        {
            enemyHealth.TakeDamage(CurrentDamage);
        }
    }

    private System.Collections.IEnumerator RemoveAttackBoostAfterDelay(int amount, float duration)
    {
        yield return new WaitForSeconds(duration);
        attackBonus = Mathf.Max(0, attackBonus - amount);
    }

    /// <summary>
    /// 在Scene视图中绘制攻击范围，方便调试
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (gameObject == null) return;

        // 计算攻击检测点
        Vector3 attackCenter = transform.position + transform.forward * attackDistance;

        // 设置颜色：绿色表示攻击范围
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawSphere(attackCenter, attackRange);

        // 绘制边框
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(attackCenter, attackRange);

        // 绘制从玩家到攻击中心的线
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, attackCenter);
    }
}