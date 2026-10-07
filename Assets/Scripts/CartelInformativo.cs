using System.Collections;
using UnityEngine;

/// <summary>
/// Cartel informativo ambiental para los parques de Huancayo en 'Iyari'.
/// Muestra datos reales del ODS 12 y 15 (Producción Responsable y Vida de Ecosistemas Terrestres).
/// Al aproximarse el jugador:
/// - El cartel en el mundo aumenta de escala suavemente con un pop visual.
/// - Un indicador flotante interactivo superior vibra y llama la atención.
/// - Se despliega en pantalla un Gran Panel ODS de alta visibilidad con tipografía grande y marco arcade.
/// Al alejarse, el panel se oculta y el cartel vuelve a su escala normal.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class CartelInformativo : MonoBehaviour
{
    [Header("Información Ambiental ODS 12 & 15")]
    [SerializeField] private string tituloParque = "PARQUE DE HUANCAYO";
    [TextArea(3, 6)]
    [SerializeField] private string mensajeEcológico = "ODS 15: En Huancayo se generan más de 300 toneladas de residuos diarios. ¡Cuidemos nuestros parques y cerros!";

    [Header("Efectos Visuales en el Mundo")]
    [SerializeField] private float factorAumento = 1.28f;
    [SerializeField] private float velocidadEscala = 8.0f;

    private BoxCollider2D triggerCol;
    private Transform transVisual;
    private Transform transIndicador;
    private Vector3 escalaOriginalVisual = Vector3.one;
    private Vector3 escalaTargetVisual = Vector3.one;
    private bool jugadorCerca = false;

    private void Awake()
    {
        triggerCol = GetComponent<BoxCollider2D>();
        if (triggerCol != null) triggerCol.isTrigger = true;

        transVisual = transform.Find("Visual");
        if (transVisual != null)
        {
            escalaOriginalVisual = transVisual.localScale;
            escalaTargetVisual = escalaOriginalVisual;
        }

        CrearIndicadorFlotante();
    }

    private void Start()
    {
        if (transVisual != null)
        {
            escalaOriginalVisual = transVisual.localScale;
            escalaTargetVisual = escalaOriginalVisual;
        }
    }

    private void CrearIndicadorFlotante()
    {
        if (transform.Find("Indicador_Lectura") != null) return;

        GameObject ind = new GameObject("Indicador_Lectura");
        ind.transform.SetParent(transform, false);
        ind.transform.localPosition = new Vector3(0f, 1.65f, 0f);
        transIndicador = ind.transform;

        // Sprite de exclamación / info
        SpriteRenderer sr = ind.AddComponent<SpriteRenderer>();
        sr.sprite = GameManagerIyari.CargarSpriteUI("IconoBotonContinuar");
        if (sr.sprite == null) sr.sprite = GameManagerIyari.ObtenerSpriteBlanco();
        sr.color = new Color(0.2f, 1f, 0.88f, 0.95f);
        sr.sortingOrder = 5;
        ind.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
    }

    private void Update()
    {
        // Animación suave de escala al acercarse
        if (transVisual != null)
        {
            transVisual.localScale = Vector3.Lerp(transVisual.localScale, escalaTargetVisual, Time.deltaTime * velocidadEscala);
        }

        // Bobbing vertical del indicador flotante
        if (transIndicador != null)
        {
            float bobbing = Mathf.Sin(Time.time * 4.5f) * 0.12f;
            float rotZ = Mathf.Sin(Time.time * 3f) * 4f;
            transIndicador.localPosition = new Vector3(0f, 1.65f + bobbing, 0f);
            transIndicador.localRotation = Quaternion.Euler(0f, 0f, rotZ);

            // Escala pulsante del indicador
            float pulso = jugadorCerca ? (1f + Mathf.PingPong(Time.time * 2f, 0.25f)) : 0.9f;
            transIndicador.localScale = new Vector3(0.55f * pulso, 0.55f * pulso, 1f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugadorCerca = true;
            escalaTargetVisual = escalaOriginalVisual * factorAumento;
            AudioManager.Instancia?.ReproducirSFX("click");

            if (GameManagerIyari.Instancia != null)
            {
                GameManagerIyari.Instancia.MostrarCartelODS(mensajeEcológico, tituloParque);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugadorCerca = false;
            escalaTargetVisual = escalaOriginalVisual;

            if (GameManagerIyari.Instancia != null)
            {
                GameManagerIyari.Instancia.OcultarCartelODS();
            }
        }
    }

    public void ConfigurarMensaje(string nuevoMensaje, string nuevoTitulo = "")
    {
        mensajeEcológico = nuevoMensaje;
        if (!string.IsNullOrEmpty(nuevoTitulo))
        {
            tituloParque = nuevoTitulo;
        }
    }
}
