using System.Collections;
using UnityEngine;

/// <summary>
/// Efecto visual animado de salpicadura de agua ('Splash') al impactar el proyectil de Illari.
/// Cicla a través de frames de agua pura cristalina y se auto-destruye suavemente.
/// </summary>
public class EfectoSplashAgua : MonoBehaviour
{
    private Sprite[] frames;
    private float duracion = 0.22f;

    public void Configurar(Sprite[] spritesSplash, float tiempoTotal = 0.22f)
    {
        frames = spritesSplash;
        duracion = tiempoTotal;
        StartCoroutine(RutinaAnimacion());
    }

    private IEnumerator RutinaAnimacion()
    {
        SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 15;
        sr.color = Color.white;

        if (frames != null && frames.Length > 0)
        {
            float delay = duracion / frames.Length;
            for (int i = 0; i < frames.Length; i++)
            {
                sr.sprite = frames[i];
                yield return new WaitForSeconds(delay);
            }
        }

        Destroy(gameObject);
    }
}
