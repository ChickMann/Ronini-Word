using ControlManager;
using UnityEngine;

public class CheckPlayerHitbox : MonoBehaviour
{
    [SerializeField] private EnemyController _enemyController;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if(_enemyController) _enemyController.SetHasPlayerDetected(true);
        }
    }
}
