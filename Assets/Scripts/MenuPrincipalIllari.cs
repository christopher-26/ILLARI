using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador Arcade del Menú Principal de 'Iyari' (Huancayo 2026).
/// Gestiona de forma limpia y receptiva:
/// 1. "Iniciar Aventura": Resetea progreso y carga el Nivel 1 (Parque de la Identidad Huanca).
/// 2. "Selector de Misiones / Parques": Modal con acceso a los 9 niveles de Huancayo.
/// 3. "Registro ODS 12 & 15": Modal educativo con datos reales de la crisis ambiental en Huancayo.
/// 4. "Ajustes de Sonido": Sliders de Volumen Master, Música (BGM) y SFX conectados a AudioManager.
/// 5. "Guía de Controles": Overlay visual con la distribución de teclas arcade.
/// 6. "Salir": Cierra el juego de forma segura.
/// </summary>
public class MenuPrincipalIllari : MonoBehaviour
{
    public static MenuPrincipalIllari Instancia { get; private set; }

    [Header("Paneles Principales")]
    [SerializeField] private GameObject panelInicio;
    [SerializeField] private GameObject panelSeleccion;
    [SerializeField] private GameObject panelODS;
    [SerializeField] private GameObject panelSonido;
    [SerializeField] private GameObject panelControles;

    [Header("Sliders de Audio")]
    [SerializeField] private Slider sliderMaster;
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private Slider sliderSFX;

    [Header("Textos de Porcentaje de Audio")]
    [SerializeField] private Text txtValorMaster;
    [SerializeField] private Text txtValorMusica;
    [SerializeField] private Text txtValorSFX;

    [Header("Animación de Logo")]
    [SerializeField] private RectTransform rectLogo;
    private Vector2 posOriginalLogo;

    private void Awake()
    {
        Instancia = this;
        AsegurarAudioManager();
    }

    private void AsegurarAudioManager()
    {
        if (AudioManager.Instancia == null)
        {
            AudioManager amExistente = Object.FindAnyObjectByType<AudioManager>();
            if (amExistente == null)
            {
                GameObject amObj = new GameObject("AudioManager");
                amObj.AddComponent<AudioManager>();
            }
        }
    }

    private void Start()
    {
        // Asegurar que el tiempo de juego esté activo
        Time.timeScale = 1.0f;

        if (rectLogo != null)
            posOriginalLogo = rectLogo.anchoredPosition;

        // Mostrar solo el panel de inicio
        MostrarSoloInicio();

        // Inicializar sliders de audio con valores de AudioManager
        InicializarSlidersAudio();

        // Animar logo flotante
        StartCoroutine(RutinaFlotarLogo());
    }

    private void InicializarSlidersAudio()
    {
        if (AudioManager.Instancia != null)
        {
            if (sliderMaster != null)
            {
                sliderMaster.value = AudioManager.Instancia.VolumenMaster;
                ActualizarTextoMaster(sliderMaster.value);
                sliderMaster.onValueChanged.AddListener(OnSliderMasterChanged);
            }
            if (sliderMusica != null)
            {
                sliderMusica.value = AudioManager.Instancia.VolumenMusica;
                ActualizarTextoMusica(sliderMusica.value);
                sliderMusica.onValueChanged.AddListener(OnSliderMusicaChanged);
            }
            if (sliderSFX != null)
            {
                sliderSFX.value = AudioManager.Instancia.VolumenSFX;
                ActualizarTextoSFX(sliderSFX.value);
                sliderSFX.onValueChanged.AddListener(OnSliderSFXChanged);
            }
        }
    }

    private void ActualizarTextoMaster(float val)
    {
        if (txtValorMaster != null)
            txtValorMaster.text = $"{Mathf.RoundToInt(val * 100f)}%";
    }

    private void ActualizarTextoMusica(float val)
    {
        if (txtValorMusica != null)
            txtValorMusica.text = $"{Mathf.RoundToInt(val * 100f)}%";
    }

    private void ActualizarTextoSFX(float val)
    {
        if (txtValorSFX != null)
            txtValorSFX.text = $"{Mathf.RoundToInt(val * 100f)}%";
    }

    public void OnSliderMasterChanged(float val)
    {
        ActualizarTextoMaster(val);
        if (AudioManager.Instancia != null)
            AudioManager.Instancia.SetMasterVolume(val);
    }

    public void OnSliderMusicaChanged(float val)
    {
        ActualizarTextoMusica(val);
        if (AudioManager.Instancia != null)
            AudioManager.Instancia.SetMusicVolume(val);
    }

    public void OnSliderSFXChanged(float val)
    {
        ActualizarTextoSFX(val);
        if (AudioManager.Instancia != null)
        {
            AudioManager.Instancia.SetSFXVolume(val);
            AudioManager.Instancia.ReproducirSFX("click");
        }
    }

    public void MostrarSoloInicio()
    {
        if (panelInicio != null) panelInicio.SetActive(true);
        if (panelSeleccion != null) panelSeleccion.SetActive(false);
        if (panelODS != null) panelODS.SetActive(false);
        if (panelSonido != null) panelSonido.SetActive(false);
        if (panelControles != null) panelControles.SetActive(false);
    }

    // 1. INICIAR AVENTURA
    public void BotonIniciarAventura()
    {
        ReproducirClick();
        SistemaProgreso.ReiniciarTodo();
        SistemaGuardado.ReiniciarPartida();

        // Cargar Nivel 1 (Parque de la Identidad Huanca)
        CargarEscenaSegura("NivelParqueColonial", "Nivel1");
    }

    // 2. SELECTOR DE MISIONES
    public void BotonAbrirSelectorMisiones()
    {
        ReproducirClick();
        if (panelInicio != null) panelInicio.SetActive(false);
        if (panelSeleccion != null)
        {
            panelSeleccion.SetActive(true);
            ActualizarTarjetasMisiones();
        }
    }

    public void ActualizarTarjetasMisiones()
    {
        if (panelSeleccion == null) return;

        // Ocultar y desactivar completamente los niveles 4 al 9 para que NO estén en pantalla ni se pueda acceder
        for (int i = 4; i <= 9; i++)
        {
            Transform card = panelSeleccion.transform.Find($"Ventana/Grid_Niveles/Card_Nivel_{i}");
            if (card != null)
            {
                card.gameObject.SetActive(false);
            }
        }

        // Configurar niveles 1 al 3: comprobar si están desbloqueados progresivamente
        for (int i = 1; i <= 3; i++)
        {
            Transform card = panelSeleccion.transform.Find($"Ventana/Grid_Niveles/Card_Nivel_{i}");
            if (card != null)
            {
                card.gameObject.SetActive(true);
                Button btn = card.GetComponent<Button>();
                bool desbloqueado = SistemaProgreso.EsNivelDesbloqueado(i);
                if (btn != null)
                {
                    btn.interactable = desbloqueado;
                }
            }
        }
    }

    public void CargarMision(int numeroNivel)
    {
        // BLOQUEO TOTAL Y ABSOLUTO: Del 4 al 9 están bloqueados y jamás se permite entrar
        if (numeroNivel < 1 || numeroNivel > 3)
        {
            Debug.LogWarning($"<color=#FF3333><b>[BLOQUEO TOTAL]</b> El nivel {numeroNivel} está bloqueado. Prohibido el ingreso.</color>");
            return;
        }

        // Validar que el nivel esté desbloqueado por progreso
        if (!SistemaProgreso.EsNivelDesbloqueado(numeroNivel))
        {
            Debug.LogWarning($"<color=#FF9900><b>[BLOQUEO]</b> El nivel {numeroNivel} aún no está desbloqueado.</color>");
            return;
        }

        ReproducirClick();
        string[] nombresEscenas = new string[]
        {
            "NivelParqueColonial",
            "Nivel2_CerritoLibertad",
            "Nivel3_ParqueTupacAmaru"
        };

        string escena = nombresEscenas[numeroNivel - 1];
        if (numeroNivel == 1)
            CargarEscenaSegura(escena, "Nivel1");
        else
            SceneManager.LoadScene(escena);
    }

    // 3. REGISTRO ODS 12 & 15
    public void BotonAbrirODS()
    {
        ReproducirClick();
        if (panelODS != null) panelODS.SetActive(true);
    }

    public void BotonCerrarODS()
    {
        ReproducirClick();
        if (panelODS != null) panelODS.SetActive(false);
    }

    // 4. AJUSTES DE SONIDO
    public void BotonAbrirAjustesSonido()
    {
        ReproducirClick();
        if (panelSonido != null)
        {
            panelSonido.SetActive(true);
            if (AudioManager.Instancia != null)
            {
                if (sliderMaster != null) sliderMaster.value = AudioManager.Instancia.VolumenMaster;
                if (sliderMusica != null) sliderMusica.value = AudioManager.Instancia.VolumenMusica;
                if (sliderSFX != null) sliderSFX.value = AudioManager.Instancia.VolumenSFX;
            }
        }
    }

    public void BotonCerrarAjustesSonido()
    {
        ReproducirClick();
        if (panelSonido != null) panelSonido.SetActive(false);
    }

    // 5. GUÍA DE CONTROLES
    public void BotonAbrirControles()
    {
        ReproducirClick();
        if (panelControles != null) panelControles.SetActive(true);
    }

    public void BotonCerrarControles()
    {
        ReproducirClick();
        if (panelControles != null) panelControles.SetActive(false);
    }

    // 6. SALIR
    public void BotonSalir()
    {
        ReproducirClick();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void BotonVolverAlInicio()
    {
        ReproducirClick();
        MostrarSoloInicio();
    }

    private void ReproducirClick()
    {
        if (AudioManager.Instancia != null)
            AudioManager.Instancia.ReproducirSFX("click");
    }

    private void CargarEscenaSegura(string principal, string fallback)
    {
        if (Application.CanStreamedLevelBeLoaded(principal))
            SceneManager.LoadScene(principal);
        else if (Application.CanStreamedLevelBeLoaded(fallback))
            SceneManager.LoadScene(fallback);
        else
            SceneManager.LoadScene(1);
    }

    private IEnumerator RutinaFlotarLogo()
    {
        if (rectLogo == null) yield break;
        float t = 0f;
        while (true)
        {
            t += Time.deltaTime * 2.2f;
            float offY = Mathf.Sin(t) * 8f;
            rectLogo.anchoredPosition = posOriginalLogo + new Vector2(0f, offY);
            yield return null;
        }
    }
}
