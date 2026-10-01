using UnityEngine;

/// <summary>
/// Cinta transportadora de reciclaje:
/// Desplaza continuamente a cualquier cuerpo rígido en contacto a una velocidad constante.
/// </summary>
public class CintaTransportadora : MonoBehaviour
{
    [Header("Velocidad de Transporte")]
    [SerializeField] private float velocidadCinta = 3.5f;

    private void OnCollisionStay2D(Collision2D collision)
    {
        Rigidbody2D rb = collision.rigidbody;
        if (rb != null)
        {
            // Solo empuja si el objeto está apoyado sobre la superficie superior
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    rb.linearVelocity = new Vector2(velocidadCinta, rb.linearVelocity.y);
                    break;
                }
            }
        }
    }
}
