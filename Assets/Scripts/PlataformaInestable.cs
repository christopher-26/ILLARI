using System.Collections;
using UnityEngine;

/// <summary>
/// Plataforma inestable de madera o ramas de sauce andino:
/// Al ser pisada por el jugador, tiembla durante un breve retardo (0.8s) y luego
/// cae o se desactiva temporalmente, reapareciendo tras unos segundos.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PlataformaInestable : MonoBehaviour
{
    [Header("Tiempos")]
    [SerializeField] private float tiempoAntesDeCaer = 0.8f;
    [SerializeField] private float tiempoParaReaparecer = 3.0f;
    [SerializeField] private float intensidadTemblor = 0.05f;

    private Collider2D col;
    private SpriteRenderer sr;
    private Vector3 posicionOriginal;
    private bool estaCayendo = false;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        posicionOriginal = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (estaCayendo) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            // Verifica que el jugador esté encima de la plataforma
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    StartCoroutine(RutinaCaida());
                    break;
                }
            }
        }
    }

    private IEnumerator RutinaCaida()
    {
        estaCayendo = true;
        float elapsed = 0f;

        // Fase de temblor
        while (elapsed < tiempoAntesDeCaer)
        {
            float offsetX = Random.Range(-intensidadTemblor, intensidadTemblor);
            float offsetY = Random.Range(-intensidadTemblor, intensidadTemblor);
            transform.position = posicionOriginal + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = posicionOriginal;

        // Desactivación temporal
        if (col != null) col.enabled = false;
        if (sr != null) sr.enabled = false;

        // Espera de reaparición
        yield return new WaitForSeconds(tiempoParaReaparecer);

        if (col != null) col.enabled = true;
        if (sr != null) sr.enabled = true;
        estaCayendo = false;
    }
}
