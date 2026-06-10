using UnityEngine;

public class EnemyDummy : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 3;
    public int goldDrop = 10;

    private int _currentHealth;
    public int CurrentHealth { get { return _currentHealth; } }
    private EnemyHealth _enemyHealth;

    private void Start()
    {
        _currentHealth = maxHealth;
        _enemyHealth = GetComponent<EnemyHealth>();

        if (_enemyHealth != null)
        {
            _enemyHealth.maxHealth = maxHealth;
            _enemyHealth.currentHealth = maxHealth;

            // Keep dummy UI/logic in sync with the real EnemyHealth component.
            _enemyHealth.HealthChanged += (cur, max) => { _currentHealth = Mathf.CeilToInt(cur); };
            _enemyHealth.Died += Die;
        }
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;

        if (_enemyHealth != null)
            _enemyHealth.TakeDamage(damage);

        Debug.Log(string.Format("[EnemyDummy] {0} took {1} damage. HP: {2}/{3}",
            gameObject.name, damage, _currentHealth, maxHealth));

        if (_currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        Debug.Log(string.Format("[EnemyDummy] {0} died. Dropped {1} gold.", gameObject.name, goldDrop));

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.AddGold(goldDrop);

        Destroy(gameObject);
    }
}
