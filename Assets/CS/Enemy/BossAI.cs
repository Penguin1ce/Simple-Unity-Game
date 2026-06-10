using UnityEngine;
using System.Collections;

/// <summary>
/// 王座 Boss 专用 AI：
/// 阶段1 — 闪现撞击 + 触碰伤害
/// 阶段2 (血量≤50%) — 额外发射能量球
/// </summary>
public class BossAI : MonoBehaviour
{
    [Header("玩家引用")]
    public Transform player;

    [Header("触碰伤害")]
    [Tooltip("碰到玩家时造成的伤害")]
    public float contactDamage = 20f;
    [Tooltip("两次触碰伤害的最小间隔（秒）")]
    public float contactCooldown = 1f;

    [Header("闪现撞击")]
    [Tooltip("闪现冷却（秒）")]
    public float teleportCooldown = 5f;
    [Tooltip("闪现到离玩家多近（0=贴脸）")]
    public float teleportOffset = 1.5f;
    [Tooltip("闪现前蓄力/预警时间（秒）")]
    public float teleportWarmup = 0.8f;

    [Header("能量球（半血后）")]
    [Tooltip("能量球预制体")]
    public GameObject energyBallPrefab;
    [Tooltip("发射间隔（秒）")]
    public float energyBallCooldown = 2f;
    [Tooltip("每次发射几颗")]
    public int energyBallsPerShot = 3;
    [Tooltip("能量球速度")]
    public float energyBallSpeed = 8f;
    [Tooltip("发射点（boss 身上的空物体，从哪发射）")]
    public Transform firePoint;

    [Header("阶段")]
    [Tooltip("进入二阶段的血量百分比")]
    [Range(0.1f, 1f)]
    public float phase2Threshold = 0.5f;

    [Header("闪现代替品")]
    [Tooltip("闪现时播放的临时特效（一团烟之类），没做就用空物体")]
    public GameObject teleportEffectPrefab;

    private EnemyHealth health;
    private bool isDead;
    private bool inPhase2;
    private float lastContactTime;
    private float lastTeleportTime;
    private float lastEnergyBallTime;
    private bool isTeleporting;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (firePoint == null)
            firePoint = transform;
    }

    private void Start()
    {
        // Boss 出生后等一会儿再开始闪现
        lastTeleportTime = Time.time + 2f;
        lastEnergyBallTime = Time.time + 3f;
    }

    private void Update()
    {
        if (isDead) return;
        if (player == null) return;

        // 阶段切换检测
        if (!inPhase2 && health != null && health.currentHealth <= health.maxHealth * phase2Threshold)
        {
            EnterPhase2();
        }

        // 闪现冷却到期
        if (!isTeleporting && Time.time - lastTeleportTime >= teleportCooldown)
        {
            StartCoroutine(TeleportRoutine());
        }

        // 二阶段：能量球
        if (inPhase2 && Time.time - lastEnergyBallTime >= energyBallCooldown)
        {
            StartCoroutine(FireEnergyBalls());
        }
    }

    // ---------------------------------------------------------------
    //  触碰伤害（Boss 身上的 Trigger Collider 调用）
    // ---------------------------------------------------------------
    private void OnTriggerStay(Collider other)
    {
        if (isDead || isTeleporting) return;
        if (!other.CompareTag("Player")) return;
        if (Time.time - lastContactTime < contactCooldown) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null) return;

        lastContactTime = Time.time;
        playerHealth.TakeDamage(contactDamage);
        Debug.Log(string.Format("[BossAI] Contact damage: {0}", contactDamage));
    }

    // ---------------------------------------------------------------
    //  闪现
    // ---------------------------------------------------------------
    private IEnumerator TeleportRoutine()
    {
        isTeleporting = true;
        lastTeleportTime = Time.time;

        // 预警
        if (teleportEffectPrefab != null)
            Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(teleportWarmup);

        if (isDead) { isTeleporting = false; yield break; }

        // 目标位置：玩家附近
        Vector3 dir = Random.insideUnitSphere;
        dir.y = 0;
        if (dir == Vector3.zero) dir = Vector3.forward;
        Vector3 targetPos = player.position + dir.normalized * teleportOffset;

        // 确保在 NavMesh 上
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(targetPos, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            targetPos = hit.position;

        // 闪现
        transform.position = targetPos;

        // 落地特效
        if (teleportEffectPrefab != null)
            Instantiate(teleportEffectPrefab, targetPos, Quaternion.identity);

        // 闪现后立刻造成一次 AOE 伤害
        if (Vector3.Distance(transform.position, player.position) <= teleportOffset + 1f)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(contactDamage * 1.5f);
                Debug.Log(string.Format("[BossAI] Teleport strike: {0} damage", contactDamage * 1.5f));
            }
        }

        isTeleporting = false;
    }

    // ---------------------------------------------------------------
    //  能量球（二阶段）
    // ---------------------------------------------------------------
    private IEnumerator FireEnergyBalls()
    {
        lastEnergyBallTime = Time.time;

        for (int i = 0; i < energyBallsPerShot; i++)
        {
            if (isDead) yield break;

            ShootOneBall();
            yield return new WaitForSeconds(0.3f);
        }
    }

    private void ShootOneBall()
    {
        if (energyBallPrefab == null)
        {
            Debug.LogWarning("[BossAI] energyBallPrefab 没赋值！在 Inspector 里拖一个预制体进去。");
            return;
        }

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 1.5f;
        GameObject ball = Instantiate(energyBallPrefab, spawnPos, Quaternion.identity);

        // 飞向玩家
        Vector3 dir = (player.position - spawnPos).normalized;
        EnergyBall ballScript = ball.GetComponent<EnergyBall>();
        if (ballScript != null)
        {
            ballScript.Init(dir, energyBallSpeed);
        }
        else
        {
            // 预制体没挂 EnergyBall 脚本 → 用 Rigidbody 推
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null)
                rb.velocity = dir * energyBallSpeed;
        }
    }

    // ---------------------------------------------------------------
    //  阶段切换
    // ---------------------------------------------------------------
    private void EnterPhase2()
    {
        inPhase2 = true;
        teleportCooldown *= 0.6f;         // 闪现更频繁
        Debug.Log("[BossAI] ⚡ 进入二阶段 — 开始发射能量球！");
    }

    // ---------------------------------------------------------------
    //  死亡（EnemyHealth.Die 也调用，这里做额外清理）
    // ---------------------------------------------------------------
    public void Die()
    {
        isDead = true;
        StopAllCoroutines();
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(player.position, teleportOffset);
    }
}
