using System.Collections;
using UnityEngine;

/// <summary>
/// Fangoso Cañonero: Enemigo pesado con mortero de lodo montado en la espalda.
/// Dispara proyectiles en arco parabólico hacia la posición de Illari.
/// Incorpora animación flipbook completa (Idle, Mortar, Hurt, Death)
/// cargada desde Assets/Sprites/FangosoCanionero.
/// Al ser purificado, suelta obligatoriamente 1 Botella de Plástico (ODS 12/15).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class EnemyCannoneer : MonoBehaviour
{
    [Header("Parámetros de Salud")]
    [SerializeField] private int salud = 4;

    [Header("Ataque")]
    [SerializeField] private GameObject prefabBalaBarro;
    [SerializeField] private GameObject prefabBotellaDrop;
    [SerializeField] private float cadenciaDisparo = 2.8f;
    [SerializeField] private float rangoDeteccion = 14f;
    [SerializeField] private float velocidadMortero = 10f;

    [Header("Sprites Flipbook Cañonero")]
    [SerializeField] private Sprite[] spritesIdle;
    [SerializeField] private Sprite[] spritesMortar;
    [SerializeField] private Sprite[] spritesHurt;
    [SerializeField] private Sprite[] spritesDeath;
    [SerializeField] private float fpsAnim = 8f;

    private Rigidbody2D rb;
    private BoxCollider2D col;
    private SpriteRenderer sr;
    private Transform jugador;
    private float tiempoSiguienteDisparo = 0f;
    private bool estaMuerto = false;
    private bool estaDisparando = false;
    private bool estaHerido = false;
    private Transform visualTrans;
    private Vector3 escalaInicial = Vector3.one;
    private float animTimer = 0f;
    private int frameIndex = 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        col = GetComponent<BoxCollider2D>();
        visualTrans = transform.Find("Visual");
        if (visualTrans == null) visualTrans = transform;
        sr = visualTrans.GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        escalaInicial = visualTrans.localScale;

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
        if (p != null) jugador = p.transform;
        tiempoSiguienteDisparo = Time.time + 1.2f;

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
            spritesIdle = CargarArraySprites("Assets/Sprites/FangosoCanionero/Idle_", 4);
            spritesMortar = CargarArraySprites("Assets/Sprites/FangosoCanionero/Mortar_", 6);
            spritesHurt = CargarArraySprites("Assets/Sprites/FangosoCanionero/Hurt_", 2);
            spritesDeath = CargarArraySprites("Assets/Sprites/FangosoCanionero/Death_", 5);
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

        if (jugador == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) jugador = p.transform;
        }

        // Animación idle cuando no dispara ni está herido
        if (!estaDisparando && !estaHerido && spritesIdle != null && spritesIdle.Length > 0)
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

        if (jugador == null) return;

        float distancia = Vector2.Distance(transform.position, jugador.position);
        if (distancia <= rangoDeteccion)
        {
            float dirX = (jugador.position.x > transform.position.x) ? 1f : -1f;
            if (visualTrans != null)
            {
                visualTrans.localScale = new Vector3(Mathf.Abs(escalaInicial.x) * dirX, escalaInicial.y, escalaInicial.z);
            }

            if (Time.time >= tiempoSiguienteDisparo && !estaDisparando)
            {
                tiempoSiguienteDisparo = Time.time + cadenciaDisparo;
                StartCoroutine(RutinaDispararMorteroAnimado());
            }
        }
    }

    private IEnumerator RutinaDispararMorteroAnimado()
    {
        estaDisparando = true;

        if (spritesMortar != null && spritesMortar.Length > 0 && sr != null)
        {
            // Reproducir frames de apuntado y retroceso
            for (int i = 0; i < spritesMortar.Length; i++)
            {
                if (spritesMortar[i] != null) sr.sprite = spritesMortar[i];

                // En el frame 3 se suelta la bala del mortero
                if (i == 3)
                {
                    DispararProyectilMortero();
                }

                yield return new WaitForSeconds(0.12f);
            }
        }
        else
        {
            DispararProyectilMortero();
            yield return new WaitForSeconds(0.3f);
        }

        estaDisparando = false;
    }

    private void DispararProyectilMortero()
    {
        if (prefabBalaBarro == null || jugador == null) return;

        Vector3 spawnPos = transform.position + new Vector3(transform.localScale.x > 0 ? 0.8f : -0.8f, 1.0f, 0f);
        GameObject bala = Instantiate(prefabBalaBarro, spawnPos, Quaternion.identity);

        Rigidbody2D rbBala = bala.GetComponent<Rigidbody2D>();
        if (rbBala != null)
        {
            rbBala.gravityScale = 1.4f;
            float dirX = transform.localScale.x > 0 ? 1f : -1f;
            float distanciaX = Mathf.Abs(jugador.position.x - transform.position.x);
            float velX = dirX * Mathf.Clamp(distanciaX * 0.9f, 4f, velocidadMortero);
            float velY = 8.5f;

            rbBala.linearVelocity = new Vector2(velX, velY);
        }

        Bala bComp = bala.GetComponent<Bala>();
        if (bComp != null)
        {
            bComp.ConfigurarDireccion(Vector2.up);
        }
    }

    public void RecibirDanio(int cantidad = 1)
    {
        if (estaMuerto) return;

        salud -= cantidad;
        if (salud <= 0)
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
            sr.sprite = spritesHurt[0];
            sr.color = Color.cyan;
            yield return new WaitForSeconds(0.15f);
            if (spritesHurt.Length > 1) sr.sprite = spritesHurt[1];
            yield return new WaitForSeconds(0.10f);
            sr.color = Color.white;
        }
        else
        {
            if (sr != null) sr.color = Color.white;
            yield return new WaitForSeconds(0.15f);
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
                yield return new WaitForSeconds(0.10f);
            }
        }

        if (prefabBotellaDrop != null)
        {
            Instantiate(prefabBotellaDrop, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
