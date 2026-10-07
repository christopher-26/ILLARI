using System.Collections;
using UnityEngine;

/// <summary>
/// Mini-Jefe: Fangoso Titán del Mantaro (Nivel 4 - Parque de los Sombreros).
/// Monstruo colosal de lodo tóxico de 3 metros con dos fases de ataque:
/// 1. Golpe de puño sísmico contra el suelo (Ground Slam).
/// 2. Rugido y disparo triple en abanico de lodo tóxico (Roar).
/// Incorpora animación flipbook completa (Idle, GroundSlam, Roar, Hurt, Death)
/// cargada desde Assets/Sprites/FangosoTitan.
/// Al ser derrotado/purificado, suelta 2 Botellas de Plástico finales.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BossTitanManager : MonoBehaviour
{
    [Header("Salud del Jefe")]
    [SerializeField] private int saludMax = 10;
    [SerializeField] private int saludActual = 10;
    [SerializeField] private GameObject prefabBotellaDrop;

    [Header("Ataques y Proyectiles")]
    [SerializeField] private GameObject prefabBalaBarro;
    [SerializeField] private float intervaloEntreAtaques = 2.8f;
    [SerializeField] private float velocidadProyectil = 10f;

    [Header("Sprites Flipbook Fangoso Titán")]
    [SerializeField] private Sprite[] spritesIdle;
    [SerializeField] private Sprite[] spritesGroundSlam;
    [SerializeField] private Sprite[] spritesRoar;
    [SerializeField] private Sprite[] spritesHurt;
    [SerializeField] private Sprite[] spritesDeath;
    [SerializeField] private float fpsAnim = 8f;

    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;
    private Transform visualTrans;
    private Transform transformJugador;
    private float tiempoProximoAtaque = 0f;
    private bool estaMuerto = false;
    private bool estaAtacando = false;
    private bool estaHerido = false;
    private float animTimer = 0f;
    private int frameIndex = 0;

    private Vector3 escalaInicial = Vector3.one;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        visualTrans = transform.Find("Visual");
        if (visualTrans == null) visualTrans = transform;
        sr = visualTrans.GetComponent<SpriteRenderer>();
        if (sr == null) sr = visualTrans.gameObject.AddComponent<SpriteRenderer>();
        rb.freezeRotation = true;
        escalaInicial = visualTrans.localScale;
        saludActual = saludMax;

        CargarSpritesSiEsNecesario();
    }

    public void ConfigurarEscalaVisual(Vector3 escala)
    {
        escalaInicial = escala;
        if (visualTrans != null) visualTrans.localScale = escala;
    }

    private void Start()
    {
        if (visualTrans != null) escalaInicial = visualTrans.localScale;
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) transformJugador = p.transform;
        tiempoProximoAtaque = Time.time + 1.5f;

        if (sr != null && spritesIdle != null && spritesIdle.Length > 0)
        {
            sr.sprite = spritesIdle[0];
        }
    }

    private void CargarSpritesSiEsNecesario()
    {
#if UNITY_EDITOR
        if (spritesIdle == null || spritesIdle.Length == 0)
        {
            spritesIdle = CargarArraySprites("Assets/Sprites/FangosoTitan/Idle_", 4);
            spritesGroundSlam = CargarArraySprites("Assets/Sprites/FangosoTitan/GroundSlam_", 6);
            spritesRoar = CargarArraySprites("Assets/Sprites/FangosoTitan/Roar_", 4);
            spritesHurt = CargarArraySprites("Assets/Sprites/FangosoTitan/Hurt_", 3);
            spritesDeath = CargarArraySprites("Assets/Sprites/FangosoTitan/Death_", 6);
        }
#endif
    }

#if UNITY_EDITOR
    private Sprite[] CargarArraySprites(string basePath, int count)
    {
        Sprite[] arr = new Sprite[count];
        for (int i = 0; i < count; i++)
        {
            arr[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"{basePath}{i}.png");
        }
        return arr;
    }
#endif

    private void Update()
    {
        if (estaMuerto) return;

        if (transformJugador == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) transformJugador = p.transform;
        }

        // Animación idle cuando no está atacando ni herido
        if (!estaAtacando && !estaHerido && spritesIdle != null && spritesIdle.Length > 0)
        {
            animTimer += Time.deltaTime;
            if (animTimer >= 1f / fpsAnim)
            {
                animTimer = 0f;
                frameIndex = (frameIndex + 1) % spritesIdle.Length;
                if (sr != null && spritesIdle[frameIndex] != null)
                {
                    sr.sprite = spritesIdle[frameIndex];
                }
            }
        }

        if (transformJugador == null) return;

        // Mirar hacia el jugador
        float dirX = transformJugador.position.x - transform.position.x;
        float signo = dirX >= 0 ? 1f : -1f;
        if (visualTrans != null)
        {
            visualTrans.localScale = new Vector3(signo * Mathf.Abs(escalaInicial.x), escalaInicial.y, escalaInicial.z);
        }

        if (Time.time >= tiempoProximoAtaque && !estaAtacando)
        {
            tiempoProximoAtaque = Time.time + intervaloEntreAtaques;
            if (Random.value > 0.5f)
            {
                StartCoroutine(AtaqueAbanicoLodo(signo));
            }
            else
            {
                StartCoroutine(AtaqueGolpeSuelo());
            }
        }
    }

    private IEnumerator AtaqueAbanicoLodo(float direccionX)
    {
        estaAtacando = true;

        if (spritesRoar != null && spritesRoar.Length > 0 && sr != null)
        {
            for (int i = 0; i < spritesRoar.Length; i++)
            {
                if (spritesRoar[i] != null) sr.sprite = spritesRoar[i];
                yield return new WaitForSeconds(0.12f);
            }
        }

        // Ráfaga en abanico de 3 proyectiles de lodo
        Vector2[] direcciones = new Vector2[]
        {
            new Vector2(direccionX, 0.4f).normalized,
            new Vector2(direccionX, 0f).normalized,
            new Vector2(direccionX, -0.3f).normalized
        };

        foreach (var dir in direcciones)
        {
            if (prefabBalaBarro != null)
            {
                GameObject b = Instantiate(prefabBalaBarro, transform.position + new Vector3(direccionX * 1.5f, 0.8f, 0f), Quaternion.identity);
                Bala comp = b.GetComponent<Bala>();
                if (comp != null) comp.ConfigurarDireccion(dir);
                else
                {
                    Rigidbody2D rbB = b.GetComponent<Rigidbody2D>();
                    if (rbB != null) rbB.linearVelocity = dir * velocidadProyectil;
                }
            }
        }

        yield return new WaitForSeconds(0.2f);
        estaAtacando = false;
    }

    private IEnumerator AtaqueGolpeSuelo()
    {
        estaAtacando = true;

        // Salto vertical y azote en el suelo con animación GroundSlam
        if (spritesGroundSlam != null && spritesGroundSlam.Length > 0 && sr != null)
        {
            // Fase ascenso
            if (spritesGroundSlam.Length > 0) sr.sprite = spritesGroundSlam[0];
            rb.linearVelocity = new Vector2(0f, 7.5f);
            yield return new WaitForSeconds(0.25f);

            if (spritesGroundSlam.Length > 1) sr.sprite = spritesGroundSlam[1];
            yield return new WaitForSeconds(0.15f);

            // Fase descenso violento
            if (spritesGroundSlam.Length > 2) sr.sprite = spritesGroundSlam[2];
            rb.linearVelocity = new Vector2(0f, -14f);
            yield return new WaitForSeconds(0.2f);

            // Impacto en el suelo
            for (int i = 3; i < spritesGroundSlam.Length; i++)
            {
                if (spritesGroundSlam[i] != null) sr.sprite = spritesGroundSlam[i];
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, 7f);
            yield return new WaitForSeconds(0.4f);
            rb.linearVelocity = new Vector2(0f, -12f);
            yield return new WaitForSeconds(0.2f);
        }

        // Onda sísmica en área cercana
        if (transformJugador != null && Vector2.Distance(transform.position, transformJugador.position) < 5f)
        {
            IllariPlatformer illari = transformJugador.GetComponent<IllariPlatformer>();
            if (illari != null) illari.RecibirDanio(1);
        }

        estaAtacando = false;
    }

    public void RecibirDanio(int cantidad = 1)
    {
        if (estaMuerto) return;

        saludActual -= cantidad;
        if (saludActual <= 0)
        {
            StartCoroutine(RutinaMuerteAnimada());
        }
        else
        {
            StartCoroutine(RutinaHurtAnimado());
        }
    }

    private IEnumerator RutinaHurtAnimado()
    {
        estaHerido = true;
        if (spritesHurt != null && spritesHurt.Length > 0 && sr != null)
        {
            for (int i = 0; i < spritesHurt.Length; i++)
            {
                if (spritesHurt[i] != null) sr.sprite = spritesHurt[i];
                sr.color = Color.cyan;
                yield return new WaitForSeconds(0.10f);
            }
            sr.color = Color.white;
        }
        else
        {
            if (sr != null) sr.color = Color.cyan;
            yield return new WaitForSeconds(0.15f);
            if (sr != null) sr.color = Color.white;
        }
        estaHerido = false;
    }

    private IEnumerator RutinaMuerteAnimada()
    {
        estaMuerto = true;
        if (col != null) col.enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        if (spritesDeath != null && spritesDeath.Length > 0 && sr != null)
        {
            for (int i = 0; i < spritesDeath.Length; i++)
            {
                if (spritesDeath[i] != null) sr.sprite = spritesDeath[i];
                yield return new WaitForSeconds(0.12f);
            }
        }

        // Drop de 2 botellas de plástico finales
        if (prefabBotellaDrop != null)
        {
            Instantiate(prefabBotellaDrop, transform.position + Vector3.left * 0.8f, Quaternion.identity);
            Instantiate(prefabBotellaDrop, transform.position + Vector3.right * 0.8f, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
