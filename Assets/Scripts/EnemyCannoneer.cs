using System.Collections;
using UnityEngine;

/// <summary>
/// Fangoso Cañonero: Enemigo pesado con mortero de lodo montado en la espalda.
/// Dispara proyectiles en arco parabólico hacia la posición de Illari.
/// Al ser purificado, suelta obligatoriamente 1 Botella de Plástico (ODS 15).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class EnemyCannoneer : MonoBehaviour
{
    [Header("Parámetros de Salud")]
    [SerializeField] private int salud = 4;

    [Header("Ataque")]
    [SerializeField] private GameObject prefabBalaBarro;
    [SerializeField] private GameObject prefabBotellaDrop;
    [SerializeField] private float cadenciaDisparo = 2.8f;
    [SerializeField] private float rangoDeteccion = 14f;
    [SerializeField] private float velocidadMortero = 10f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform jugador;
    private float tiempoSiguienteDisparo = 0f;
    private bool estaMuerto = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null) sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) jugador = p.transform;
        tiempoSiguienteDisparo = Time.time + 1.2f;
    }

    private void Update()
    {
        if (estaMuerto) return;

        if (jugador == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) jugador = p.transform;
            return;
        }

        float distancia = Vector2.Distance(transform.position, jugador.position);
        if (distancia <= rangoDeteccion)
        {
            // Orientar cañón hacia el jugador
            float dirX = (jugador.position.x > transform.position.x) ? 1f : -1f;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * dirX, transform.localScale.y, transform.localScale.z);

            if (Time.time >= tiempoSiguienteDisparo)
            {
                tiempoSiguienteDisparo = Time.time + cadenciaDisparo;
                DispararMorteroLodo();
            }
        }
    }

    private void DispararMorteroLodo()
    {
        if (prefabBalaBarro == null || jugador == null) return;

        Vector3 spawnPos = transform.position + new Vector3(transform.localScale.x > 0 ? 0.8f : -0.8f, 1.0f, 0f);
        GameObject bala = Instantiate(prefabBalaBarro, spawnPos, Quaternion.identity);

        Rigidbody2D rbBala = bala.GetComponent<Rigidbody2D>();
        if (rbBala != null)
        {
            rbBala.gravityScale = 1.4f; // Arco parabólico en Unity 6
            float dirX = transform.localScale.x > 0 ? 1f : -1f;
            float distanciaX = Mathf.Abs(jugador.position.x - transform.position.x);
            float velX = dirX * Mathf.Clamp(distanciaX * 0.9f, 4f, velocidadMortero);
            float velY = 8.5f;

            rbBala.linearVelocity = new Vector2(velX, velY);
        }

        Bala bComp = bala.GetComponent<Bala>();
        if (bComp != null)
        {
            // Configurar como bala enemiga
            bComp.ConfigurarDireccion(Vector2.up);
        }
    }

    public void RecibirDanio(int cantidad = 1)
    {
        if (estaMuerto) return;

        salud -= cantidad;
        StartCoroutine(FlashDanio());

        if (salud <= 0)
        {
            Morir();
        }
    }

    private IEnumerator FlashDanio()
    {
        if (sr != null)
        {
            Color orig = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.08f);
            sr.color = orig;
        }
    }

    private void Morir()
    {
        estaMuerto = true;

        if (prefabBotellaDrop != null)
        {
            Instantiate(prefabBotellaDrop, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }

        Destroy(gameObject, 0.1f);
    }
}
