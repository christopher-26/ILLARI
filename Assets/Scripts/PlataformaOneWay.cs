using System.Collections;
using UnityEngine;

/// <summary>
/// Plataforma de un solo sentido (One-Way):
/// Permite que Illari salte a través de ella desde abajo.
/// Si el jugador presiona 'S' o 'Flecha Abajo' junto con 'Espacio', se desactiva
/// brevemente la colisión para permitir descender.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class PlataformaOneWay : MonoBehaviour
{
    private BoxCollider2D col;
    private GameObject jugador;
    private Collider2D colJugador;

    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        jugador = GameObject.FindWithTag("Player");
        if (jugador != null)
        {
            colJugador = jugador.GetComponent<Collider2D>();
        }
    }

    private void Update()
    {
        if (jugador == null || colJugador == null) return;

        // Descender de la plataforma al presionar abajo + salto
        bool presionaAbajo = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        bool presionaSalto = Input.GetKeyDown(KeyCode.Space);

        if (presionaAbajo && presionaSalto)
        {
            StartCoroutine(IgnorarColisionTemporal());
        }
    }

    private IEnumerator IgnorarColisionTemporal()
    {
        if (col != null && colJugador != null)
        {
            Physics2D.IgnoreCollision(col, colJugador, true);
            yield return new WaitForSeconds(0.45f);
            Physics2D.IgnoreCollision(col, colJugador, false);
        }
    }
}
