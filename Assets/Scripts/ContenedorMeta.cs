using UnityEngine;

/// <summary>
/// Contenedor de reciclaje y Meta del nivel para 'Iyari'.
/// Exige un número determinado de botellas de vidrio recolectadas para activar la victoria.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class ContenedorMeta : MonoBehaviour
{
    [Header("Condición de Victoria")]
    [SerializeField] private int botellasRequeridas = 15;

    [Header("Efectos")]
    [SerializeField] private AudioClip sonidoVictoria;
    [SerializeField] private GameObject prefabEfectoApertura;

    private BoxCollider2D col;
    private bool nivelCompletado = false;
    private float tiempoUltimoAviso = 0f;

    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (nivelCompletado) return;

        if (other.CompareTag("Player"))
        {
            int recolectadas = GameManagerIyari.Instancia != null ? GameManagerIyari.Instancia.ObtenerBotellas() : 0;

            if (recolectadas >= botellasRequeridas)
            {
                // ¡Victoria!
                nivelCompletado = true;

                if (sonidoVictoria != null)
                {
                    AudioSource.PlayClipAtPoint(sonidoVictoria, transform.position);
                }

                if (prefabEfectoApertura != null)
                {
                    Instantiate(prefabEfectoApertura, transform.position, Quaternion.identity);
                }

                if (GameManagerIyari.Instancia != null)
                {
                    GameManagerIyari.Instancia.CompletarNivel();
                }
            }
            else
            {
                // Faltan botellas
                if (Time.time >= tiempoUltimoAviso + 2.0f)
                {
                    tiempoUltimoAviso = Time.time;
                    int faltantes = botellasRequeridas - recolectadas;

                    if (GameManagerIyari.Instancia != null)
                    {
                        GameManagerIyari.Instancia.MostrarMensajeAviso($"¡Faltan {faltantes} botellas para reciclar y abrir la meta!");
                    }
                }
            }
        }
    }

    public int ObtenerBotellasRequeridas()
    {
        return botellasRequeridas;
    }
}
