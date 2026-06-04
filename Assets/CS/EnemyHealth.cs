using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("References")]
    public Animator animator;

    private bool isDead = false;

    private void Awake()
    {
        currentHealth = maxHealth;
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

        // 触发死亡动画
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // 打印日志
        Debug.Log("敌人死亡");

        // 2秒后销毁物体
        Destroy(gameObject, 2f);
    }
}