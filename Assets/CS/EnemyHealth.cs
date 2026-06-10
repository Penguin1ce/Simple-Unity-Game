using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("References")]
    public Animator animator;

    private bool isDead = false;

    public event Action<float, float> HealthChanged;
    public event Action Died;

    private void Awake()
    {
        currentHealth = maxHealth;
        NotifyHealthChanged();
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0f, currentHealth - damage);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }
        else
        {
            NotifyHealthChanged();

            // 触发受击动画
            if (animator != null)
            {
                animator.SetTrigger("Hit");
            }
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

        // 触发死亡动画
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        // 禁用NavMeshAgent和EnemyAI组件
        var navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = false;
        }

        var enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.enabled = false;
        }

        // 打印日志
        Debug.Log("敌人死亡");

        if (Died != null)
        {
            Died();
        }

        NotifyQuestEnemyKilled();

        // 1.5秒内缩小并销毁物体
        StartCoroutine(ShrinkAndDestroy());
    }

    private void NotifyQuestEnemyKilled()
    {
        QuestEnemyMarker marker = GetComponent<QuestEnemyMarker>();
        if (marker == null || MainQuestManager.Instance == null)
        {
            return;
        }

        MainQuestManager.Instance.OnEnemyKilled(marker.EnemyType);
    }

    /// <summary>
    /// 1.5秒内缩小并销毁物体
    /// </summary>
    private System.Collections.IEnumerator ShrinkAndDestroy()
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

        // 销毁物体
        Destroy(gameObject);
    }

    private void NotifyHealthChanged()
    {
        if (HealthChanged != null)
        {
            HealthChanged(currentHealth, maxHealth);
        }
    }
}
