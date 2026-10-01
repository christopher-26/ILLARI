using UnityEngine;

/// <summary>
/// Trampolín de llantas recicladas:
/// Al ser pisado por el jugador, proporciona un impulso vertical elástico de 18 m/s.
/// </summary>
public class TrampolinLlantas : MonoBehaviour
{
    [Header("Impulso Vertical")]
    [SerializeField] private float fuerzaRebote = 18.0f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.rigidbody;
            if (rb != null)
            {
                foreach (ContactPoint2D contact in collision.contacts)
                {
                    if (contact.normal.y < -0.5f)
                    {
                        rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaRebote);
                        break;
                    }
                }
            }
        }
    }
}
