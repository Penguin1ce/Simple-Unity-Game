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

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryAttack();
        }
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
                // 获取敌人血量组件并造成伤害
                EnemyHealth enemyHealth = hitCollider.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
                }
            }
        }
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