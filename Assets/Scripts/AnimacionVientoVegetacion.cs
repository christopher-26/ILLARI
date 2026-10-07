using UnityEngine;

/// <summary>
/// Animación orgánica de viento andino para los árboles y arbustos del Bosque Dorado de Paccha (ODS 15).
/// Simula la brisa del Valle del Mantaro mediante:
/// 1. Balanceo angular suave en el eje Z (oscilación armónica doble no repetitiva).
/// 2. Respiración elástica del follaje (escala Y).
/// 3. Desfase individual por coordenada X para que el viento viaje a través del bosque.
/// </summary>
public class AnimacionVientoVegetacion : MonoBehaviour
{
    [Header("Configuración de Viento")]
    [Tooltip("Velocidad del ciclo de viento")]
    [SerializeField] private float velocidadViento = 1.6f;

    [Tooltip("Ángulo máximo de balanceo en grados")]
    [SerializeField] private float anguloBalanceo = 3.2f;

    [Tooltip("Amplitud de la respiración elástica vertical del follaje")]
    [SerializeField] private float amplitudRespiracion = 0.025f;

    [Tooltip("Desfase de tiempo de la onda de viento")]
    [SerializeField] private float desfaseViento = 0f;

    private Vector3 escalaOriginal;
    private Quaternion rotacionOriginal;
    private float frecuenciaPrincipal;
    private float frecuenciaSecundaria;

    private void Awake()
    {
        escalaOriginal = transform.localScale;
        rotacionOriginal = transform.localRotation;

        // Desfase orgánico basado en la posición X en el mundo
        desfaseViento = (transform.position.x * 0.35f) + Random.Range(0f, 1.5f);
        frecuenciaPrincipal = velocidadViento * Random.Range(0.9f, 1.15f);
        frecuenciaSecundaria = frecuenciaPrincipal * 2.15f;
    }

    private void Update()
    {
        float t = Time.time;
        float onda1 = Mathf.Sin(t * frecuenciaPrincipal + desfaseViento);
        float onda2 = Mathf.Sin(t * frecuenciaSecundaria + desfaseViento * 1.4f);

        // Balanceo angular suave
        float sway = (onda1 * anguloBalanceo) + (onda2 * (anguloBalanceo * 0.3f));
        transform.localRotation = rotacionOriginal * Quaternion.Euler(0f, 0f, sway);

        // Respiración elástica del follaje en Y
        float factorY = 1f + (onda1 * amplitudRespiracion);
        transform.localScale = new Vector3(escalaOriginal.x, escalaOriginal.y * factorY, escalaOriginal.z);
    }
}
