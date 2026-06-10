using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Detection Settings")]
    public float chaseRange = 10f;
    [Tooltip("追到多近开始出招")]
    public float attackTriggerRange = 3f;
    [Tooltip("攻击伤害实际能打多远")]
    public float damageReach = 2.5f;
    [Tooltip("停在离玩家多远的地方")]
    public float stoppingDistance = 1.5f;
    [Tooltip("移动速度（NavMeshAgent 的 Speed，在 Awake 时自动同步）")]
    public float moveSpeed = 3.5f;

    [Header("Attack Settings")]
    public float attackCooldown = 1.5f;
    public int attackDamage = 10;
    [Tooltip("动画触发后延迟多久结算伤害")]
    public float damageDelay = 1.0f;
    [Tooltip("攻击扇形角度，180=无限制")]
    public float attackAngle = 180f;
    [Tooltip("出招前需要面朝玩家的角度容差")]
    public float facingTolerance = 90f;
    [Tooltip("旋转速度（度/秒）")]
    public float rotationSpeed = 360f;

    [Header("受击硬直")]
    public float stunDuration = 0.6f;
    public float knockbackForce = 3f;

    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;
    [Tooltip("模型根节点（带 SkinnedMeshRenderer 的那层）")]
    public Transform modelRoot;
    [Tooltip("模型FBX里的视觉前方偏移角度。如果敌人屁股朝你就改这个，试试 180、90、-90")]
    public float modelYawOffset = 0f;
    public Animator animator;

    private PlayerHealth playerHealth;
    private float lastAttackTime;
    private bool isDead;

    private float stunEndTime;
    private bool IsStunned => Time.time < stunEndTime;
    private int attackToken;

    private void Awake()
    {
        // ---------- 组件引用 ----------
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();

        // ---------- 配置 NavMeshAgent ----------
        if (agent != null)
        {
            // 速度
            agent.speed = moveSpeed;

            // 我们自己控制旋转，NavMeshAgent 只管位移
            agent.updateRotation = false;

            // 停止距离
            agent.stoppingDistance = stoppingDistance;

            // ★ 关键修复1：关闭动态避障，防止绕圈
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

            // ★ 关键修复2：关闭自动刹车，减少到达目标后的微调抖动
            agent.autoBraking = false;

            // 高加速度，到达目标快
            agent.acceleration = 50f;
        }

        // ---------- Rigidbody 冲突 ----------
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            Debug.LogWarning(string.Format(
                "[EnemyAI] {0} has non-kinematic Rigidbody — 已自动设为 Kinematic", gameObject.name), this);
            rb.isKinematic = true;
        }
    }

    private void OnValidate()
    {
        if (stoppingDistance >= attackTriggerRange)
            stoppingDistance = attackTriggerRange - 0.5f;
        if (agent != null)
            agent.stoppingDistance = stoppingDistance;
    }

    private void Update()
    {
        if (isDead || IsPlayerDead()) return;
        if (player == null) return;

        Vector3 myPos = agent != null ? agent.transform.position : transform.position;
        float distanceToPlayer = Vector3.Distance(myPos, player.position);

        // 每帧面朝玩家
        LookAtPlayer();

        if (distanceToPlayer <= chaseRange)
            ChasePlayer(distanceToPlayer);
        else
            IdleOnly();
    }

    // ---------------------------------------------------------------
    //  追逐 / 攻击
    // ---------------------------------------------------------------
    private void ChasePlayer(float dist)
    {
        if (dist <= attackTriggerRange)
        {
            // 进入攻击范围：停住
            if (agent != null && agent.isActiveAndEnabled)
            {
                if (!agent.isStopped)
                {
                    agent.isStopped = true;
                    agent.ResetPath();          // ★ 清掉残留路径
                    agent.velocity = Vector3.zero;
                }
            }

            if (animator != null)
                animator.SetFloat("Speed", 0f);

            if (!IsStunned)
                TryAttack();
        }
        else
        {
            // 还在追
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }

            if (animator != null)
                animator.SetFloat("Speed", 1f);
        }
    }

    private void IdleOnly()
    {
        if (agent != null && agent.isActiveAndEnabled && !agent.isStopped)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
        if (animator != null)
            animator.SetFloat("Speed", 0f);
    }

    // ---------------------------------------------------------------
    //  旋转
    // ---------------------------------------------------------------
    /// <summary>
    /// 旋转 transform，使模型的视觉前方（modelRoot.forward 或 transform.forward）朝向玩家。
    /// modelYawOffset 用于补偿 FBX 里模型面朝方向的差异。
    /// </summary>
    private void LookAtPlayer()
    {
        if (player == null) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        if (dir == Vector3.zero) return;

        // 目标：模型前方 = dir
        // 先算 modelRoot 的 local 前方方向
        Vector3 modelLocalForward = modelRoot != null
            ? modelRoot.localRotation * Vector3.forward
            : Vector3.forward;

        // 目标旋转：让 modelLocalForward（在 transform 的本地空间里）指向 dir
        // 即 transform.rotation * modelLocalForward = dir
        // → transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Inverse(Quaternion.LookRotation(modelLocalForward))
        Quaternion offset = Quaternion.Inverse(Quaternion.LookRotation(modelLocalForward))
                         * Quaternion.Euler(0, modelYawOffset, 0);

        Quaternion target = Quaternion.LookRotation(dir) * offset;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target,
            rotationSpeed * Time.deltaTime);
    }

    private Vector3 GetAttackForward()
    {
        return modelRoot != null ? modelRoot.forward : transform.forward;
    }

    // ---------------------------------------------------------------
    //  攻击
    // ---------------------------------------------------------------
    private void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        if (IsStunned) return;

        // 面朝玩家才打
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;
        if (Vector3.Angle(GetAttackForward(), dir) > facingTolerance) return;

        lastAttackTime = Time.time;

        if (animator != null)
            animator.SetTrigger("Attack");

        int token = attackToken;
        StartCoroutine(DelayDealDamage(token));
    }

    private System.Collections.IEnumerator DelayDealDamage(int token)
    {
        yield return new WaitForSeconds(damageDelay);

        if (token != attackToken) yield break;
        if (isDead) yield break;
        if (player == null || playerHealth == null) yield break;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > damageReach + 0.5f) yield break;

        Vector3 dir = (player.position - transform.position).normalized;
        if (Vector3.Angle(GetAttackForward(), dir) <= attackAngle / 2f)
        {
            if (token != attackToken || IsStunned) yield break;
            playerHealth.TakeDamage(attackDamage);
        }
    }

    // ---------------------------------------------------------------
    //  受击
    // ---------------------------------------------------------------
    public void Stun()
    {
        stunEndTime = Time.time + stunDuration;
        attackToken++;

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
        }

        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        if (animator != null)
            animator.SetFloat("Speed", 0f);

        StartCoroutine(RecoverFromStun());
    }

    private System.Collections.IEnumerator RecoverFromStun()
    {
        yield return new WaitForSeconds(stunDuration);
        if (!isDead && agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            agent.isStopped = false;
    }

    // ---------------------------------------------------------------
    //  死亡
    // ---------------------------------------------------------------
    public void Die()
    {
        isDead = true;
        attackToken++;
        if (agent != null && agent.isActiveAndEnabled)
            agent.isStopped = true;
    }

    private bool IsPlayerDead()
    {
        return playerHealth != null && playerHealth.currentHealth <= 0;
    }

    // ---------------------------------------------------------------
    //  Gizmos
    // ---------------------------------------------------------------
    private void OnDrawGizmosSelected()
    {
        Vector3 forward = GetAttackForward();
        Vector3 p = transform.position;

        // 追逐范围（黄）
        Gizmos.color = new Color(1, 1, 0, 0.2f);
        Gizmos.DrawSphere(p, chaseRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(p, chaseRange);

        // 攻击触发范围（橙）
        Gizmos.color = new Color(1, 0.5f, 0, 0.2f);
        Gizmos.DrawSphere(p, attackTriggerRange);
        Gizmos.color = new Color(1, 0.5f, 0);
        Gizmos.DrawWireSphere(p, attackTriggerRange);

        // 伤害实际范围（红）
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(p, damageReach);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(p, damageReach);

        // 攻击扇形
        Vector3 l = Quaternion.Euler(0, -attackAngle / 2f, 0) * forward;
        Vector3 r = Quaternion.Euler(0, attackAngle / 2f, 0) * forward;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(p, p + l * damageReach);
        Gizmos.DrawLine(p, p + r * damageReach);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(p, p + forward * damageReach);
    }
}
