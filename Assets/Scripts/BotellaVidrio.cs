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

    private void Awake()
    {
        col2D = GetComponent<BoxCollider2D>();
        col2D.isTrigger = true;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = colorBotella;
    }

    private void Start()
    {
        posicionInicial = transform.position;
    }

    private void Update()
    {
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
