using UnityEngine;

/// <summary>
/// Peligro ambiental de Charco Tóxico / Radiactivo para 'Illari'.
/// Daña periódicamente a Illari cuando entra en contacto (OnTriggerStay2D).
/// Incluye animación visual: ondulación del sprite, brillo pulsante verde neón,
/// burbujas procedurales y emisión de partículas de gas tóxico.
/// Sigue las reglas de AGENTS.md: Visual en hijo, sin GetComponent en Update.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class CharcoToxico : MonoBehaviour
{
    [Header("Configuración de Daño")]
    [SerializeField] private int danio = 1;
    [SerializeField] private float cadenciaDanio = 0.8f;

    [Header("Visual Animado")]
    [SerializeField] private float velocidadOndulacion = 2.2f;
    [SerializeField] private float amplitudOndulacion = 0.04f;
    [SerializeField] private float velocidadBrillo = 3.5f;

    // Referencias cacheadas en Awake
    private BoxCollider2D col;
    private SpriteRenderer sr;
    private Transform visualTransform;
    private float tiempoProximoDanio = 0f;
    private Vector3 escalaBase;
    private Color colorBase;

    // Burbujas procedurales
    private GameObject[] burbujas;
    private int numBurbujas = 6;
    private float[] faseBurbuja;
    private float[] velocidadBurbuja;

    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;

        // Buscar SpriteRenderer en hijo Visual
        Transform hijo = transform.Find("Visual");
        if (hijo != null)
        {
            visualTransform = hijo;
            sr = hijo.GetComponent<SpriteRenderer>();
        }
        else
        {
            // Fallback: buscar en este mismo objeto
            sr = GetComponentInChildren<SpriteRenderer>();
            visualTransform = (sr != null) ? sr.transform : transform;
        }

        if (sr != null)
        {
            escalaBase = visualTransform.localScale;
            colorBase = new Color(0.1f, 0.95f, 0.25f, 0.88f);
            sr.color = colorBase;
        }

        CrearBurbujas();
    }

    private void CrearBurbujas()
    {
        burbujas = new GameObject[numBurbujas];
        faseBurbuja = new float[numBurbujas];
        velocidadBurbuja = new float[numBurbujas];

        if (col == null) return;
        float ancho = col.size.x;
        float alto = col.size.y;

        for (int i = 0; i < numBurbujas; i++)
        {
            GameObject b = new GameObject($"Burbuja_{i}");
            b.transform.SetParent(transform);

            float x = Random.Range(-ancho * 0.45f, ancho * 0.45f);
            float y = Random.Range(-alto * 0.3f, alto * 0.2f);
            b.transform.localPosition = new Vector3(x, y, -0.1f);

            float sz = Random.Range(0.08f, 0.22f);
            b.transform.localScale = Vector3.one * sz;

            SpriteRenderer bsr = b.AddComponent<SpriteRenderer>();
            // Crear textura circular procedural
            bsr.sprite = CrearSpriteCiruclo();
            bsr.color = new Color(0.0f, 1f, 0.35f, 0.75f);
            bsr.sortingOrder = 4;

            faseBurbuja[i] = Random.Range(0f, Mathf.PI * 2f);
            velocidadBurbuja[i] = Random.Range(0.6f, 1.6f);
            burbujas[i] = b;
        }
    }

    private Sprite CrearSpriteCiruclo()
    {
        int res = 16;
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color transparente = new Color(0, 0, 0, 0);
        Color blanco = Color.white;
        float radio = res / 2f;
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = x - radio + 0.5f;
                float dy = y - radio + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float borde = Mathf.Clamp01((radio - dist) / 1.5f);
                tex.SetPixel(x, y, dist < radio ? new Color(1, 1, 1, borde) : transparente);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), res);
    }

    private void Update()
    {
        AnimarVisual();
        AnimarBurbujas();
    }

    private void AnimarVisual()
    {
        if (sr == null || visualTransform == null) return;

        float t = Time.time;

        // Brillo pulsante neón verde
        float pulso = (Mathf.Sin(t * velocidadBrillo) * 0.5f + 0.5f);
        sr.color = Color.Lerp(
            new Color(0.05f, 0.78f, 0.18f, 0.82f),
            new Color(0.35f, 1f, 0.45f, 0.98f),
            pulso
        );

        // Ondulación suave (squash & stretch)
        float onda = Mathf.Sin(t * velocidadOndulacion);
        float scaleX = escalaBase.x * (1f + amplitudOndulacion * onda);
        float scaleY = escalaBase.y * (1f - amplitudOndulacion * 0.5f * onda);
        visualTransform.localScale = new Vector3(scaleX, scaleY, escalaBase.z);
    }

    private void AnimarBurbujas()
    {
        if (burbujas == null || col == null) return;

        float alto = col.size.y;

        for (int i = 0; i < numBurbujas; i++)
        {
            if (burbujas[i] == null) continue;

            float fase = faseBurbuja[i] + Time.time * velocidadBurbuja[i];
            Vector3 pos = burbujas[i].transform.localPosition;

            // Las burbujas suben y se reinician
            pos.y += velocidadBurbuja[i] * 0.18f * Time.deltaTime;
            pos.x += Mathf.Sin(fase * 1.5f) * Time.deltaTime * 0.12f;

            // Cuando sale por arriba, reiniciar abajo
            if (pos.y > alto * 0.55f)
            {
                pos.y = -alto * 0.4f;
                pos.x = Random.Range(-col.size.x * 0.4f, col.size.x * 0.4f);
                faseBurbuja[i] = Random.Range(0f, Mathf.PI * 2f);
            }

            burbujas[i].transform.localPosition = pos;

            // Pulso de opacidad en la burbuja
            SpriteRenderer bsr = burbujas[i].GetComponent<SpriteRenderer>();
            if (bsr != null)
            {
                float alpha = Mathf.Abs(Mathf.Sin(fase * 2.5f)) * 0.7f + 0.15f;
                Color c = bsr.color;
                c.a = alpha;
                bsr.color = c;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time < tiempoProximoDanio) return;

        IllariPlatformer jugador = other.GetComponent<IllariPlatformer>();
        if (jugador != null)
        {
            if (jugador.TieneEscudo()) return; // Inmunidad total a líquidos tóxicos con el Escudo Acuático
            tiempoProximoDanio = Time.time + cadenciaDanio;
            jugador.RecibirDanio(danio);
        }
    }
}
