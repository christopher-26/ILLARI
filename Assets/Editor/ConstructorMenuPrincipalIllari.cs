using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Constructor de escena para 'MenuPrincipal.unity' de Iyari (Huancayo 2026).
/// Implementa una interfaz arcade retro moderna, limpia y receptiva con:
/// 1. Canvas Scaler configurado a Scale With Screen Size (1920x1080, match 0.5).
/// 2. Botonera principal con los 6 módulos requeridos:
///    - Iniciar Aventura (Nivel 1)
///    - Selector de Misiones / Parques (9 niveles)
///    - Registro ODS 12 & 15 (Datos ecológicos reales de Huancayo)
///    - Ajustes de Sonido (Sliders Master, Música y SFX)
///    - Guía de Controles (Layout de teclas arcade)
///    - Salir del Juego
/// 3. Eventos persistentes serializados con UnityEventTools.
/// 4. Vinculación completa con MenuPrincipalIllari y AudioManager.
/// </summary>
public static class ConstructorMenuPrincipalIllari
{
    private static Font fuenteTitulo;
    private static Font fuenteCuerpo;
    private static Sprite sprBlanco;

    private static Font ObtenerFuenteTitulo()
    {
        if (fuenteTitulo == null)
            fuenteTitulo = Resources.Load<Font>("Fonts/Roboto-Bold");
        if (fuenteTitulo == null)
            fuenteTitulo = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return fuenteTitulo;
    }

    private static Font ObtenerFuenteCuerpo()
    {
        if (fuenteCuerpo == null)
            fuenteCuerpo = Resources.Load<Font>("Fonts/Roboto-Bold");
        if (fuenteCuerpo == null)
            fuenteCuerpo = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return fuenteCuerpo;
    }

    private static Sprite ObtenerSprBlanco()
    {
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Blanco.png");
        if (s == null)
            s = Resources.Load<Sprite>("UI/Blanco");
        return s;
    }

    private static Sprite CargarSprite(string nombre)
    {
        Sprite s = Resources.Load<Sprite>("UI/" + nombre);
        if (s == null)
            s = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/" + nombre + ".png");
        return s;
    }

    [MenuItem("Illari/Construir Todo (Menú y 9 Niveles)")]
    public static void ConstruirTodo()
    {
        ConstruirEscenaMenu();
        GeneradorNivelesIllari.GenerarTodosLosNiveles();
    }

    [MenuItem("Illari/Construir Menú Principal Artesanal")]
    public static void ConstruirEscenaMenu()
    {
        string scenePath = "Assets/Scenes/MenuPrincipal.unity";
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // Limpiar objetos existentes de la escena
        GameObject[] roots = scene.GetRootGameObjects();
        foreach (var r in roots)
        {
            Object.DestroyImmediate(r);
        }

        Debug.Log("<color=#00FFE0><b>[MENU]</b> Construyendo Menú Principal Arcade de Iyari (Huancayo 2026)...</color>");

        // 1. MAIN CAMERA
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        camObj.transform.position = new Vector3(0, 0, -10);
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.035f, 0.055f, 0.095f, 1f); // Azul andino nocturno profundo
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 100f;
        camObj.AddComponent<AudioListener>();

        // 2. EVENT SYSTEM
        GameObject eventObj = new GameObject("EventSystem");
        var es = eventObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.sendNavigationEvents = true;
        es.pixelDragThreshold = 10;
        var inputModType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (inputModType != null)
        {
            eventObj.AddComponent(inputModType);
        }
        else
        {
            eventObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 3. AUDIO MANAGER CON BANDA SONORA DEL MENÚ Y SFX
        GeneradorNivelesIllari.ConfigurarSistemaAudio("Assets/Audio/Music/MenuPrincipal.mp3");

        // 4. CANVAS RAÍZ
        GameObject canvasObj = new GameObject("Canvas_Menu", typeof(RectTransform));
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
        Transform ct = canvasObj.transform;

        // Cargar sprites de UI
        Sprite sprPortada = CargarSprite("PortadaMenuIllari");
        Sprite sprLogo = CargarSprite("LogoIllari");
        Sprite sprMarco = CargarSprite("PanelMarco");
        // Cargar sprites de UI (priorizando el atlas Botones_Menu.png)
        Sprite sprBtnNormal = null;
        Sprite sprBtnHover = null;
        Sprite sprBtnPressed = null;

        var menuSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Botones_Menu.png");
        foreach (var obj in menuSprites)
        {
            if (obj is Sprite s)
            {
                if (s.name == "Botones_Menu_0") sprBtnNormal = s;
                else if (s.name == "Botones_Menu_1") sprBtnHover = s;
                else if (s.name == "Botones_Menu_2") sprBtnPressed = s;
            }
        }
        if (sprBtnNormal == null) sprBtnNormal = CargarSprite("BotonArcade_Normal");
        if (sprBtnHover == null) sprBtnHover = CargarSprite("BotonArcade_Hover");
        if (sprBtnPressed == null) sprBtnPressed = CargarSprite("BotonArcade_Pressed");

        // 5. FONDO PORTADA ILUSTRADA
        GameObject fondoObj = new GameObject("Fondo_Portada", typeof(RectTransform));
        fondoObj.transform.SetParent(ct, false);
        RectTransform rtFondo = fondoObj.GetComponent<RectTransform>();
        rtFondo.anchorMin = Vector2.zero;
        rtFondo.anchorMax = Vector2.one;
        rtFondo.sizeDelta = Vector2.zero;
        Image imgFondo = fondoObj.AddComponent<Image>();
        imgFondo.sprite = sprPortada;
        imgFondo.color = Color.white;
        imgFondo.raycastTarget = false;

        // 6. DEGRADADO / VIÑETA CINEMÁTICA
        GameObject vigObj = new GameObject("Vignette_Contraste", typeof(RectTransform));
        vigObj.transform.SetParent(ct, false);
        RectTransform rtVig = vigObj.GetComponent<RectTransform>();
        rtVig.anchorMin = Vector2.zero;
        rtVig.anchorMax = Vector2.one;
        rtVig.sizeDelta = Vector2.zero;
        Image imgVig = vigObj.AddComponent<Image>();
        imgVig.sprite = ObtenerSprBlanco();
        imgVig.color = new Color(0.02f, 0.05f, 0.08f, 0.35f);
        imgVig.raycastTarget = false;

        // 7. LOGOTIPO DORADO ILLARI
        GameObject logoObj = new GameObject("Logo_Illari", typeof(RectTransform));
        logoObj.transform.SetParent(ct, false);
        RectTransform rtLogo = logoObj.GetComponent<RectTransform>();
        rtLogo.anchorMin = new Vector2(0.5f, 1f);
        rtLogo.anchorMax = new Vector2(0.5f, 1f);
        rtLogo.pivot = new Vector2(0.5f, 1f);
        rtLogo.anchoredPosition = new Vector2(0, -35);
        rtLogo.sizeDelta = new Vector2(660, 310);
        Image imgLogo = logoObj.AddComponent<Image>();
        imgLogo.sprite = sprLogo;
        imgLogo.preserveAspect = true;
        imgLogo.raycastTarget = false;

        // 8. SUBTÍTULO ECO-ARCADE
        GameObject subObj = new GameObject("Texto_Slogan", typeof(RectTransform));
        subObj.transform.SetParent(ct, false);
        RectTransform rtSub = subObj.GetComponent<RectTransform>();
        rtSub.anchorMin = new Vector2(0.5f, 1f);
        rtSub.anchorMax = new Vector2(0.5f, 1f);
        rtSub.pivot = new Vector2(0.5f, 1f);
        rtSub.anchoredPosition = new Vector2(0, -365);
        rtSub.sizeDelta = new Vector2(1100, 48);
        Text txtSub = subObj.AddComponent<Text>();
        txtSub.font = ObtenerFuenteTitulo();
        txtSub.fontSize = 18;
        txtSub.fontStyle = FontStyle.Bold;
        txtSub.alignment = TextAnchor.MiddleCenter;
        txtSub.color = new Color(0.1f, 1f, 0.88f, 0.98f);
        txtSub.text = "🌿   PARQUES DE HUANCAYO 2026  •  MISIÓN ECOLÓGICA ODS 15: VIDA DE ECOSISTEMAS TERRESTRES   🌿";
        txtSub.raycastTarget = false;
        var shadowSub = subObj.AddComponent<Shadow>();
        shadowSub.effectColor = new Color(0f, 0f, 0f, 0.95f);
        shadowSub.effectDistance = new Vector2(2, -2);

        // 9. CONTENEDOR BOTONES PRINCIPALES (6 Módulos Arcade)
        GameObject contBotones = new GameObject("Contenedor_Botones_Principales", typeof(RectTransform));
        contBotones.transform.SetParent(ct, false);
        RectTransform rtCont = contBotones.GetComponent<RectTransform>();
        rtCont.anchorMin = new Vector2(0.5f, 0f);
        rtCont.anchorMax = new Vector2(0.5f, 0f);
        rtCont.pivot = new Vector2(0.5f, 0f);
        rtCont.anchoredPosition = new Vector2(0, 18);
        rtCont.sizeDelta = new Vector2(530, 480);

        Vector2 tamanoBoton = new Vector2(500, 72);

        // Botón 1: Iniciar Aventura
        Button btnJugar = CrearBotonArcade(contBotones.transform, "Boton_Jugar", new Vector2(0, 400), tamanoBoton,
            sprBtnNormal, sprBtnHover, sprBtnPressed, "▶   INICIAR AVENTURA", new Color(1f, 0.92f, 0.35f));

        // Botón 2: Selector de Misiones / Parques
        Button btnSeleccion = CrearBotonArcade(contBotones.transform, "Boton_SeleccionNiveles", new Vector2(0, 325), tamanoBoton,
            sprBtnNormal, sprBtnHover, sprBtnPressed, "📂   SELECTOR DE MISIONES", Color.white);

        // Botón 3: Registro ODS 15
        Button btnODS = CrearBotonArcade(contBotones.transform, "Boton_ODS", new Vector2(0, 250), tamanoBoton,
            sprBtnNormal, sprBtnHover, sprBtnPressed, "🌿   ODS 15: VIDA TERRESTRE", new Color(0.35f, 1f, 0.6f));

        // Botón 4: Ajustes de Sonido
        Button btnSonido = CrearBotonArcade(contBotones.transform, "Boton_AjustesSonido", new Vector2(0, 175), tamanoBoton,
            sprBtnNormal, sprBtnHover, sprBtnPressed, "🔊   AJUSTES DE SONIDO", new Color(0.2f, 0.95f, 1f));

        // Botón 5: Guía de Controles
        Button btnControles = CrearBotonArcade(contBotones.transform, "Boton_Controles", new Vector2(0, 100), tamanoBoton,
            sprBtnNormal, sprBtnHover, sprBtnPressed, "🎮   GUÍA DE CONTROLES", Color.white);

        // Botón 6: Salir del Juego
        Button btnSalir = CrearBotonArcade(contBotones.transform, "Boton_Salir", new Vector2(0, 25), tamanoBoton,
            sprBtnNormal, sprBtnHover, sprBtnPressed, "🚪   SALIR DEL JUEGO", new Color(1f, 0.45f, 0.45f));

        // 10. MODAL: SELECCIÓN DE NIVELES (Grid de 9 parques)
        GameObject panelSel = CrearModalBase(ct, "Panel_SeleccionNiveles", sprMarco, new Vector2(1180, 800));
        Transform ventanaSel = panelSel.transform.Find("Ventana");
        CrearTextoModal(ventanaSel, "Titulo", new Vector2(0, 310), new Vector2(900, 48), 24, FontStyle.Bold, new Color(1f, 0.88f, 0.30f), "SELECCIÓN DE MISIONES (PARQUES DE HUANCAYO 2026)");
        CrearTextoModal(ventanaSel, "Subtitulo", new Vector2(0, 268), new Vector2(900, 30), 15, FontStyle.Bold, new Color(0.85f, 0.88f, 0.92f), "Purifica cada parque recolectando exactamente 15 botellas de plástico", true);

        GameObject gridObj = new GameObject("Grid_Niveles", typeof(RectTransform));
        gridObj.transform.SetParent(ventanaSel, false);
        RectTransform rtGrid = gridObj.GetComponent<RectTransform>();
        rtGrid.anchorMin = new Vector2(0.5f, 0.5f);
        rtGrid.anchorMax = new Vector2(0.5f, 0.5f);
        rtGrid.pivot = new Vector2(0.5f, 0.5f);
        rtGrid.anchoredPosition = new Vector2(0, 10);
        rtGrid.sizeDelta = new Vector2(1050, 440);

        GridLayoutGroup glg = gridObj.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(330, 125);
        glg.spacing = new Vector2(25, 20);
        glg.childAlignment = TextAnchor.MiddleCenter;

        Button[] arrayBtnsNiveles = new Button[9];
        for (int i = 0; i < 9; i++)
        {
            int n = i + 1;
            string nombreP = (n < SistemaProgreso.NOMBRES_NIVELES.Length) ? SistemaProgreso.NOMBRES_NIVELES[n] : $"Parque {n}";
            string iconoP = (n < SistemaProgreso.ICONOS_NIVELES.Length) ? SistemaProgreso.ICONOS_NIVELES[n] : "★";

            GameObject card = new GameObject($"Card_Nivel_{n}", typeof(RectTransform));
            card.transform.SetParent(gridObj.transform, false);

            Image imgCard = card.AddComponent<Image>();
            imgCard.sprite = sprBtnNormal;
            imgCard.type = Image.Type.Simple;

            Button btnCard = card.AddComponent<Button>();
            ConfigurarTransicionBoton(btnCard, sprBtnNormal, sprBtnHover, sprBtnPressed);

            GameObject txtCardObj = new GameObject("Texto_Nivel", typeof(RectTransform));
            txtCardObj.transform.SetParent(card.transform, false);
            RectTransform rtTxtC = txtCardObj.GetComponent<RectTransform>();
            rtTxtC.anchorMin = new Vector2(0.18f, 0.08f);
            rtTxtC.anchorMax = new Vector2(0.82f, 0.92f);
            rtTxtC.offsetMin = Vector2.zero;
            rtTxtC.offsetMax = Vector2.zero;

            Text txtC = txtCardObj.AddComponent<Text>();
            txtC.font = ObtenerFuenteTitulo();
            txtC.fontSize = 14;
            txtC.fontStyle = FontStyle.Bold;
            txtC.alignment = TextAnchor.MiddleCenter;
            txtC.color = Color.white;
            txtC.text = $"{iconoP} <b>NIVEL {n}</b>\n<size=12>{nombreP}</size>\n<size=11><color=#00FFE0>♻ 15 Botellas</color></size>";
            txtC.raycastTarget = false;

            var shadowCard = txtCardObj.AddComponent<Shadow>();
            shadowCard.effectColor = new Color(0f, 0f, 0f, 0.95f);
            shadowCard.effectDistance = new Vector2(2, -2);

            arrayBtnsNiveles[i] = btnCard;
        }

        Button btnCerrarSel = CrearBotonArcade(ventanaSel, "Boton_Cerrar_Seleccion", new Vector2(0, -280), new Vector2(300, 56),
            sprBtnNormal, sprBtnHover, sprBtnPressed, "✖   VOLVER", new Color(1f, 0.45f, 0.45f), true);
        panelSel.SetActive(false);

        // 11. MODAL: REGISTRO ODS 15 (Vida de Ecosistemas Terrestres - Huancayo 2026)
        GameObject panelODSModal = CrearModalBase(ct, "Panel_ODS", sprMarco, new Vector2(1100, 750));
        Transform ventanaODS = panelODSModal.transform.Find("Ventana");
        CrearTextoModal(ventanaODS, "Titulo", new Vector2(0, 270), new Vector2(920, 48), 24, FontStyle.Bold, new Color(1f, 0.88f, 0.30f), "🌿   ODS 15: VIDA DE ECOSISTEMAS TERRESTRES (HUANCAYO)");
        CrearTextoModal(ventanaODS, "Subtitulo", new Vector2(0, 230), new Vector2(920, 28), 15, FontStyle.Bold, new Color(0.85f, 0.88f, 0.92f), "Metas de las Naciones Unidas para restaurar los suelos, bosques y glaciares de Junín", true);

        GameObject cajaODS = new GameObject("Contenido_Caja", typeof(RectTransform));
        cajaODS.transform.SetParent(ventanaODS, false);
        RectTransform rtCajaODS = cajaODS.GetComponent<RectTransform>();
        rtCajaODS.anchorMin = new Vector2(0.5f, 0.5f);
        rtCajaODS.anchorMax = new Vector2(0.5f, 0.5f);
        rtCajaODS.pivot = new Vector2(0.5f, 0.5f);
        rtCajaODS.anchoredPosition = new Vector2(0, -5);
        rtCajaODS.sizeDelta = new Vector2(940, 390);
        Image imgCajaODS = cajaODS.AddComponent<Image>();
        imgCajaODS.sprite = ObtenerSprBlanco();
        imgCajaODS.color = new Color(0.04f, 0.08f, 0.14f, 0.88f);

        string textoODS =
            "<b><color=#FFD700>▶ META 15.3 • LUCHA CONTRA LA DEGRADACIÓN DEL SUELO Y RESIDUOS:</color></b>\n" +
            "En Huancayo se generan más de <b>210 toneladas diarias de basura</b>. Las botellas plásticas abandonadas en parques y riberas tardan hasta <b>500 años en descomponerse</b>, liberando toxinas y microplásticos que asfixian los suelos fértiles del Valle del Mantaro.\n\n" +
            "<b><color=#00FFE0>▶ META 15.1 & 15.2 • PRESERVACIÓN DE BOSQUES ANDINOS DE QUEÑUALES:</color></b>\n" +
            "Ecosistemas únicos como el <b>Bosque Dorado de Paccha</b> albergan árboles milenarios de queñual y quinual, esenciales para capturar agua de niebla y prevenir deslizamientos y erosión de tierras altas.\n\n" +
            "<b><color=#4EFA98>▶ META 15.4 • CONSERVACIÓN DE ECOSISTEMAS MONTAÑOSOS Y GLACIARES:</color></b>\n" +
            "El <b>Nevado Huaytapallana (5,557 msnm)</b> y la cuenca del Río Shullcas abastecen de vida a toda la provincia. Detener la contaminación por plástico y fango es crucial para evitar el deshielo acelerado.\n\n" +
            "<b><color=#70D6FF>▶ ACCIÓN EN 'IYARI':</color></b>\n" +
            "Empuña la mochila purificadora de agua, disuelve los lodos contaminantes, recupera cada botella plástica y devuélvele la pureza a los 9 parques y reservas de Huancayo.";

        GameObject goDescODS = CrearTextoModal(cajaODS.transform, "Descripcion", Vector2.zero, new Vector2(900, 370), 15, FontStyle.Normal, Color.white, textoODS, true);
        Text tODS = goDescODS.GetComponent<Text>();
        tODS.alignment = TextAnchor.MiddleLeft;
        tODS.lineSpacing = 1.35f;

        Button btnCerrarODS = CrearBotonArcade(ventanaODS, "Boton_Cerrar_ODS", new Vector2(0, -270), new Vector2(340, 58),
            sprBtnNormal, sprBtnHover, sprBtnPressed, "✔   ¡COMPROMISO ODS 15!", new Color(0.35f, 1f, 0.55f), true);
        panelODSModal.SetActive(false);

        // 12. MODAL: AJUSTES DE SONIDO (Sliders Master, BGM y SFX)
        GameObject panelSonidoModal = CrearModalBase(ct, "Panel_AjustesSonido", sprMarco, new Vector2(950, 680));
        Transform ventanaSonido = panelSonidoModal.transform.Find("Ventana");
        CrearTextoModal(ventanaSonido, "Titulo", new Vector2(0, 240), new Vector2(800, 48), 24, FontStyle.Bold, new Color(0.2f, 0.95f, 1f), "🔊   AJUSTES DE SONIDO Y AUDIO");
        CrearTextoModal(ventanaSonido, "Subtitulo", new Vector2(0, 198), new Vector2(800, 28), 15, FontStyle.Bold, new Color(0.85f, 0.88f, 0.92f), "Banda sonora Chiptune Andina & Efectos Neo Geo arcade", true);

        Text txtValMaster, txtValMusica, txtValSFX;
        Slider sMaster = CrearFilaSlider(ventanaSonido, "Fila_Master", new Vector2(0, 110), "🔊   VOLUMEN GENERAL", 1.0f, out txtValMaster);
        Slider sMusica = CrearFilaSlider(ventanaSonido, "Fila_Musica", new Vector2(0, 30), "🎵   MÚSICA ANDINA (BGM)", 0.8f, out txtValMusica);
        Slider sSFX = CrearFilaSlider(ventanaSonido, "Fila_SFX", new Vector2(0, -50), "💥   EFECTOS RETRO (SFX)", 0.9f, out txtValSFX);

        Button btnCerrarSonido = CrearBotonArcade(ventanaSonido, "Boton_Cerrar_Sonido", new Vector2(0, -210), new Vector2(320, 58),
            sprBtnNormal, sprBtnHover, sprBtnPressed, "✔   GUARDAR Y VOLVER", new Color(0.35f, 1f, 0.55f), true);
        panelSonidoModal.SetActive(false);

        // 13. MODAL: GUÍA DE CONTROLES
        GameObject panelCtrlModal = CrearModalBase(ct, "Panel_Controles", sprMarco, new Vector2(1100, 750));
        Transform ventanaCtrl = panelCtrlModal.transform.Find("Ventana");
        CrearTextoModal(ventanaCtrl, "Titulo", new Vector2(0, 270), new Vector2(900, 48), 24, FontStyle.Bold, new Color(0.1f, 1f, 0.88f), "🎮   GUÍA DE CONTROLES ARCADE");
        CrearTextoModal(ventanaCtrl, "Subtitulo", new Vector2(0, 230), new Vector2(900, 28), 15, FontStyle.Bold, new Color(0.85f, 0.88f, 0.92f), "Técnicas de parkour y combate ecológico para Illari", true);

        GameObject cajaCtrl = new GameObject("Contenido_Caja", typeof(RectTransform));
        cajaCtrl.transform.SetParent(ventanaCtrl, false);
        RectTransform rtCajaCtrl = cajaCtrl.GetComponent<RectTransform>();
        rtCajaCtrl.anchorMin = new Vector2(0.5f, 0.5f);
        rtCajaCtrl.anchorMax = new Vector2(0.5f, 0.5f);
        rtCajaCtrl.pivot = new Vector2(0.5f, 0.5f);
        rtCajaCtrl.anchoredPosition = new Vector2(0, -5);
        rtCajaCtrl.sizeDelta = new Vector2(940, 390);
        Image imgCajaCtrl = cajaCtrl.AddComponent<Image>();
        imgCajaCtrl.sprite = ObtenerSprBlanco();
        imgCajaCtrl.color = new Color(0.04f, 0.08f, 0.14f, 0.88f);

        string textoControles =
            "<b><color=#FFD700>🏃 MOVIMIENTO HORIZONTAL:</color></b>  Teclas <b>[A] / [D]</b>  •  Flechas Izquierda / Derecha\n\n" +
            "<b><color=#00FFE0>🦘 SALTO Y PARKOUR:</color></b>  Tecla <b>[ESPACIO]</b> (Salto sobre plataformas y bancos)\n\n" +
            "<b><color=#4EFA98>💦 CAÑÓN DE AGUA PURIFICADORA:</color></b>  Teclas <b>[J]</b>, <b>[Z]</b>  •  <b>Clic Izquierdo</b> del ratón\n\n" +
            "<b><color=#E0A0FF>🔄 PIRUETA / DOBLE SALTO:</color></b>  Tecla <b>[K]</b>  •  Tecla <b>[W]</b> en el aire (Parkour acrobático)\n\n" +
            "<b><color=#FFD700>⬆ DISPARO VERTICAL HACIA ARRIBA:</color></b>  Mantener <b>[W] + Disparar [J]</b> (Para enemigos aéreos)\n\n" +
            "<b><color=#FFAA00>⏸ MENÚ DE PAUSA TÁCTICA:</color></b>  Tecla <b>[ESC]</b>, <b>[P]</b>  •  Botón táctil circular en pantalla\n\n" +
            "<b><color=#00FFFF>♻ REGLA ODS 15 (15 BOTELLAS):</color></b> 9 botellas en plataformas + 6 botellas soltadas por monstruos. ¡Limpia el parque y llega al contenedor final!";

        GameObject goDescCtrl = CrearTextoModal(cajaCtrl.transform, "Descripcion", Vector2.zero, new Vector2(900, 370), 16, FontStyle.Normal, Color.white, textoControles, true);
        Text tDesc = goDescCtrl.GetComponent<Text>();
        tDesc.alignment = TextAnchor.MiddleLeft;
        tDesc.lineSpacing = 1.35f;

        Button btnCerrarCtrl = CrearBotonArcade(ventanaCtrl, "Boton_Cerrar_Controles", new Vector2(0, -270), new Vector2(340, 58),
            sprBtnNormal, sprBtnHover, sprBtnPressed, "✔   ENTENDIDO", new Color(0.35f, 1f, 0.55f), true);
        panelCtrlModal.SetActive(false);

        // 14. OVERLAY DE TRANSICIÓN SUAVE
        GameObject transObj = new GameObject("Overlay_Transicion", typeof(RectTransform));
        transObj.transform.SetParent(ct, false);
        transObj.transform.SetAsLastSibling();
        RectTransform rtTrans = transObj.GetComponent<RectTransform>();
        rtTrans.anchorMin = Vector2.zero;
        rtTrans.anchorMax = Vector2.one;
        rtTrans.sizeDelta = Vector2.zero;
        Image imgTrans = transObj.AddComponent<Image>();
        imgTrans.sprite = ObtenerSprBlanco();
        imgTrans.color = Color.black;
        imgTrans.raycastTarget = false;
        CanvasGroup cgTrans = transObj.AddComponent<CanvasGroup>();
        cgTrans.alpha = 0f;
        cgTrans.blocksRaycasts = false;
        transObj.SetActive(false);

        // 15. SCRIPT CONTROLLER (MenuPrincipalIllari)
        GameObject mgrObj = new GameObject("MenuPrincipal_Manager");
        MenuPrincipalIllari menuMgr = mgrObj.AddComponent<MenuPrincipalIllari>();

        // Serializar campos en el Inspector
        SerializedObject so = new SerializedObject(menuMgr);
        so.FindProperty("panelInicio").objectReferenceValue = contBotones;
        so.FindProperty("panelSeleccion").objectReferenceValue = panelSel;
        so.FindProperty("panelODS").objectReferenceValue = panelODSModal;
        so.FindProperty("panelSonido").objectReferenceValue = panelSonidoModal;
        so.FindProperty("panelControles").objectReferenceValue = panelCtrlModal;

        so.FindProperty("sliderMaster").objectReferenceValue = sMaster;
        so.FindProperty("sliderMusica").objectReferenceValue = sMusica;
        so.FindProperty("sliderSFX").objectReferenceValue = sSFX;

        so.FindProperty("txtValorMaster").objectReferenceValue = txtValMaster;
        so.FindProperty("txtValorMusica").objectReferenceValue = txtValMusica;
        so.FindProperty("txtValorSFX").objectReferenceValue = txtValSFX;

        so.FindProperty("rectLogo").objectReferenceValue = rtLogo;

        so.ApplyModifiedProperties();

        // 16. CONEXIÓN PERSISTENTE DE EVENTOS SERIALIZADOS EN LA ESCENA
        UnityEventTools.AddPersistentListener(btnJugar.onClick, menuMgr.BotonIniciarAventura);
        UnityEventTools.AddPersistentListener(btnSeleccion.onClick, menuMgr.BotonAbrirSelectorMisiones);
        UnityEventTools.AddPersistentListener(btnODS.onClick, menuMgr.BotonAbrirODS);
        UnityEventTools.AddPersistentListener(btnSonido.onClick, menuMgr.BotonAbrirAjustesSonido);
        UnityEventTools.AddPersistentListener(btnControles.onClick, menuMgr.BotonAbrirControles);
        UnityEventTools.AddPersistentListener(btnSalir.onClick, menuMgr.BotonSalir);

        UnityEventTools.AddPersistentListener(btnCerrarSel.onClick, menuMgr.BotonVolverAlInicio);
        UnityEventTools.AddPersistentListener(btnCerrarODS.onClick, menuMgr.BotonCerrarODS);
        UnityEventTools.AddPersistentListener(btnCerrarSonido.onClick, menuMgr.BotonCerrarAjustesSonido);
        UnityEventTools.AddPersistentListener(btnCerrarCtrl.onClick, menuMgr.BotonCerrarControles);

        for (int i = 0; i < 9; i++)
        {
            int n = i + 1;
            UnityEventTools.AddIntPersistentListener(arrayBtnsNiveles[i].onClick, menuMgr.CargarMision, n);
        }

        // Guardar escena
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("<color=#00FF88><b>[MENU]</b> ¡Menú Principal de Iyari completamente reconstruido y guardado con éxito en Assets/Scenes/MenuPrincipal.unity!</color>");
    }

    private static Slider CrearFilaSlider(Transform parent, string nombre, Vector2 pos, string etiqueta, float valorDefecto, out Text txtValor)
    {
        GameObject cont = new GameObject(nombre, typeof(RectTransform));
        cont.transform.SetParent(parent, false);
        RectTransform rtCont = cont.GetComponent<RectTransform>();
        rtCont.anchorMin = new Vector2(0.5f, 0.5f);
        rtCont.anchorMax = new Vector2(0.5f, 0.5f);
        rtCont.pivot = new Vector2(0.5f, 0.5f);
        rtCont.anchoredPosition = pos;
        rtCont.sizeDelta = new Vector2(760, 60);

        // Etiqueta del slider
        GameObject lblObj = new GameObject("Label", typeof(RectTransform));
        lblObj.transform.SetParent(cont.transform, false);
        RectTransform rtLbl = lblObj.GetComponent<RectTransform>();
        rtLbl.anchorMin = new Vector2(0f, 0.5f);
        rtLbl.anchorMax = new Vector2(0f, 0.5f);
        rtLbl.pivot = new Vector2(0f, 0.5f);
        rtLbl.anchoredPosition = new Vector2(10, 0);
        rtLbl.sizeDelta = new Vector2(270, 40);

        Text txtLbl = lblObj.AddComponent<Text>();
        txtLbl.font = ObtenerFuenteTitulo();
        txtLbl.fontSize = 17;
        txtLbl.fontStyle = FontStyle.Bold;
        txtLbl.alignment = TextAnchor.MiddleLeft;
        txtLbl.color = new Color(0.9f, 0.95f, 1f);
        txtLbl.text = etiqueta;
        txtLbl.raycastTarget = false;

        // Slider GameObject
        GameObject sliderObj = new GameObject("Slider", typeof(RectTransform));
        sliderObj.transform.SetParent(cont.transform, false);
        RectTransform rtSlider = sliderObj.GetComponent<RectTransform>();
        rtSlider.anchorMin = new Vector2(0f, 0.5f);
        rtSlider.anchorMax = new Vector2(0f, 0.5f);
        rtSlider.pivot = new Vector2(0f, 0.5f);
        rtSlider.anchoredPosition = new Vector2(290, 0);
        rtSlider.sizeDelta = new Vector2(360, 32);

        Slider slider = sliderObj.AddComponent<Slider>();

        // Background track
        GameObject bgObj = new GameObject("Background", typeof(RectTransform));
        bgObj.transform.SetParent(sliderObj.transform, false);
        RectTransform rtBg = bgObj.GetComponent<RectTransform>();
        rtBg.anchorMin = new Vector2(0f, 0.35f);
        rtBg.anchorMax = new Vector2(1f, 0.65f);
        rtBg.sizeDelta = Vector2.zero;
        Image imgBg = bgObj.AddComponent<Image>();
        imgBg.sprite = ObtenerSprBlanco();
        imgBg.color = new Color(0.06f, 0.12f, 0.20f, 0.95f);

        // Fill Area
        GameObject fillAreaObj = new GameObject("Fill Area", typeof(RectTransform));
        fillAreaObj.transform.SetParent(sliderObj.transform, false);
        RectTransform rtFillArea = fillAreaObj.GetComponent<RectTransform>();
        rtFillArea.anchorMin = new Vector2(0f, 0.35f);
        rtFillArea.anchorMax = new Vector2(1f, 0.65f);
        rtFillArea.sizeDelta = new Vector2(-16, 0);
        rtFillArea.anchoredPosition = Vector2.zero;

        GameObject fillObj = new GameObject("Fill", typeof(RectTransform));
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        RectTransform rtFill = fillObj.GetComponent<RectTransform>();
        rtFill.sizeDelta = Vector2.zero;
        Image imgFill = fillObj.AddComponent<Image>();
        imgFill.sprite = ObtenerSprBlanco();
        imgFill.color = new Color(0.0f, 1f, 0.88f, 1f);

        // Handle Area
        GameObject handleAreaObj = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleAreaObj.transform.SetParent(sliderObj.transform, false);
        RectTransform rtHandleArea = handleAreaObj.GetComponent<RectTransform>();
        rtHandleArea.anchorMin = Vector2.zero;
        rtHandleArea.anchorMax = Vector2.one;
        rtHandleArea.sizeDelta = new Vector2(-24, 0);
        rtHandleArea.anchoredPosition = Vector2.zero;

        GameObject handleObj = new GameObject("Handle", typeof(RectTransform));
        handleObj.transform.SetParent(handleAreaObj.transform, false);
        RectTransform rtHandle = handleObj.GetComponent<RectTransform>();
        rtHandle.sizeDelta = new Vector2(26, 32);
        Image imgHandle = handleObj.AddComponent<Image>();
        imgHandle.sprite = ObtenerSprBlanco();
        imgHandle.color = new Color(1f, 0.9f, 0.35f, 1f);

        slider.fillRect = rtFill;
        slider.handleRect = rtHandle;
        slider.targetGraphic = imgHandle;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = valorDefecto;

        // Texto de porcentaje
        GameObject valObj = new GameObject("Texto_Valor", typeof(RectTransform));
        valObj.transform.SetParent(cont.transform, false);
        RectTransform rtVal = valObj.GetComponent<RectTransform>();
        rtVal.anchorMin = new Vector2(1f, 0.5f);
        rtVal.anchorMax = new Vector2(1f, 0.5f);
        rtVal.pivot = new Vector2(1f, 0.5f);
        rtVal.anchoredPosition = new Vector2(-10, 0);
        rtVal.sizeDelta = new Vector2(90, 40);

        Text txtVal = valObj.AddComponent<Text>();
        txtVal.font = ObtenerFuenteTitulo();
        txtVal.fontSize = 18;
        txtVal.fontStyle = FontStyle.Bold;
        txtVal.alignment = TextAnchor.MiddleRight;
        txtVal.color = new Color(0.0f, 1f, 0.88f);
        txtVal.text = $"{Mathf.RoundToInt(valorDefecto * 100f)}%";
        txtVal.raycastTarget = false;

        txtValor = txtVal;
        return slider;
    }

    private static Button CrearBotonArcade(Transform parent, string nombre, Vector2 pos, Vector2 tamano,
        Sprite normal, Sprite hover, Sprite pressed, string texto, Color colorTexto, bool anclarCentro = false)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        if (anclarCentro)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
        }
        else
        {
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
        }
        rt.anchoredPosition = pos;
        rt.sizeDelta = tamano;

        Image img = go.AddComponent<Image>();
        img.sprite = normal;
        img.type = Image.Type.Simple;
        img.preserveAspect = false;
        img.raycastTarget = true;

        Button btn = go.AddComponent<Button>();
        ConfigurarTransicionBoton(btn, normal, hover, pressed);

        if (go.GetComponent<BotonArcadeEfecto>() == null)
        {
            go.AddComponent<BotonArcadeEfecto>();
        }

        GameObject txtObj = new GameObject("Texto", typeof(RectTransform));
        txtObj.transform.SetParent(go.transform, false);
        RectTransform rtTxt = txtObj.GetComponent<RectTransform>();
        // Margen horizontal (18% a cada lado) para centrarse perfectamente en la placa de pizarra oscura,
        // sin solapar nunca los tubos de ensayo de agua cian a los lados ni los ribetes mecánicos.
        rtTxt.anchorMin = new Vector2(0.18f, 0.10f);
        rtTxt.anchorMax = new Vector2(0.82f, 0.90f);
        rtTxt.offsetMin = Vector2.zero;
        rtTxt.offsetMax = Vector2.zero;

        Text txt = txtObj.AddComponent<Text>();
        txt.font = ObtenerFuenteTitulo();
        txt.fontSize = 16;
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = colorTexto;
        txt.text = texto;
        txt.resizeTextForBestFit = true;
        txt.resizeTextMinSize = 11;
        txt.resizeTextMaxSize = 17;
        txt.raycastTarget = false;

        var shadow = txtObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.95f);
        shadow.effectDistance = new Vector2(2, -2);

        return btn;
    }

    private static void ConfigurarTransicionBoton(Button btn, Sprite normal, Sprite hover, Sprite pressed)
    {
        btn.transition = Selectable.Transition.SpriteSwap;
        SpriteState ss = new SpriteState();
        ss.highlightedSprite = hover;
        ss.pressedSprite = pressed;
        ss.selectedSprite = hover;
        btn.spriteState = ss;
    }

    private static GameObject CrearModalBase(Transform canvas, string nombre, Sprite marcoSprite, Vector2 tamanoVentana)
    {
        GameObject panelObj = new GameObject(nombre, typeof(RectTransform));
        panelObj.transform.SetParent(canvas, false);
        RectTransform rtPanel = panelObj.GetComponent<RectTransform>();
        rtPanel.anchorMin = Vector2.zero;
        rtPanel.anchorMax = Vector2.one;
        rtPanel.sizeDelta = Vector2.zero;

        Image imgBack = panelObj.AddComponent<Image>();
        imgBack.sprite = ObtenerSprBlanco();
        imgBack.color = new Color(0.01f, 0.02f, 0.04f, 0.88f);
        imgBack.raycastTarget = true;

        GameObject ventana = new GameObject("Ventana", typeof(RectTransform));
        ventana.transform.SetParent(panelObj.transform, false);
        RectTransform rtVent = ventana.GetComponent<RectTransform>();
        rtVent.anchorMin = new Vector2(0.5f, 0.5f);
        rtVent.anchorMax = new Vector2(0.5f, 0.5f);
        rtVent.pivot = new Vector2(0.5f, 0.5f);
        rtVent.anchoredPosition = Vector2.zero;
        rtVent.sizeDelta = tamanoVentana;

        Image imgVent = ventana.AddComponent<Image>();
        imgVent.sprite = marcoSprite;
        imgVent.type = Image.Type.Simple;
        imgVent.raycastTarget = false;

        return panelObj;
    }

    private static GameObject CrearTextoModal(Transform parent, string nombre, Vector2 pos, Vector2 tamano,
        int fontSize, FontStyle estilo, Color color, string contenido, bool esCuerpo = false)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = tamano;

        Text txt = go.AddComponent<Text>();
        txt.font = esCuerpo ? ObtenerFuenteCuerpo() : ObtenerFuenteTitulo();
        txt.fontSize = fontSize;
        txt.fontStyle = estilo;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = color;
        txt.text = contenido;
        txt.raycastTarget = false;

        var shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.95f);
        shadow.effectDistance = new Vector2(2, -2);

        return go;
    }
}
