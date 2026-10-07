using UnityEngine;

/// <summary>
/// Coleccionable de Botella de Vidrio para el nivel de parkour en 'Iyari'.
/// Al ser recolectada por el jugador, suma puntos en el GameManager y se destruye.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class BotellaVidrio : MonoBehaviour
{
    [Header("Configuración de Puntuación")]
    [SerializeField] private int valorPuntos = 1;

    [Header("Color Prototipo")]
    [SerializeField] private Color colorBotella = new Color(0f, 0.9f, 1f); // Celeste / Cyan

    [Header("Efectos Opcionales")]
    [SerializeField] private AudioClip sonidoRecoleccion;
    [SerializeField] private GameObject prefabEfectoParticulas;

    [Header("Animación en Reposo")]
    [SerializeField] private float velocidadFlotacion = 3f;
    [SerializeField] private float amplitudFlotacion = 0.15f;

    private BoxCollider2D col2D;
    private SpriteRenderer sr;
    private Vector2 posicionInicial;
    private bool yaRecolectada = false;
    private bool esDropFisico = false;
    private Rigidbody2D rb;
    private CircleCollider2D colFisicoSuelo;
    private float tiempoDrop = 0f;

    private void Awake()
    {
        col2D = GetComponent<BoxCollider2D>();
        if (col2D != null) col2D.isTrigger = true;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = colorBotella;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        posicionInicial = transform.position;
    }

    /// <summary>
    /// Configura la botella para realizar un drop físico con rebote suave hacia arriba al morir un enemigo.
    /// </summary>
    public void IniciarDropFisico(Vector2 impulso)
    {
        esDropFisico = true;
        tiempoDrop = 0f;
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 2.5f;
        rb.freezeRotation = true;
        rb.linearVelocity = impulso;

        // Collider físico para rebotar en el suelo
        colFisicoSuelo = gameObject.AddComponent<CircleCollider2D>();
        colFisicoSuelo.radius = 0.22f;
        colFisicoSuelo.isTrigger = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (esDropFisico && collision.gameObject.CompareTag("Ground"))
        {
            // Pequeño rebote y asentamiento
            if (tiempoDrop > 0.2f)
            {
                FinalizarDropFisico();
            }
        }
    }

    private void FinalizarDropFisico()
    {
        esDropFisico = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        if (colFisicoSuelo != null)
        {
            Destroy(colFisicoSuelo);
        }
        posicionInicial = transform.position;
    }

    private void Update()
    {
        if (esDropFisico)
        {
            tiempoDrop += Time.deltaTime;
            if (tiempoDrop > 0.4f && rb != null && Mathf.Abs(rb.linearVelocity.y) < 0.15f)
            {
                FinalizarDropFisico();
            }
            return;
        }

        // Movimiento de flotación suave tipo arcade
        float nuevoY = posicionInicial.y + Mathf.Sin(Time.time * velocidadFlotacion) * amplitudFlotacion;
        transform.position = new Vector3(posicionInicial.x, nuevoY, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (yaRecolectada) return;

        // Validamos que sea el Jugador mediante el Tag
        if (collision.CompareTag("Player"))
        {
            yaRecolectada = true;

            // Notificamos al GameManager
            if (GameManagerIyari.Instancia != null)
            {
                GameManagerIyari.Instancia.SumarBotella(valorPuntos);
            }

            // Reproducir sonido si está asignado
            if (sonidoRecoleccion != null)
            {
                AudioSource.PlayClipAtPoint(sonidoRecoleccion, transform.position);
            }

            // Instanciar partículas si existen
            if (prefabEfectoParticulas != null)
            {
                Instantiate(prefabEfectoParticulas, transform.position, Quaternion.identity);
            }

            // Destrucción del objeto recolectado
            Destroy(gameObject);
        }
    }
}
