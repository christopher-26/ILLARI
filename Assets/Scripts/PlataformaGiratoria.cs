using UnityEngine;

/// <summary>
/// Plataforma rotatoria temática (Sombreros Monumentales):
/// Rota continuamente sobre su eje Z.
/// </summary>
public class PlataformaGiratoria : MonoBehaviour
{
    [Header("Velocidad de Giro")]
    [SerializeField] private float velocidadRotacion = 25f; // Grados por segundo

    private void Update()
    {
        transform.Rotate(0f, 0f, velocidadRotacion * Time.deltaTime);
    }
}
