using UnityEngine;

/// <summary>
/// Sistema de seguimiento de cámara 2D suave para 'Illari'.
/// Sigue al jugador horizontal y verticalmente respetando los límites de cada parque de Huancayo 2026.
/// Se auto-vincula al jugador mediante el tag 'Player' si la referencia inicial es nula.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target;
    public Vector3 offset = new Vector3(2f, 1.5f, -10f);
    public float smoothSpeed = 0.125f;

    [Header("Limites del Nivel")]
    public bool usarLimites = true;
    public float limiteIzquierdo = -6f;
    public float limiteDerecho = 135f;
    public float limiteAbajo = -4f;
    public float limiteArriba = 8f;

    private void Awake()
    {
        BuscarJugador();
    }

    private void Start()
    {
        BuscarJugador();
        CentrarInmediato();
    }

    public void BuscarJugador()
    {
        if (target == null)
        {
            GameObject jugador = GameObject.FindWithTag("Player");
            if (jugador != null)
            {
                target = jugador.transform;
            }
        }
    }

    public void CentrarInmediato()
    {
        if (target != null)
        {
            Vector3 posDeseada = target.position + offset;
            float x = usarLimites ? Mathf.Clamp(posDeseada.x, limiteIzquierdo, limiteDerecho) : posDeseada.x;
            float y = usarLimites ? Mathf.Clamp(posDeseada.y, limiteAbajo, limiteArriba) : posDeseada.y;
            transform.position = new Vector3(x, y, offset.z);
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            BuscarJugador();
            if (target == null) return;
        }

        Vector3 posDeseada = target.position + offset;

        float x = usarLimites ? Mathf.Clamp(posDeseada.x, limiteIzquierdo, limiteDerecho) : posDeseada.x;
        float y = usarLimites ? Mathf.Clamp(posDeseada.y, limiteAbajo, limiteArriba) : posDeseada.y;

        Vector3 posLimitada = new Vector3(x, y, offset.z);
        transform.position = Vector3.Lerp(transform.position, posLimitada, smoothSpeed);
    }

    public void ConfigurarLimites(float izq, float der, float abajo, float arriba)
    {
        limiteIzquierdo = izq;
        limiteDerecho = der;
        limiteAbajo = abajo;
        limiteArriba = arriba;
        usarLimites = true;
    }
}
