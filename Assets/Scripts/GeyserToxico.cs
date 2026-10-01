using System.Collections;
using UnityEngine;

/// <summary>
/// Peligro ambiental: Geyser de vapor tóxico efervescente.
/// Expulsa periódicamente una columna de gas ácido que inflige daño
/// periódico al jugador si entra en contacto mientras está activo.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class GeyserToxico : MonoBehaviour
{
    [Header("Tiempos de Ciclo")]
    [SerializeField] private float tiempoInactivo = 2.5f;
    [SerializeField] private float tiempoActivo = 1.5f;

    [Header("Daño Periódico")]
    [SerializeField] private int danioPorGolpe = 1;
    [SerializeField] private float intervaloDanio = 0.8f;

    private Collider2D col;
    private SpriteRenderer srVisual;
    private bool estaActivo = false;
    private float tiempoSiguienteDanio = 0f;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
        srVisual = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        StartCoroutine(CicloGeyser());
    }

    private IEnumerator CicloGeyser()
    {
        while (true)
        {
            // Estado Inactivo (reposo)
            estaActivo = false;
            if (col != null) col.enabled = false;
            if (srVisual != null) srVisual.color = new Color(0.2f, 1f, 0.3f, 0.15f);

            yield return new WaitForSeconds(tiempoInactivo);

            // Estado Activo (expulsión de vapor)
            estaActivo = true;
            if (col != null) col.enabled = true;
            if (srVisual != null) srVisual.color = new Color(0.1f, 1f, 0.2f, 0.85f);

            yield return new WaitForSeconds(tiempoActivo);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!estaActivo) return;

        if (other.CompareTag("Player") && Time.time >= tiempoSiguienteDanio)
        {
            tiempoSiguienteDanio = Time.time + intervaloDanio;
            var jugador = other.GetComponent<IllariPlatformer>();
            if (jugador != null)
            {
                jugador.RecibirDanio(danioPorGolpe);
            }
            else
            {
                GameManagerIyari.Instancia?.RecibirDanio(danioPorGolpe);
            }
        }
    }
}
