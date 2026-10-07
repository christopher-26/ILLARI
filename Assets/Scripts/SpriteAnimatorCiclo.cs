using UnityEngine;

/// <summary>
/// Animador por ciclos de sprites para la vegetación viva del Bosque Dorado de Paccha (ODS 15).
/// Reproduce las secuencias animadas de Queñuales, Flor de Cantuta y Pasto Ichu.
/// </summary>
public class SpriteAnimatorCiclo : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float fps = 3.5f;
    [SerializeField] private bool pingPong = true;

    private SpriteRenderer sr;
    private float tiempoAcumulado;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        tiempoAcumulado = Random.Range(0f, 3f);
    }

    public void Configurar(Sprite[] nuevosFrames, float nuevaVelocidad = 3.5f, bool usarPingPong = true)
    {
        frames = nuevosFrames;
        fps = nuevaVelocidad;
        pingPong = usarPingPong;
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (frames != null && frames.Length > 0 && sr != null)
        {
            sr.sprite = frames[0];
        }
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0 || sr == null) return;

        tiempoAcumulado += Time.deltaTime * fps;
        int total = frames.Length;

        if (pingPong && total > 2)
        {
            int ciclo = (total - 1) * 2;
            int idx = Mathf.FloorToInt(tiempoAcumulado) % ciclo;
            if (idx >= total)
            {
                idx = ciclo - idx;
            }
            sr.sprite = frames[idx];
        }
        else
        {
            int idx = Mathf.FloorToInt(tiempoAcumulado) % total;
            sr.sprite = frames[idx];
        }
    }
}
