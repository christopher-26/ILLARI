using System.Collections;
using UnityEngine;

/// <summary>
/// Mini-Jefe: Fangoso Titán del Mantaro (Nivel 4 - Parque de los Sombreros).
/// Monstruo colosal de lodo tóxico de 3 metros con dos fases de ataque:
/// 1. Golpe de puño contra el suelo.
/// 2. Disparo triple en abanico de bolas de barro.
/// Al ser derrotado/purificado, suelta 2 Botellas de Plástico finales.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BossTitanManager : MonoBehaviour
{
    [Header("Salud del Jefe")]
    [SerializeField] private int saludMax = 8;
    [SerializeField] private int saludActual = 8;
    [SerializeField] private GameObject prefabBotellaDrop;

    [Header("Ataques y Proyectiles")]
    [SerializeField] private GameObject prefabBalaBarro;
    [SerializeField] private float intervaloEntreAtaques = 2.8f;
    [SerializeField] private float velocidadProyectil = 10f;

    private Rigidbody2D rb;
    private Transform visualTrans;
    private Transform transformJugador;
    private float tiempoProximoAtaque = 0f;
    private bool estaMuerto = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        visualTrans = transform.Find("Visual");
        if (visualTrans == null) visualTrans = transform;
        rb.freezeRotation = true;
        saludActual = saludMax;
    }

    private void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) transformJugador = p.transform;
        tiempoProximoAtaque = Time.time + 1.5f;
    }

    private void Update()
    {
        if (estaMuerto || transformJugador == null) return;

        // Mirar hacia el jugador
        float dirX = transformJugador.position.x - transform.position.x;
        float signo = dirX >= 0 ? 1f : -1f;
        visualTrans.localScale = new Vector3(signo * Mathf.Abs(visualTrans.localScale.x), visualTrans.localScale.y, visualTrans.localScale.z);

        if (Time.time >= tiempoProximoAtaque)
        {
            tiempoProximoAtaque = Time.time + intervaloEntreAtaques;
            // Alternar entre golpe sísmico y abanico
            if (Random.value > 0.5f)
            {
                StartCoroutine(AtaqueAbanicoLodo(signo));
            }
            else
            {
                StartCoroutine(AtaqueGolpeSuelo());
            }
        }
    }

    private IEnumerator AtaqueAbanicoLodo(float direccionX)
    {
        // Ráfaga en abanico de 3 proyectiles
        Vector2[] direcciones = new Vector2[]
        {
            new Vector2(direccionX, 0.4f).normalized,
            new Vector2(direccionX, 0f).normalized,
            new Vector2(direccionX, -0.3f).normalized
        };

        foreach (var dir in direcciones)
        {
            if (prefabBalaBarro != null)
            {
                GameObject b = Instantiate(prefabBalaBarro, transform.position + new Vector3(direccionX * 1.2f, 0.5f, 0f), Quaternion.identity);
                Bala comp = b.GetComponent<Bala>();
                if (comp != null) comp.ConfigurarDireccion(dir);
                else
                {
                    Rigidbody2D rbB = b.GetComponent<Rigidbody2D>();
                    if (rbB != null) rbB.linearVelocity = dir * velocidadProyectil;
                }
            }
        }
        yield return null;
    }

    private IEnumerator AtaqueGolpeSuelo()
    {
        // Salto vertical y azote en el suelo
        rb.linearVelocity = new Vector2(0f, 7f);
        yield return new WaitForSeconds(0.4f);
        rb.linearVelocity = new Vector2(0f, -12f);

        yield return new WaitForSeconds(0.2f);
        // Daño de impacto en área cercana
        if (transformJugador != null && Vector2.Distance(transform.position, transformJugador.position) < 4.5f)
        {
            GameManagerIyari.Instancia?.RecibirDanio(1);
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
        SpriteRenderer sr = visualTrans.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.cyan;
            yield return new WaitForSeconds(0.12f);
            sr.color = Color.white;
        }
    }

    private void Morir()
    {
        estaMuerto = true;

        // Drop de 2 botellas de plástico finales
        if (prefabBotellaDrop != null)
        {
            Instantiate(prefabBotellaDrop, transform.position + Vector3.left * 0.8f, Quaternion.identity);
            Instantiate(prefabBotellaDrop, transform.position + Vector3.right * 0.8f, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
