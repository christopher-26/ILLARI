using System.Collections;
using UnityEngine;

/// <summary>
/// Script de proyectil ('Proyectil de Agua' en el universo de Illari).
/// Se desplaza en línea recta usando linearVelocity en Unity 6.
/// Cuenta con animación de gota de agua pura (frames de oscilación hidrodinámica)
/// y animación de salpicadura (Splash) al impactar contra enemigos o superficies.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Bala : MonoBehaviour
{
    [Header("Físicas del Proyectil")]
    [SerializeField] private float velocidad = 20f;
    [SerializeField] private float tiempoDeVida = 3f;

    [Header("Combate")]
    [SerializeField] private int danio = 1;
    [SerializeField] private bool esBalaEnemiga = false;
    [SerializeField] private bool esMegaBala = false;
    [SerializeField] private bool esPenetrante = false;
    [SerializeField] private int penetracionesRestantes = 3;

    [Header("Animación de Gota de Agua")]
    [SerializeField] private Sprite[] spritesGotaAgua;
    [SerializeField] private Sprite[] spritesSplash;
    [SerializeField] private float fpsGota = 16f;

    [Header("Color Prototipo / Enemigo")]
    [SerializeField] private Color colorBalaJugador = Color.white;
    [SerializeField] private Color colorBalaEnemiga = new Color(1f, 0.25f, 0.15f);

    private Rigidbody2D rb;
    private CircleCollider2D col;
    private SpriteRenderer sr;
    private bool impactado = false;
    private float timerGota = 0f;
    private int indiceFrameGota = 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
        sr = GetComponent<SpriteRenderer>();

        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
        }

        // Físicas Unity 6
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        col.isTrigger = true;
        sr.sortingOrder = 10;

        CargarSpritesSiEsNecesario();
        ActualizarVisual();
    }

    private void Start()
    {
        Destroy(gameObject, tiempoDeVida);
    }

    private void CargarSpritesSiEsNecesario()
    {
        if (!esBalaEnemiga && (spritesGotaAgua == null || spritesGotaAgua.Length == 0))
        {
            // Intentar cargar sprites de gota de agua desde Resources o AssetDatabase
#if UNITY_EDITOR
            spritesGotaAgua = new Sprite[4];
            for (int i = 0; i < 4; i++)
            {
                spritesGotaAgua[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/ProyectilAgua/GotaAgua_{i}.png");
            }

            spritesSplash = new Sprite[4];
            for (int i = 0; i < 4; i++)
            {
                spritesSplash[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/ProyectilAgua/SplashAgua_{i}.png");
            }
#endif
        }
    }

    private void Update()
    {
        // Animación continua de la gota de agua en vuelo
        if (!esBalaEnemiga && spritesGotaAgua != null && spritesGotaAgua.Length > 0)
        {
            timerGota += Time.deltaTime;
            if (timerGota >= 1f / fpsGota)
            {
                timerGota = 0f;
                indiceFrameGota = (indiceFrameGota + 1) % spritesGotaAgua.Length;
                if (sr != null && spritesGotaAgua[indiceFrameGota] != null)
                {
                    sr.sprite = spritesGotaAgua[indiceFrameGota];
                }
            }
        }
    }

    private void ActualizarVisual()
    {
        if (sr == null) return;

        if (esBalaEnemiga)
        {
            sr.color = colorBalaEnemiga;
            transform.localScale = new Vector3(0.18f, 0.18f, 1f);
        }
        else
        {
            sr.color = Color.white;
            transform.localScale = new Vector3(1.1f, 1.1f, 1f);
            if (spritesGotaAgua != null && spritesGotaAgua.Length > 0 && spritesGotaAgua[0] != null)
            {
                sr.sprite = spritesGotaAgua[0];
            }
        }
    }

    /// <summary>
    /// Inicializa la trayectoria recta del proyectil en Unity 6.
    /// </summary>
    public void Inicializar(Vector2 direccion, bool esEnemigo = false)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        esBalaEnemiga = esEnemigo;
        CargarSpritesSiEsNecesario();
        ActualizarVisual();

        rb.linearVelocity = direccion.normalized * velocidad;

        float anguloZ = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, anguloZ);
    }

    public void InicializarEspecial(Vector2 direccion, int danioPersonalizado, bool mega = false, bool penetrante = false)
    {
        danio = danioPersonalizado;
        esMegaBala = mega;
        esPenetrante = penetrante;
        penetracionesRestantes = 3;
        Inicializar(direccion, false);

        if (esMegaBala)
        {
            transform.localScale = new Vector3(2.4f, 2.4f, 1f);
            if (sr != null)
            {
                sr.color = new Color(0.25f, 1f, 1f, 0.98f);
            }
        }
    }

    public void ConfigurarDireccion(Vector2 direccion)
    {
        Inicializar(direccion, true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (impactado) return;

        // 1. Colisión con Superficies (Ground)
        if (collision.CompareTag("Ground"))
        {
            EjecutarImpacto();
            return;
        }

        // 2. Destruir proyectiles de barro enemigos al chocar con agua purificadora
        if (!esBalaEnemiga)
        {
            Bala balaEnemiga = collision.GetComponent<Bala>();
            if (balaEnemiga != null && balaEnemiga.esBalaEnemiga)
            {
                balaEnemiga.EjecutarImpacto();
                if (!esPenetrante)
                {
                    EjecutarImpacto();
                    return;
                }
            }
        }

        // 3. Proyectil de Illari golpea a Enemigos
        if (!esBalaEnemiga && collision.CompareTag("Enemy"))
        {
            var enemigo = collision.GetComponent<EnemyManager>();
            if (enemigo != null) enemigo.RecibirDanio(danio);

            var volador = collision.GetComponent<EnemyFlyingManager>();
            if (volador != null) volador.RecibirDanio(danio);

            var blindado = collision.GetComponent<EnemyArmored>();
            if (blindado != null) blindado.RecibirDanio(danio, rb.linearVelocity.normalized);

            var titan = collision.GetComponent<BossTitanManager>();
            if (titan != null) titan.RecibirDanio(danio);

            var canionero = collision.GetComponent<EnemyCannoneer>();
            if (canionero != null) canionero.RecibirDanio(danio);

            var supremo = collision.GetComponent<BossSupremeHuaytapallana>();
            if (supremo != null) supremo.RecibirDanio(danio);

            if (esPenetrante)
            {
                penetracionesRestantes--;
                if (spritesSplash != null && spritesSplash.Length > 0 && spritesSplash[0] != null)
                {
                    GameObject splash = new GameObject("Splash_Agua_Impacto");
                    splash.transform.position = transform.position;
                    var eff = splash.AddComponent<EfectoSplashAgua>();
                    eff.Configurar(spritesSplash);
                }

                if (penetracionesRestantes <= 0)
                {
                    EjecutarImpacto();
                }
                return;
            }

            EjecutarImpacto();
            return;
        }

        // 4. Proyectil de Enemigo golpea a Illari
        if (esBalaEnemiga && collision.CompareTag("Player"))
        {
            IllariPlatformer jugador = collision.GetComponent<IllariPlatformer>();
            if (jugador != null)
            {
                jugador.RecibirDanio(danio);
            }

            EjecutarImpacto();
            return;
        }
    }

    public void EjecutarImpacto()
    {
        impactado = true;

        // Instanciar splash animado de agua si es proyectil purificador
        if (!esBalaEnemiga && spritesSplash != null && spritesSplash.Length > 0 && spritesSplash[0] != null)
        {
            GameObject splash = new GameObject("Splash_Agua_Impacto");
            splash.transform.position = transform.position;
            var eff = splash.AddComponent<EfectoSplashAgua>();
            eff.Configurar(spritesSplash);
        }

        Destroy(gameObject);
    }
}
