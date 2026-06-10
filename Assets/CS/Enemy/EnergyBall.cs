using UnityEngine;

/// <summary>
/// 能量球：Boss 二阶段发射的弹幕，碰到玩家或障碍物后销毁
/// </summary>
public class EnergyBall : MonoBehaviour
{
    [Header("伤害")]
    public float damage = 15f;

    [Header("生命周期")]
    [Tooltip("几秒后自动销毁")]
    public float lifetime = 5f;

    [Header("特效")]
    [Tooltip("命中后播放的粒子/特效")]
    public GameObject hitEffectPrefab;

    private Vector3 velocity;

    public void Init(Vector3 direction, float speed)
    {
        velocity = direction.normalized * speed;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += velocity * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // ★ 忽略敌人/Boss 自身，防止出生就炸
        if (other.CompareTag("Enemy")) return;

        // 打中玩家
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
                ph.TakeDamage(damage);
        }

        if (hitEffectPrefab != null)
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
