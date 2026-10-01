using System.Collections;
using UnityEngine;

/// <summary>
/// Controlador del Fangoso Volador (Moscardón de Basura):
/// Vuela en el aire con movimiento sinusoidal, detecta al jugador por debajo
/// y lanza proyectiles de lodo ácido en picada vertical.
/// Al morir, obligatoriamente suelta una Botella de Plástico coleccionable.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyFlyingManager : MonoBehaviour
{
    [Header("Parámetros de Vuelo")]
    [SerializeField] private float velocidadHorizontal = 2.2f;
    [SerializeField] private float amplitudOnda = 1.2f;
    [SerializeField] private float frecuenciaOnda = 2.5f;
    [SerializeField] private float distanciaPatrulla = 8f;

    [Header("Salud y Recompensas")]
    [SerializeField] private int salud = 2;
    [SerializeField] private GameObject prefabBotellaDrop;

    [Header("Ataque Ácido")]
    [SerializeField] private float cadenciaDisparo = 2.2f;
    [SerializeField] private float rangoAtaqueVertical = 8f;
    [SerializeField] private GameObject prefabBalaBarro;

    private Rigidbody2D rb;
    private Transform visualTrans;
    private Transform transformJugador;
    private Vector2 posicionInicial;
    private float direccionX = 1f;
    private float tiempoProximoDisparo = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        visualTrans = transform.Find("Visual");
        if (visualTrans == null) visualTrans = transform;
        posicionInicial = transform.position;
    }

    private void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) transformJugador = p.transform;
    }

    private void Update()
    {
        PatrullaAerea();
        VerificarAtaqueVertical();
    }

    private void PatrullaAerea()
    {
        float despX = (transform.position.x - posicionInicial.x);
        if (Mathf.Abs(despX) >= distanciaPatrulla)
        {
            direccionX = -Mathf.Sign(despX);
            visualTrans.localScale = new Vector3(direccionX * Mathf.Abs(visualTrans.localScale.x), visualTrans.localScale.y, visualTrans.localScale.z);
        }

        float nuevoX = transform.position.x + direccionX * velocidadHorizontal * Time.deltaTime;
        float nuevoY = posicionInicial.y + Mathf.Sin(Time.time * frecuenciaOnda) * amplitudOnda;

        transform.position = new Vector3(nuevoX, nuevoY, transform.position.z);
    }

    private void VerificarAtaqueVertical()
    {
        if (transformJugador == null) return;

        float distHoriz = Mathf.Abs(transformJugador.position.x - transform.position.x);
        float distVert = transform.position.y - transformJugador.position.y;

        // Si el jugador está directamente debajo
        if (distHoriz < 2.0f && distVert > 0f && distVert <= rangoAtaqueVertical)
        {
            if (Time.time >= tiempoProximoDisparo)
            {
                tiempoProximoDisparo = Time.time + cadenciaDisparo;
                DispararHaciaAbajo();
            }
        }
    }

    private void DispararHaciaAbajo()
    {
        if (prefabBalaBarro == null) return;

        GameObject bala = Instantiate(prefabBalaBarro, transform.position + Vector3.down * 0.6f, Quaternion.identity);
        Bala compBala = bala.GetComponent<Bala>();
        if (compBala != null)
        {
            compBala.ConfigurarDireccion(Vector2.down);
        }
        else
        {
            Rigidbody2D rbBala = bala.GetComponent<Rigidbody2D>();
            if (rbBala != null) rbBala.linearVelocity = Vector2.down * 8f;
        }
    }

    public void RecibirDanio(int cantidad = 1)
    {
        salud -= cantidad;
        StartCoroutine(FlashDanio());

        if (salud <= 0)
        {
            Morir();
        }
    }

    private IEnumerator FlashDanio()
    {
        SpriteRenderer sr = visualTrans.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.cyan;
            yield return new WaitForSeconds(0.1f);
            sr.color = Color.white;
        }
    }

    private void Morir()
    {
        if (prefabBotellaDrop != null)
        {
            Instantiate(prefabBotellaDrop, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
