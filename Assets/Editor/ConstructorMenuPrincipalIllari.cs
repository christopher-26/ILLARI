using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Constructor de escena para 'MenuPrincipal.unity'.
/// Crea una interfaz arcade artesanal, elegante y completa con:
/// - Main Camera y AudioListener (Soluciona "No cameras rendering")
/// - EventSystem robusto compatible
/// - Portada artística ilustrada en alta resolución
/// - Logotipo dorado/turquesa con patrones andinos y animación flotante
/// - Botones arcade con eventos persistentes serializados (UnityEventTools)
/// - Modales temáticos: Selección de 9 Niveles, Controles y ODS 15
/// - Serialización completa en MenuPrincipalIllari
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

    private static Font ObtenerFuente()
    {
        return ObtenerFuenteTitulo();
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

        Debug.Log("<color=#00FFE0><b>[MENU]</b> Construyendo Menú Principal de Illari 'a mano' con eventos persistentes...</color>");

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

        // 3. CANVAS RAÍZ
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

        // Cargar sprites
        Sprite sprPortada = CargarSprite("PortadaMenuIllari");
        Sprite sprLogo = CargarSprite("LogoIllari");
        Sprite sprMarco = CargarSprite("PanelMarco");
        Sprite sprBtnNormal = CargarSprite("BotonArcade_Normal");
        Sprite sprBtnHover = CargarSprite("BotonArcade_Hover");
        Sprite sprBtnPressed = CargarSprite("BotonArcade_Pressed");

        // 4. FONDO PORTADA ILUSTRADA
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

        // 5. DEGRADADO / VIÑETA CINEMÁTICA
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

        // 6. LOGOTIPO DORADO ILLARI
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

        // 7. SUBTÍTULO ECO-ARCADE
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
        txtSub.text = "🌿   PARQUES DE HUANCAYO 2026  •  MISIÓN ECOLÓGICA ODS 15   🌿";
        txtSub.raycastTarget = false;
        var shadowSub = subObj.AddComponent<Shadow>();
        shadowSub.effectColor = new Color(0f, 0f, 0f, 0.95f);
        shadowSub.effectDistance = new Vector2(2, -2);

        // 8. CONTENEDOR BOTONES PRINCIPALES
        GameObject contBotones = new GameObject("Contenedor_Botones_Principales", typeof(RectTransform));
        contBotones.transform.SetParent(ct, false);
        RectTransform rtCont = contBotones.GetComponent<RectTransform>();
        rtCont.anchorMin = new Vector2(0.5f, 0f);
        rtCont.anchorMax = new Vector2(0.5f, 0f);
        rtCont.pivot = new Vector2(0.5f, 0f);
        rtCont.anchoredPosition = new Vector2(0, 40);
        rtCont.sizeDelta = new Vector2(500, 500);

        // Botones arcade del menú principal
        Button btnJugar = CrearBotonArcade(contBotones.transform, "Boton_Jugar", new Vector2(0, 400), new Vector2(460, 72),
            sprBtnNormal, sprBtnHover, sprBtnPressed, "▶   INICIAR AVENTURA", new Color(1f, 0.92f, 0.35f));

        Button btnSeleccion = CrearBotonArcade(contBotones.transform, "Boton_SeleccionNiveles", new Vector2(0, 315), new Vector2(460, 72),
            sprBtnNormal, sprBtnHover, sprBtnPressed, "📂   SELECCIONAR PARQUE", Color.white);

        Button btnControles = CrearBotonArcade(contBotones.transform, "Boton_Controles", new Vector2(0, 230), new Vector2(460, 72),
            sprBtnNormal, sprBtnHover, sprBtnPressed, "🎮   GUÍA DE CONTROLES", Color.white);

        Button btnODS15 = CrearBotonArcade(contBotones.transform, "Boton_ODS15", new Vector2(0, 145), new Vector2(460, 72),
            sprBtnNormal, sprBtnHover, sprBtnPressed, "🌿   MISIÓN ECOLÓGICA (ODS 15)", new Color(0.35f, 1f, 0.6f));

        Button btnSalir = CrearBotonArcade(contBotones.transform, "Boton_Salir", new Vector2(0, 60), new Vector2(460, 72),
            sprBtnNormal, sprBtnHover, sprBtnPressed, "🚪   SALIR DEL JUEGO", new Color(1f, 0.45f, 0.45f));

        // 9. PANEL MODAL: SELECCIÓN DE NIVELES (Grid de 9 parques)
        GameObject panelSel = CrearModalBase(ct, "Panel_SeleccionNiveles", sprMarco, new Vector2(1180, 800));
        Transform ventanaSel = panelSel.transform.Find("Ventana");
        CrearTextoModal(ventanaSel, "Titulo", new Vector2(0, 310), new Vector2(900, 48), 24, FontStyle.Bold, new Color(1f, 0.88f, 0.30f), "SELECCIÓN DE PARQUES (HUANCAYO 2026)");
        CrearTextoModal(ventanaSel, "Subtitulo", new Vector2(0, 268), new Vector2(900, 30), 15, FontStyle.Bold, new Color(0.85f, 0.88f, 0.92f), "Purifica cada parque recolectando exactamente 15 botellas de plástico", true);

        // Grid 3x3 de niveles
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
        Text[] arrayTxtsNiveles = new Text[9];

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
            rtTxtC.anchorMin = Vector2.zero;
            rtTxtC.anchorMax = Vector2.one;
            rtTxtC.sizeDelta = Vector2.zero;

            Text txtC = txtCardObj.AddComponent<Text>();
            txtC.font = ObtenerFuenteTitulo();
            txtC.fontSize = 15;
            txtC.fontStyle = FontStyle.Bold;
            txtC.alignment = TextAnchor.MiddleCenter;
            txtC.color = Color.white;
            txtC.text = $"{iconoP} <b>NIVEL {n}</b>\n<size=13>{nombreP}</size>\n<size=11><color=#00FFE0>♻ 15 Botellas</color></size>";
            txtC.raycastTarget = false;

            var shadowCard = txtCardObj.AddComponent<Shadow>();
            shadowCard.effectColor = new Color(0f, 0f, 0f, 0.95f);
            shadowCard.effectDistance = new Vector2(2, -2);

            arrayBtnsNiveles[i] = btnCard;
            arrayTxtsNiveles[i] = txtC;
        }

        Button btnCerrarSel = CrearBotonArcade(ventanaSel, "Boton_Cerrar_Seleccion", new Vector2(0, -280), new Vector2(300, 56),
            sprBtnNormal, sprBtnHover, sprBtnPressed, "✖   VOLVER", new Color(1f, 0.45f, 0.45f), true);
        panelSel.SetActive(false);

        // 10. PANEL MODAL: GUÍA DE CONTROLES
        GameObject panelCtrl = CrearModalBase(ct, "Panel_Controles", sprMarco, new Vector2(1100, 740));
        Transform ventanaCtrl = panelCtrl.transform.Find("Ventana");
        CrearTextoModal(ventanaCtrl, "Titulo", new Vector2(0, 260), new Vector2(900, 48), 24, FontStyle.Bold, new Color(0.1f, 1f, 0.88f), "🎮   GUÍA DE CONTROLES Y TÁCTICAS");

        // Contenedor interior de lectura
        GameObject cajaCtrl = new GameObject("Contenido_Caja", typeof(RectTransform));
        cajaCtrl.transform.SetParent(ventanaCtrl, false);
        RectTransform rtCajaCtrl = cajaCtrl.GetComponent<RectTransform>();
        rtCajaCtrl.anchorMin = new Vector2(0.5f, 0.5f);
        rtCajaCtrl.anchorMax = new Vector2(0.5f, 0.5f);
        rtCajaCtrl.pivot = new Vector2(0.5f, 0.5f);
        rtCajaCtrl.anchoredPosition = new Vector2(0, 10);
        rtCajaCtrl.sizeDelta = new Vector2(940, 390);
        Image imgCajaCtrl = cajaCtrl.AddComponent<Image>();
        imgCajaCtrl.sprite = ObtenerSprBlanco();
        imgCajaCtrl.color = new Color(0.04f, 0.08f, 0.14f, 0.88f);

        string textoControles =
            "<b><color=#FFD700>🏃 MOVIMIENTO:</color></b>  Teclas <b>[A] / [D]</b>  •  Flechas Izquierda / Derecha\n\n" +
            "<b><color=#00FFE0>🦘 SALTO Y PARKOUR:</color></b>  Tecla <b>[ESPACIO]</b>  •  Tecla <b>[W]</b> (Giro acrobático aéreo)\n\n" +
            "<b><color=#4EFA98>💦 CAÑÓN DE AGUA:</color></b>  Teclas <b>[J]</b>, <b>[Z]</b>  •  <b>Clic Izquierdo</b> del ratón\n\n" +
            "<b><color=#FFAA00>⏸ PAUSA DE MISIÓN:</color></b>  Tecla <b>[ESC]</b>, <b>[P]</b>  •  Botón táctil en pantalla\n\n" +
            "<b><color=#00FFFF>♻ REGLA ODS 15 (15 BOTELLAS POR PARQUE):</color></b> Cada nivel contiene exactamente 15 botellas:\n" +
            "   • <b>9 botellas</b> distribuidas en plataformas y techos del parque.\n" +
            "   • <b>6 botellas</b> soltadas por los monstruos de lodo al purificarlos.\n" +
            "   • ¡Limpia las 15 botellas y entra al <b>Eco-Contenedor</b> al final del parque!";

        GameObject goDescCtrl = CrearTextoModal(cajaCtrl.transform, "Descripcion", Vector2.zero, new Vector2(900, 370), 16, FontStyle.Normal, Color.white, textoControles, true);
        Text tDesc = goDescCtrl.GetComponent<Text>();
        tDesc.alignment = TextAnchor.MiddleLeft;
        tDesc.lineSpacing = 1.35f;

        Button btnCerrarCtrl = CrearBotonArcade(ventanaCtrl, "Boton_Cerrar_Controles", new Vector2(0, -265), new Vector2(340, 60),
            sprBtnNormal, sprBtnHover, sprBtnPressed, "✔   ENTENDIDO", new Color(0.35f, 1f, 0.55f), true);
        panelCtrl.SetActive(false);

        // 11. PANEL MODAL: MISIÓN ECOLÓGICA ODS 15
        GameObject panelODS = CrearModalBase(ct, "Panel_ODS15", sprMarco, new Vector2(1100, 740));
        Transform ventanaODS = panelODS.transform.Find("Ventana");
        CrearTextoModal(ventanaODS, "Titulo", new Vector2(0, 260), new Vector2(900, 48), 24, FontStyle.Bold, new Color(1f, 0.88f, 0.30f), "🌿   ODS 15: VIDA DE ECOSISTEMAS TERRESTRES");

        // Contenedor interior de lectura
        GameObject cajaODS = new GameObject("Contenido_Caja", typeof(RectTransform));
        cajaODS.transform.SetParent(ventanaODS, false);
        RectTransform rtCajaODS = cajaODS.GetComponent<RectTransform>();
        rtCajaODS.anchorMin = new Vector2(0.5f, 0.5f);
        rtCajaODS.anchorMax = new Vector2(0.5f, 0.5f);
        rtCajaODS.pivot = new Vector2(0.5f, 0.5f);
        rtCajaODS.anchoredPosition = new Vector2(0, 10);
        rtCajaODS.sizeDelta = new Vector2(940, 390);
        Image imgCajaODS = cajaODS.AddComponent<Image>();
        imgCajaODS.sprite = ObtenerSprBlanco();
        imgCajaODS.color = new Color(0.04f, 0.08f, 0.14f, 0.88f);

        string textoODS =
            "<b><color=#FFD700>▶ MISIÓN HUANCAYO 2026:</color></b>\n" +
            "En el Valle del Mantaro y la provincia de Huancayo (Junín, Perú), el arrojo descontrolado de plásticos y efluentes contamina parques emblemáticos y la cuenca del Río Shullcas.\n\n" +
            "<b><color=#00FFE0>▶ LA HEROÍNA ILLARI:</color></b>\n" +
            "Equipada con su mochila eco-purificadora, Illari recorre 9 parques y reservas desde el Cerrito de la Libertad hasta los glaciares del Nevado Huaytapallana para neutralizar a las criaturas de lodo tóxico y recolectar residuos reciclables.\n\n" +
            "<b><color=#4EFA98>▶ IMPACTO REAL (ODS 15):</color></b>\n" +
            "Cada botella plástica recogida simboliza el compromiso con el Objetivo de Desarrollo Sostenible 15: proteger, restablecer y promover el uso sostenible de los ecosistemas terrestres.";

        GameObject goDescODS = CrearTextoModal(cajaODS.transform, "Descripcion", Vector2.zero, new Vector2(900, 370), 16, FontStyle.Normal, Color.white, textoODS, true);
        Text tODS = goDescODS.GetComponent<Text>();
        tODS.alignment = TextAnchor.MiddleLeft;
        tODS.lineSpacing = 1.35f;

        Button btnCerrarODS = CrearBotonArcade(ventanaODS, "Boton_Cerrar_ODS15", new Vector2(0, -265), new Vector2(340, 60),
            sprBtnNormal, sprBtnHover, sprBtnPressed, "✔   ¡A PURIFICAR!", new Color(0.35f, 1f, 0.55f), true);
        panelODS.SetActive(false);

        // 12. OVERLAY DE TRANSICIÓN SUAVE
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

        // 13. SCRIPT CONTROLLER (MenuPrincipalIllari)
        GameObject mgrObj = new GameObject("MenuPrincipal_Manager");
        MenuPrincipalIllari menuMgr = mgrObj.AddComponent<MenuPrincipalIllari>();

        // Serializar campos privados en el Inspector
        SerializedObject so = new SerializedObject(menuMgr);
        so.FindProperty("panelInicio").objectReferenceValue = contBotones;
        so.FindProperty("panelSeleccion").objectReferenceValue = panelSel;
        so.FindProperty("panelControles").objectReferenceValue = panelCtrl;
        so.FindProperty("panelODS15").objectReferenceValue = panelODS;

        so.FindProperty("botonJugar").objectReferenceValue = btnJugar;
        so.FindProperty("botonSeleccion").objectReferenceValue = btnSeleccion;
        so.FindProperty("botonControles").objectReferenceValue = btnControles;
        so.FindProperty("botonODS15").objectReferenceValue = btnODS15;
        so.FindProperty("botonSalir").objectReferenceValue = btnSalir;

        so.FindProperty("botonCerrarSeleccion").objectReferenceValue = btnCerrarSel;
        so.FindProperty("botonCerrarControles").objectReferenceValue = btnCerrarCtrl;
        so.FindProperty("botonCerrarODS15").objectReferenceValue = btnCerrarODS;

        so.FindProperty("rectLogo").objectReferenceValue = rtLogo;
        so.FindProperty("canvasGroupTransicion").objectReferenceValue = cgTrans;

        SerializedProperty propBtns = so.FindProperty("botonesNiveles");
        SerializedProperty propTxts = so.FindProperty("textosNiveles");
        for (int i = 0; i < 9; i++)
        {
            propBtns.GetArrayElementAtIndex(i).objectReferenceValue = arrayBtnsNiveles[i];
            propTxts.GetArrayElementAtIndex(i).objectReferenceValue = arrayTxtsNiveles[i];
        }
        so.ApplyModifiedProperties();

        // CONEXIÓN PERSISTENTE DE EVENTOS SERIALIZADOS EN LA ESCENA
        UnityEventTools.AddPersistentListener(btnJugar.onClick, menuMgr.IniciarAventura);
        UnityEventTools.AddPersistentListener(btnSeleccion.onClick, menuMgr.AbrirSeleccionNiveles);
        UnityEventTools.AddPersistentListener(btnControles.onClick, menuMgr.AbrirControles);
        UnityEventTools.AddPersistentListener(btnODS15.onClick, menuMgr.AbrirODS15);
        UnityEventTools.AddPersistentListener(btnSalir.onClick, menuMgr.SalirDelJuego);

        UnityEventTools.AddPersistentListener(btnCerrarSel.onClick, menuMgr.CerrarTodosLosModales);
        UnityEventTools.AddPersistentListener(btnCerrarCtrl.onClick, menuMgr.CerrarTodosLosModales);
        UnityEventTools.AddPersistentListener(btnCerrarODS.onClick, menuMgr.CerrarTodosLosModales);

        for (int i = 0; i < 9; i++)
        {
            int n = i + 1;
            UnityEventTools.AddIntPersistentListener(arrayBtnsNiveles[i].onClick, menuMgr.SeleccionarNivel, n);
        }

        // Guardar escena
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("<color=#00FF88><b>[MENU]</b> ¡Menú Principal construido con persistencia y guardado en Assets/Scenes/MenuPrincipal.unity!</color>");
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
        img.raycastTarget = true; // CRÍTICO: Debe interceptar clicks

        Button btn = go.AddComponent<Button>();
        ConfigurarTransicionBoton(btn, normal, hover, pressed);

        if (go.GetComponent<BotonArcadeEfecto>() == null)
        {
            go.AddComponent<BotonArcadeEfecto>();
        }

        GameObject txtObj = new GameObject("Texto", typeof(RectTransform));
        txtObj.transform.SetParent(go.transform, false);
        RectTransform rtTxt = txtObj.GetComponent<RectTransform>();
        rtTxt.anchorMin = Vector2.zero;
        rtTxt.anchorMax = Vector2.one;
        rtTxt.sizeDelta = Vector2.zero;

        Text txt = txtObj.AddComponent<Text>();
        txt.font = ObtenerFuenteTitulo();
        txt.fontSize = 19;
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = colorTexto;
        txt.text = texto;
        txt.raycastTarget = false; // CRÍTICO: el click debe ir al botón padre

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

        // Fondo oscuro translúcido
        Image imgBack = panelObj.AddComponent<Image>();
        imgBack.sprite = ObtenerSprBlanco();
        imgBack.color = new Color(0.01f, 0.02f, 0.04f, 0.88f);
        imgBack.raycastTarget = true; // Bloquea clicks al fondo mientras el modal está abierto

        // Ventana central con el marco arcade
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
