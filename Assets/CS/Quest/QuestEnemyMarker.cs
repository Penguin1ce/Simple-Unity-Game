using UnityEngine;

public class QuestEnemyMarker : MonoBehaviour
{
    [SerializeField] private EnemyType enemyType;

    public EnemyType EnemyType => enemyType;
}
