using UnityEngine;

/// <summary>
/// Cartel informativo ambiental para los parques de Huancayo en 'Illari'.
/// Muestra datos reales del ODS 15 (Vida de Ecosistemas Terrestres) y
/// educación sobre residuos sólidos al aproximarse el jugador.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class CartelInformativo : MonoBehaviour
{
    [Header("Información Ambiental ODS 15")]
    [TextArea(2, 4)]
    [SerializeField] private string mensajeEcológico = "ODS 15: En Huancayo se generan más de 300 toneladas de residuos diarios. ¡Cuidemos nuestros parques y cerros!";
    

    private BoxCollider2D triggerCol;
    private bool mensajeMostrado = false;

    private void Awake()
    {
        triggerCol = GetComponent<BoxCollider2D>();
        triggerCol.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (mensajeMostrado) return;

        if (collision.CompareTag("Player"))
        {
            mensajeMostrado = true;
            if (GameManagerIyari.Instancia != null)
            {
                GameManagerIyari.Instancia.MostrarMensajeAviso(mensajeEcológico);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Permitir volver a leer el cartel si el jugador vuelve a pasar más tarde
            mensajeMostrado = false;
        }
    }

    public void ConfigurarMensaje(string nuevoMensaje)
    {
        mensajeEcológico = nuevoMensaje;
    }
}
