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

    [Header("Sprites de Animación Flipbook")]
    [SerializeField] private Sprite[] spritesVuelo;
    [SerializeField] private Sprite[] spritesPicada;
    [SerializeField] private Sprite[] spritesDanio;
    [SerializeField] private Sprite[] spritesMuerte;

    private Rigidbody2D rb;
    private Collider2D col;
    private Transform visualTrans;
    private SpriteRenderer spriteRenderer;
    private Transform transformJugador;
    private Vector2 posicionInicial;
    private float direccionX = 1f;
    private float tiempoProximoDisparo = 0f;
    private float timerAnim = 0f;
    private int indiceFrameVuelo = 0;
    private bool estaAtacando = false;
    private bool estaEnDanio = false;
    private bool estaMuerto = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        col = GetComponent<Collider2D>();

        visualTrans = transform.Find("Visual");
        if (visualTrans == null) visualTrans = transform;
        spriteRenderer = visualTrans.GetComponent<SpriteRenderer>();
        posicionInicial = transform.position;
    }

    private void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) transformJugador = p.transform;
    }

    private void Update()
    {
        if (estaMuerto) return;

        PatrullaAerea();
        VerificarAtaqueVertical();
        ActualizarFlipbookVuelo();
    }

    private void ActualizarFlipbookVuelo()
    {
        if (spriteRenderer == null || estaAtacando || estaEnDanio || estaMuerto) return;

        if (spritesVuelo != null && spritesVuelo.Length > 0)
        {
            timerAnim += Time.deltaTime;
            if (timerAnim >= 0.10f)
            {
                timerAnim = 0f;
                indiceFrameVuelo = (indiceFrameVuelo + 1) % spritesVuelo.Length;
                spriteRenderer.sprite = spritesVuelo[indiceFrameVuelo];
            }
        }
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

        if (distHoriz < 2.0f && distVert > 0f && distVert <= rangoAtaqueVertical)
        {
            if (Time.time >= tiempoProximoDisparo)
            {
                tiempoProximoDisparo = Time.time + cadenciaDisparo;
                if (spritesPicada != null && spritesPicada.Length > 0)
                {
                    StartCoroutine(RutinaPicadaVisual());
                }
                else
                {
                    DispararHaciaAbajo();
                }
            }
        }
    }

    private IEnumerator RutinaPicadaVisual()
    {
        estaAtacando = true;
        int frameDisparo = Mathf.Max(1, spritesPicada.Length / 2);

        for (int i = 0; i < spritesPicada.Length; i++)
        {
            if (estaMuerto) yield break;
            if (spriteRenderer != null) spriteRenderer.sprite = spritesPicada[i];
            if (i == frameDisparo)
            {
                DispararHaciaAbajo();
            }
            yield return new WaitForSeconds(0.08f);
        }

        estaAtacando = false;
    }

    private void DispararHaciaAbajo()
    {
        if (prefabBalaBarro == null || estaMuerto) return;

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
        if (estaMuerto) return;

        salud -= cantidad;

        if (salud <= 0)
        {
            estaMuerto = true;
            if (spritesMuerte != null && spritesMuerte.Length > 0)
            {
                StartCoroutine(RutinaMuerteVolador());
            }
            else
            {
                FinalizarMuerteVolador();
            }
        }
        else
        {
            if (spritesDanio != null && spritesDanio.Length > 0)
            {
                StartCoroutine(RutinaDanioVisual());
            }
            else
            {
                StartCoroutine(FlashDanio());
            }
        }
    }

    private IEnumerator RutinaDanioVisual()
    {
        estaEnDanio = true;
        Color colOriginal = (spriteRenderer != null) ? spriteRenderer.color : Color.white;

        for (int i = 0; i < spritesDanio.Length; i++)
        {
            if (estaMuerto) yield break;
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = spritesDanio[i];
                spriteRenderer.color = new Color(1f, 0.4f, 0.4f, 1f);
            }
            yield return new WaitForSeconds(0.12f);
        }

        if (spriteRenderer != null) spriteRenderer.color = colOriginal;
        estaEnDanio = false;
    }

    private IEnumerator FlashDanio()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.cyan;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
        }
    }

    private IEnumerator RutinaMuerteVolador()
    {
        estaMuerto = true;
        rb.linearVelocity = Vector2.zero;
        if (col != null) col.enabled = false;

        for (int i = 0; i < spritesMuerte.Length; i++)
        {
            if (spriteRenderer != null) spriteRenderer.sprite = spritesMuerte[i];
            yield return new WaitForSeconds(0.08f);
        }

        FinalizarMuerteVolador();
    }

    private void FinalizarMuerteVolador()
    {
        if (prefabBotellaDrop != null)
        {
            Instantiate(prefabBotellaDrop, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
