using System.Collections;
using UnityEngine;

/// <summary>
/// Enemigo: Fangoso Blindado (Chatarrero).
/// Porta chatarra y llantas recicladas en su frente que bloquean los disparos directos.
/// El jugador debe dispararle por la espalda o impactar 3 veces para destruir su blindaje.
/// Al ser purificado, suelta una Botella de Plástico coleccionable.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyArmored : MonoBehaviour
{
    [Header("Salud y Blindaje")]
    [SerializeField] private int salud = 3;
    [SerializeField] private bool tieneBlindajeFrontal = true;
    [SerializeField] private GameObject prefabBotellaDrop;

    [Header("Movimiento")]
    [SerializeField] private float velocidadPatrulla = 1.2f;
    [SerializeField] private LayerMask capaSuelo;
    [SerializeField] private string tagSuelo = "Ground";

    private Rigidbody2D rb;
    private Collider2D col;
    private Transform visualTrans;
    private float direccionX = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        visualTrans = transform.Find("Visual");
        if (visualTrans == null) visualTrans = transform;
        rb.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        PatrullaPesada();
    }

    private void PatrullaPesada()
    {
        float radioX = (col != null) ? col.bounds.extents.x : 0.6f;
        float radioY = (col != null) ? col.bounds.extents.y : 0.7f;

        Vector2 origenFrente = (Vector2)transform.position + new Vector2(direccionX * (radioX + 0.15f), 0f);
        RaycastHit2D hitMuro = Physics2D.Raycast(origenFrente, Vector2.right * direccionX, 0.5f, capaSuelo);
        bool hayMuro = hitMuro.collider != null && hitMuro.collider.CompareTag(tagSuelo);

        Vector2 origenVacio = (Vector2)transform.position + new Vector2(direccionX * (radioX + 0.25f), -radioY + 0.25f);
        RaycastHit2D hitSuelo = Physics2D.Raycast(origenVacio, Vector2.down, 1.2f, capaSuelo);
        bool hayVacio = hitSuelo.collider == null || !hitSuelo.collider.CompareTag(tagSuelo);

        if (hayMuro || hayVacio)
        {
            direccionX *= -1f;
            visualTrans.localScale = new Vector3(direccionX * Mathf.Abs(visualTrans.localScale.x), visualTrans.localScale.y, visualTrans.localScale.z);
        }

        rb.linearVelocity = new Vector2(direccionX * velocidadPatrulla, rb.linearVelocity.y);
    }

    public void RecibirDanio(int cantidad = 1, Vector2 direccionImpacto = default)
    {
        // Si el impacto viene de frente y aún tiene blindaje
        bool impactoFrontal = (direccionImpacto.x * direccionX) < 0f;

        if (tieneBlindajeFrontal && impactoFrontal)
        {
            // Rompe el blindaje tras recibir suficiente impacto
            salud -= 1;
            StartCoroutine(FlashBlindaje());
            if (salud <= 2)
            {
                tieneBlindajeFrontal = false;
            }
            return;
        }

        salud -= cantidad;
        StartCoroutine(FlashDanio());

        if (salud <= 0)
        {
            Morir();
        }
    }

    private IEnumerator FlashBlindaje()
    {
        SpriteRenderer sr = visualTrans.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.yellow;
            yield return new WaitForSeconds(0.1f);
            sr.color = Color.white;
        }
    }

    private IEnumerator FlashDanio()
    {
        SpriteRenderer sr = visualTrans.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.cyan;
            yield return new WaitForSeconds(0.1f);
            sr.color = Color.white;
        }
    }

    private void Morir()
    {
        if (prefabBotellaDrop != null)
        {
            Instantiate(prefabBotellaDrop, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
