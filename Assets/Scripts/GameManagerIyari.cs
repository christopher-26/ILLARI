using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// GameManager central para 'Illari' (Huancayo 2026).
/// Gestiona:
/// 1. Salud del jugador con corazones gráficos arcades y barra de vida con daño fantasma.
/// 2. Conteo estricto de 15 botellas de plástico para la meta ecológica ODS 15.
/// 3. Identificación del parque/nivel actual.
/// 4. Botón de Pausa táctil circular arcade en pantalla y atajos (Escape / P).
/// 5. Menú de Pausa arcade con marco andino, estadísticas en tiempo real y botones estilizados.
/// 6. Auto-construcción / auto-vinculación de UI robusta con sprites dedicados libres de fugas de atlas.
/// </summary>
public class GameManagerIyari : MonoBehaviour
{
    public static GameManagerIyari Instancia { get; private set; }

    [Header("Configuración de Salud")]
    [SerializeField] private int vidaMaxima = 3;
    [SerializeField] private int vidaActual = 3;

    [Header("Puntuación y Meta Ecológica")]
    [SerializeField] private int botellasRecolectadas = 0;
    [SerializeField] private int botellasRequeridas = 15;

    [Header("UI - Barra de Vida y Corazones")]
    [SerializeField] private Image barraVidaFill;
    [SerializeField] private Image barraVidaGhost;
    [SerializeField] private Image[] imagenesCorazones = new Image[3];
    [SerializeField] private Text textoVidaLegacy;
    [SerializeField] private TMP_Text textoVidaTMP;

    [Header("UI - Botellas")]
    [SerializeField] private Image iconoBotella;
    [SerializeField] private Text textoBotellasLegacy;
    [SerializeField] private TMP_Text textoBotellasTMP;

    [Header("UI - Poderes")]
    [SerializeField] private Text textoPoderLegacy;

    [Header("UI - Nivel")]
    [SerializeField] private Text textoNivelLegacy;
    [SerializeField] private TMP_Text textoNivelTMP;

    [Header("UI - Avisos y Paneles")]
    [SerializeField] private GameObject panelAviso;
    [SerializeField] private Text textoAvisoLegacy;
    [SerializeField] private TMP_Text textoAvisoTMP;
    [SerializeField] private GameObject panelVictoria;
    [SerializeField] private GameObject panelGameOver;

    [Header("UI - Pausa y Guardado")]
    [SerializeField] private GameObject panelPausa;
    [SerializeField] private Button botonPausaPantalla;
    [SerializeField] private Text textoFeedbackPausa;
    [SerializeField] private Text textoProgresoPausa;

    [Header("Parámetros de Juego")]
    [SerializeField] private float tiempoEsperaReinicio = 1.8f;

    private bool juegoTerminado = false;
    private bool estaPausado = false;
    private float tiempoUltimaPausa = -1f;
    private int frameUltimaPausa = -1;
    private Coroutine corrutinaAviso;

    // HUD Animación
    private Coroutine corrutinaFlashDanio;
    private Coroutine corrutinaAnimBarraVida;
    private float barraVidaTargetFill = 1f;
    private Image panelFlashRojo;

    private static Sprite sprBlanco;
    private static Font fuenteUI;

    public static Sprite ObtenerSpriteBlanco()
    {
        if (sprBlanco == null)
        {
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color[] cols = new Color[16 * 16];
            for (int i = 0; i < cols.Length; i++) cols[i] = Color.white;
            tex.SetPixels(cols);
            tex.Apply();
            sprBlanco = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 100f);
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

    private static Font fuenteTitulo;
    private static Font fuenteCuerpo;

    public static Font ObtenerFuenteTitulo()
    {
        if (fuenteTitulo == null)
        {
            fuenteTitulo = Resources.Load<Font>("Fonts/Roboto-Bold");
            if (fuenteTitulo == null) fuenteTitulo = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        return fuenteTitulo;
    }

    public static Font ObtenerFuenteCuerpo()
    {
        if (fuenteCuerpo == null)
        {
            fuenteCuerpo = Resources.Load<Font>("Fonts/Roboto-Bold");
            if (fuenteCuerpo == null) fuenteCuerpo = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        return fuenteCuerpo;
    }

    public static Font ObtenerFuenteUI()
    {
        return ObtenerFuenteTitulo();
    }

    private void Awake()
    {
        Instancia = this;
    }

    private void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    private void Start()
    {
        Time.timeScale = 1f;

        // Restaurar estado guardado si se solicitó cargar partida
        int vidaCargada, botellasCargadas;
        if (SistemaProgreso.DebeRestaurarEstado(out vidaCargada, out botellasCargadas))
        {
            vidaActual = Mathf.Clamp(vidaCargada, 1, vidaMaxima);
            botellasRecolectadas = Mathf.Clamp(botellasCargadas, 0, botellasRequeridas);
            Debug.Log($"<color=#00FF88><b>[CARGA]</b> Estado restaurado: Vida={vidaActual}, Botellas={botellasRecolectadas}</color>");
        }
        else
        {
            vidaActual = vidaMaxima;
        }

        GarantizarUI();
        ActualizarUI();

        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (panelPausa != null) panelPausa.SetActive(false);
        if (panelAviso != null) panelAviso.SetActive(false);

        // Mensaje de bienvenida al parque
        MostrarMensajeAviso($"¡Bienvenido a {ObtenerNombreParqueSimple()}!\nRecolecta 15 botellas de plástico para limpiar el parque.");
    }

    private void Update()
    {
        // Tecla rápida de Pausa (Escape o P) o clic directo en el botón de pantalla
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            AlternarPausa();
        }
        else if (Input.GetMouseButtonDown(0) && !estaPausado && botonPausaPantalla != null && botonPausaPantalla.gameObject.activeInHierarchy)
        {
            RectTransform rtPausa = botonPausaPantalla.GetComponent<RectTransform>();
            if (rtPausa != null && RectTransformUtility.RectangleContainsScreenPoint(rtPausa, Input.mousePosition, null))
            {
                AlternarPausa();
            }
        }

        // Pulso en la barra de vida cuando HP es crítico
        AnimarBarraVidaPulso();
    }

    private void AnimarBarraVidaPulso()
    {
        if (barraVidaFill == null) return;
        float pct = barraVidaTargetFill;
        if (pct <= 0.34f && !juegoTerminado)
        {
            // Pulsado rojo urgente
            float pulso = Mathf.Abs(Mathf.Sin(Time.time * 4.0f));
            barraVidaFill.color = Color.Lerp(
                new Color(0.9f, 0.1f, 0.1f),
                new Color(1f, 0.55f, 0.0f),
                pulso
            );

            // Escala pulsante del panel HUD si existe
            if (panelFlashRojo != null)
            {
                Color cFlash = panelFlashRojo.color;
                cFlash.a = pulso * 0.22f;
                panelFlashRojo.color = cFlash;
            }
        }
    }

    // ==========================================
    // SISTEMA DE SALUD Y BOTELLAS
    // ==========================================
    public void SumarBotella(int cantidad = 1)
    {
        if (juegoTerminado) return;

        botellasRecolectadas += cantidad;
        ActualizarUI();

        if (botellasRecolectadas >= botellasRequeridas)
        {
            MostrarMensajeAviso("¡15 BOTELLAS RECOLECTADAS!\nEl Eco-Contenedor está listo para ser activado.");
        }
    }

    public void ModificarVida(int cambio)
    {
        if (juegoTerminado) return;

        vidaActual = Mathf.Clamp(vidaActual + cambio, 0, vidaMaxima);
        ActualizarUI();

        // Flash rojo al recibir daño
        if (cambio < 0)
        {
            if (corrutinaFlashDanio != null) StopCoroutine(corrutinaFlashDanio);
            corrutinaFlashDanio = StartCoroutine(RutinaFlashDanio());
        }

        if (vidaActual <= 0)
        {
            EjecutarGameOver();
        }
    }

    private IEnumerator RutinaFlashDanio()
    {
        // Crea el panel de flash rojo si aún no existe
        if (panelFlashRojo == null)
        {
            Canvas cv = Object.FindFirstObjectByType<Canvas>();
            if (cv != null)
            {
                GameObject goFlash = new GameObject("Panel_Flash_Danio", typeof(RectTransform));
                goFlash.transform.SetParent(cv.transform, false);
                RectTransform rt = goFlash.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
                rt.SetAsLastSibling();
                panelFlashRojo = goFlash.AddComponent<Image>();
                panelFlashRojo.sprite = ObtenerSpriteBlanco();
                panelFlashRojo.color = new Color(1f, 0.05f, 0.05f, 0f);
                panelFlashRojo.raycastTarget = false;
            }
        }

        if (panelFlashRojo == null) yield break;

        // Flash rápido de entrada
        float t = 0f;
        while (t < 0.12f)
        {
            t += Time.deltaTime;
            Color c = panelFlashRojo.color;
            c.a = Mathf.Lerp(0f, 0.45f, t / 0.12f);
            panelFlashRojo.color = c;
            yield return null;
        }

        // Fade out
        t = 0f;
        while (t < 0.35f)
        {
            t += Time.deltaTime;
            Color c = panelFlashRojo.color;
            c.a = Mathf.Lerp(0.45f, 0f, t / 0.35f);
            panelFlashRojo.color = c;
            yield return null;
        }

        Color final = panelFlashRojo.color;
        final.a = 0f;
        panelFlashRojo.color = final;
    }

    public void RecibirDanio(int cantidad = 1)
    {
        ModificarVida(-Mathf.Abs(cantidad));
    }

    public int ObtenerVida() => vidaActual;
    public int ObtenerBotellas() => botellasRecolectadas;
    public int ObtenerBotellasRequeridas() => botellasRequeridas;

    public void ConfigurarMeta(int requeridas)
    {
        botellasRequeridas = requeridas;
        ActualizarUI();
    }

    // ==========================================
    // SISTEMA DE PAUSA Y GUARDADO
    // ==========================================
    public void AlternarPausa()
    {
        if (juegoTerminado) return;
        if (Time.unscaledTime - tiempoUltimaPausa < 0.25f || Time.frameCount == frameUltimaPausa) return;
        tiempoUltimaPausa = Time.unscaledTime;
        frameUltimaPausa = Time.frameCount;

        estaPausado = !estaPausado;
        Time.timeScale = estaPausado ? 0f : 1f;

        if (panelPausa != null)
        {
            panelPausa.SetActive(estaPausado);
            if (textoFeedbackPausa != null)
            {
                textoFeedbackPausa.text = "";
            }
            if (textoProgresoPausa != null)
            {
                textoProgresoPausa.text = $"{ObtenerNombreNivelCompleto()}   |   ♻ {botellasRecolectadas} / {botellasRequeridas} BOTELLAS";
            }
        }
    }

    public void ReanudarJuego()
    {
        tiempoUltimaPausa = Time.unscaledTime;
        frameUltimaPausa = Time.frameCount;
        estaPausado = false;
        Time.timeScale = 1f;
        if (panelPausa != null) panelPausa.SetActive(false);
    }

    public void GuardarPartida()
    {
        string escenaActual = SceneManager.GetActiveScene().name;
        SistemaProgreso.GuardarPartida(escenaActual, vidaActual, botellasRecolectadas);

        if (textoFeedbackPausa != null)
        {
            textoFeedbackPausa.text = "¡Partida guardada con éxito!";
            textoFeedbackPausa.color = new Color(0.2f, 1f, 0.4f);
        }
    }

    public void CargarPartidaGuardada()
    {
        if (SistemaProgreso.ExistePartida())
        {
            Time.timeScale = 1f;
            SistemaProgreso.CargarPartida();
        }
        else
        {
            if (textoFeedbackPausa != null)
            {
                textoFeedbackPausa.text = "No hay partida guardada previa.";
                textoFeedbackPausa.color = new Color(1f, 0.4f, 0.4f);
            }
        }
    }

    public void ReiniciarNivelActual()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ==========================================
    // ACTUALIZACIÓN VISUAL DEL HUD
    // ==========================================
    private void ActualizarUI()
    {
        // 1. Salud y Barra de Vida
        float pctVida = (float)vidaActual / Mathf.Max(1, vidaMaxima);
        barraVidaTargetFill = pctVida;

        if (barraVidaFill != null)
        {
            if (corrutinaAnimBarraVida != null) StopCoroutine(corrutinaAnimBarraVida);
            corrutinaAnimBarraVida = StartCoroutine(AnimarFillBarraVida(pctVida));
        }

        // Actualizar Corazones Gráficos
        if (imagenesCorazones != null && imagenesCorazones.Length >= 3)
        {
            Sprite sprLleno = CargarSpriteUI("CorazonLleno");
            Sprite sprVacio = CargarSpriteUI("CorazonVacio");
            for (int i = 0; i < imagenesCorazones.Length; i++)
            {
                if (imagenesCorazones[i] != null)
                {
                    imagenesCorazones[i].sprite = (i < vidaActual) ? sprLleno : sprVacio;
                    imagenesCorazones[i].color = Color.white;
                }
            }
        }

        string txtVida = $"VIDA: {vidaActual} / {vidaMaxima}";
        SetTexto(textoVidaLegacy, textoVidaTMP, txtVida);

        // 2. Contador de Botellas
        float pctBot = (float)botellasRecolectadas / Mathf.Max(1, botellasRequeridas);
        string txtBot = $"BOTELLAS: {botellasRecolectadas} / {botellasRequeridas}";
        if (botellasRecolectadas >= botellasRequeridas) txtBot += "  ✓ ¡COMPLETO!";
        SetTexto(textoBotellasLegacy, textoBotellasTMP, txtBot);

        if (textoBotellasLegacy != null)
        {
            if (pctBot >= 1f) textoBotellasLegacy.color = new Color(0.2f, 1f, 0.5f);
            else if (pctBot >= 0.5f) textoBotellasLegacy.color = new Color(0.0f, 1f, 0.88f);
            else textoBotellasLegacy.color = new Color(0.5f, 0.88f, 1f);
        }

        // 3. Nombre del Nivel
        string txtNivel = ObtenerNombreNivelCompleto();
        SetTexto(textoNivelLegacy, textoNivelTMP, txtNivel);

        // 4. Progreso en menú de pausa si está visible
        if (textoProgresoPausa != null)
        {
            textoProgresoPausa.text = $"{ObtenerNombreNivelCompleto()}   |   ♻ {botellasRecolectadas} / {botellasRequeridas} BOTELLAS";
        }
    }

    private IEnumerator AnimarFillBarraVida(float targetFill)
    {
        if (barraVidaFill == null) yield break;

        float startFill = barraVidaFill.fillAmount;
        bool esDanio = targetFill < startFill;

        float t = 0f;
        float duracion = 0.20f;

        while (t < duracion)
        {
            t += Time.deltaTime;
            float pct = Mathf.Clamp01(t / duracion);
            float eased = 1f - (1f - pct) * (1f - pct);
            barraVidaFill.fillAmount = Mathf.Lerp(startFill, targetFill, eased);

            if (targetFill > 0.34f)
            {
                if (targetFill > 0.65f)
                    barraVidaFill.color = new Color(0.18f, 0.96f, 0.45f);
                else
                    barraVidaFill.color = new Color(1f, 0.78f, 0.1f);
            }

            yield return null;
        }

        barraVidaFill.fillAmount = targetFill;

        // Barra Ghost (daño retardado arcade)
        if (barraVidaGhost != null)
        {
            if (esDanio)
            {
                yield return new WaitForSeconds(0.25f);
                float tGhost = 0f;
                float duracionGhost = 0.40f;
                float startGhost = barraVidaGhost.fillAmount;
                while (tGhost < duracionGhost)
                {
                    tGhost += Time.deltaTime;
                    float pctG = Mathf.Clamp01(tGhost / duracionGhost);
                    float easedG = 1f - (1f - pctG) * (1f - pctG);
                    barraVidaGhost.fillAmount = Mathf.Lerp(startGhost, targetFill, easedG);
                    yield return null;
                }
                barraVidaGhost.fillAmount = targetFill;
            }
            else
            {
                barraVidaGhost.fillAmount = targetFill;
            }
        }
    }

    private void SetTexto(Text legacy, TMP_Text tmp, string texto)
    {
        if (legacy != null) legacy.text = texto;
        if (tmp != null) tmp.text = texto;
    }

    public void MostrarMensajeAviso(string mensaje)
    {
        if (corrutinaAviso != null) StopCoroutine(corrutinaAviso);
        corrutinaAviso = StartCoroutine(RutinaMostrarAviso(mensaje));
    }

    private IEnumerator RutinaMostrarAviso(string mensaje)
    {
        if (panelAviso == null) yield break;
        panelAviso.SetActive(true);
        SetTexto(textoAvisoLegacy, textoAvisoTMP, mensaje);

        RectTransform rt = panelAviso.GetComponent<RectTransform>();
        CanvasGroup cg = panelAviso.GetComponent<CanvasGroup>();
        if (cg == null) cg = panelAviso.AddComponent<CanvasGroup>();

        // Animación suave de entrada (desliza de Y=12 a Y=-22)
        float t = 0f;
        while (t < 0.22f)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / 0.22f);
            float eased = 1f - (1f - p) * (1f - p);
            if (rt != null) rt.anchoredPosition = new Vector2(90f, Mathf.Lerp(12f, -22f, eased));
            cg.alpha = eased;
            yield return null;
        }
        if (rt != null) rt.anchoredPosition = new Vector2(90f, -22f);
        cg.alpha = 1f;

        yield return new WaitForSecondsRealtime(5.0f);

        // Animación suave de salida
        t = 0f;
        while (t < 0.25f)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / 0.25f);
            if (rt != null) rt.anchoredPosition = new Vector2(90f, Mathf.Lerp(-22f, 12f, p));
            cg.alpha = 1f - p;
            yield return null;
        }

        panelAviso.SetActive(false);
    }

    public void ActualizarPoderHUD(string nombrePoder, int municion)
    {
        if (textoPoderLegacy == null)
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                Transform tP = canvas.transform.Find("HUD_SuperiorIzquierdo/Texto_Poder");
                if (tP != null) textoPoderLegacy = tP.GetComponent<Text>();
            }
        }

        if (textoPoderLegacy == null) return;

        if (nombrePoder == "TRIPLE")
        {
            textoPoderLegacy.text = $"⚡ PODER: 💦 DISPARO TRIPLE (x{municion})";
            textoPoderLegacy.color = new Color(0f, 0.95f, 1f);
        }
        else if (nombrePoder == "ESCUDO")
        {
            textoPoderLegacy.text = "⚡ PODER: 🛡 ESCUDO ACUÁTICO (ACTIVO)";
            textoPoderLegacy.color = new Color(0.2f, 0.7f, 1f);
        }
        else
        {
            textoPoderLegacy.text = "⚡ PODER: NORMAL (CHORRO DE AGUA)";
            textoPoderLegacy.color = new Color(0.85f, 0.95f, 1f, 0.85f);
        }
    }

    public void CompletarNivel()
    {
        if (juegoTerminado) return;
        juegoTerminado = true;

        if (panelVictoria != null) panelVictoria.SetActive(true);
        MostrarMensajeAviso("¡VICTORIA! ¡PARQUE PURIFICADO AL 100%!");

        int nivelActual = SistemaProgreso.ObtenerIndiceNivelActual();
        if (nivelActual > 0)
        {
            SistemaProgreso.DesbloquearSiguienteNivel(nivelActual);
        }

        Debug.Log("<color=#00FF88><b>[ILLARI]</b> ¡Nivel Completado! Avanzando al siguiente parque...</color>");
        StartCoroutine(RutinaSiguienteNivel());
    }

    private IEnumerator RutinaSiguienteNivel()
    {
        yield return new WaitForSeconds(2.8f);

        int sigEscena = SceneManager.GetActiveScene().buildIndex + 1;
        if (sigEscena < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(sigEscena);
        }
        else
        {
            MostrarMensajeAviso("¡FELICITACIONES!\n¡TODOS LOS PARQUES DE HUANCAYO HAN SIDO PURIFICADOS!");
            yield return new WaitForSeconds(3f);
            SceneManager.LoadScene(0); // Menú Principal
        }
    }

    private void EjecutarGameOver()
    {
        juegoTerminado = true;
        if (panelGameOver != null) panelGameOver.SetActive(true);
        MostrarMensajeAviso("¡MISIÓN FALLIDA!\nIllari cayó por contaminación. Reiniciando...");

        StartCoroutine(RutinaReiniciarNivel());
    }

    private IEnumerator RutinaReiniciarNivel()
    {
        yield return new WaitForSeconds(tiempoEsperaReinicio);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ==========================================
    // AUTO-CONSTRUCCIÓN ROBUSTA DE UI ARCADE
    // ==========================================
    public void GarantizarUI()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas_UI", typeof(RectTransform));
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        var eventSys = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (eventSys == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            eventSys = esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        }
        var inputModType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (inputModType != null)
        {
            if (eventSys.GetComponent(inputModType) == null)
            {
                var sim = eventSys.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                if (sim != null) DestroyImmediate(sim);
                eventSys.gameObject.AddComponent(inputModType);
            }
        }
        else
        {
            if (eventSys.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>() == null)
            {
                eventSys.gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        Transform ct = canvas.transform;
        Sprite sprSolido = ObtenerSpriteBlanco();
        Font fuente = ObtenerFuenteUI();

        // 1. HUD Superior Izquierdo (Fondo, Nivel, Corazones, Barra de Vida, Botellas)
        Transform hud = ct.Find("HUD_SuperiorIzquierdo");
        bool necesitaReconstruirHUD = (hud == null || hud.Find("Corazon_0") == null);
        if (necesitaReconstruirHUD)
        {
            if (hud != null) DestroyImmediate(hud.gameObject);

            GameObject hudObj = new GameObject("HUD_SuperiorIzquierdo", typeof(RectTransform));
            hudObj.transform.SetParent(ct, false);
            hud = hudObj.transform;

            RectTransform rtHUD = hudObj.GetComponent<RectTransform>();
            rtHUD.anchorMin = new Vector2(0f, 1f);
            rtHUD.anchorMax = new Vector2(0f, 1f);
            rtHUD.pivot = new Vector2(0f, 1f);
            rtHUD.anchoredPosition = new Vector2(25, -25);
            rtHUD.sizeDelta = new Vector2(500, 196);

            Image imgHUD = hudObj.AddComponent<Image>();
            imgHUD.sprite = sprSolido;
            imgHUD.color = new Color(0.04f, 0.07f, 0.12f, 0.94f);
            imgHUD.raycastTarget = false;

            // Borde Neón Superior
            GameObject goAccent = new GameObject("HUD_BordeNeon", typeof(RectTransform));
            goAccent.transform.SetParent(hud, false);
            RectTransform rtAcc = goAccent.GetComponent<RectTransform>();
            rtAcc.anchorMin = new Vector2(0f, 1f);
            rtAcc.anchorMax = new Vector2(1f, 1f);
            rtAcc.pivot = new Vector2(0.5f, 1f);
            rtAcc.anchoredPosition = Vector2.zero;
            rtAcc.sizeDelta = new Vector2(0, 3);
            Image imgAcc = goAccent.AddComponent<Image>();
            imgAcc.sprite = sprSolido;
            imgAcc.color = new Color(0.0f, 1f, 0.88f, 0.95f);
            imgAcc.raycastTarget = false;

            // Texto Nombre del Nivel
            GameObject goNivel = CrearTextoHijo(hud, "Texto_Nivel", new Vector2(20, -14), new Vector2(460, 30), 22, FontStyle.Bold, new Color(1f, 0.88f, 0.30f), TextAnchor.MiddleLeft, ObtenerNombreNivelCompleto());
            textoNivelLegacy = goNivel.GetComponent<Text>();

            // Corazones Gráficos
            Sprite sprCorazonLleno = CargarSpriteUI("CorazonLleno");
            imagenesCorazones = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject heartObj = new GameObject($"Corazon_{i}", typeof(RectTransform));
                heartObj.transform.SetParent(hud, false);
                RectTransform rtH = heartObj.GetComponent<RectTransform>();
                rtH.anchorMin = new Vector2(0f, 1f);
                rtH.anchorMax = new Vector2(0f, 1f);
                rtH.pivot = new Vector2(0f, 1f);
                rtH.anchoredPosition = new Vector2(20 + i * 42, -44);
                rtH.sizeDelta = new Vector2(38, 38);

                Image imgH = heartObj.AddComponent<Image>();
                imgH.sprite = sprCorazonLleno;
                imgH.preserveAspect = true;
                imgH.raycastTarget = false;
                imagenesCorazones[i] = imgH;
            }

            // Texto Vida
            GameObject goVidaTxt = CrearTextoHijo(hud, "Texto_Vida", new Vector2(156, -48), new Vector2(220, 28), 20, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft, "VIDA: 3 / 3");
            textoVidaLegacy = goVidaTxt.GetComponent<Text>();

            // Barra de Vida con Marco Arcade
            GameObject goBarraBg = new GameObject("BarraVida_Fondo", typeof(RectTransform));
            goBarraBg.transform.SetParent(hud, false);
            RectTransform rtBg = goBarraBg.GetComponent<RectTransform>();
            rtBg.anchorMin = new Vector2(0f, 1f);
            rtBg.anchorMax = new Vector2(0f, 1f);
            rtBg.pivot = new Vector2(0f, 1f);
            rtBg.anchoredPosition = new Vector2(20, -86);
            rtBg.sizeDelta = new Vector2(280, 24);

            Image imgBg = goBarraBg.AddComponent<Image>();
            imgBg.sprite = sprSolido;
            imgBg.color = new Color(0.08f, 0.10f, 0.14f, 0.95f);
            imgBg.raycastTarget = false;

            // Relleno Ghost
            GameObject goBarraGhost = new GameObject("BarraVida_Ghost", typeof(RectTransform));
            goBarraGhost.transform.SetParent(goBarraBg.transform, false);
            RectTransform rtGhost = goBarraGhost.GetComponent<RectTransform>();
            rtGhost.anchorMin = Vector2.zero;
            rtGhost.anchorMax = Vector2.one;
            rtGhost.sizeDelta = Vector2.zero;
            Image imgGhost = goBarraGhost.AddComponent<Image>();
            imgGhost.sprite = sprSolido;
            imgGhost.type = Image.Type.Filled;
            imgGhost.fillMethod = Image.FillMethod.Horizontal;
            imgGhost.fillOrigin = 0;
            imgGhost.fillAmount = 1f;
            imgGhost.color = new Color(0.95f, 0.38f, 0.12f, 0.95f);
            imgGhost.raycastTarget = false;
            barraVidaGhost = imgGhost;

            // Relleno Frontal
            GameObject goBarraFill = new GameObject("BarraVida_Fill", typeof(RectTransform));
            goBarraFill.transform.SetParent(goBarraBg.transform, false);
            RectTransform rtFill = goBarraFill.GetComponent<RectTransform>();
            rtFill.anchorMin = Vector2.zero;
            rtFill.anchorMax = Vector2.one;
            rtFill.sizeDelta = Vector2.zero;
            Image imgFill = goBarraFill.AddComponent<Image>();
            imgFill.sprite = sprSolido;
            imgFill.type = Image.Type.Filled;
            imgFill.fillMethod = Image.FillMethod.Horizontal;
            imgFill.fillOrigin = 0;
            imgFill.fillAmount = 1f;
            imgFill.color = new Color(0.18f, 0.96f, 0.45f);
            imgFill.raycastTarget = false;
            barraVidaFill = imgFill;

            // Marco frontal metálico
            Sprite sprMarcoBarra = CargarSpriteUI("BarraVidaMarco");
            if (sprMarcoBarra != null)
            {
                GameObject goMarcoBarra = new GameObject("BarraVida_Marco", typeof(RectTransform));
                goMarcoBarra.transform.SetParent(goBarraBg.transform, false);
                RectTransform rtMb = goMarcoBarra.GetComponent<RectTransform>();
                rtMb.anchorMin = Vector2.zero;
                rtMb.anchorMax = Vector2.one;
                rtMb.offsetMin = new Vector2(-4, -4);
                rtMb.offsetMax = new Vector2(4, 4);
                Image imgMb = goMarcoBarra.AddComponent<Image>();
                imgMb.sprite = sprMarcoBarra;
                imgMb.type = Image.Type.Simple;
                imgMb.raycastTarget = false;
            }

            // Sección de Botellas
            GameObject goBotIcon = new GameObject("Icono_Botella", typeof(RectTransform));
            goBotIcon.transform.SetParent(hud, false);
            RectTransform rtBi = goBotIcon.GetComponent<RectTransform>();
            rtBi.anchorMin = new Vector2(0f, 1f);
            rtBi.anchorMax = new Vector2(0f, 1f);
            rtBi.pivot = new Vector2(0f, 1f);
            rtBi.anchoredPosition = new Vector2(20, -120);
            rtBi.sizeDelta = new Vector2(36, 36);
            Image imgBi = goBotIcon.AddComponent<Image>();
            imgBi.sprite = CargarSpriteUI("IconoBotellaHUD");
            imgBi.preserveAspect = true;
            imgBi.raycastTarget = false;
            iconoBotella = imgBi;

            GameObject goBot = CrearTextoHijo(hud, "Texto_Botellas", new Vector2(62, -122), new Vector2(400, 32), 22, FontStyle.Bold, new Color(0.20f, 1f, 0.90f), TextAnchor.MiddleLeft, "BOTELLAS: 0 / 15");
            textoBotellasLegacy = goBot.GetComponent<Text>();

            // Sección de Poder Activo
            GameObject goPoder = CrearTextoHijo(hud, "Texto_Poder", new Vector2(20, -156), new Vector2(460, 26), 16, FontStyle.Bold, new Color(0f, 0.95f, 1f), TextAnchor.MiddleLeft, "⚡ PODER: NORMAL (CHORRO DE AGUA)");
            textoPoderLegacy = goPoder.GetComponent<Text>();
        }
        else
        {
            // Reconectar componentes
            Transform tNivel = hud.Find("Texto_Nivel");
            if (tNivel != null) textoNivelLegacy = tNivel.GetComponent<Text>();

            Transform tBg = hud.Find("BarraVida_Fondo");
            if (tBg != null)
            {
                Transform tFill = tBg.Find("BarraVida_Fill");
                if (tFill != null) barraVidaFill = tFill.GetComponent<Image>();

                Transform tGhost = tBg.Find("BarraVida_Ghost");
                if (tGhost != null) barraVidaGhost = tGhost.GetComponent<Image>();
            }

            imagenesCorazones = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                Transform tH = hud.Find($"Corazon_{i}");
                if (tH != null) imagenesCorazones[i] = tH.GetComponent<Image>();
            }

            Transform tVida = hud.Find("Texto_Vida");
            if (tVida != null) textoVidaLegacy = tVida.GetComponent<Text>();

            Transform tBot = hud.Find("Texto_Botellas");
            if (tBot != null) textoBotellasLegacy = tBot.GetComponent<Text>();

            Transform tPod = hud.Find("Texto_Poder");
            if (tPod != null) textoPoderLegacy = tPod.GetComponent<Text>();
        }

        // 2. Botón de Pausa Circular Arcade (Esquina Superior Derecha)
        Transform tBtnPausa = ct.Find("Boton_Pausa_Pantalla");
        bool necesitaReconstruirBtnPausa = (tBtnPausa == null || tBtnPausa.Find("Texto_Boton") != null);
        if (necesitaReconstruirBtnPausa)
        {
            if (tBtnPausa != null) DestroyImmediate(tBtnPausa.gameObject);

            GameObject btnObj = new GameObject("Boton_Pausa_Pantalla", typeof(RectTransform));
            btnObj.transform.SetParent(ct, false);
            btnObj.transform.SetAsLastSibling();

            RectTransform rtBtn = btnObj.GetComponent<RectTransform>();
            rtBtn.anchorMin = new Vector2(1f, 1f);
            rtBtn.anchorMax = new Vector2(1f, 1f);
            rtBtn.pivot = new Vector2(1f, 1f);
            rtBtn.anchoredPosition = new Vector2(-25, -25);
            rtBtn.sizeDelta = new Vector2(80, 80);

            Image imgBtn = btnObj.AddComponent<Image>();
            imgBtn.sprite = CargarSpriteUI("BotonPausa");
            imgBtn.color = Color.white;
            imgBtn.preserveAspect = true;
            imgBtn.raycastTarget = true;

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
            cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            cb.selectedColor = Color.white;
            btn.colors = cb;
            btn.targetGraphic = imgBtn;
            btn.interactable = true;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(AlternarPausa);
#if UNITY_EDITOR
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, AlternarPausa);
#endif

            if (btnObj.GetComponent<BotonArcadeEfecto>() == null)
            {
                btnObj.AddComponent<BotonArcadeEfecto>();
            }

            botonPausaPantalla = btn;
        }
        else
        {
            tBtnPausa.SetAsLastSibling();
            botonPausaPantalla = tBtnPausa.GetComponent<Button>();
            if (botonPausaPantalla != null)
            {
                botonPausaPantalla.interactable = true;
                botonPausaPantalla.onClick.RemoveListener(AlternarPausa);
                botonPausaPantalla.onClick.AddListener(AlternarPausa);
#if UNITY_EDITOR
                if (botonPausaPantalla.onClick.GetPersistentEventCount() == 0)
                {
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(botonPausaPantalla.onClick, AlternarPausa);
                }
#endif
            }
            if (tBtnPausa.GetComponent<BotonArcadeEfecto>() == null)
            {
                tBtnPausa.gameObject.AddComponent<BotonArcadeEfecto>();
            }
        }

        // 3. Panel Modal de Pausa (Ventana Central con PanelMarco)
        Transform tPausa = ct.Find("Panel_Menu_Pausa");
        bool necesitaReconstruirPausa = (tPausa == null || tPausa.Find("Ventana_Central/Card_Progreso") == null);
        if (necesitaReconstruirPausa)
        {
            if (tPausa != null) DestroyImmediate(tPausa.gameObject);

            GameObject pPausa = new GameObject("Panel_Menu_Pausa", typeof(RectTransform));
            pPausa.transform.SetParent(ct, false);

            RectTransform rtPausa = pPausa.GetComponent<RectTransform>();
            rtPausa.anchorMin = Vector2.zero;
            rtPausa.anchorMax = Vector2.one;
            rtPausa.sizeDelta = Vector2.zero;

            Image imgOverlay = pPausa.AddComponent<Image>();
            imgOverlay.sprite = sprSolido;
            imgOverlay.color = new Color(0.01f, 0.02f, 0.05f, 0.88f);

            // Ventana modal central
            GameObject ventana = new GameObject("Ventana_Central", typeof(RectTransform));
            ventana.transform.SetParent(pPausa.transform, false);
            RectTransform rtVent = ventana.GetComponent<RectTransform>();
            rtVent.anchorMin = new Vector2(0.5f, 0.5f);
            rtVent.anchorMax = new Vector2(0.5f, 0.5f);
            rtVent.pivot = new Vector2(0.5f, 0.5f);
            rtVent.anchoredPosition = Vector2.zero;
            rtVent.sizeDelta = new Vector2(580, 740);

            Image imgVent = ventana.AddComponent<Image>();
            imgVent.sprite = CargarSpriteUI("PanelMarco");
            imgVent.type = Image.Type.Simple;

            // Título y Subtítulo
            CrearTextoHijo(ventana.transform, "Titulo", new Vector2(0, 275), new Vector2(500, 48), 34, FontStyle.Bold, new Color(0.0f, 1f, 0.88f), TextAnchor.MiddleCenter, "⏸   JUEGO PAUSADO");
            CrearTextoHijo(ventana.transform, "Subtitulo", new Vector2(0, 235), new Vector2(500, 28), 16, FontStyle.Bold, new Color(0.85f, 0.88f, 0.92f), TextAnchor.MiddleCenter, "MISIÓN ECOLÓGICA HUANCAYO 2026", true);

            // Tarjeta de progreso actual
            GameObject cardProg = new GameObject("Card_Progreso", typeof(RectTransform));
            cardProg.transform.SetParent(ventana.transform, false);
            RectTransform rtCp = cardProg.GetComponent<RectTransform>();
            rtCp.anchorMin = new Vector2(0.5f, 0.5f);
            rtCp.anchorMax = new Vector2(0.5f, 0.5f);
            rtCp.pivot = new Vector2(0.5f, 0.5f);
            rtCp.anchoredPosition = new Vector2(0, 180);
            rtCp.sizeDelta = new Vector2(460, 48);
            Image imgCp = cardProg.AddComponent<Image>();
            imgCp.sprite = sprSolido;
            imgCp.color = new Color(0.03f, 0.08f, 0.14f, 0.85f);

            GameObject goProg = CrearTextoHijo(cardProg.transform, "Texto_Progreso", Vector2.zero, new Vector2(450, 40), 19, FontStyle.Bold, new Color(1f, 0.92f, 0.35f), TextAnchor.MiddleCenter, $"{ObtenerNombreNivelCompleto()}  |  ♻ {botellasRecolectadas}/{botellasRequeridas}");
            textoProgresoPausa = goProg.GetComponent<Text>();

            // Botones de acción arcade
            CrearBotonModal(ventana.transform, "Btn_Continuar", new Vector2(0, 110), new Vector2(390, 50), new Color(0.35f, 1f, 0.55f), "▶   CONTINUAR MISIÓN", ReanudarJuego);
            CrearBotonModal(ventana.transform, "Btn_Reiniciar", new Vector2(0, 50), new Vector2(390, 50), new Color(1f, 0.92f, 0.40f), "↺   REINICIAR PARQUE", ReiniciarNivelActual);
            CrearBotonModal(ventana.transform, "Btn_Guardar", new Vector2(0, -10), new Vector2(390, 50), new Color(0.40f, 0.90f, 1f), "💾   GUARDAR PARTIDA", GuardarPartida);
            CrearBotonModal(ventana.transform, "Btn_Cargar", new Vector2(0, -70), new Vector2(390, 50), new Color(0.40f, 0.90f, 1f), "⟳   CARGAR PARTIDA", CargarPartidaGuardada);
            CrearBotonModal(ventana.transform, "Btn_Menu", new Vector2(0, -130), new Vector2(390, 50), Color.white, "🏠   IR AL MENÚ PRINCIPAL", IrAlMenu);
            CrearBotonModal(ventana.transform, "Btn_Salir", new Vector2(0, -190), new Vector2(390, 50), new Color(1f, 0.45f, 0.45f), "⏏   SALIR DEL JUEGO", SalirDelJuego);

            // Feedback de guardado
            GameObject goFeedback = CrearTextoHijo(ventana.transform, "Texto_Feedback", new Vector2(0, -250), new Vector2(480, 32), 17, FontStyle.Bold, new Color(0.2f, 1f, 0.4f), TextAnchor.MiddleCenter, "", true);
            textoFeedbackPausa = goFeedback.GetComponent<Text>();

            panelPausa = pPausa;
            panelPausa.SetActive(false);
        }
        else
        {
            panelPausa = tPausa.gameObject;
            Transform tVentana = tPausa.Find("Ventana_Central");
            if (tVentana != null)
            {
                Transform tFeed = tVentana.Find("Texto_Feedback");
                if (tFeed != null) textoFeedbackPausa = tFeed.GetComponent<Text>();

                Transform tProg = tVentana.Find("Card_Progreso/Texto_Progreso");
                if (tProg != null) textoProgresoPausa = tProg.GetComponent<Text>();

                ReconectarBotonModal(tVentana.Find("Btn_Continuar"), ReanudarJuego);
                ReconectarBotonModal(tVentana.Find("Btn_Reiniciar"), ReiniciarNivelActual);
                ReconectarBotonModal(tVentana.Find("Btn_Guardar"), GuardarPartida);
                ReconectarBotonModal(tVentana.Find("Btn_Cargar"), CargarPartidaGuardada);
                ReconectarBotonModal(tVentana.Find("Btn_Menu"), IrAlMenu);
                ReconectarBotonModal(tVentana.Find("Btn_Salir"), SalirDelJuego);
            }
        }

        // 4. Panel de Avisos Flotantes (Centro Superior)
        Transform tAviso = ct.Find("Panel_Avisos_Flotante");
        if (tAviso != null)
        {
            DestroyImmediate(tAviso.gameObject);
            tAviso = null;
        }

        GameObject pAviso = new GameObject("Panel_Avisos_Flotante", typeof(RectTransform));
        pAviso.transform.SetParent(ct, false);

        RectTransform rtAviso = pAviso.GetComponent<RectTransform>();
        rtAviso.anchorMin = new Vector2(0.5f, 1f);
        rtAviso.anchorMax = new Vector2(0.5f, 1f);
        rtAviso.pivot = new Vector2(0.5f, 1f);
        rtAviso.anchoredPosition = new Vector2(90f, -22f);
        rtAviso.sizeDelta = new Vector2(700f, 96f);

        Image imgAviso = pAviso.AddComponent<Image>();
        imgAviso.sprite = sprSolido;
        imgAviso.color = new Color(0.02f, 0.05f, 0.09f, 0.95f);
        imgAviso.raycastTarget = false;

        // Borde Neón Superior (Cian)
        GameObject goBordeSup = new GameObject("Aviso_BordeSup", typeof(RectTransform));
        goBordeSup.transform.SetParent(pAviso.transform, false);
        RectTransform rtBs = goBordeSup.GetComponent<RectTransform>();
        rtBs.anchorMin = new Vector2(0f, 1f);
        rtBs.anchorMax = new Vector2(1f, 1f);
        rtBs.pivot = new Vector2(0.5f, 1f);
        rtBs.anchoredPosition = Vector2.zero;
        rtBs.sizeDelta = new Vector2(0f, 3f);
        Image imgBs = goBordeSup.AddComponent<Image>();
        imgBs.sprite = sprSolido;
        imgBs.color = new Color(0.0f, 1f, 0.88f, 1f);
        imgBs.raycastTarget = false;

        // Borde Neón Inferior
        GameObject goBordeInf = new GameObject("Aviso_BordeInf", typeof(RectTransform));
        goBordeInf.transform.SetParent(pAviso.transform, false);
        RectTransform rtBinf = goBordeInf.GetComponent<RectTransform>();
        rtBinf.anchorMin = new Vector2(0f, 0f);
        rtBinf.anchorMax = new Vector2(1f, 0f);
        rtBinf.pivot = new Vector2(0.5f, 0f);
        rtBinf.anchoredPosition = Vector2.zero;
        rtBinf.sizeDelta = new Vector2(0f, 2f);
        Image imgBinf = goBordeInf.AddComponent<Image>();
        imgBinf.sprite = sprSolido;
        imgBinf.color = new Color(0.0f, 0.8f, 0.7f, 0.6f);
        imgBinf.raycastTarget = false;

        // Adornos laterales dorados
        GameObject goAdornoIzq = new GameObject("Aviso_AdornoIzq", typeof(RectTransform));
        goAdornoIzq.transform.SetParent(pAviso.transform, false);
        RectTransform rtAi = goAdornoIzq.GetComponent<RectTransform>();
        rtAi.anchorMin = new Vector2(0f, 0.5f);
        rtAi.anchorMax = new Vector2(0f, 0.5f);
        rtAi.pivot = new Vector2(0f, 0.5f);
        rtAi.anchoredPosition = new Vector2(3f, 0f);
        rtAi.sizeDelta = new Vector2(6f, 84f);
        Image imgAi = goAdornoIzq.AddComponent<Image>();
        imgAi.sprite = sprSolido;
        imgAi.color = new Color(1f, 0.85f, 0.25f, 0.95f);
        imgAi.raycastTarget = false;

        GameObject goAdornoDer = new GameObject("Aviso_AdornoDer", typeof(RectTransform));
        goAdornoDer.transform.SetParent(pAviso.transform, false);
        RectTransform rtAd = goAdornoDer.GetComponent<RectTransform>();
        rtAd.anchorMin = new Vector2(1f, 0.5f);
        rtAd.anchorMax = new Vector2(1f, 0.5f);
        rtAd.pivot = new Vector2(1f, 0.5f);
        rtAd.anchoredPosition = new Vector2(-3f, 0f);
        rtAd.sizeDelta = new Vector2(6f, 84f);
        Image imgAd = goAdornoDer.AddComponent<Image>();
        imgAd.sprite = sprSolido;
        imgAd.color = new Color(1f, 0.85f, 0.25f, 0.95f);
        imgAd.raycastTarget = false;

        // Encabezado
        CrearTextoHijo(pAviso.transform, "Texto_Aviso_Encabezado", new Vector2(0f, 24f), new Vector2(660f, 26f), 15, FontStyle.Bold, new Color(0.1f, 1f, 0.88f), TextAnchor.MiddleCenter, "🌿   CARTEL ECOLÓGICO ODS 15 — HUANCAYO 2026", false);

        // Texto del Cartel
        GameObject goAvisoTxt = CrearTextoHijo(pAviso.transform, "Texto_Aviso", new Vector2(0f, -14f), new Vector2(660f, 50f), 14, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, "", true);
        textoAvisoLegacy = goAvisoTxt.GetComponent<Text>();
        textoAvisoLegacy.lineSpacing = 1.25f;

        panelAviso = pAviso;
        panelAviso.SetActive(false);

        ActualizarUI();
    }

    private GameObject CrearTextoHijo(Transform padre, string nombre, Vector2 pos, Vector2 tamano, int fontSize, FontStyle estilo, Color color, TextAnchor align, string contenido = "", bool esCuerpo = false)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        RectTransform rt = go.GetComponent<RectTransform>();

        if (pos == Vector2.zero && (padre.GetComponent<Button>() != null || padre.name.Contains("Panel_Avisos") || padre.name.Contains("Card_Progreso")))
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
        }
        else if (pos.x == 0 && (padre.name.Contains("Ventana_Central") || padre.name.Contains("Panel_Avisos")))
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = tamano;
        }
        else
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = tamano;
        }

        Text txt = go.AddComponent<Text>();
        txt.font = esCuerpo ? ObtenerFuenteCuerpo() : ObtenerFuenteTitulo();
        txt.fontSize = fontSize;
        txt.fontStyle = estilo;
        txt.color = color;
        txt.alignment = align;
        txt.text = contenido;
        txt.raycastTarget = false; // CRÍTICO: El texto no intercepta clicks

        var shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.95f);
        shadow.effectDistance = new Vector2(2, -2);

        return go;
    }

    private void CrearBotonModal(Transform padre, string nombre, Vector2 posCentro, Vector2 tamano, Color colorTexto, string texto, UnityEngine.Events.UnityAction accion)
    {
        GameObject btnObj = new GameObject(nombre, typeof(RectTransform));
        btnObj.transform.SetParent(padre, false);

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = posCentro;
        rt.sizeDelta = tamano;

        Image img = btnObj.AddComponent<Image>();
        Sprite sprNormal = CargarSpriteUI("BotonArcade_Normal");
        Sprite sprHover = CargarSpriteUI("BotonArcade_Hover");
        Sprite sprPressed = CargarSpriteUI("BotonArcade_Pressed");

        img.sprite = sprNormal;
        img.type = Image.Type.Simple;
        img.color = Color.white;

        Button btn = btnObj.AddComponent<Button>();
        btn.transition = Selectable.Transition.SpriteSwap;
        SpriteState ss = new SpriteState();
        ss.highlightedSprite = sprHover;
        ss.pressedSprite = sprPressed;
        ss.selectedSprite = sprHover;
        btn.spriteState = ss;
        btn.onClick.AddListener(accion);
#if UNITY_EDITOR
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, accion);
#endif

        if (btnObj.GetComponent<BotonArcadeEfecto>() == null)
        {
            btnObj.AddComponent<BotonArcadeEfecto>();
        }

        GameObject txtObj = new GameObject("Texto", typeof(RectTransform));
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform rtTxt = txtObj.GetComponent<RectTransform>();
        rtTxt.anchorMin = Vector2.zero;
        rtTxt.anchorMax = Vector2.one;
        rtTxt.sizeDelta = Vector2.zero;

        Text txt = txtObj.AddComponent<Text>();
        txt.font = ObtenerFuenteTitulo();
        txt.fontSize = 21;
        txt.fontStyle = FontStyle.Bold;
        txt.color = colorTexto;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.text = texto;
        txt.raycastTarget = false; // CRÍTICO: Permite que el click llegue al botón padre

        var shadow = txtObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.95f);
        shadow.effectDistance = new Vector2(2, -2);
    }

    private void ReconectarBotonModal(Transform tBtn, UnityEngine.Events.UnityAction accion)
    {
        if (tBtn == null) return;
        Button btn = tBtn.GetComponent<Button>();
        if (btn == null) return;
        btn.onClick.RemoveListener(accion);
        btn.onClick.AddListener(accion);
#if UNITY_EDITOR
        if (btn.onClick.GetPersistentEventCount() == 0)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, accion);
        }
#endif
    }

    private string ObtenerNombreParqueSimple()
    {
        int idx = SistemaProgreso.ObtenerIndiceNivelActual();
        if (idx >= 1 && idx <= SistemaProgreso.TOTAL_NIVELES)
            return SistemaProgreso.NOMBRES_NIVELES[idx];
        return SceneManager.GetActiveScene().name;
    }

    private string ObtenerNombreNivelCompleto()
    {
        int idx = SistemaProgreso.ObtenerIndiceNivelActual();
        if (idx >= 1 && idx <= SistemaProgreso.TOTAL_NIVELES)
            return $"NIVEL {idx}: {SistemaProgreso.NOMBRES_NIVELES[idx]}";
        return SceneManager.GetActiveScene().name;
    }

    public void IrAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void SalirDelJuego()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
