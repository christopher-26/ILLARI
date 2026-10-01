using System.Collections;
using UnityEngine;

/// <summary>
/// Ítem coleccionable de Mejora / Poder (Power-Up) para Illari.
/// Puede ser encontrado en plataformas elevadas (parkour) o soltado por enemigos derrotados.
/// Tipos de Poder:
/// - DisparoTriple: Dispara 3 ráfagas de agua en abanico (18°).
/// - EscudoAcuatico: Esfera de agua protectora que absorbe 1 impacto e inmuniza contra charcos tóxicos.
/// - DobleSalto: Botas de impulso hidráulico para saltar dos veces en el aire.
/// - CuracionVida: Frasco de agua purificada que restaura 1 corazón de salud.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class ItemMejora : MonoBehaviour
{
    public enum TipoMejora
    {
        DisparoTriple,
        EscudoAcuatico,
        DobleSalto,
        CuracionVida
    }

    [Header("Configuración del Poder")]
    [SerializeField] private TipoMejora tipo = TipoMejora.DisparoTriple;
    [SerializeField] private int cantidadMunicion = 30;

    [Header("Animación de Levitación")]
    [SerializeField] private float velocidadFlotado = 3.2f;
    [SerializeField] private float alturaFlotado = 0.16f;
    [SerializeField] private float velocidadPulso = 4f;

    private CircleCollider2D col;
    private SpriteRenderer srFondo;
    private SpriteRenderer srIcono;
    private Vector3 posInicial;
    private bool recolectado = false;

    private void Awake()
    {
        col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        posInicial = transform.position;
        ConstruirVisualesProcedurales();
    }

    private void Start()
    {
        posInicial = transform.position;
    }

    public void ConfigurarTipo(TipoMejora nuevoTipo)
    {
        tipo = nuevoTipo;
        ConstruirVisualesProcedurales();
    }

    private void ConstruirVisualesProcedurales()
    {
        // 1. Orbe exterior con brillo pulsante
        Transform trFondo = transform.Find("Visual_Orbe");
        if (trFondo == null)
        {
            GameObject goF = new GameObject("Visual_Orbe");
            goF.transform.SetParent(transform, false);
            trFondo = goF.transform;
        }

        srFondo = trFondo.GetComponent<SpriteRenderer>();
        if (srFondo == null) srFondo = trFondo.gameObject.AddComponent<SpriteRenderer>();
        srFondo.sprite = ObtenerSpriteCirculo();
        srFondo.sortingOrder = 6;
        trFondo.localScale = new Vector3(0.72f, 0.72f, 1f);

        // 2. Ícono interior distintivo
        Transform trIcono = transform.Find("Visual_Icono");
        if (trIcono == null)
        {
            GameObject goI = new GameObject("Visual_Icono");
            goI.transform.SetParent(transform, false);
            trIcono = goI.transform;
        }

        srIcono = trIcono.GetComponent<SpriteRenderer>();
        if (srIcono == null) srIcono = trIcono.gameObject.AddComponent<SpriteRenderer>();
        srIcono.sortingOrder = 7;
        trIcono.localScale = new Vector3(0.48f, 0.48f, 1f);

        Color colorOrbe;
        Sprite sprIcono = null;

        switch (tipo)
        {
            case TipoMejora.DisparoTriple:
                colorOrbe = new Color(0f, 0.95f, 1f, 0.85f); // Cian agua
                sprIcono = CargarSprite("IconoBotellaHUD");
                break;
            case TipoMejora.EscudoAcuatico:
                colorOrbe = new Color(0.2f, 0.6f, 1f, 0.85f); // Azul zafiro
                sprIcono = CargarSprite("IconoBotellaHUD");
                break;
            case TipoMejora.DobleSalto:
                colorOrbe = new Color(1f, 0.85f, 0.2f, 0.85f); // Dorado brillante
                sprIcono = CargarSprite("IconoBotellaHUD");
                break;
            case TipoMejora.CuracionVida:
            default:
                colorOrbe = new Color(0.2f, 1f, 0.4f, 0.85f); // Verde esmeralda curativo
                sprIcono = CargarSprite("CorazonLleno");
                break;
        }

        srFondo.color = colorOrbe;
        if (sprIcono != null)
        {
            srIcono.sprite = sprIcono;
            srIcono.color = Color.white;
        }
    }

    private Sprite CargarSprite(string nombre)
    {
        Sprite s = Resources.Load<Sprite>("UI/" + nombre);
#if UNITY_EDITOR
        if (s == null)
        {
            s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/" + nombre + ".png");
        }
#endif
        return s;
    }

    private static Sprite sprCirculoCache;
    private static Sprite ObtenerSpriteCirculo()
    {
        if (sprCirculoCache == null)
        {
            int r = 32;
            int diam = r * 2;
            Texture2D tex = new Texture2D(diam, diam, TextureFormat.RGBA32, false);
            Color[] cols = new Color[diam * diam];
            Vector2 c = new Vector2(r, r);

            for (int y = 0; y < diam; y++)
            {
                for (int x = 0; x < diam; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), c);
                    if (dist < r - 2)
                    {
                        float alpha = Mathf.Clamp01(1f - (dist / (r - 2)));
                        cols[y * diam + x] = new Color(1f, 1f, 1f, 0.5f + alpha * 0.5f);
                    }
                    else if (dist <= r)
                    {
                        cols[y * diam + x] = new Color(1f, 1f, 1f, (r - dist) / 2f);
                    }
                    else
                    {
                        cols[y * diam + x] = Color.clear;
                    }
                }
            }
            tex.SetPixels(cols);
            tex.Apply();
            sprCirculoCache = Sprite.Create(tex, new Rect(0, 0, diam, diam), new Vector2(0.5f, 0.5f), 64f);
        }
        return sprCirculoCache;
    }

    private void Update()
    {
        if (recolectado) return;

        // Movimiento de flotación senoidal suave
        float flotado = Mathf.Sin(Time.time * velocidadFlotado) * alturaFlotado;
        transform.position = posInicial + new Vector3(0, flotado, 0);

        // Pulso de brillo en el orbe
        if (srFondo != null)
        {
            float pulso = Mathf.Sin(Time.time * velocidadPulso) * 0.15f;
            Transform trF = srFondo.transform;
            trF.localScale = new Vector3(0.72f + pulso, 0.72f + pulso, 1f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (recolectado) return;
        if (!collision.CompareTag("Player")) return;

        IllariPlatformer jugador = collision.GetComponent<IllariPlatformer>();
        if (jugador == null) return;

        recolectado = true;
        AplicarMejora(jugador);
        StartCoroutine(RutinaEfectoRecoleccion());
    }

    private void AplicarMejora(IllariPlatformer jugador)
    {
        switch (tipo)
        {
            case TipoMejora.DisparoTriple:
                jugador.ActivarPoderDisparoTriple(cantidadMunicion);
                if (GameManagerIyari.Instancia != null)
                {
                    GameManagerIyari.Instancia.MostrarMensajeAviso("💦 ¡DISPARO TRIPLE OBTENIDO!\nRáfaga en abanico para limpiar grupos de monstruos.");
                }
                break;

            case TipoMejora.EscudoAcuatico:
                jugador.ActivarEscudoAcuatico();
                if (GameManagerIyari.Instancia != null)
                {
                    GameManagerIyari.Instancia.MostrarMensajeAviso("🛡 ¡ESCUDO ACUÁTICO ACTIVADO!\nInmunidad contra charcos tóxicos y absorbe 1 impacto.");
                }
                break;

            case TipoMejora.DobleSalto:
                jugador.ActivarDobleSalto();
                if (GameManagerIyari.Instancia != null)
                {
                    GameManagerIyari.Instancia.MostrarMensajeAviso("🦘 ¡BOTAS HIDRÁULICAS ACTIVADAS!\nSalto acrobático doble para alcanzar plataformas altas.");
                }
                break;

            case TipoMejora.CuracionVida:
                if (GameManagerIyari.Instancia != null)
                {
                    GameManagerIyari.Instancia.ModificarVida(1);
                    GameManagerIyari.Instancia.MostrarMensajeAviso("💖 ¡VIDA PURIFICADA!\n+1 Corazón de salud restaurado.");
                }
                break;
        }
    }

    private IEnumerator RutinaEfectoRecoleccion()
    {
        // Explosión de partículas visuales al recoger
        col.enabled = false;
        float duracion = 0.25f;
        float t = 0f;
        Vector3 escalaInicial = transform.localScale;

        while (t < duracion)
        {
            t += Time.deltaTime;
            float p = t / duracion;
            transform.localScale = escalaInicial * (1f + p * 1.2f);
            if (srFondo != null)
            {
                Color c = srFondo.color;
                c.a = 1f - p;
                srFondo.color = c;
            }
            if (srIcono != null)
            {
                Color c = srIcono.color;
                c.a = 1f - p;
                srIcono.color = c;
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}
