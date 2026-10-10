using UnityEngine;

/// <summary>
/// Reenvía eventos de animación ejecutados en el objeto 'Visual'
/// o en el Animator hacia el componente EnemyManager correspondiente.
/// </summary>
public class EnemyAnimationEvents : MonoBehaviour
{
    private EnemyManager enemyManager;

    private void Awake()
    {
        enemyManager = GetComponentInParent<EnemyManager>();
        if (enemyManager == null)
        {
            enemyManager = GetComponent<EnemyManager>();
        }
    }

    public void DispararBolaLodo()
    {
        if (enemyManager != null)
        {
            enemyManager.DispararBolaLodo();
        }
    }
}
