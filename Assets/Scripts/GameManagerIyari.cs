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
    [SerializeField] private TMP_Text textoPoderTMP;

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
    [SerializeField] private GameObject subpanelAudioPausa;

    [Header("UI - Misión")]
    [SerializeField] private GameObject panelBannerMision;
    [SerializeField] private Text textoMisionTitulo;
    [SerializeField] private TMP_Text textoMisionTituloTMP;
    [SerializeField] private Text textoMisionDesc;
    [SerializeField] private TMP_Text textoMisionDescTMP;

    [Header("UI - Gran Cartel ODS Ampliado")]
    [SerializeField] private GameObject panelCartelODS;
    [SerializeField] private Text textoCartelTitulo;
    [SerializeField] private Text textoCartelCuerpo;
    private CanvasGroup canvasGroupCartel;
    private Coroutine corrutinaCartel;

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
        if (s == null) s = Resources.Load<Sprite>(nombre);
        if (s == null)
        {
#if UNITY_EDITOR
            s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/" + nombre + ".png");
            if (s == null)
                s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/" + nombre + ".png");
            if (s == null)
            {
                var all = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/" + nombre + ".png");
                foreach (var a in all) { if (a is Sprite spr) { s = spr; break; } }
            }
#endif
        }
        return s;
    }

    private static Font fuenteTitulo;
    private static Font fuenteCuerpo;
    private static Font fuenteCartelTitulo;
    private static Font fuenteCartelCuerpo;
    private static Font fuenteCartelPie;

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

    public static Font ObtenerFuenteCartelTitulo()
    {
        if (fuenteCartelTitulo == null)
        {
            fuenteCartelTitulo = CargarFuenteEspecifica("PressStart2P-Regular");
            if (fuenteCartelTitulo == null) fuenteCartelTitulo = ObtenerFuenteTitulo();
        }
        return fuenteCartelTitulo;
    }

    public static Font ObtenerFuenteCartelCuerpo()
    {
        if (fuenteCartelCuerpo == null)
        {
            fuenteCartelCuerpo = CargarFuenteEspecifica("m5x7");
            if (fuenteCartelCuerpo == null) fuenteCartelCuerpo = ObtenerFuenteCuerpo();
        }
        return fuenteCartelCuerpo;
    }

    public static Font ObtenerFuenteCartelPie()
    {
        if (fuenteCartelPie == null)
        {
            fuenteCartelPie = CargarFuenteEspecifica("Tiny5-Regular");
            if (fuenteCartelPie == null) fuenteCartelPie = ObtenerFuenteCuerpo();
        }
        return fuenteCartelPie;
    }

    private static Font fuenteThaleahFat;

    public static Font ObtenerFuenteThaleahFat()
    {
        if (fuenteThaleahFat == null)
        {
            fuenteThaleahFat = CargarFuenteEspecifica("ThaleahFat");
            if (fuenteThaleahFat == null) fuenteThaleahFat = ObtenerFuenteTitulo();
        }
        return fuenteThaleahFat;
    }

    public static Font ObtenerFuenteM5x7()
    {
        return ObtenerFuenteCartelCuerpo();
    }

    private static Font CargarFuenteEspecifica(string nombre)
    {
        Font f = Resources.Load<Font>("Fonts/" + nombre);
        if (f != null) return f;
#if UNITY_EDITOR
        f = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/" + nombre + ".ttf");
        if (f != null) return f;
        f = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Fonts/" + nombre + ".ttf");
        if (f != null) return f;
#endif
        return null;
    }

    public static Font ObtenerFuenteUI()
    {
        return ObtenerFuenteTitulo();
    }

    private void Awake()
    {
        Instancia = this;
        AudioManager.AsegurarInstancia();
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
        AudioManager.Instancia?.ReproducirSFX("botella");
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

        // Flash rojo al recibir daño y SFX
        if (cambio < 0)
        {
            AudioManager.Instancia?.ReproducirSFX("danio");
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
                textoProgresoPausa.text = $"<b>{ObtenerNombreParqueSimple().ToUpper()}</b>\n<color=#00FFE0>♻ {botellasRecolectadas} / {botellasRequeridas} BOTELLAS</color>   •   <color=#FF4560>❤ {vidaActual} / {vidaMaxima} VIDAS</color>";
            }
            if (!estaPausado && subpanelAudioPausa != null)
            {
                subpanelAudioPausa.SetActive(false);
            }
        }
    }

    public void AlternarSubpanelAudioPausa()
    {
        if (subpanelAudioPausa != null)
        {
            subpanelAudioPausa.SetActive(!subpanelAudioPausa.activeSelf);
        }
    }

    public void ReanudarJuego()
    {
        tiempoUltimaPausa = Time.unscaledTime;
        frameUltimaPausa = Time.frameCount;
        estaPausado = false;
        Time.timeScale = 1f;
        if (subpanelAudioPausa != null) subpanelAudioPausa.SetActive(false);
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

        Color colBot = (pctBot >= 1f) ? new Color(0.2f, 1f, 0.5f) : new Color(0f, 1f, 1f); // #00FFFF Cian agua
        if (textoBotellasLegacy != null) textoBotellasLegacy.color = colBot;
        if (textoBotellasTMP != null) textoBotellasTMP.color = colBot;

        // 3. Nombre del Nivel
        string txtNivel = ObtenerNombreNivelCompleto();
        SetTexto(textoNivelLegacy, textoNivelTMP, txtNivel);
        Color colNivel = new Color(1f, 0.843f, 0f); // #FFD700 Amarillo dorado
        if (textoNivelLegacy != null) textoNivelLegacy.color = colNivel;
        if (textoNivelTMP != null) textoNivelTMP.color = colNivel;

        // 4. Banner de Misión
        if (textoMisionTitulo == null && textoMisionTituloTMP == null)
        {
            Canvas cM = Object.FindFirstObjectByType<Canvas>();
            if (cM != null)
            {
                Transform tTit = cM.transform.Find("HUD_Banner_Mision/Texto_Mision_Titulo");
                if (tTit != null)
                {
                    textoMisionTitulo = tTit.GetComponent<Text>();
                    textoMisionTituloTMP = tTit.GetComponent<TMP_Text>();
                }
            }
        }
        SetTexto(textoMisionTitulo, textoMisionTituloTMP, "OBJETIVO");
        Color colMisTit = new Color(1f, 0.843f, 0f); // #FFD700
        if (textoMisionTitulo != null) textoMisionTitulo.color = colMisTit;
        if (textoMisionTituloTMP != null) textoMisionTituloTMP.color = colMisTit;

        if (textoMisionDesc == null && textoMisionDescTMP == null)
        {
            Canvas cM = Object.FindFirstObjectByType<Canvas>();
            if (cM != null)
            {
                Transform tM = cM.transform.Find("HUD_Banner_Mision/Texto_Mision_Desc");
                if (tM != null)
                {
                    textoMisionDesc = tM.GetComponent<Text>();
                    textoMisionDescTMP = tM.GetComponent<TMP_Text>();
                }
            }
        }
        string descMision = (botellasRecolectadas >= botellasRequeridas)
            ? "¡OBJETIVO CUMPLIDO! Ve a la Estación de Reciclaje"
            : $"Recolecta {botellasRequeridas} botellas de plástico ({botellasRecolectadas}/{botellasRequeridas})";
        Color colMisDesc = (botellasRecolectadas >= botellasRequeridas)
            ? new Color(0.35f, 1f, 0.55f)
            : new Color(0.96f, 0.96f, 0.96f); // #F5F5F5 Blanco hueso

        SetTexto(textoMisionDesc, textoMisionDescTMP, descMision);
        if (textoMisionDesc != null) textoMisionDesc.color = colMisDesc;
        if (textoMisionDescTMP != null) textoMisionDescTMP.color = colMisDesc;

        // 5. Progreso en menú de pausa si está visible
        if (textoProgresoPausa != null)
        {
            textoProgresoPausa.text = $"<b>{ObtenerNombreParqueSimple().ToUpper()}</b>\n<color=#00FFE0>♻ {botellasRecolectadas} / {botellasRequeridas} BOTELLAS</color>   •   <color=#FF4560>❤ {vidaActual} / {vidaMaxima} VIDAS</color>";
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

    public static string LimpiarTextoParaPixelFont(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return "";
        var sb = new System.Text.StringBuilder();
        foreach (char c in texto)
        {
            if (char.IsSurrogate(c)) continue;
            if (c >= 0x2600 && c <= 0x27BF) continue;
            if (c == 0xFE0F || c == 0x200D) continue;
            sb.Append(c);
        }
        return sb.ToString().Trim();
    }

    public static string FormatearMensajeCartel(string mensaje, string titulo)
    {
        string limpio = LimpiarTextoParaPixelFont(mensaje);
        int idx = limpio.IndexOf(':');
        if (idx > 0 && idx < 35)
        {
            string prefijo = limpio.Substring(0, idx).Trim();
            if (!string.IsNullOrEmpty(titulo) && (titulo.ToUpper().Contains(prefijo.ToUpper()) || prefijo.ToUpper().Contains("BOSQUE DORADO") || prefijo.ToUpper().Contains("ALERTA") || prefijo.ToUpper().Contains("SABIDURÍA") || prefijo.ToUpper().Contains("PELIGRO") || prefijo.ToUpper().Contains("VIDA SILVESTRE") || prefijo.ToUpper().Contains("META ECOLÓGICA")))
            {
                limpio = limpio.Substring(idx + 1).Trim();
            }
        }
        return limpio;
    }

    public void MostrarCartelODS(string mensaje, string titulo = "")
    {
        if (corrutinaCartel != null) StopCoroutine(corrutinaCartel);
        corrutinaCartel = StartCoroutine(RutinaMostrarCartelODS(mensaje, titulo));
    }

    public void OcultarCartelODS()
    {
        if (corrutinaCartel != null) StopCoroutine(corrutinaCartel);
        corrutinaCartel = StartCoroutine(RutinaOcultarCartelODS());
    }

    private IEnumerator RutinaMostrarCartelODS(string mensaje, string titulo)
    {
        if (panelCartelODS == null)
        {
            MostrarMensajeAviso(mensaje);
            yield break;
        }

        panelCartelODS.SetActive(true);

        if (textoCartelTitulo != null)
        {
            string t = string.IsNullOrEmpty(titulo) ? "BOSQUE DORADO DE PACCHA (ODS 15)" : LimpiarTextoParaPixelFont(titulo).ToUpper();
            textoCartelTitulo.text = t;
        }

        if (textoCartelCuerpo != null)
        {
            textoCartelCuerpo.text = FormatearMensajeCartel(mensaje, titulo);
        }

        RectTransform rt = panelCartelODS.GetComponent<RectTransform>();
        if (canvasGroupCartel == null) canvasGroupCartel = panelCartelODS.GetComponent<CanvasGroup>();
        if (canvasGroupCartel == null) canvasGroupCartel = panelCartelODS.AddComponent<CanvasGroup>();

        // Animación de entrada fluida con escala pop (de 0.60f a 1.05f y asienta en 1.0f)
        float dur = 0.22f;
        float tAnim = 0f;
        while (tAnim < dur)
        {
            tAnim += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(tAnim / dur);
            float scale = Mathf.Lerp(0.60f, 1.0f, Mathf.Sin(p * Mathf.PI * 0.5f));
            if (p > 0.65f) scale = Mathf.Lerp(1.05f, 1.0f, (p - 0.65f) / 0.35f);
            if (rt != null) rt.localScale = new Vector3(scale, scale, 1f);
            canvasGroupCartel.alpha = Mathf.Lerp(0f, 1f, p);
            yield return null;
        }
        if (rt != null) rt.localScale = Vector3.one;
        canvasGroupCartel.alpha = 1f;
    }

    private IEnumerator RutinaOcultarCartelODS()
    {
        if (panelCartelODS == null || !panelCartelODS.activeInHierarchy) yield break;

        RectTransform rt = panelCartelODS.GetComponent<RectTransform>();
        if (canvasGroupCartel == null) canvasGroupCartel = panelCartelODS.GetComponent<CanvasGroup>();

        float dur = 0.18f;
        float tAnim = 0f;
        while (tAnim < dur)
        {
            tAnim += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(tAnim / dur);
            float scale = Mathf.Lerp(1.0f, 0.75f, p);
            if (rt != null) rt.localScale = new Vector3(scale, scale, 1f);
            if (canvasGroupCartel != null) canvasGroupCartel.alpha = 1f - p;
            yield return null;
        }

        panelCartelODS.SetActive(false);
        if (rt != null) rt.localScale = Vector3.one;
    }

    public void ActualizarPoderHUD(string nombrePoder, int municion)
    {
        if (textoPoderLegacy == null && textoPoderTMP == null)
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                Transform tP = canvas.transform.Find("HUD_SuperiorIzquierdo/Texto_Poder");
                if (tP != null)
                {
                    textoPoderLegacy = tP.GetComponent<Text>();
                    textoPoderTMP = tP.GetComponent<TMP_Text>();
                }
            }
        }

        string txt = "PODER: NORMAL";
        Color col = new Color(0.85f, 0.95f, 1f, 0.85f);

        if (nombrePoder == "TRIPLE")
        {
            txt = $"PODER: DISPARO TRIPLE (x{municion})";
            col = new Color(0f, 0.95f, 1f);
        }
        else if (nombrePoder == "LASER")
        {
            txt = $"PODER: LASER PURIFICADOR (x{municion})";
            col = new Color(0.2f, 0.6f, 1f);
        }
        else if (nombrePoder == "BOMBA")
        {
            txt = $"PODER: BOMBA HIDRAULICA (x{municion})";
            col = new Color(1f, 0.65f, 0.1f);
        }
        else if (nombrePoder == "TURBO")
        {
            txt = $"PODER: TURBO ACUATICO ({municion}s)";
            col = new Color(1f, 0.9f, 0.2f);
        }
        else if (nombrePoder == "ESCUDO")
        {
            txt = "PODER: ESCUDO ACUATICO (ACTIVO)";
            col = new Color(0.2f, 0.8f, 1f);
        }
        else
        {
            txt = "PODER: NORMAL";
            col = new Color(0.85f, 0.95f, 1f, 0.85f);
        }

        SetTexto(textoPoderLegacy, textoPoderTMP, txt);
        if (textoPoderLegacy != null) textoPoderLegacy.color = col;
        if (textoPoderTMP != null) textoPoderTMP.color = col;
    }

    public void CompletarNivel()
    {
        if (juegoTerminado) return;
        juegoTerminado = true;

        if (panelVictoria != null) panelVictoria.SetActive(true);
        AudioManager.Instancia?.ReproducirSFX("victoria");
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

        // 1. HUD Superior Izquierdo (Placa Andina: Nivel, Corazones, Barra de Vida, Botellas, Poder)
        Transform hud = ct.Find("HUD_SuperiorIzquierdo");
        bool necesitaReconstruirHUD = (hud == null || hud.Find("Corazon_0") == null || hud.GetComponent<Image>()?.sprite?.name != "HUD_Panel_Jugador");
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
            rtHUD.anchoredPosition = new Vector2(25f, -34f);
            rtHUD.sizeDelta = new Vector2(510f, 210f);

            Image imgHUD = hudObj.AddComponent<Image>();
            Sprite sprPlaca = CargarSpriteUI("HUD_Panel_Jugador");
            if (sprPlaca != null)
            {
                imgHUD.sprite = sprPlaca;
                imgHUD.type = Image.Type.Sliced;
            }
            else
            {
                imgHUD.sprite = sprSolido;
                imgHUD.color = new Color(0.04f, 0.07f, 0.12f, 0.94f);
            }
            imgHUD.raycastTarget = false;

            // Encabezado Nombre de Nivel (Flotante arriba del marco con contorno sólido)
            GameObject goNivel = CrearTextoHijo(hud, "Texto_Nivel", new Vector2(8f, 6f), new Vector2(460f, 26f), 22, FontStyle.Normal, new Color(1f, 0.843f, 0f), TextAnchor.MiddleLeft, ObtenerNombreNivelCompleto(), false, ObtenerFuenteThaleahFat(), true);
            textoNivelLegacy = goNivel.GetComponent<Text>();
            RectTransform rtNiv = goNivel.GetComponent<RectTransform>();
            rtNiv.anchorMin = new Vector2(0f, 1f);
            rtNiv.anchorMax = new Vector2(0f, 1f);
            rtNiv.pivot = new Vector2(0f, 0f);
            rtNiv.anchoredPosition = new Vector2(8f, 6f);

            // Fila 1 Cavidad de Pizarra: Corazones y Texto Vida
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
                rtH.anchoredPosition = new Vector2(146f + i * 38f, -56f);
                rtH.sizeDelta = new Vector2(32f, 32f);

                Image imgH = heartObj.AddComponent<Image>();
                imgH.sprite = sprCorazonLleno;
                imgH.preserveAspect = true;
                imgH.raycastTarget = false;
                imagenesCorazones[i] = imgH;
            }

            // Texto Vida (ThaleahFat, Blanco hueso, Outline negro 2px)
            GameObject goVidaTxt = CrearTextoHijo(hud, "Texto_Vida", new Vector2(266f, -59f), new Vector2(190f, 26f), 18, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft, $"VIDA: {vidaActual} / {vidaMaxima}", false, ObtenerFuenteThaleahFat(), true);
            textoVidaLegacy = goVidaTxt.GetComponent<Text>();

            // Fila 2 Cavidad de Pizarra: Barra de Vida con Marco Metálico Arcade
            GameObject goBarraBg = new GameObject("BarraVida_Fondo", typeof(RectTransform));
            goBarraBg.transform.SetParent(hud, false);
            RectTransform rtBg = goBarraBg.GetComponent<RectTransform>();
            rtBg.anchorMin = new Vector2(0f, 1f);
            rtBg.anchorMax = new Vector2(0f, 1f);
            rtBg.pivot = new Vector2(0f, 1f);
            rtBg.anchoredPosition = new Vector2(146f, -98f);
            rtBg.sizeDelta = new Vector2(320f, 20f);

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

            // Fila 3 Cavidad de Pizarra: Sección de Botellas (Izq) y Poder Activo (Der)
            GameObject goBotIcon = new GameObject("Icono_Botella", typeof(RectTransform));
            goBotIcon.transform.SetParent(hud, false);
            RectTransform rtBi = goBotIcon.GetComponent<RectTransform>();
            rtBi.anchorMin = new Vector2(0f, 1f);
            rtBi.anchorMax = new Vector2(0f, 1f);
            rtBi.pivot = new Vector2(0f, 1f);
            rtBi.anchoredPosition = new Vector2(146f, -136f);
            rtBi.sizeDelta = new Vector2(28f, 28f);
            Image imgBi = goBotIcon.AddComponent<Image>();
            imgBi.sprite = CargarSpriteUI("IconoBotellaHUD");
            imgBi.preserveAspect = true;
            imgBi.raycastTarget = false;
            iconoBotella = imgBi;

            GameObject goBot = CrearTextoHijo(hud, "Texto_Botellas", new Vector2(178f, -138f), new Vector2(150f, 24f), 19, FontStyle.Normal, new Color(0f, 1f, 1f), TextAnchor.MiddleLeft, $"BOTELLAS: {botellasRecolectadas} / {botellasRequeridas}", false, ObtenerFuenteThaleahFat(), true);
            textoBotellasLegacy = goBot.GetComponent<Text>();

            GameObject goPoder = CrearTextoHijo(hud, "Texto_Poder", new Vector2(335f, -138f), new Vector2(155f, 24f), 18, FontStyle.Normal, new Color(0f, 0.95f, 1f), TextAnchor.MiddleLeft, "PODER: NORMAL", false, ObtenerFuenteM5x7(), true);
            textoPoderLegacy = goPoder.GetComponent<Text>();
        }
        else
        {
            // Reconectar componentes
            Transform tNivel = hud.Find("Texto_Nivel");
            if (tNivel != null)
            {
                textoNivelLegacy = tNivel.GetComponent<Text>();
                textoNivelTMP = tNivel.GetComponent<TMP_Text>();
            }

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
            if (tVida != null)
            {
                textoVidaLegacy = tVida.GetComponent<Text>();
                textoVidaTMP = tVida.GetComponent<TMP_Text>();
            }

            Transform tBot = hud.Find("Texto_Botellas");
            if (tBot != null)
            {
                textoBotellasLegacy = tBot.GetComponent<Text>();
                textoBotellasTMP = tBot.GetComponent<TMP_Text>();
            }

            Transform tPod = hud.Find("Texto_Poder");
            if (tPod != null)
            {
                textoPoderLegacy = tPod.GetComponent<Text>();
                textoPoderTMP = tPod.GetComponent<TMP_Text>();
            }
        }

        // 2. HUD Banner de Misión (Centro Superior - Top-Center Anchor) - 640 x 94
        Transform tMision = ct.Find("HUD_Banner_Mision");
        bool necesitaReconstruirMision = (tMision == null || tMision.Find("Texto_Mision_Titulo") == null || tMision.GetComponent<Image>()?.sprite?.name != "HUD_Banner_Mision");
        if (necesitaReconstruirMision)
        {
            if (tMision != null) DestroyImmediate(tMision.gameObject);

            GameObject misObj = new GameObject("HUD_Banner_Mision", typeof(RectTransform));
            misObj.transform.SetParent(ct, false);
            tMision = misObj.transform;

            RectTransform rtMis = misObj.GetComponent<RectTransform>();
            rtMis.anchorMin = new Vector2(0.5f, 1f);
            rtMis.anchorMax = new Vector2(0.5f, 1f);
            rtMis.pivot = new Vector2(0.5f, 1f);
            rtMis.anchoredPosition = new Vector2(0f, -25f);
            rtMis.sizeDelta = new Vector2(640f, 94f);

            Image imgMis = misObj.AddComponent<Image>();
            Sprite sprMis = CargarSpriteUI("HUD_Banner_Mision");
            if (sprMis != null)
            {
                imgMis.sprite = sprMis;
                imgMis.type = Image.Type.Sliced;
            }
            else
            {
                imgMis.sprite = sprSolido;
                imgMis.color = new Color(0.04f, 0.07f, 0.12f, 0.94f);
            }
            imgMis.raycastTarget = false;

            // Título Misión: ThaleahFat, 15pt, #FFD700 (Amarillo dorado), outline negro 2px
            GameObject goMisTit = CrearTextoHijo(tMision, "Texto_Mision_Titulo", new Vector2(0f, -32f), new Vector2(500f, 20f), 15, FontStyle.Normal, new Color(1f, 0.843f, 0f), TextAnchor.MiddleCenter, "OBJETIVO ODS 15", false, ObtenerFuenteThaleahFat(), true);
            textoMisionTitulo = goMisTit.GetComponent<Text>();

            // Descripción Misión: m5x7, 23pt, #F5F5F5 (Blanco hueso), outline negro 2px
            GameObject goMisDesc = CrearTextoHijo(tMision, "Texto_Mision_Desc", new Vector2(0f, -54f), new Vector2(580f, 26f), 23, FontStyle.Normal, new Color(0.96f, 0.96f, 0.96f), TextAnchor.MiddleCenter, $"Recolecta {botellasRequeridas} botellas de plástico ({botellasRecolectadas}/{botellasRequeridas})", true, ObtenerFuenteM5x7(), true);
            textoMisionDesc = goMisDesc.GetComponent<Text>();

            panelBannerMision = misObj;
        }
        else
        {
            panelBannerMision = tMision.gameObject;
            Transform tTit = tMision.Find("Texto_Mision_Titulo");
            if (tTit != null)
            {
                textoMisionTitulo = tTit.GetComponent<Text>();
                textoMisionTituloTMP = tTit.GetComponent<TMP_Text>();
            }
            Transform tDesc = tMision.Find("Texto_Mision_Desc");
            if (tDesc != null)
            {
                textoMisionDesc = tDesc.GetComponent<Text>();
                textoMisionDescTMP = tDesc.GetComponent<TMP_Text>();
            }
        }

        // 3. Botón de Pausa Rústico de Piedra y Oro (Esquina Superior Derecha - Top-Right Anchor)
        Transform tBtnPausa = ct.Find("Boton_Pausa_Pantalla");
        bool necesitaReconstruirBtnPausa = (tBtnPausa == null || tBtnPausa.Find("Texto_Boton") != null || tBtnPausa.GetComponent<Image>()?.sprite?.name != "HUD_Boton_Pausa");
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
            rtBtn.anchoredPosition = new Vector2(-25f, -25f);
            rtBtn.sizeDelta = new Vector2(76f, 76f);

            Image imgBtn = btnObj.AddComponent<Image>();
            Sprite sprBotonPausa = CargarSpriteUI("HUD_Boton_Pausa");
            if (sprBotonPausa == null) sprBotonPausa = CargarSpriteUI("BotonPausa");
            imgBtn.sprite = sprBotonPausa;
            imgBtn.color = Color.white;
            imgBtn.preserveAspect = true;
            imgBtn.raycastTarget = true;

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.25f, 1.25f, 1.25f, 1f); // Más brillante al pasar cursor
            cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
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

            // Tarjeta de progreso actual en tiempo real
            GameObject cardProg = new GameObject("Card_Progreso", typeof(RectTransform));
            cardProg.transform.SetParent(ventana.transform, false);
            RectTransform rtCp = cardProg.GetComponent<RectTransform>();
            rtCp.anchorMin = new Vector2(0.5f, 0.5f);
            rtCp.anchorMax = new Vector2(0.5f, 0.5f);
            rtCp.pivot = new Vector2(0.5f, 0.5f);
            rtCp.anchoredPosition = new Vector2(0, 180);
            rtCp.sizeDelta = new Vector2(480, 56);
            Image imgCp = cardProg.AddComponent<Image>();
            imgCp.sprite = sprSolido;
            imgCp.color = new Color(0.03f, 0.08f, 0.14f, 0.88f);

            GameObject goProg = CrearTextoHijo(cardProg.transform, "Texto_Progreso", Vector2.zero, new Vector2(470, 48), 17, FontStyle.Bold, new Color(1f, 0.92f, 0.35f), TextAnchor.MiddleCenter, $"<b>{ObtenerNombreParqueSimple().ToUpper()}</b>\n<color=#00FFE0>♻ {botellasRecolectadas} / {botellasRequeridas} BOTELLAS</color>   •   <color=#FF4560>❤ {vidaActual} / {vidaMaxima} VIDAS</color>");
            textoProgresoPausa = goProg.GetComponent<Text>();

            // Botones de acción arcade requeridos
            CrearBotonModal(ventana.transform, "Btn_Continuar", new Vector2(0, 105), new Vector2(410, 52), new Color(0.35f, 1f, 0.55f), "▶   CONTINUAR MISIÓN", ReanudarJuego);
            CrearBotonModal(ventana.transform, "Btn_Reiniciar", new Vector2(0, 45), new Vector2(410, 52), new Color(1f, 0.92f, 0.40f), "↺   REINICIAR PARQUE", ReiniciarNivelActual);
            CrearBotonModal(ventana.transform, "Btn_Audio", new Vector2(0, -15), new Vector2(410, 52), new Color(0.20f, 0.95f, 1f), "🔊   AJUSTES DE AUDIO", AlternarSubpanelAudioPausa);
            CrearBotonModal(ventana.transform, "Btn_Menu", new Vector2(0, -75), new Vector2(410, 52), Color.white, "🏠   VOLVER AL MENÚ PRINCIPAL", IrAlMenu);
            CrearBotonModal(ventana.transform, "Btn_Guardar", new Vector2(0, -135), new Vector2(410, 52), new Color(0.40f, 0.90f, 1f), "💾   GUARDAR PARTIDA", GuardarPartida);

            // Subpanel de Audio en Pausa (Sliders Master, Música, SFX)
            GameObject pAudio = new GameObject("Subpanel_Audio_Pausa", typeof(RectTransform));
            pAudio.transform.SetParent(ventana.transform, false);
            RectTransform rtPa = pAudio.GetComponent<RectTransform>();
            rtPa.anchorMin = new Vector2(0.5f, 0.5f);
            rtPa.anchorMax = new Vector2(0.5f, 0.5f);
            rtPa.pivot = new Vector2(0.5f, 0.5f);
            rtPa.anchoredPosition = new Vector2(0, -20);
            rtPa.sizeDelta = new Vector2(490, 310);
            Image imgPa = pAudio.AddComponent<Image>();
            imgPa.sprite = sprSolido;
            imgPa.color = new Color(0.02f, 0.05f, 0.10f, 0.96f);

            CrearTextoHijo(pAudio.transform, "TituloAudio", new Vector2(0, 120), new Vector2(460, 32), 19, FontStyle.Bold, new Color(0.2f, 0.95f, 1f), TextAnchor.MiddleCenter, "🔊   AJUSTES DE VOLUMEN");

            // Slider Master
            CrearFilaAudioPausa(pAudio.transform, "Master", new Vector2(0, 70), "MASTER", AudioManager.Instancia != null ? AudioManager.Instancia.VolumenMaster : 1f, (v) => { if (AudioManager.Instancia != null) AudioManager.Instancia.SetMasterVolume(v); });
            // Slider Música
            CrearFilaAudioPausa(pAudio.transform, "Musica", new Vector2(0, 15), "MÚSICA", AudioManager.Instancia != null ? AudioManager.Instancia.VolumenMusica : 0.8f, (v) => { if (AudioManager.Instancia != null) AudioManager.Instancia.SetMusicVolume(v); });
            // Slider SFX
            CrearFilaAudioPausa(pAudio.transform, "SFX", new Vector2(0, -40), "SFX", AudioManager.Instancia != null ? AudioManager.Instancia.VolumenSFX : 0.9f, (v) => { if (AudioManager.Instancia != null) AudioManager.Instancia.SetSFXVolume(v); });

            CrearBotonModal(pAudio.transform, "Btn_CerrarAudio", new Vector2(0, -105), new Vector2(260, 44), new Color(0.35f, 1f, 0.55f), "✔   CERRAR", AlternarSubpanelAudioPausa);
            pAudio.SetActive(false);
            subpanelAudioPausa = pAudio;

            // Feedback de guardado
            GameObject goFeedback = CrearTextoHijo(ventana.transform, "Texto_Feedback", new Vector2(0, -230), new Vector2(480, 32), 17, FontStyle.Bold, new Color(0.2f, 1f, 0.4f), TextAnchor.MiddleCenter, "", true);
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

                Transform tSubAudio = tVentana.Find("Subpanel_Audio_Pausa");
                if (tSubAudio != null) subpanelAudioPausa = tSubAudio.gameObject;

                ReconectarBotonModal(tVentana.Find("Btn_Continuar"), ReanudarJuego);
                ReconectarBotonModal(tVentana.Find("Btn_Reiniciar"), ReiniciarNivelActual);
                ReconectarBotonModal(tVentana.Find("Btn_Audio"), AlternarSubpanelAudioPausa);
                ReconectarBotonModal(tVentana.Find("Btn_Menu"), IrAlMenu);
                ReconectarBotonModal(tVentana.Find("Btn_Guardar"), GuardarPartida);
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
        imgBs.color = new Color(1f, 0.85f, 0.25f, 1f); // Dorado Neón
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
        imgBinf.color = new Color(0.20f, 0.85f, 0.45f, 0.80f); // Esmeralda Neón
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
        CrearTextoHijo(pAviso.transform, "Texto_Aviso_Encabezado", new Vector2(0f, 24f), new Vector2(660f, 26f), 15, FontStyle.Bold, new Color(1f, 0.85f, 0.28f), TextAnchor.MiddleCenter, "🌿   CARTEL ECOLÓGICO ODS 15 — HUANCAYO 2026", false);

        // Texto del Cartel
        GameObject goAvisoTxt = CrearTextoHijo(pAviso.transform, "Texto_Aviso", new Vector2(0f, -14f), new Vector2(660f, 50f), 14, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, "", true);
        textoAvisoLegacy = goAvisoTxt.GetComponent<Text>();
        textoAvisoLegacy.lineSpacing = 1.25f;

        panelAviso = pAviso;
        panelAviso.SetActive(false);

        // 5. Gran Panel Cartel ODS Ampliado e Interactivo (Centro Inferior)
        Transform tCartel = ct.Find("Panel_Cartel_ODS");
        if (tCartel != null)
        {
            DestroyImmediate(tCartel.gameObject);
            tCartel = null;
        }

        GameObject pCartel = new GameObject("Panel_Cartel_ODS", typeof(RectTransform));
        pCartel.transform.SetParent(ct, false);

        RectTransform rtCartel = pCartel.GetComponent<RectTransform>();
        rtCartel.anchorMin = new Vector2(0.5f, 0f);
        rtCartel.anchorMax = new Vector2(0.5f, 0f);
        rtCartel.pivot = new Vector2(0.5f, 0f);
        rtCartel.anchoredPosition = new Vector2(0f, 25f);
        rtCartel.sizeDelta = new Vector2(1040f, 364f);

        Image imgCartelBg = pCartel.AddComponent<Image>();
        Sprite sprCuadro = CargarSpriteUI("CuadroMensajes-Photoroom");
        if (sprCuadro != null)
        {
            imgCartelBg.sprite = sprCuadro;
            imgCartelBg.type = Image.Type.Simple;
            imgCartelBg.preserveAspect = true;
            imgCartelBg.color = Color.white;
        }
        else
        {
            imgCartelBg.sprite = sprSolido;
            imgCartelBg.color = new Color(0.02f, 0.06f, 0.12f, 0.96f);
        }
        imgCartelBg.raycastTarget = false;

        // Encabezado del Cartel (Press Start 2P) - Centrado con amplio margen respecto a la moldura superior
        GameObject goTit = CrearTextoHijo(pCartel.transform, "Texto_Cartel_Titulo", new Vector2(0f, -90f), new Vector2(720f, 24f), 12, FontStyle.Normal, new Color(1f, 0.82f, 0.40f), TextAnchor.MiddleCenter, "BOSQUE DORADO DE PACCHA (ODS 15)", false);
        textoCartelTitulo = goTit.GetComponent<Text>();
        textoCartelTitulo.font = ObtenerFuenteCartelTitulo();

        // Cuerpo del Cartel (m5x7 por Daniel Linssen) - Centrado armónico y márgenes equilibrados
        GameObject goCuerpo = CrearTextoHijo(pCartel.transform, "Texto_Cartel_Cuerpo", new Vector2(0f, -124f), new Vector2(720f, 104f), 26, FontStyle.Normal, new Color(0.95f, 0.96f, 1f), TextAnchor.MiddleCenter, "", true);
        textoCartelCuerpo = goCuerpo.GetComponent<Text>();
        textoCartelCuerpo.font = ObtenerFuenteCartelCuerpo();
        textoCartelCuerpo.lineSpacing = 1.18f;

        // Pie de interacción (Tiny5) - Tono dorado cálido arcade (sin color turquesa)
        GameObject goPie = CrearTextoHijo(pCartel.transform, "Texto_Cartel_Pie", new Vector2(0f, -238f), new Vector2(720f, 20f), 16, FontStyle.Normal, new Color(1f, 0.84f, 0.40f), TextAnchor.MiddleCenter, "[ ALÉJATE DEL CARTEL PARA CONTINUAR ]", false);
        Text txtPie = goPie.GetComponent<Text>();
        txtPie.font = ObtenerFuenteCartelPie();

        panelCartelODS = pCartel;
        canvasGroupCartel = pCartel.AddComponent<CanvasGroup>();
        panelCartelODS.SetActive(false);

        ActualizarUI();
    }

    private GameObject CrearTextoHijo(Transform padre, string nombre, Vector2 pos, Vector2 tamano, int fontSize, FontStyle estilo, Color color, TextAnchor align, string contenido = "", bool esCuerpo = false, Font fuenteCustom = null, bool usarOutline = false)
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
        else if (padre.name.Contains("Panel_Cartel_ODS") || padre.name.Contains("HUD_Banner_Mision"))
        {
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = tamano;
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
        if (fuenteCustom != null)
            txt.font = fuenteCustom;
        else
            txt.font = esCuerpo ? ObtenerFuenteCuerpo() : ObtenerFuenteTitulo();
        txt.fontSize = fontSize;
        txt.fontStyle = estilo;
        txt.color = color;
        txt.alignment = align;
        txt.text = contenido;
        txt.horizontalOverflow = HorizontalWrapMode.Wrap;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
        txt.raycastTarget = false; // CRÍTICO: El texto no intercepta clicks

        if (usarOutline)
        {
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 1f);
            outline.effectDistance = new Vector2(2f, -2f);
        }
        else
        {
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.95f);
            shadow.effectDistance = new Vector2(2, -2);
        }

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

    private void CrearFilaAudioPausa(Transform padre, string nombre, Vector2 posCentro, string etiqueta, float valorDefecto, UnityEngine.Events.UnityAction<float> alCambiar)
    {
        GameObject filaObj = new GameObject("Fila_" + nombre, typeof(RectTransform));
        filaObj.transform.SetParent(padre, false);
        RectTransform rtFila = filaObj.GetComponent<RectTransform>();
        rtFila.anchorMin = new Vector2(0.5f, 0.5f);
        rtFila.anchorMax = new Vector2(0.5f, 0.5f);
        rtFila.pivot = new Vector2(0.5f, 0.5f);
        rtFila.anchoredPosition = posCentro;
        rtFila.sizeDelta = new Vector2(450, 44);

        GameObject lblObj = new GameObject("Label", typeof(RectTransform));
        lblObj.transform.SetParent(filaObj.transform, false);
        RectTransform rtLbl = lblObj.GetComponent<RectTransform>();
        rtLbl.anchorMin = new Vector2(0f, 0.5f);
        rtLbl.anchorMax = new Vector2(0f, 0.5f);
        rtLbl.pivot = new Vector2(0f, 0.5f);
        rtLbl.anchoredPosition = new Vector2(5, 0);
        rtLbl.sizeDelta = new Vector2(100, 36);

        Text txtLbl = lblObj.AddComponent<Text>();
        txtLbl.font = ObtenerFuenteTitulo();
        txtLbl.fontSize = 15;
        txtLbl.fontStyle = FontStyle.Bold;
        txtLbl.color = Color.white;
        txtLbl.alignment = TextAnchor.MiddleLeft;
        txtLbl.text = etiqueta;
        txtLbl.raycastTarget = false;

        GameObject sliderObj = new GameObject("Slider", typeof(RectTransform));
        sliderObj.transform.SetParent(filaObj.transform, false);
        RectTransform rtSlider = sliderObj.GetComponent<RectTransform>();
        rtSlider.anchorMin = new Vector2(0f, 0.5f);
        rtSlider.anchorMax = new Vector2(0f, 0.5f);
        rtSlider.pivot = new Vector2(0f, 0.5f);
        rtSlider.anchoredPosition = new Vector2(115, 0);
        rtSlider.sizeDelta = new Vector2(250, 24);

        Slider slider = sliderObj.AddComponent<Slider>();

        GameObject bg = new GameObject("Background", typeof(RectTransform));
        bg.transform.SetParent(sliderObj.transform, false);
        RectTransform rtBg = bg.GetComponent<RectTransform>();
        rtBg.anchorMin = new Vector2(0, 0.35f);
        rtBg.anchorMax = new Vector2(1, 0.65f);
        rtBg.sizeDelta = Vector2.zero;
        Image imgBg = bg.AddComponent<Image>();
        imgBg.sprite = ObtenerSpriteBlanco();
        imgBg.color = new Color(0.1f, 0.15f, 0.25f, 0.95f);

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform rtFa = fillArea.GetComponent<RectTransform>();
        rtFa.anchorMin = new Vector2(0, 0.35f);
        rtFa.anchorMax = new Vector2(1, 0.65f);
        rtFa.sizeDelta = new Vector2(-10, 0);

        GameObject fill = new GameObject("Fill", typeof(RectTransform));
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform rtFill = fill.GetComponent<RectTransform>();
        rtFill.sizeDelta = Vector2.zero;
        Image imgFill = fill.AddComponent<Image>();
        imgFill.sprite = ObtenerSpriteBlanco();
        imgFill.color = new Color(0f, 1f, 0.88f, 1f);

        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform rtHa = handleArea.GetComponent<RectTransform>();
        rtHa.anchorMin = Vector2.zero;
        rtHa.anchorMax = Vector2.one;
        rtHa.sizeDelta = new Vector2(-20, 0);

        GameObject handle = new GameObject("Handle", typeof(RectTransform));
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform rtHandle = handle.GetComponent<RectTransform>();
        rtHandle.sizeDelta = new Vector2(20, 24);
        Image imgHandle = handle.AddComponent<Image>();
        imgHandle.sprite = ObtenerSpriteBlanco();
        imgHandle.color = new Color(1f, 0.9f, 0.35f, 1f);

        slider.fillRect = rtFill;
        slider.handleRect = rtHandle;
        slider.targetGraphic = imgHandle;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = valorDefecto;

        GameObject valObj = new GameObject("Valor", typeof(RectTransform));
        valObj.transform.SetParent(filaObj.transform, false);
        RectTransform rtVal = valObj.GetComponent<RectTransform>();
        rtVal.anchorMin = new Vector2(1f, 0.5f);
        rtVal.anchorMax = new Vector2(1f, 0.5f);
        rtVal.pivot = new Vector2(1f, 0.5f);
        rtVal.anchoredPosition = new Vector2(-5, 0);
        rtVal.sizeDelta = new Vector2(70, 36);

        Text txtVal = valObj.AddComponent<Text>();
        txtVal.font = ObtenerFuenteTitulo();
        txtVal.fontSize = 15;
        txtVal.fontStyle = FontStyle.Bold;
        txtVal.color = new Color(0f, 1f, 0.88f);
        txtVal.alignment = TextAnchor.MiddleRight;
        txtVal.text = $"{Mathf.RoundToInt(valorDefecto * 100f)}%";
        txtVal.raycastTarget = false;

        slider.onValueChanged.AddListener((v) =>
        {
            txtVal.text = $"{Mathf.RoundToInt(v * 100f)}%";
            alCambiar?.Invoke(v);
        });
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
