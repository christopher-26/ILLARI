using System.Collections;
using UnityEngine;

/// <summary>
/// Enemigo: Fangoso Blindado (Chatarrero de Llantas).
/// Porta chatarra y llantas recicladas en su frente que bloquean los disparos directos.
/// Cuenta con animación flipbook completa (Idle, Walk, ShieldBash, Hurt, Death)
/// cargada desde Assets/Sprites/FangosoBlindado.
/// Al ser purificado, suelta una Botella de Plástico coleccionable (ODS 12/15).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyArmored : MonoBehaviour
{
    [Header("Salud y Blindaje")]
    [SerializeField] private int salud = 4;
    [SerializeField] private bool tieneBlindajeFrontal = true;
    [SerializeField] private GameObject prefabBotellaDrop;

    [Header("Movimiento")]
    [SerializeField] private float velocidadPatrulla = 1.3f;
    [SerializeField] private LayerMask capaSuelo;
    [SerializeField] private string tagSuelo = "Ground";

    [Header("Sprites Flipbook Fangoso Blindado")]
    [SerializeField] private Sprite[] spritesIdle;
    [SerializeField] private Sprite[] spritesWalk;
    [SerializeField] private Sprite[] spritesShieldBash;
    [SerializeField] private Sprite[] spritesHurt;
    [SerializeField] private Sprite[] spritesDeath;
    [SerializeField] private float fpsAnim = 10f;

    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;
    private Transform visualTrans;
    private float direccionX = 1f;
    private bool estaMuerto = false;
    private bool estaHerido = false;
    private Vector3 escalaInicial = Vector3.one;
    private float animTimer = 0f;
    private int frameIndex = 0;

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
        if (sr != null && spritesWalk != null && spritesWalk.Length > 0)
        {
            sr.sprite = spritesWalk[0];
        }
    }

    private void CargarSpritesSiEsNecesario()
    {
#if UNITY_EDITOR
        if (spritesWalk == null || spritesWalk.Length == 0)
        {
            spritesIdle = CargarArraySprites("Assets/Sprites/FangosoBlindado/Idle_", 4);
            spritesWalk = CargarArraySprites("Assets/Sprites/FangosoBlindado/Walk_", 6);
            spritesShieldBash = CargarArraySprites("Assets/Sprites/FangosoBlindado/ShieldBash_", 4);
            spritesHurt = CargarArraySprites("Assets/Sprites/FangosoBlindado/Hurt_", 2);
            spritesDeath = CargarArraySprites("Assets/Sprites/FangosoBlindado/Death_", 6);
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
        if (estaMuerto || estaHerido) return;

        // Animación de caminata pesada
        if (spritesWalk != null && spritesWalk.Length > 0)
        {
            animTimer += Time.deltaTime;
            if (animTimer >= 1f / fpsAnim)
            {
                animTimer = 0f;
                frameIndex = (frameIndex + 1) % spritesWalk.Length;
                if (sr != null && spritesWalk[frameIndex] != null)
                {
                    sr.sprite = spritesWalk[frameIndex];
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (estaMuerto)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        PatrullaPesada();
    }

    private void PatrullaPesada()
    {
        float radioX = (col != null) ? col.bounds.extents.x : 0.6f;
        float radioY = (col != null) ? col.bounds.extents.y : 0.7f;

        Vector2 origenFrente = (Vector2)transform.position + new Vector2(direccionX * (radioX + 0.15f), 0f);
        RaycastHit2D hitMuro = Physics2D.Raycast(origenFrente, Vector2.right * direccionX, 0.5f, capaSuelo);
        bool hayMuro = hitMuro.collider != null && hitMuro.collider.CompareTag(tagSuelo);

        Vector2 origenVacio = (Vector2)transform.position + new Vector2(direccionX * (radioX + 0.25f), -radioY + 0.25f);
        RaycastHit2D hitSuelo = Physics2D.Raycast(origenVacio, Vector2.down, 1.2f, capaSuelo);
        bool hayVacio = hitSuelo.collider == null || !hitSuelo.collider.CompareTag(tagSuelo);

        if (hayMuro || hayVacio)
        {
            direccionX *= -1f;
            visualTrans.localScale = new Vector3(direccionX * Mathf.Abs(visualTrans.localScale.x), visualTrans.localScale.y, visualTrans.localScale.z);
        }

        rb.linearVelocity = new Vector2(direccionX * velocidadPatrulla, rb.linearVelocity.y);
    }

    public void RecibirDanio(int cantidad = 1, Vector2 direccionImpacto = default)
    {
        if (estaMuerto) return;

        bool impactoFrontal = (direccionImpacto.x * direccionX) < 0f;

        if (tieneBlindajeFrontal && impactoFrontal)
        {
            salud -= 1;
            StartCoroutine(RutinaFlashBlindaje());
            if (salud <= 2)
            {
                tieneBlindajeFrontal = false;
            }
            return;
        }

        salud -= cantidad;
        if (salud <= 0)
        {
            StartCoroutine(RutinaMuerteAnimada());
        }
        else
        {
            StartCoroutine(RutinaFlashDanio());
        }
    }

    private IEnumerator RutinaFlashBlindaje()
    {
        if (sr != null)
        {
            sr.color = Color.yellow;
            yield return new WaitForSeconds(0.12f);
            sr.color = Color.white;
        }
    }

    private IEnumerator RutinaFlashDanio()
    {
        estaHerido = true;
        if (spritesHurt != null && spritesHurt.Length > 0 && sr != null)
        {
            sr.sprite = spritesHurt[0];
            sr.color = Color.cyan;
            yield return new WaitForSeconds(0.18f);
            if (spritesHurt.Length > 1) sr.sprite = spritesHurt[1];
            yield return new WaitForSeconds(0.12f);
            sr.color = Color.white;
        }
        else
        {
            if (sr != null) sr.color = Color.cyan;
            yield return new WaitForSeconds(0.2f);
            if (sr != null) sr.color = Color.white;
        }
        estaHerido = false;
    }

    private IEnumerator RutinaMuerteAnimada()
    {
        estaMuerto = true;
        col.enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        if (spritesDeath != null && spritesDeath.Length > 0 && sr != null)
        {
            for (int i = 0; i < spritesDeath.Length; i++)
            {
                if (spritesDeath[i] != null) sr.sprite = spritesDeath[i];
                yield return new WaitForSeconds(0.08f);
            }
        }

        if (prefabBotellaDrop != null)
        {
            Instantiate(prefabBotellaDrop, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
