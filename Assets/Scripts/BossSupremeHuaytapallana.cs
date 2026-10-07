using System.Collections;
using UnityEngine;

/// <summary>
/// Jefe Final Supremo: Gran Coloso del Huaytapallana (Nivel 9).
/// Máxima amenaza ecológica de la cordillera Junín 2026.
/// Posee 20 puntos de salud, azotes sísmicos glaciales y ráfagas espirales de lodo tóxico.
/// Incorpora animación flipbook completa (Stance, IceSpikes, AcidBeam, Hurt, Death)
/// cargada desde Assets/Sprites/ColosoHuaytapallana.
/// Al ser purificado, suelta exactamente 3 Botellas de Plástico para completar los 15 requeridos.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class BossSupremeHuaytapallana : MonoBehaviour
{
    [Header("Parámetros del Jefe")]
    [SerializeField] private int saludMaxima = 20;
    [SerializeField] private int saludActual = 20;

    [Header("Ataques")]
    [SerializeField] private GameObject prefabBalaBarro;
    [SerializeField] private GameObject prefabBotellaDrop;
    [SerializeField] private float cadenciaAtaques = 2.4f;
    [SerializeField] private float rangoDeteccion = 20f;

    [Header("Sprites Flipbook Coloso Huaytapallana")]
    [SerializeField] private Sprite[] spritesStance;
    [SerializeField] private Sprite[] spritesIceSpikes;
    [SerializeField] private Sprite[] spritesAcidBeam;
    [SerializeField] private Sprite[] spritesHurt;
    [SerializeField] private Sprite[] spritesDeath;
    [SerializeField] private float fpsAnim = 8f;

    private Rigidbody2D rb;
    private BoxCollider2D col;
    private SpriteRenderer sr;
    private Transform jugador;
    private float tiempoSiguienteAtaque = 0f;
    private bool estaMuerto = false;
    private bool estaAtacando = false;
    private bool estaHerido = false;
    private int cicloAtaque = 0;
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
        saludActual = saludMaxima;
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) jugador = p.transform;
        tiempoSiguienteAtaque = Time.time + 1.5f;

        if (sr != null && spritesStance != null && spritesStance.Length > 0)
        {
            sr.sprite = spritesStance[0];
        }
    }

    private void CargarSpritesSiEsNecesario()
    {
#if UNITY_EDITOR
        if (spritesStance == null || spritesStance.Length == 0)
        {
            spritesStance = CargarArraySprites("Assets/Sprites/ColosoHuaytapallana/Stance_", 4);
            spritesIceSpikes = CargarArraySprites("Assets/Sprites/ColosoHuaytapallana/IceSpikes_", 6);
            spritesAcidBeam = CargarArraySprites("Assets/Sprites/ColosoHuaytapallana/AcidBeam_", 6);
            spritesHurt = CargarArraySprites("Assets/Sprites/ColosoHuaytapallana/Hurt_", 3);
            spritesDeath = CargarArraySprites("Assets/Sprites/ColosoHuaytapallana/Death_", 6);
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

        // Animación Stance cuando no ataca ni está herido
        if (!estaAtacando && !estaHerido && spritesStance != null && spritesStance.Length > 0)
        {
            animTimer += Time.deltaTime;
            if (animTimer >= 1f / fpsAnim)
            {
                animTimer = 0f;
                frameIndex = (frameIndex + 1) % spritesStance.Length;
                if (sr != null && spritesStance[frameIndex] != null)
                {
                    sr.sprite = spritesStance[frameIndex];
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

            if (Time.time >= tiempoSiguienteAtaque && !estaAtacando)
            {
                tiempoSiguienteAtaque = Time.time + cadenciaAtaques;
                EjecutarAtaqueSecuencial();
            }
        }
    }

    private void EjecutarAtaqueSecuencial()
    {
        cicloAtaque = (cicloAtaque + 1) % 3;
        if (cicloAtaque == 0)
        {
            StartCoroutine(AtaqueRafagaEspiral());
        }
        else if (cicloAtaque == 1)
        {
            StartCoroutine(AtaqueSaltoSismicoGlacial());
        }
        else
        {
            StartCoroutine(AtaqueLluviaLodo());
        }
    }

    private IEnumerator AtaqueRafagaEspiral()
    {
        estaAtacando = true;

        if (spritesAcidBeam != null && spritesAcidBeam.Length > 0 && sr != null)
        {
            for (int i = 0; i < spritesAcidBeam.Length; i++)
            {
                if (spritesAcidBeam[i] != null) sr.sprite = spritesAcidBeam[i];
                yield return new WaitForSeconds(0.10f);
            }
        }

        float dirX = transform.localScale.x > 0 ? 1f : -1f;
        Vector2[] direcciones = new Vector2[]
        {
            new Vector2(dirX, 0.6f).normalized,
            new Vector2(dirX, 0.2f).normalized,
            new Vector2(dirX, -0.2f).normalized,
            new Vector2(dirX, -0.6f).normalized
        };

        foreach (var dir in direcciones)
        {
            if (prefabBalaBarro != null)
            {
                GameObject b = Instantiate(prefabBalaBarro, transform.position + new Vector3(dirX * 1.6f, 0.9f, 0f), Quaternion.identity);
                Bala comp = b.GetComponent<Bala>();
                if (comp != null) comp.ConfigurarDireccion(dir);
            }
            yield return new WaitForSeconds(0.12f);
        }

        estaAtacando = false;
    }

    private IEnumerator AtaqueSaltoSismicoGlacial()
    {
        estaAtacando = true;

        if (spritesIceSpikes != null && spritesIceSpikes.Length > 0 && sr != null)
        {
            sr.sprite = spritesIceSpikes[0];
            rb.linearVelocity = new Vector2(0f, 9.5f);
            yield return new WaitForSeconds(0.35f);

            if (spritesIceSpikes.Length > 1) sr.sprite = spritesIceSpikes[1];
            yield return new WaitForSeconds(0.15f);

            if (spritesIceSpikes.Length > 2) sr.sprite = spritesIceSpikes[2];
            rb.linearVelocity = new Vector2(0f, -16f);
            yield return new WaitForSeconds(0.2f);

            for (int i = 3; i < spritesIceSpikes.Length; i++)
            {
                if (spritesIceSpikes[i] != null) sr.sprite = spritesIceSpikes[i];
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, 9.5f);
            yield return new WaitForSeconds(0.5f);
            rb.linearVelocity = new Vector2(0f, -16f);
            yield return new WaitForSeconds(0.25f);
        }

        // Onda expansiva de impacto
        if (jugador != null && Vector2.Distance(transform.position, jugador.position) < 6.5f)
        {
            IllariPlatformer illari = jugador.GetComponent<IllariPlatformer>();
            if (illari != null) illari.RecibirDanio(1);
        }

        estaAtacando = false;
    }

    private IEnumerator AtaqueLluviaLodo()
    {
        estaAtacando = true;

        if (spritesAcidBeam != null && spritesAcidBeam.Length > 0 && sr != null)
        {
            for (int i = 0; i < 3; i++)
            {
                if (spritesAcidBeam[i] != null) sr.sprite = spritesAcidBeam[i];
                yield return new WaitForSeconds(0.1f);
            }
        }

        for (int i = 0; i < 4; i++)
        {
            if (prefabBalaBarro != null && jugador != null)
            {
                Vector3 spawnPos = new Vector3(jugador.position.x + (i - 1.5f) * 2.2f, transform.position.y + 7f, 0f);
                GameObject b = Instantiate(prefabBalaBarro, spawnPos, Quaternion.identity);
                Rigidbody2D rbB = b.GetComponent<Rigidbody2D>();
                if (rbB != null) rbB.linearVelocity = Vector2.down * 7.5f;

                Bala comp = b.GetComponent<Bala>();
                if (comp != null) comp.ConfigurarDireccion(Vector2.down);
            }
            yield return new WaitForSeconds(0.15f);
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
            StartCoroutine(RutinaHurtAnimada());
        }
    }

    private IEnumerator RutinaHurtAnimada()
    {
        estaHerido = true;
        if (spritesHurt != null && spritesHurt.Length > 0 && sr != null)
        {
            for (int i = 0; i < spritesHurt.Length; i++)
            {
                if (spritesHurt[i] != null) sr.sprite = spritesHurt[i];
                sr.color = new Color(0.3f, 1f, 1f);
                yield return new WaitForSeconds(0.10f);
            }
            sr.color = Color.white;
        }
        else
        {
            if (sr != null)
            {
                sr.color = new Color(0.3f, 1f, 1f);
                yield return new WaitForSeconds(0.15f);
                sr.color = Color.white;
            }
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

        // Suelta exactamente 3 botellas para completar las 15 del nivel final
        if (prefabBotellaDrop != null)
        {
            Instantiate(prefabBotellaDrop, transform.position + new Vector3(-1.2f, 0.8f, 0f), Quaternion.identity);
            Instantiate(prefabBotellaDrop, transform.position + new Vector3(0f, 1.2f, 0f), Quaternion.identity);
            Instantiate(prefabBotellaDrop, transform.position + new Vector3(1.2f, 0.8f, 0f), Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
