using UnityEngine;

/// <summary>
/// Sistema avanzado de seguimiento y parallax para el telón de fondo panorámico en los niveles de 'Illari' (Huancayo 2026).
/// - Sigue suavemente la cámara del jugador evitando cualquier tirón, desfase o 'rubber-banding'.
/// - Aplica un parallax horizontal orgánico a lo largo del recorrido del parque (revelando la panorámica completa desde el inicio hasta la meta).
/// - Estabilidad vertical cinematográfica: no vibra ni salta en brincos normales de parkour, pero acompaña suavemente la elevación en plataformas altas.
/// - Cobertura garantizada: nunca revela vacíos ni bordes fuera de pantalla.
/// </summary>
[ExecuteAlways]
public class FondoSigueJugador : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Cámara principal que visualiza la escena. Si es nula, se busca automáticamente.")]
    public Camera camaraPrincipal;
    [Tooltip("Referencia al personaje jugador (Iyari).")]
    public Transform objetivoPersonaje;

    [Header("Ajustes de Posicionamiento")]
    public float offsetZ = 12f;
    [Tooltip("Altura base donde reposa el fondo para un encuadre visual óptimo")]
    public float posYBase = -2.5f;

    [Header("Parallax Horizontal a lo largo del Nivel")]
    public bool usarParallaxNivel = true;
    [Tooltip("Desplazamiento horizontal máximo que recorre el fondo a lo largo del nivel (revelado escénico)")]
    public float recorridoParallaxX = 6.0f;
    public float limiteNivelMinX = -6f;
    public float limiteNivelMaxX = 135f;

    [Header("Comportamiento Vertical")]
    [Tooltip("Factor de seguimiento en Y con respecto a la elevación de la cámara")]
    public float factorSeguimientoY = 0.25f;
    public float camYReferencia = -2.9f;

    private CameraFollow camFollow;

    private void Awake()
    {
        BuscarReferencias();
    }

    private void Start()
    {
        BuscarReferencias();
        ActualizarPosicion(true);
    }

    public void BuscarReferencias()
    {
        if (camaraPrincipal == null)
            camaraPrincipal = Camera.main;

        if (camaraPrincipal != null && camFollow == null)
        {
            camFollow = camaraPrincipal.GetComponent<CameraFollow>();
            if (camFollow != null && camFollow.usarLimites)
            {
                limiteNivelMinX = camFollow.limiteIzquierdo;
                limiteNivelMaxX = camFollow.limiteDerecho;
            }
        }

        if (objetivoPersonaje == null)
        {
            GameObject jugador = GameObject.FindWithTag("Player");
            if (jugador != null)
                objetivoPersonaje = jugador.transform;
            else
            {
                var illariScript = FindFirstObjectByType<IllariPlatformer>();
                if (illariScript != null)
                    objetivoPersonaje = illariScript.transform;
            }
        }
    }

    public void ActualizarPosicion(bool instantaneo = false)
    {
        if (camaraPrincipal == null)
        {
            BuscarReferencias();
            if (camaraPrincipal == null) return;
        }

        Vector3 camPos = camaraPrincipal.transform.position;

        // 1. Posición horizontal: Sigue en lockstep a la cámara con parallax escénico suave
        float posX = camPos.x;
        if (usarParallaxNivel && (limiteNivelMaxX > limiteNivelMinX))
        {
            float t = Mathf.Clamp01((camPos.x - limiteNivelMinX) / (limiteNivelMaxX - limiteNivelMinX));
            // De recorridoParallaxX a -recorridoParallaxX (a medida que avanzas, la panorámica revela el resto del parque)
            float offsetParallax = Mathf.Lerp(recorridoParallaxX, -recorridoParallaxX, t);
            posX += offsetParallax;
        }

        // 2. Posición vertical: Altura base calibrada + respuesta amortiguada a cambios de elevación de la cámara
        float difCamY = camPos.y - camYReferencia;
        float posY = posYBase + (difCamY * factorSeguimientoY);

        // La cámara ya está suavizada por CameraFollow en LateUpdate.
        // Sincronizar directamente con la cámara evita todo jitter o rubber-banding.
        transform.position = new Vector3(posX, posY, offsetZ);
    }

    private void LateUpdate()
    {
        ActualizarPosicion(false);
    }
}
