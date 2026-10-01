using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Efecto visual táctil arcade para botones:
/// - Escala suave (1.05x) al pasar el cursor (PointerEnter/PointerExit).
/// - Pulso (0.95x) al presionar (PointerDown/PointerUp).
/// - Sonido o feedback visual de activación.
/// </summary>
public class BotonArcadeEfecto : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Vector3 escalaOriginal = Vector3.one;
    private Coroutine corrutinaAnim;

    private void Awake()
    {
        escalaOriginal = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var btn = GetComponent<Button>();
        if (btn != null && !btn.interactable) return;
        AnimarEscala(escalaOriginal * 1.05f, 0.12f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AnimarEscala(escalaOriginal, 0.12f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        var btn = GetComponent<Button>();
        if (btn != null && !btn.interactable) return;
        AnimarEscala(escalaOriginal * 0.95f, 0.08f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        AnimarEscala(escalaOriginal * 1.05f, 0.08f);
    }

    private void AnimarEscala(Vector3 target, float duracion)
    {
        if (!gameObject.activeInHierarchy) return;
        if (corrutinaAnim != null) StopCoroutine(corrutinaAnim);
        corrutinaAnim = StartCoroutine(RutinaEscala(target, duracion));
    }

    private IEnumerator RutinaEscala(Vector3 target, float duracion)
    {
        Vector3 start = transform.localScale;
        float t = 0f;
        while (t < duracion)
        {
            t += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(start, target, t / duracion);
            yield return null;
        }
        transform.localScale = target;
    }

    private void OnDisable()
    {
        transform.localScale = escalaOriginal;
    }
}
