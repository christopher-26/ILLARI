using UnityEngine;

/// <summary>
/// Plataforma móvil para parkour andino en 'Illari'.
/// Se desplaza continuamente entre dos puntos usando Rigidbody2D.linearVelocity en Unity 6.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlataformaMovil : MonoBehaviour
{
    [SerializeField] private Vector2 puntoA;
    [SerializeField] private Vector2 puntoB;
    [SerializeField] private float velocidad = 2.5f;

    private Rigidbody2D rb;
    private bool yendoHaciaB = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        if (puntoA == Vector2.zero && puntoB == Vector2.zero)
        {
            puntoA = transform.position;
            puntoB = puntoA + new Vector2(6f, 0f);
        }
    }

    private void FixedUpdate()
    {
        Vector2 destino = yendoHaciaB ? puntoB : puntoA;
        Vector2 direccion = (destino - (Vector2)transform.position).normalized;

        rb.linearVelocity = direccion * velocidad;

        if (Vector2.Distance(transform.position, destino) < 0.25f)
        {
            yendoHaciaB = !yendoHaciaB;
        }
    }

    public void ConfigurarPuntos(Vector2 a, Vector2 b, float vel)
    {
        puntoA = a;
        puntoB = b;
        velocidad = vel;
    }
}
