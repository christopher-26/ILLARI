using UnityEngine;

/// <summary>
/// Animación arcade para las botellas de plástico recolectables en 'Illari'.
/// Proporciona un giro horizontal continuo (eje Y) y una suave oscilación
/// sinusoidal vertical para lograr una respuesta visual clásica de coleccionable.
/// </summary>
public class RotadorBotella : MonoBehaviour
{
    [Header("Giro")]
    [SerializeField] private float velocidadGiroY = 180f; // Grados por segundo

    [Header("Flotación")]
    [SerializeField] private float velocidadFlotacion = 3.5f;
    [SerializeField] private float amplitudFlotacion = 0.12f;

    private Vector3 posicionInicialRelativa;

    private void Start()
    {
        posicionInicialRelativa = transform.localPosition;
    }

    private void Update()
    {
        // Giro continuo horizontal sobre el eje Y
        transform.Rotate(0f, velocidadGiroY * Time.deltaTime, 0f, Space.Self);

        // Flotación suave sobre el eje Y local
        float offsetFlotacion = Mathf.Sin(Time.time * velocidadFlotacion) * amplitudFlotacion;
        transform.localPosition = new Vector3(
            posicionInicialRelativa.x,
            posicionInicialRelativa.y + offsetFlotacion,
            posicionInicialRelativa.z
        );
    }
}
