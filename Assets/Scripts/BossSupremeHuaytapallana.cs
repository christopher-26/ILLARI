using System.Collections;
using UnityEngine;

/// <summary>
/// Jefe Final Supremo: Gran Coloso del Huaytapallana (Nivel 9).
/// Máxima amenaza ecológica de la cordillera Junín 2026.
/// Posee 20 puntos de salud, azotes sísmicos glaciales y ráfagas espirales de lodo tóxico.
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

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform jugador;
    private float tiempoSiguienteAtaque = 0f;
    private bool estaMuerto = false;
    private int cicloAtaque = 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null) sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        saludActual = saludMaxima;
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) jugador = p.transform;
        tiempoSiguienteAtaque = Time.time + 1.5f;
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
            // Orientar hacia el jugador
            float dirX = (jugador.position.x > transform.position.x) ? 1f : -1f;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * dirX, transform.localScale.y, transform.localScale.z);

            if (Time.time >= tiempoSiguienteAtaque)
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
                GameObject b = Instantiate(prefabBalaBarro, transform.position + new Vector3(dirX * 1.5f, 0.8f, 0f), Quaternion.identity);
                Bala comp = b.GetComponent<Bala>();
                if (comp != null) comp.ConfigurarDireccion(dir);
            }
            yield return new WaitForSeconds(0.12f);
        }
    }

    private IEnumerator AtaqueSaltoSismicoGlacial()
    {
        // Salto colosal en Unity 6
        rb.linearVelocity = new Vector2(0f, 9.5f);
        yield return new WaitForSeconds(0.5f);
        rb.linearVelocity = new Vector2(0f, -16f);

        yield return new WaitForSeconds(0.25f);
        // Onda expansiva de impacto
        if (jugador != null && Vector2.Distance(transform.position, jugador.position) < 6.5f)
        {
            GameManagerIyari.Instancia?.RecibirDanio(1);
        }
    }

    private IEnumerator AtaqueLluviaLodo()
    {
        // Disparos verticales que caen como lluvia sobre Illari
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
    }

    public void RecibirDanio(int cantidad = 1)
    {
        if (estaMuerto) return;

        saludActual -= cantidad;
        StartCoroutine(FlashDanio());

        if (saludActual <= 0)
        {
            Morir();
        }
    }

    private IEnumerator FlashDanio()
    {
        if (sr != null)
        {
            Color orig = sr.color;
            sr.color = new Color(0.3f, 1f, 1f); // Resplandor cian glacial al ser purificado
            yield return new WaitForSeconds(0.08f);
            sr.color = orig;
        }
    }

    private void Morir()
    {
        estaMuerto = true;

        // Suelta exactamente 3 botellas para completar las 15 del nivel final
        if (prefabBotellaDrop != null)
        {
            Instantiate(prefabBotellaDrop, transform.position + new Vector3(-1.2f, 0.8f, 0f), Quaternion.identity);
            Instantiate(prefabBotellaDrop, transform.position + new Vector3(0f, 1.2f, 0f), Quaternion.identity);
            Instantiate(prefabBotellaDrop, transform.position + new Vector3(1.2f, 0.8f, 0f), Quaternion.identity);
        }

        Destroy(gameObject, 0.2f);
    }
}
