using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sistema de Audio y Banda Sonora centralizado y persistente para 'Iyari' (Huancayo 2026).
/// - Inicialización automática garantizada en toda escena con [RuntimeInitializeOnLoadMethod].
/// - Persiste entre cambios de escena con DontDestroyOnLoad.
/// - Control independiente de volúmenes: Master, Música (BGM) y Efectos de Sonido (SFX).
/// - Soporte para AudioClips grabados en Assets/Audio y generador procedural integrado como respaldo.
/// - Cambio dinámico y crossfade suave de pistas según el parque/nivel cargado.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instancia { get; private set; }

    [Header("Fuentes de Audio")]
    [SerializeField] private AudioSource audioSourceMusica;
    [SerializeField] private AudioSource audioSourceSFX;

    [Header("Banda Sonora por Niveles de Huancayo")]
    [SerializeField] private AudioClip musicaMenu;
    [SerializeField] private AudioClip musicaNivel1;
    [SerializeField] private AudioClip musicaNivelIdentidad;
    [SerializeField] private AudioClip musicaNivelCerrito;
    [SerializeField] private AudioClip musicaNivelBosqueDorado;
    [SerializeField] private AudioClip musicaNivelTorreTorre;
    [SerializeField] private AudioClip musicaNivelBoss;

    [Header("Efectos de Sonido Arcade")]
    [SerializeField] private AudioClip sfxDisparoAgua;
    [SerializeField] private AudioClip sfxSalto;
    [SerializeField] private AudioClip sfxRecogerBotella;
    [SerializeField] private AudioClip sfxDanio;
    [SerializeField] private AudioClip sfxBotonUI;
    [SerializeField] private AudioClip sfxPurificacion;

    // Volúmenes configurables
    private float volMaster = 1.0f;
    private float volMusica = 0.85f;
    private float volSFX = 0.90f;

    private Dictionary<string, AudioClip> clipsCargados = new Dictionary<string, AudioClip>();
    private string pistaActual = "";

    public float VolumenMaster => volMaster;
    public float VolumenMusica => volMusica;
    public float VolumenSFX => volSFX;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInicializarEnTodaEscena()
    {
        AsegurarInstancia();
    }

    public static AudioManager AsegurarInstancia()
    {
        if (Instancia != null) return Instancia;

        AudioManager amExistente = Object.FindAnyObjectByType<AudioManager>();
        if (amExistente != null)
        {
            Instancia = amExistente;
            return Instancia;
        }

        GameObject amObj = new GameObject("AudioManager");
        Instancia = amObj.AddComponent<AudioManager>();
        return Instancia;
    }

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        DontDestroyOnLoad(gameObject);

        InicializarAudioSources();
        CargarPreferenciasVolumen();
        CargarClipsDeAudio();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
    }

    private void Start()
    {
        AlCargarEscena(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void InicializarAudioSources()
    {
        if (audioSourceMusica == null)
        {
            GameObject musicaObj = new GameObject("AudioSource_Musica");
            musicaObj.transform.SetParent(transform);
            audioSourceMusica = musicaObj.AddComponent<AudioSource>();
            audioSourceMusica.loop = true;
            audioSourceMusica.playOnAwake = false;
        }

        if (audioSourceSFX == null)
        {
            GameObject sfxObj = new GameObject("AudioSource_SFX");
            sfxObj.transform.SetParent(transform);
            audioSourceSFX = sfxObj.AddComponent<AudioSource>();
            audioSourceSFX.loop = false;
            audioSourceSFX.playOnAwake = false;
        }
    }

    private void CargarClipsDeAudio()
    {
#if UNITY_EDITOR
        if (musicaMenu == null)
            musicaMenu = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/MenuPrincipal.mp3");
        if (musicaMenu == null)
            musicaMenu = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Musica_Menu.wav");

        if (musicaNivel1 == null)
            musicaNivel1 = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Nivel1.mp3");
        if (musicaNivel1 == null)
            musicaNivel1 = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Musica_NivelBosqueDorado.wav");

        if (musicaNivelIdentidad == null)
            musicaNivelIdentidad = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Nivel1.mp3");
        if (musicaNivelIdentidad == null)
            musicaNivelIdentidad = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Musica_NivelIdentidad.wav");

        if (musicaNivelCerrito == null)
            musicaNivelCerrito = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Musica_NivelCerrito.wav");

        if (musicaNivelBosqueDorado == null)
            musicaNivelBosqueDorado = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Nivel1.mp3");
        if (musicaNivelBosqueDorado == null)
            musicaNivelBosqueDorado = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Musica_NivelBosqueDorado.wav");

        if (musicaNivelTorreTorre == null)
            musicaNivelTorreTorre = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Musica_NivelTorreTorre.wav");
        if (musicaNivelBoss == null)
            musicaNivelBoss = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Musica_NivelBoss.wav");

        if (sfxDisparoAgua == null)
            sfxDisparoAgua = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/SFX_DisparoAgua.wav");
        if (sfxSalto == null)
            sfxSalto = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/SFX_SaltoArcade.wav");
        if (sfxRecogerBotella == null)
            sfxRecogerBotella = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/SFX_BotellaCristal.wav");
        if (sfxDanio == null)
            sfxDanio = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/SFX_DanioHit.wav");
        if (sfxBotonUI == null)
            sfxBotonUI = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/SFX_ClickUI.wav");
        if (sfxPurificacion == null)
            sfxPurificacion = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/SFX_VictoriaPurificacion.wav");
#endif
    }

    private void CargarPreferenciasVolumen()
    {
        volMaster = PlayerPrefs.GetFloat("Iyari_VolMaster", 1.0f);
        volMusica = PlayerPrefs.GetFloat("Iyari_VolMusica", 0.85f);
        volSFX = PlayerPrefs.GetFloat("Iyari_VolSFX", 0.90f);
        ActualizarVolumenesInternos();
    }

    private void ActualizarVolumenesInternos()
    {
        if (audioSourceMusica != null)
            audioSourceMusica.volume = volMaster * volMusica;

        if (audioSourceSFX != null)
            audioSourceSFX.volume = volMaster * volSFX;
    }

    public void SetMasterVolume(float nuevoVol)
    {
        volMaster = Mathf.Clamp01(nuevoVol);
        PlayerPrefs.SetFloat("Iyari_VolMaster", volMaster);
        ActualizarVolumenesInternos();
    }

    public void SetMusicVolume(float nuevoVol)
    {
        volMusica = Mathf.Clamp01(nuevoVol);
        PlayerPrefs.SetFloat("Iyari_VolMusica", volMusica);
        ActualizarVolumenesInternos();
    }

    public void SetSFXVolume(float nuevoVol)
    {
        volSFX = Mathf.Clamp01(nuevoVol);
        PlayerPrefs.SetFloat("Iyari_VolSFX", volSFX);
        ActualizarVolumenesInternos();
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        string nombre = escena.name;

        if (nombre.Contains("MenuPrincipal"))
        {
            AudioClip clipMenu = musicaMenu != null ? musicaMenu : ObtenerProcedural("Musica_Menu");
            ReproducirMusicaPista("Menu", clipMenu);
        }
        else if (nombre == "Nivel1" || nombre.Contains("Nivel1") || nombre.Contains("ParqueColonial"))
        {
            // Nivel 1: Bosque Dorado de Paccha
            AudioClip clipNivel1 = musicaNivel1 != null ? musicaNivel1 : (musicaNivelBosqueDorado != null ? musicaNivelBosqueDorado : musicaNivelIdentidad);
            ReproducirMusicaPista("Nivel1", clipNivel1 != null ? clipNivel1 : ObtenerProcedural("Musica_BosqueDorado"));
        }
        else if (nombre.Contains("Nivel9") || nombre.Contains("Huaytapallana"))
        {
            ReproducirMusicaPista("Boss", musicaNivelBoss != null ? musicaNivelBoss : ObtenerProcedural("Musica_Boss"));
        }
        else if (nombre.Contains("Nivel8") || nombre.Contains("TorreTorre"))
        {
            ReproducirMusicaPista("TorreTorre", musicaNivelTorreTorre != null ? musicaNivelTorreTorre : ObtenerProcedural("Musica_TorreTorre"));
        }
        else if (nombre.Contains("Nivel3") || nombre.Contains("Nivel6") || nombre.Contains("Nivel7") || nombre.Contains("Bosque") || nombre.Contains("Shullcas") || nombre.Contains("Tupac"))
        {
            ReproducirMusicaPista("BosqueDorado", musicaNivelBosqueDorado != null ? musicaNivelBosqueDorado : ObtenerProcedural("Musica_BosqueDorado"));
        }
        else if (nombre.Contains("Nivel2") || nombre.Contains("Nivel4") || nombre.Contains("Cerrito") || nombre.Contains("Sombreros"))
        {
            ReproducirMusicaPista("Cerrito", musicaNivelCerrito != null ? musicaNivelCerrito : ObtenerProcedural("Musica_Cerrito"));
        }
        else
        {
            // Nivel 5 (Plaza Constitución) u otras escenas
            ReproducirMusicaPista("Identidad", musicaNivelIdentidad != null ? musicaNivelIdentidad : ObtenerProcedural("Musica_Identidad"));
        }
    }

    public void ReproducirMusicaPista(string identificador, AudioClip clip)
    {
        if (clip == null) return;
        if (pistaActual == identificador && audioSourceMusica.isPlaying) return;

        pistaActual = identificador;
        StopAllCoroutines();
        StartCoroutine(RutinaCrossfadeMusica(clip));
    }

    private IEnumerator RutinaCrossfadeMusica(AudioClip nuevoClip)
    {
        float duracionFade = 0.35f;
        float volObjetivo = volMaster * volMusica;

        if (audioSourceMusica.isPlaying)
        {
            float tiempo = 0f;
            float volInicial = audioSourceMusica.volume;
            while (tiempo < duracionFade)
            {
                tiempo += Time.unscaledDeltaTime;
                audioSourceMusica.volume = Mathf.Lerp(volInicial, 0f, tiempo / duracionFade);
                yield return null;
            }
        }

        audioSourceMusica.clip = nuevoClip;
        audioSourceMusica.Play();

        float tSubida = 0f;
        while (tSubida < duracionFade)
        {
            tSubida += Time.unscaledDeltaTime;
            audioSourceMusica.volume = Mathf.Lerp(0f, volObjetivo, tSubida / duracionFade);
            yield return null;
        }

        audioSourceMusica.volume = volObjetivo;
    }

    public void ReproducirSFX(string tipoSFX)
    {
        AudioClip clip = null;
        float pitch = 1.0f;

        switch (tipoSFX.ToLower())
        {
            case "disparo":
                clip = sfxDisparoAgua != null ? sfxDisparoAgua : ObtenerProcedural("SFX_Disparo");
                pitch = Random.Range(0.95f, 1.08f);
                break;
            case "salto":
                clip = sfxSalto != null ? sfxSalto : ObtenerProcedural("SFX_Salto");
                pitch = Random.Range(0.98f, 1.05f);
                break;
            case "botella":
                clip = sfxRecogerBotella != null ? sfxRecogerBotella : ObtenerProcedural("SFX_Botella");
                pitch = Random.Range(1.0f, 1.15f);
                break;
            case "danio":
                clip = sfxDanio != null ? sfxDanio : ObtenerProcedural("SFX_Danio");
                pitch = 0.9f;
                break;
            case "click":
            case "boton":
                clip = sfxBotonUI != null ? sfxBotonUI : ObtenerProcedural("SFX_Click");
                pitch = 1.1f;
                break;
            case "purificacion":
            case "victoria":
                clip = sfxPurificacion != null ? sfxPurificacion : ObtenerProcedural("SFX_Purificacion");
                pitch = 1.0f;
                break;
        }

        if (clip != null && audioSourceSFX != null)
        {
            audioSourceSFX.pitch = pitch;
            audioSourceSFX.PlayOneShot(clip, volMaster * volSFX);
        }
    }

    // =========================================================================
    // SÍNTESIS PROCEDURAL ANDINA ARCADE (Fallback garantizado en tiempo real)
    // =========================================================================
    private AudioClip ObtenerProcedural(string clave)
    {
        if (clipsCargados.TryGetValue(clave, out AudioClip c) && c != null)
            return c;

        AudioClip nuevo = null;
        switch (clave)
        {
            case "Musica_Menu":
                nuevo = CrearMusicaAndinaArcade(115f, new int[] { 60, 64, 67, 69, 72, 69, 67, 64 }, 12.0f);
                break;
            case "Musica_Identidad":
                nuevo = CrearMusicaAndinaArcade(128f, new int[] { 60, 64, 67, 64, 69, 67, 64, 60 }, 12.0f);
                break;
            case "Musica_Cerrito":
                nuevo = CrearMusicaAndinaArcade(132f, new int[] { 62, 65, 69, 65, 72, 69, 65, 62 }, 12.0f);
                break;
            case "Musica_BosqueDorado":
                nuevo = CrearMusicaAndinaArcade(130f, new int[] { 57, 60, 64, 67, 69, 67, 64, 60 }, 12.0f);
                break;
            case "Musica_TorreTorre":
                nuevo = CrearMusicaAndinaArcade(138f, new int[] { 55, 58, 62, 65, 67, 65, 62, 58 }, 12.0f);
                break;
            case "Musica_Boss":
                nuevo = CrearMusicaAndinaArcade(148f, new int[] { 55, 58, 60, 62, 65, 67, 65, 62 }, 10.0f);
                break;
            case "SFX_Disparo":
                nuevo = CrearSFXDisparo();
                break;
            case "SFX_Salto":
                nuevo = CrearSFXSalto();
                break;
            case "SFX_Botella":
                nuevo = CrearSFXBotella();
                break;
            case "SFX_Danio":
                nuevo = CrearSFXDanio();
                break;
            case "SFX_Click":
                nuevo = CrearSFXClick();
                break;
            case "SFX_Purificacion":
                nuevo = CrearSFXPurificacion();
                break;
        }

        if (nuevo != null)
            clipsCargados[clave] = nuevo;

        return nuevo;
    }

    private AudioClip CrearMusicaAndinaArcade(float bpm, int[] escalaMidi, float duracionSegundos)
    {
        int sampleRate = 22050;
        int totalSamples = (int)(sampleRate * duracionSegundos);
        float[] samples = new float[totalSamples];

        float pasoNota = 60f / bpm * 0.5f; // corcheas melodiosas
        int samplesPorPaso = (int)(sampleRate * pasoNota);

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / sampleRate;
            int pasoActual = i / Mathf.Max(1, samplesPorPaso);
            float tEnNota = (float)(i % samplesPorPaso) / samplesPorPaso;

            int notaMidi = escalaMidi[pasoActual % escalaMidi.Length];
            float freq = 440f * Mathf.Pow(2f, (notaMidi - 69) / 12f);

            // Envolvente de nota suave (evita clics)
            float envNota = Mathf.Sin(tEnNota * Mathf.PI);

            // Flauta dulce andina (quena) con vibrato suave
            float vibrato = Mathf.Sin(t * 33f) * 2.5f;
            float phase = t * (freq + vibrato);
            float quenaWave = (Mathf.Sin(phase * 2f * Mathf.PI) * 0.8f + Mathf.Sin(phase * 4f * Mathf.PI) * 0.2f) * envNota * 0.35f;

            // Arpa / charango de fondo con armónicos cálidos
            float tCharango = (t * 2f) % 1f;
            float envCharango = Mathf.Exp(-tCharango * 3.5f);
            float charango = Mathf.Sin(t * freq * 0.5f * 2f * Mathf.PI) * envCharango * 0.20f;

            // Bombo andino sordo y cálido
            float tEnCompas = t % (60f / bpm);
            float bombo = Mathf.Exp(-tEnCompas * 16f) * Mathf.Sin(tEnCompas * 50f * 2f * Mathf.PI) * 0.30f;

            samples[i] = Mathf.Clamp(quenaWave + charango + bombo, -0.95f, 0.95f);
        }

        AudioClip clip = AudioClip.Create("MusicaAndina_" + bpm, totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CrearSFXDisparo()
    {
        int sampleRate = 22050;
        int count = (int)(sampleRate * 0.16f);
        float[] s = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            float freq = Mathf.Lerp(800f, 250f, t);
            s[i] = Mathf.Sin(i * freq * 2f * Mathf.PI / sampleRate) * (1f - t) * 0.35f;
        }
        AudioClip c = AudioClip.Create("SFX_DisparoAgua", count, 1, sampleRate, false);
        c.SetData(s, 0);
        return c;
    }

    private AudioClip CrearSFXSalto()
    {
        int sampleRate = 22050;
        int count = (int)(sampleRate * 0.18f);
        float[] s = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            float freq = Mathf.Lerp(220f, 650f, t);
            s[i] = Mathf.Sin(i * freq * 2f * Mathf.PI / sampleRate) * (1f - t) * 0.32f;
        }
        AudioClip c = AudioClip.Create("SFX_SaltoArcade", count, 1, sampleRate, false);
        c.SetData(s, 0);
        return c;
    }

    private AudioClip CrearSFXBotella()
    {
        int sampleRate = 22050;
        int count = (int)(sampleRate * 0.24f);
        float[] s = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            float freq = (t < 0.45f) ? 1046.5f : 1318.5f;
            s[i] = Mathf.Sin(i * freq * 2f * Mathf.PI / sampleRate) * (1f - t) * 0.38f;
        }
        AudioClip c = AudioClip.Create("SFX_BotellaCristal", count, 1, sampleRate, false);
        c.SetData(s, 0);
        return c;
    }

    private AudioClip CrearSFXDanio()
    {
        int sampleRate = 22050;
        int count = (int)(sampleRate * 0.22f);
        float[] s = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            float noise = Random.Range(-0.4f, 0.4f);
            float lowFreq = Mathf.Sin(i * 120f * 2f * Mathf.PI / sampleRate) * 0.5f;
            s[i] = (noise + lowFreq) * (1f - t) * 0.45f;
        }
        AudioClip c = AudioClip.Create("SFX_DanioHit", count, 1, sampleRate, false);
        c.SetData(s, 0);
        return c;
    }

    private AudioClip CrearSFXClick()
    {
        int sampleRate = 22050;
        int count = (int)(sampleRate * 0.06f);
        float[] s = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            s[i] = Mathf.Sin(i * 1400f * 2f * Mathf.PI / sampleRate) * (1f - t) * 0.25f;
        }
        AudioClip c = AudioClip.Create("SFX_ClickUI", count, 1, sampleRate, false);
        c.SetData(s, 0);
        return c;
    }

    private AudioClip CrearSFXPurificacion()
    {
        int sampleRate = 22050;
        int count = (int)(sampleRate * 0.6f);
        float[] s = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            float freq = Mathf.Lerp(440f, 1760f, t);
            s[i] = (Mathf.Sin(i * freq * 2f * Mathf.PI / sampleRate) + Mathf.Sin(i * (freq * 1.5f) * 2f * Mathf.PI / sampleRate) * 0.5f) * (1f - t) * 0.35f;
        }
        AudioClip c = AudioClip.Create("SFX_PurificacionEco", count, 1, sampleRate, false);
        c.SetData(s, 0);
        return c;
    }
}
