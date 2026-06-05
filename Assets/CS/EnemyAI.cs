using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Detection Settings")]
    public float chaseRange = 10f;
    public float attackRange = 2f;

    [Header("Attack Settings")]
    public float attackCooldown = 1.5f;
    public int attackDamage = 10;
    [Tooltip("攻击前方角度范围（度），180度为半个球体，90度为正前方扇形")]
    public float attackAngle = 90f;

    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;

    private PlayerHealth playerHealth;
    public Animator animator;
    private float lastAttackTime;
    private bool isDead = false;

    private void Awake()
    {
        // 自动获取NavMeshAgent
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        // 查找玩家
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        // 获取玩家血量组件
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }
    }

    private void Update()
    {
        // 敌人死亡或玩家死亡则停止行为
        if (isDead || IsPlayerDead())
        {
            return;
        }

        if (player == null)
        {
            return;
        }

        // 计算到玩家的距离
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 距离小于10米开始追击
        if (distanceToPlayer <= chaseRange)
        {
            ChasePlayer(distanceToPlayer);
        }
        else
        {
            StopChase();
        }
    }

    /// <summary>
    /// 追击玩家
    /// </summary>
    private void ChasePlayer(float distanceToPlayer)
    {
        // 距离小于2米，停止移动并攻击
        if (distanceToPlayer <= attackRange)
        {
            StopChase();
            TryAttack();
        }
        else
        {
            // 继续追击
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }

            // 触发行走动画
            if (animator != null)
            {
                animator.SetFloat("Speed", 1f);
            }
        }
    }

    /// <summary>
    /// 停止追击
    /// </summary>
    private void StopChase()
    {
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }

        // 停止行走动画
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }
    }

    /// <summary>
    /// 尝试攻击
    /// </summary>
    private void TryAttack()
    {
        // 检查攻击冷却
        if (Time.time - lastAttackTime < attackCooldown)
        {
            return;
        }

        lastAttackTime = Time.time;

        // 触发攻击动画
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // 延迟0.5秒后对玩家造成伤害，与动画挥击动作同步
        StartCoroutine(DelayDealDamage());
    }

    /// <summary>
    /// 延迟造成伤害的协程
    /// </summary>
    private System.Collections.IEnumerator DelayDealDamage()
    {
        yield return new WaitForSeconds(1.5f);

        // 检查玩家是否仍在攻击范围内
        if (player == null || playerHealth == null)
        {
            yield break;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 检查距离
        if (distanceToPlayer > attackRange)
        {
            yield break;
        }

        // 检查玩家是否在前方扇形范围内
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if (angleToPlayer <= attackAngle / 2f)
        {
            // 玩家在前方扇形范围内，造成伤害
            playerHealth.TakeDamage(attackDamage);
        }
    }

    /// <summary>
    /// 检查玩家是否死亡
    /// </summary>
    private bool IsPlayerDead()
    {
        return playerHealth != null && playerHealth.currentHealth <= 0;
    }

    /// <summary>
    /// 敌人死亡时调用
    /// </summary>
    public void Die()
    {
        isDead = true;
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }
    }

    /// <summary>
    /// 在Scene视图中绘制检测范围
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (gameObject == null) return;

        // 追击范围（黄色）
        Gizmos.color = new Color(1, 1, 0, 0.2f);
        Gizmos.DrawSphere(transform.position, chaseRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // 攻击范围（红色）
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, attackRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}