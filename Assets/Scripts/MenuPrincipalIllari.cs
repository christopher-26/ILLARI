using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

/// <summary>
/// Controlador Arcade del Menú Principal de 'Illari' (Huancayo 2026).
/// - Conecta de forma garantizada los botones principales y modales (runtime + persistente).
/// - Gestiona paneles modales: Selección de 9 Niveles, Controles/Guía, y Misión Ecológica ODS 15.
/// - Sistema de desbloqueo dinámico con SistemaProgreso.
/// - Animación flotante del logo y transiciones suaves de escena.
/// - Feedback visual al pasar el cursor o hacer clic sobre los botones.
/// </summary>
public class MenuPrincipalIllari : MonoBehaviour
{
    [Header("Paneles Principales")]
    [SerializeField] private GameObject panelInicio;
    [SerializeField] private GameObject panelSeleccion;
    [SerializeField] private GameObject panelControles;
    [SerializeField] private GameObject panelODS15;

    [Header("Botones Principales")]
    [SerializeField] private Button botonJugar;
    [SerializeField] private Button botonSeleccion;
    [SerializeField] private Button botonControles;
    [SerializeField] private Button botonODS15;
    [SerializeField] private Button botonSalir;

    [Header("Botones de Cierre de Modales")]
    [SerializeField] private Button botonCerrarSeleccion;
    [SerializeField] private Button botonCerrarControles;
    [SerializeField] private Button botonCerrarODS15;

    [Header("Elementos de Animación")]
    [SerializeField] private RectTransform rectLogo;
    [SerializeField] private CanvasGroup canvasGroupTransicion;

    [Header("Botones de Niveles (1 al 9)")]
    [SerializeField] private Button[] botonesNiveles = new Button[9];
    [SerializeField] private Text[] textosNiveles = new Text[9];

    private Vector2 posOriginalLogo;
    private bool estaTransicionando = false;

    private static Sprite sprBlanco;

    public static Sprite ObtenerSpriteBlanco()
    {
        if (sprBlanco == null)
            sprBlanco = Resources.Load<Sprite>("UI/Blanco");
        if (sprBlanco == null)
        {
#if UNITY_EDITOR
            sprBlanco = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Blanco.png");
#endif
        }
        return sprBlanco;
    }

    public static Sprite CargarSpriteUI(string nombre)
    {
        Sprite s = Resources.Load<Sprite>("UI/" + nombre);
        if (s == null)
        {
#if UNITY_EDITOR
            s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/" + nombre + ".png");
#endif
        }
        return s;
    }

    private void GarantizarEventSystem()
    {
        var es = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (es == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            es = esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        }

        var inputModType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (inputModType != null)
        {
            if (es.GetComponent(inputModType) == null)
            {
                var sim = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                if (sim != null) DestroyImmediate(sim);
                es.gameObject.AddComponent(inputModType);
            }
        }
        else
        {
            if (es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>() == null)
            {
                es.gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }
    }

    private void Awake()
    {
        Time.timeScale = 1f;

        GarantizarEventSystem();
        VincularReferencias();
        ConectarTodosLosBotones();
    }

    private void Start()
    {
        if (rectLogo != null)
        {
            posOriginalLogo = rectLogo.anchoredPosition;
        }

        // Re-conectar botones en Start para máxima redundancia
        ConectarTodosLosBotones();
        CerrarTodosLosModales();
        ActualizarGridSeleccionNiveles();
    }

    private void Update()
    {
        // Animación suave de flotación del logo de Illari
        if (rectLogo != null)
        {
            float offset = Mathf.Sin(Time.unscaledTime * 2.2f) * 10f;
            rectLogo.anchoredPosition = new Vector2(posOriginalLogo.x, posOriginalLogo.y + offset);
        }

        // Tecla Escape para cerrar modales abiertos
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (panelSeleccion != null && panelSeleccion.activeSelf) CerrarTodosLosModales();
            else if (panelControles != null && panelControles.activeSelf) CerrarTodosLosModales();
            else if (panelODS15 != null && panelODS15.activeSelf) CerrarTodosLosModales();
        }
    }

    // ==========================================
    // VINCULACIÓN ROBUSTA DE REFERENCIAS
    // ==========================================
    public void VincularReferencias()
    {
        Transform canvas = transform.root.GetComponentInChildren<Canvas>()?.transform;
        if (canvas == null) canvas = transform;

        // Paneles
        if (panelInicio == null)
        {
            Transform t = canvas.Find("Contenedor_Botones_Principales");
            if (t != null) panelInicio = t.gameObject;
        }

        if (panelSeleccion == null)
        {
            Transform t = canvas.Find("Panel_SeleccionNiveles");
            if (t != null) panelSeleccion = t.gameObject;
        }

        if (panelControles == null)
        {
            Transform t = canvas.Find("Panel_Controles");
            if (t != null) panelControles = t.gameObject;
        }

        if (panelODS15 == null)
        {
            Transform t = canvas.Find("Panel_ODS15");
            if (t != null) panelODS15 = t.gameObject;
        }

        if (rectLogo == null)
        {
            Transform t = canvas.Find("Logo_Illari");
            if (t != null) rectLogo = t.GetComponent<RectTransform>();
        }

        if (canvasGroupTransicion == null)
        {
            Transform t = canvas.Find("Overlay_Transicion");
            if (t != null) canvasGroupTransicion = t.GetComponent<CanvasGroup>();
        }

        // Botones Principales
        if (panelInicio != null)
        {
            Transform pt = panelInicio.transform;
            if (botonJugar == null) botonJugar = pt.Find("Boton_Jugar")?.GetComponent<Button>();
            if (botonSeleccion == null) botonSeleccion = pt.Find("Boton_SeleccionNiveles")?.GetComponent<Button>();
            if (botonControles == null) botonControles = pt.Find("Boton_Controles")?.GetComponent<Button>();
            if (botonODS15 == null) botonODS15 = pt.Find("Boton_ODS15")?.GetComponent<Button>();
            if (botonSalir == null) botonSalir = pt.Find("Boton_Salir")?.GetComponent<Button>();
        }

        // Botones Cerrar Modales
        if (panelSeleccion != null && botonCerrarSeleccion == null)
            botonCerrarSeleccion = panelSeleccion.transform.Find("Ventana/Boton_Cerrar_Seleccion")?.GetComponent<Button>();

        if (panelControles != null && botonCerrarControles == null)
            botonCerrarControles = panelControles.transform.Find("Ventana/Boton_Cerrar_Controles")?.GetComponent<Button>();

        if (panelODS15 != null && botonCerrarODS15 == null)
            botonCerrarODS15 = panelODS15.transform.Find("Ventana/Boton_Cerrar_ODS15")?.GetComponent<Button>();

        // Botones de niveles (1..9)
        if (panelSeleccion != null)
        {
            Transform grid = panelSeleccion.transform.Find("Ventana/Grid_Niveles");
            if (grid != null)
            {
                for (int i = 0; i < 9; i++)
                {
                    Transform card = grid.Find($"Card_Nivel_{i + 1}");
                    if (card != null)
                    {
                        Button btn = card.GetComponent<Button>();
                        if (btn != null) botonesNiveles[i] = btn;

                        Transform txtT = card.Find("Texto_Nivel");
                        if (txtT != null) textosNiveles[i] = txtT.GetComponent<Text>();
                    }
                }
            }
        }
    }

    // ==========================================
    // CONEXIÓN OBLIGATORIA DE LISTENERS
    // ==========================================
    public void ConectarTodosLosBotones()
    {
        VincularReferencias();

        // Conectar botones principales
        ConectarBoton(botonJugar, IniciarAventura);
        ConectarBoton(botonSeleccion, AbrirSeleccionNiveles);
        ConectarBoton(botonControles, AbrirControles);
        ConectarBoton(botonODS15, AbrirODS15);
        ConectarBoton(botonSalir, SalirDelJuego);

        // Conectar botones de modales
        ConectarBoton(botonCerrarSeleccion, CerrarTodosLosModales);
        ConectarBoton(botonCerrarControles, CerrarTodosLosModales);
        ConectarBoton(botonCerrarODS15, CerrarTodosLosModales);
    }

    private void ConectarBoton(Button btn, UnityEngine.Events.UnityAction accion)
    {
        if (btn == null) return;

        btn.interactable = true;
        btn.onClick.RemoveListener(accion);
        btn.onClick.AddListener(accion);

        // Añadir componente de hover y click táctil si no existe
        if (btn.GetComponent<BotonArcadeEfecto>() == null)
        {
            btn.gameObject.AddComponent<BotonArcadeEfecto>();
        }
    }

    // ==========================================
    // ACCIONES DE BOTONES PRINCIPALES
    // ==========================================
    public void IniciarAventura()
    {
        if (estaTransicionando) return;
        Debug.Log("<color=#00FF88><b>[MENU]</b> ¡Iniciando Aventura! Cargando parque...</color>");

        int nivelAIniciar = 1;
        if (PlayerPrefs.GetInt("Illari_HayPartida", 0) == 1)
        {
            string escenaGuardada = PlayerPrefs.GetString("Illari_EscenaGuardada", "");
            if (!string.IsNullOrEmpty(escenaGuardada))
            {
                IniciarCargaEscena(escenaGuardada);
                return;
            }
        }

        IniciarCargaEscena(SistemaProgreso.ESCENAS_NIVELES[nivelAIniciar]);
    }

    public void AbrirSeleccionNiveles()
    {
        Debug.Log("<color=#00FFE0><b>[MENU]</b> Abriendo Selección de Parques...</color>");
        CerrarTodosLosModales();
        ActualizarGridSeleccionNiveles();
        if (panelSeleccion != null) panelSeleccion.SetActive(true);
        if (panelInicio != null) panelInicio.SetActive(false);
    }

    public void AbrirControles()
    {
        Debug.Log("<color=#00FFE0><b>[MENU]</b> Abriendo Guía de Controles...</color>");
        CerrarTodosLosModales();
        if (panelControles != null) panelControles.SetActive(true);
        if (panelInicio != null) panelInicio.SetActive(false);
    }

    public void AbrirODS15()
    {
        Debug.Log("<color=#00FFE0><b>[MENU]</b> Abriendo Misión ODS 15...</color>");
        CerrarTodosLosModales();
        if (panelODS15 != null) panelODS15.SetActive(true);
        if (panelInicio != null) panelInicio.SetActive(false);
    }

    public void CerrarTodosLosModales()
    {
        if (panelSeleccion != null) panelSeleccion.SetActive(false);
        if (panelControles != null) panelControles.SetActive(false);
        if (panelODS15 != null) panelODS15.SetActive(false);
        if (panelInicio != null) panelInicio.SetActive(true);
    }

    public void SeleccionarNivel(int nivel)
    {
        if (estaTransicionando) return;
        if (nivel < 1 || nivel > SistemaProgreso.TOTAL_NIVELES) return;

        if (SistemaProgreso.EsNivelDesbloqueado(nivel))
        {
            string escena = SistemaProgreso.ESCENAS_NIVELES[nivel];
            Debug.Log($"<color=#00FF88><b>[MENU]</b> Cargando {SistemaProgreso.NOMBRES_NIVELES[nivel]} (Nivel {nivel})...</color>");
            IniciarCargaEscena(escena);
        }
        else
        {
            Debug.Log($"<color=#FFAA00><b>[MENU]</b> El Nivel {nivel} aún está bloqueado. Supera el parque anterior.</color>");
        }
    }

    public void SalirDelJuego()
    {
        Debug.Log("<color=#FF4444><b>[MENU]</b> Saliendo del juego...</color>");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ==========================================
    // ACTUALIZACIÓN DE ESTADO DE NIVELES
    // ==========================================
    public void ActualizarGridSeleccionNiveles()
    {
        Sprite sprBtnNormal = CargarSpriteUI("BotonArcade_Normal");
        Sprite sprBtnHover = CargarSpriteUI("BotonArcade_Hover");
        Sprite sprBtnPressed = CargarSpriteUI("BotonArcade_Pressed");

        for (int i = 0; i < 9; i++)
        {
            int nivel = i + 1;
            bool desbloqueado = SistemaProgreso.EsNivelDesbloqueado(nivel);
            string nombreNivel = (nivel < SistemaProgreso.NOMBRES_NIVELES.Length) ? SistemaProgreso.NOMBRES_NIVELES[nivel] : $"Parque {nivel}";
            string icono = (nivel < SistemaProgreso.ICONOS_NIVELES.Length) ? SistemaProgreso.ICONOS_NIVELES[nivel] : "★";

            if (botonesNiveles[i] != null)
            {
                botonesNiveles[i].interactable = desbloqueado;
                int nCapture = nivel;
                botonesNiveles[i].onClick.RemoveAllListeners();
                botonesNiveles[i].onClick.AddListener(() => SeleccionarNivel(nCapture));

                if (botonesNiveles[i].GetComponent<BotonArcadeEfecto>() == null && desbloqueado)
                {
                    botonesNiveles[i].gameObject.AddComponent<BotonArcadeEfecto>();
                }

                Image imgBtn = botonesNiveles[i].GetComponent<Image>();
                if (imgBtn != null)
                {
                    imgBtn.color = desbloqueado ? Color.white : new Color(0.40f, 0.40f, 0.40f, 0.70f);
                }
            }

            if (textosNiveles[i] != null)
            {
                if (desbloqueado)
                {
                    textosNiveles[i].text = $"{icono} <b>NIVEL {nivel}</b>\n<size=15>{nombreNivel}</size>\n<size=13><color=#00FFE0>♻ 15 Botellas</color></size>";
                    textosNiveles[i].color = Color.white;
                }
                else
                {
                    textosNiveles[i].text = $"🔒 <b>NIVEL {nivel}</b>\n<size=14><color=#AAAAAA>BLOQUEADO</color></size>\n<size=12><color=#777777>Supera Nivel {nivel - 1}</color></size>";
                    textosNiveles[i].color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
                }
            }
        }
    }

    // ==========================================
    // TRANSICIÓN FLUIDA DE ESCENA
    // ==========================================
    private void IniciarCargaEscena(string nombreEscena)
    {
        if (estaTransicionando) return;
        estaTransicionando = true;
        Time.timeScale = 1f;
        Debug.Log($"<color=#00FF88><b>[MENU]</b> Cargando escena: {nombreEscena}</color>");
        SceneManager.LoadScene(nombreEscena);
    }
}
