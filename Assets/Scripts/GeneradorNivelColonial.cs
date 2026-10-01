using UnityEngine;

/// <summary>
/// Generador de nivel integral para 'Illari' (Nivel 1 - Parque de la Identidad Huanca, Huancayo).
/// Basado en el GDD oficial y el ODS 15 (Vida de Ecosistemas Terrestres).
/// Características:
/// 1. Fondo auténtico del Parque de la Identidad Huanca (torreón de piedra, puentes de ramas, muros sinuosos, CERO personas).
/// 2. Plataformas y suelos con SpriteDrawMode.Tiled (repetición 1:1 de mampostería de canto rodado y césped, NUNCA estirados).
/// 3. Lagos de líquido tóxico radiactivo verde neón en los 3 pozos.
/// 4. 15 botellas de plástico exactas (9 en arcos de parkour + 6 soltadas por los 6 Fangosos al morir).
/// 5. Monstruos Fangosos con animación de caminata (Animator + deformación procedural de lodo) y proyectiles de 0.3m.
/// 6. Carteles informativos ecológicos ODS 15 con datos reales de Huancayo.
/// 7. Encuadre de cámara calibrado sin bordes vacíos.
/// </summary>
public class GeneradorNivelColonial : MonoBehaviour
{
    [Header("Ajustes Generales")]
    [SerializeField] private bool generarAlIniciar = true;
    [SerializeField] private bool limpiarAnterioresAlGenerar = true;

    [Header("Tags y Capas")]
    [SerializeField] private string tagSuelo = "Ground";
    [SerializeField] private string tagColeccionable = "Collectible";
    [SerializeField] private string tagEnemigo = "Enemy";

    private const string CONTENEDOR_NIVEL = "--- NIVEL_COLONIAL ---";
    private const string CONTENEDOR_FONDOS = "--- FONDOS_PANORAMICOS ---";

    // Sprites del nivel
    private Sprite spriteFondo1;
    private Sprite spriteFondo2;
    private Sprite spriteFondo3;
    private Sprite spritePiedraHuanca;
    private Sprite spritePastoHuanca;
    private Sprite spritePisoHuanca;
    private Sprite spritePlataforma;
    private Sprite spriteLagoToxico;
    private Sprite spriteBotellaPlastico;
    private Sprite spriteCartel;
    private Sprite spriteIyari;
    private Sprite spriteMonstruoBarro;
    private Sprite spriteEstacionReciclaje;
    private Sprite spritePrototipo;

    private void Start()
    {
        if (generarAlIniciar)
        {
            GenerarNivel();
        }
    }

    [ContextMenu("Generar Nivel Colonial")]
    public void GenerarNivel()
    {
        if (limpiarAnterioresAlGenerar)
        {
            LimpiarNivel();
        }

        CargarSprites();
        ConfigurarCamara();
        ConstruirFondosParqueModerno();

        GameObject contenedor = new GameObject(CONTENEDOR_NIVEL);
        contenedor.transform.position = Vector3.zero;

        Transform grpSuperficies = CrearGrupo(contenedor.transform, "01_Superficies_Y_Pozos");
        Transform grpLagos = CrearGrupo(contenedor.transform, "02_Lagos_Toxicos_Radiactivos");
        Transform grpPlataformas = CrearGrupo(contenedor.transform, "03_Plataformas_Parkour");
        Transform grpObstaculos = CrearGrupo(contenedor.transform, "04_Obstaculos_Cajas");
        Transform grpCarteles = CrearGrupo(contenedor.transform, "05_Carteles_ODS15");
        Transform grpBotellas = CrearGrupo(contenedor.transform, "06_Botellas_Plastico_9");
        Transform grpEnemigos = CrearGrupo(contenedor.transform, "07_Monstruos_Fangosos_6");
        Transform grpMeta = CrearGrupo(contenedor.transform, "08_Estacion_Reciclaje");

        // 1. Suelos Base con pozos (Tiled 1:1)
        ConstruirSueloConPozos(grpSuperficies);

        // 2. Lagos tóxicos radiactivos verde neón
        ConstruirLagosToxicos(grpLagos);

        // 3. Plataformas de Parkour Espaciosas (Tiled 1:1)
        ConstruirPlataformasEspaciosas(grpPlataformas);

        // 4. Cajas de piedra
        ConstruirCajas(grpObstaculos);

        // 5. Carteles informativos ecológicos ODS 15
        ConstruirCartelesInformativos(grpCarteles);

        // 6. 9 Botellas de plástico en escenario (6 de enemigos = 15 total)
        ConstruirBotellas(grpBotellas);

        // 7. 6 Monstruos Fangosos con animación de caminata y drop de botella
        ConstruirEnemigos(grpEnemigos);

        // 8. Estación de Reciclaje Inteligente (Meta: 15 botellas)
        ConstruirContenedorMeta(grpMeta);

        // 9. Zona de muerte inferior
        ConstruirKillZone(contenedor.transform);

        // 10. Actualizar Sprite y Animaciones de Iyari
        ActualizarSpriteIllari();

        Debug.Log("<color=#00FF88><b>[ILLARI 2026]</b> Nivel 1 del Parque de la Identidad Huanca generado con plataformas TILED sin estiramiento y animación de caminata.</color>");
    }

    [ContextMenu("Limpiar Nivel")]
    public void LimpiarNivel()
    {
        GameObject nivel = GameObject.Find(CONTENEDOR_NIVEL);
        if (nivel != null) DestroyImmediate(nivel);

        GameObject fondos = GameObject.Find(CONTENEDOR_FONDOS);
        if (fondos != null) DestroyImmediate(fondos);
    }

    private void CargarSprites()
    {
#if UNITY_EDITOR
        spriteFondo1 = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/FondoParqueModernoSeamless.png");
        spritePisoHuanca = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PisoParqueHuanca.png");
        spritePlataforma = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PlataformaParque.png");
        if (spritePlataforma == null) spritePlataforma = spritePisoHuanca;

        spritePiedraHuanca = spritePisoHuanca;
        spritePastoHuanca = spritePisoHuanca;

        spriteLagoToxico = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/LagoToxico.png");
        spriteBotellaPlastico = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/BotellaPlastico.png");
        spriteCartel = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/CartelInformativo.png");
        spriteIyari = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Iyari/Idle_0.png");
        spriteMonstruoBarro = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Fangoso/Walk_0.png");
        spriteEstacionReciclaje = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/EstacionReciclaje.png");
#endif

        if (spritePrototipo == null)
        {
            Texture2D tex = new Texture2D(16, 16);
            Color[] px = new Color[16 * 16];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            tex.SetPixels(px);
            tex.Apply();
            spritePrototipo = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
        }
    }

    public static void AjustarEscalaVisual(GameObject objVisual, SpriteRenderer sr, float targetAncho, float targetAlto)
    {
        if (objVisual == null || sr == null || sr.sprite == null) return;

        float w = sr.sprite.rect.width / sr.sprite.pixelsPerUnit;
        float h = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;

        if (w > 0.001f && h > 0.001f)
        {
            objVisual.transform.localScale = new Vector3(targetAncho / w, targetAlto / h, 1f);
        }
    }

    private void ConfigurarCamara()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.40f, 0.72f, 0.95f);
            cam.orthographic = true;
            cam.orthographicSize = 6.0f;

            CameraFollow cf = cam.GetComponent<CameraFollow>();
            if (cf != null)
            {
                cf.ConfigurarLimites(-6f, 133f, -2.0f, 3.5f);
            }

            cam.transform.position = new Vector3(-6f, -1.8f, -10f);
        }
    }

    /// <summary>
    /// Panorámica continua del Parque de la Identidad Huanca 2026 sin personas.
    /// 4 Paneles secuenciales auténticos (Portada, Puente La Huanca, Fuente Cántaros, Castillo).
    /// Proporción 16:9 nativa exacta (40.0m x 22.5m). CERO repetición, progresión continua de Metal Slug.
    /// </summary>
    private void ConstruirFondosParqueModerno()
    {
        GameObject contFondos = new GameObject(CONTENEDOR_FONDOS);
        contFondos.transform.position = Vector3.zero;

        float anchoPanel = 32.25f;
        float altoPanel = 18.0f;
        float inicioX = -10.0f;
        int cantidadPaneles = 7;

        for (int i = 0; i < cantidadPaneles; i++)
        {
            GameObject panel = new GameObject($"Fondo_Panel_{i}");
            panel.transform.SetParent(contFondos.transform);
            panel.transform.position = new Vector3(inicioX + (i * anchoPanel), 0f, 10f);

            SpriteRenderer sr = panel.AddComponent<SpriteRenderer>();
            sr.sprite = spriteFondo1;
            sr.sortingOrder = -20;

            AjustarEscalaVisual(panel, sr, anchoPanel, altoPanel);
        }
    }

    private Transform CrearGrupo(Transform padre, string nombre)
    {
        GameObject grp = new GameObject(nombre);
        grp.transform.SetParent(padre);
        return grp.transform;
    }

    private void ConstruirSueloConPozos(Transform padre)
    {
        CrearBloqueSuelo(padre, "Suelo_Entrada", new Vector2(4f, -5.5f), new Vector2(30f, 2f));
        // Pozo 1: X: 19 a 27
        CrearBloqueSuelo(padre, "Suelo_Plaza", new Vector2(39.5f, -5.5f), new Vector2(25f, 2f));
        // Pozo 2: X: 52 a 61
        CrearBloqueSuelo(padre, "Suelo_Callejon", new Vector2(74.5f, -5.5f), new Vector2(27f, 2f));
        // Pozo 3: X: 88 a 98
        CrearBloqueSuelo(padre, "Suelo_PatioFinal", new Vector2(117f, -5.5f), new Vector2(38f, 2f));

        CrearMuroTope(padre, "Muro_Inicio", -11f);
        CrearMuroTope(padre, "Muro_Final", 137f);
    }

    /// <summary>
    /// Construye suelo con texturizado TILED continuo:
    /// Las texturas se repiten a escala 1:1 real, eliminando totalmente el estiramiento visual.
    /// </summary>
    private void CrearBloqueSuelo(Transform padre, string nombre, Vector2 centro, Vector2 tamano)
    {
        GameObject bloque = new GameObject(nombre);
        bloque.transform.SetParent(padre);
        bloque.transform.position = centro;
        bloque.transform.localScale = Vector3.one;
        bloque.tag = tagSuelo;

        int layerGround = LayerMask.NameToLayer("Ground");
        if (layerGround != -1) bloque.layer = layerGround;

        BoxCollider2D col = bloque.AddComponent<BoxCollider2D>();
        col.size = tamano;

        // Visual unificado de muro de piedra labrada huanca con remate de césped natural (TILED 1:1)
        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(bloque.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = Vector3.one;

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = (spritePisoHuanca != null) ? spritePisoHuanca : spritePrototipo;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        sr.size = tamano;
        sr.sortingOrder = 0;
    }

    private void CrearMuroTope(Transform padre, string nombre, float posX)
    {
        GameObject muro = new GameObject(nombre);
        muro.transform.SetParent(padre);
        muro.transform.position = new Vector3(posX, 0f, 0f);
        muro.transform.localScale = Vector3.one;
        muro.tag = tagSuelo;

        BoxCollider2D col = muro.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 22f);
    }

    private void ConstruirLagosToxicos(Transform padre)
    {
        CrearLagoToxico(padre, "Lago_Toxico_Pozo_1", new Vector2(23f, -6.6f), new Vector2(7.5f, 1.2f));
        CrearLagoToxico(padre, "Lago_Toxico_Pozo_2", new Vector2(56.5f, -6.6f), new Vector2(8.5f, 1.2f));
        CrearLagoToxico(padre, "Lago_Toxico_Pozo_3", new Vector2(93f, -6.6f), new Vector2(9.5f, 1.2f));
        CrearLagoToxico(padre, "Charco_Callejon_Trampa", new Vector2(70f, -4.3f), new Vector2(3.5f, 0.6f));
    }

    private void CrearLagoToxico(Transform padre, string nombre, Vector2 posicion, Vector2 tamano)
    {
        GameObject lago = new GameObject(nombre);
        lago.transform.SetParent(padre);
        lago.transform.position = posicion;
        lago.transform.localScale = Vector3.one;

        BoxCollider2D col = lago.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(tamano.x, tamano.y);

        lago.AddComponent<CharcoToxico>();

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(lago.transform);
        visual.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        if (spriteLagoToxico != null)
        {
            sr.sprite = spriteLagoToxico;
            AjustarEscalaVisual(visual, sr, tamano.x, tamano.y);
        }
        else
        {
            sr.sprite = spritePrototipo;
            sr.color = new Color(0.1f, 0.95f, 0.2f, 0.9f);
            visual.transform.localScale = new Vector3(tamano.x, tamano.y, 1f);
        }
        sr.sortingOrder = 2;
    }

    private void ConstruirPlataformasEspaciosas(Transform padre)
    {
        // === ZONA 1: CRUCE DE POZO 1 (X: 10 a 28) ===
        CrearPlataforma(padre, "Andamio_Entrada_Bajo", new Vector2(11f, -2.5f), new Vector2(3.5f, 0.5f));
        CrearPlataforma(padre, "Andamio_Entrada_Alto", new Vector2(16.5f, 0.8f), new Vector2(4f, 0.5f));
        CrearPlataforma(padre, "Pozo1_Isla_1", new Vector2(21f, -2.8f), new Vector2(3f, 0.5f));
        CrearPlataforma(padre, "Pozo1_Isla_2", new Vector2(25.5f, -1.5f), new Vector2(3f, 0.5f));
        CrearPlataforma(padre, "Pozo1_Puente_Aereo", new Vector2(23f, 2.5f), new Vector2(5f, 0.5f));

        // === ZONA 2: PLAZA Y TEJADOS AMPLIOS (X: 28 a 52) ===
        CrearPlataforma(padre, "Plaza_Balcon_Bajo", new Vector2(33f, -1.2f), new Vector2(4.5f, 0.5f));
        CrearPlataforma(padre, "Plaza_Techo_Central", new Vector2(42f, 2.8f), new Vector2(7f, 0.6f));
        CrearPlataforma(padre, "Plaza_Balcon_Salida", new Vector2(49f, -1.0f), new Vector2(4f, 0.5f));

        // === ZONA 3: CRUCE DE POZO 2 (X: 52 a 62) ===
        CrearPlataforma(padre, "Pozo2_Escalon_1", new Vector2(54f, -2.5f), new Vector2(3f, 0.5f));
        CrearPlataforma(padre, "Pozo2_Escalon_2", new Vector2(58.5f, -1.0f), new Vector2(3f, 0.5f));
        CrearPlataforma(padre, "Pozo2_Trampolin_Alto", new Vector2(56.5f, 3.0f), new Vector2(4.5f, 0.5f));

        // === ZONA 4: CALLEJÓN DE CASONAS (X: 62 a 88) ===
        CrearPlataforma(padre, "Casona_Andamio", new Vector2(64f, -1.0f), new Vector2(4f, 0.5f));
        CrearPlataforma(padre, "Casona_Techo_Amplo", new Vector2(74f, 3.2f), new Vector2(8f, 0.6f));
        CrearPlataforma(padre, "Casona_Torre_Vigia", new Vector2(82f, 6.2f), new Vector2(4.5f, 0.6f));
        CrearPlataforma(padre, "Casona_Bajada", new Vector2(85f, 1.5f), new Vector2(4f, 0.5f));

        // === ZONA 5: CRUCE DE POZO 3 (X: 88 a 98) ===
        CrearPlataforma(padre, "Pozo3_Puente_Bajo1", new Vector2(90.5f, -2.5f), new Vector2(3.2f, 0.5f));
        CrearPlataforma(padre, "Pozo3_Puente_Bajo2", new Vector2(95.5f, -1.0f), new Vector2(3.2f, 0.5f));
        CrearPlataforma(padre, "Pozo3_CableAereo", new Vector2(93f, 3.2f), new Vector2(5f, 0.5f));

        // === ZONA 6: PATIO FINAL Y LLEGADA (X: 100 a 130) ===
        CrearPlataforma(padre, "Final_Balcon_1", new Vector2(106f, -1.0f), new Vector2(4.5f, 0.5f));
        CrearPlataforma(padre, "Final_Techo_Llegada", new Vector2(116f, 2.5f), new Vector2(7f, 0.6f));
        CrearPlataforma(padre, "Final_Balcon_2", new Vector2(124f, -0.5f), new Vector2(4.5f, 0.5f));
    }

    /// <summary>
    /// Plataformas con texturizado TILED continuo: escala Vector3.one y tamaño por sr.size.
    /// </summary>
    private void CrearPlataforma(Transform padre, string nombre, Vector2 posicion, Vector2 tamano)
    {
        GameObject plat = new GameObject(nombre);
        plat.transform.SetParent(padre);
        plat.transform.position = posicion;
        plat.transform.localScale = Vector3.one;
        plat.tag = tagSuelo;

        int layerGround = LayerMask.NameToLayer("Ground");
        if (layerGround != -1) plat.layer = layerGround;

        BoxCollider2D col = plat.AddComponent<BoxCollider2D>();
        col.size = tamano;

        // Visual unificado de plataforma de parque (TILED continuo)
        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(plat.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = Vector3.one;

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = (spritePlataforma != null) ? spritePlataforma : (spritePisoHuanca != null ? spritePisoHuanca : spritePrototipo);
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        sr.size = tamano;
        sr.sortingOrder = 1;
    }

    private void ConstruirCajas(Transform padre)
    {
        CrearCaja(padre, "Caja_Entrada", new Vector2(8f, -4.0f), new Vector2(1.4f, 1.4f));
        CrearCaja(padre, "Caja_Plaza", new Vector2(32f, -4.0f), new Vector2(1.5f, 1.5f));
        CrearCaja(padre, "Caja_Callejon", new Vector2(66f, -4.0f), new Vector2(1.5f, 1.5f));
        CrearCaja(padre, "Caja_Patio", new Vector2(104f, -4.0f), new Vector2(1.5f, 1.5f));
    }

    private void CrearCaja(Transform padre, string nombre, Vector2 posicion, Vector2 tamano)
    {
        GameObject caja = new GameObject(nombre);
        caja.transform.SetParent(padre);
        caja.transform.position = posicion;
        caja.transform.localScale = Vector3.one;
        caja.tag = tagSuelo;

        int layerGround = LayerMask.NameToLayer("Ground");
        if (layerGround != -1) caja.layer = layerGround;

        BoxCollider2D col = caja.AddComponent<BoxCollider2D>();
        col.size = tamano;

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(caja.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = Vector3.one;

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = (spritePiedraHuanca != null) ? spritePiedraHuanca : spritePrototipo;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        sr.size = tamano;
    }

    private void ConstruirCartelesInformativos(Transform padre)
    {
        CrearCartel(padre, "Cartel_ODS15_Entrada", new Vector2(6f, -3.7f),
            "ODS 15: En Huancayo se generan mas de 300 toneladas de residuos al dia. Cuidemos el Cerrito de la Libertad y el Parque de la Identidad!");
        
        CrearCartel(padre, "Cartel_ODS15_Plaza", new Vector2(38f, -3.7f),
            "ODS 15: Una botella de plastico tarda hasta 500 anos en descomponerse. Recolecta las 15 botellas para el Eco-Contenedor!");
        
        CrearCartel(padre, "Cartel_ODS15_Callejon", new Vector2(72f, -3.7f),
            "ODS 15: Los Fangosos nacen de la contaminacion de suelos y aguas. Purificalos con agua limpia para salvar la flora local!");
    }

    private void CrearCartel(Transform padre, string nombre, Vector2 posicion, string mensaje)
    {
        GameObject cartel = new GameObject(nombre);
        cartel.transform.SetParent(padre);
        cartel.transform.position = posicion;
        cartel.transform.localScale = Vector3.one;

        BoxCollider2D col = cartel.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(2.5f, 2.5f);

        CartelInformativo ci = cartel.AddComponent<CartelInformativo>();
        ci.ConfigurarMensaje(mensaje);

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(cartel.transform);
        visual.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        if (spriteCartel != null)
        {
            sr.sprite = spriteCartel;
            AjustarEscalaVisual(visual, sr, 1.6f, 1.6f);
        }
        else
        {
            sr.sprite = spritePrototipo;
            sr.color = new Color(0.6f, 0.4f, 0.2f);
            visual.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        }
        sr.sortingOrder = 3;
    }

    private void ConstruirBotellas(Transform padre)
    {
        // Arco 1: Entrada sobre andamios (3 botellas)
        CrearArcoBotellas(padre, 12f, 16f, -1.5f, 1.2f, 3);
        // Arco 2: Techo de la Plaza central (3 botellas)
        CrearArcoBotellas(padre, 39f, 45f, 3.2f, 4.2f, 3);
        // Arco 3: Callejón y torre de vigilancia (3 botellas)
        CrearArcoBotellas(padre, 72f, 78f, 4.0f, 5.5f, 3);
    }

    private void CrearArcoBotellas(Transform padre, float xIni, float xFin, float yIni, float yFin, int cantidad)
    {
        for (int i = 0; i < cantidad; i++)
        {
            float t = (cantidad == 1) ? 0f : (float)i / (cantidad - 1);
            float x = Mathf.Lerp(xIni, xFin, t);
            float y = Mathf.Lerp(yIni, yFin, t) + Mathf.Sin(t * Mathf.PI) * 0.9f;

            GameObject bot = new GameObject("Botella_Plastico");
            bot.transform.SetParent(padre);
            bot.transform.position = new Vector3(x, y, 0f);
            bot.transform.localScale = Vector3.one;
            bot.tag = tagColeccionable;

            BoxCollider2D col = bot.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.5f, 0.7f);

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(bot.transform);
            visual.transform.localPosition = Vector3.zero;

            SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
            if (spriteBotellaPlastico != null)
            {
                sr.sprite = spriteBotellaPlastico;
                AjustarEscalaVisual(visual, sr, 0.45f, 0.7f);
            }
            else
            {
                sr.sprite = spritePrototipo;
                sr.color = new Color(0f, 0.9f, 1f);
                visual.transform.localScale = new Vector3(0.45f, 0.7f, 1f);
            }
            sr.sortingOrder = 5;

            visual.AddComponent<RotadorBotella>();
            bot.AddComponent<BotellaVidrio>();
        }
    }

    private void ConstruirEnemigos(Transform padre)
    {
        // 4 Patrulleros
        CrearEnemigo(padre, "Fangoso_Patrullero_Entrada", new Vector2(13f, -4.0f), EnemyManager.TipoEnemigo.Patrullero);
        CrearEnemigo(padre, "Fangoso_Patrullero_Plaza", new Vector2(35f, -4.0f), EnemyManager.TipoEnemigo.Patrullero);
        CrearEnemigo(padre, "Fangoso_Patrullero_TechoPlaza", new Vector2(43f, 3.6f), EnemyManager.TipoEnemigo.Patrullero);
        CrearEnemigo(padre, "Fangoso_Patrullero_Callejon", new Vector2(76f, -4.0f), EnemyManager.TipoEnemigo.Patrullero);

        // 2 Tiradores
        CrearEnemigo(padre, "Fangoso_Tirador_Entrada", new Vector2(16.5f, 1.8f), EnemyManager.TipoEnemigo.Tirador);
        CrearEnemigo(padre, "Fangoso_Tirador_TorreAlta", new Vector2(82f, 7.2f), EnemyManager.TipoEnemigo.Tirador);
    }

    private void CrearEnemigo(Transform padre, string nombre, Vector2 posicion, EnemyManager.TipoEnemigo tipo)
    {
        GameObject enemigo = new GameObject(nombre);
        enemigo.transform.SetParent(padre);
        enemigo.transform.position = posicion;
        enemigo.transform.localScale = Vector3.one;
        enemigo.tag = tagEnemigo;

        BoxCollider2D col = enemigo.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.1f, 1.3f);

        Rigidbody2D rb = enemigo.AddComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(enemigo.transform);
        visual.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        if (spriteMonstruoBarro != null)
        {
            sr.sprite = spriteMonstruoBarro;
            AjustarEscalaVisual(visual, sr, 1.4f, 1.4f);
        }
        else
        {
            sr.sprite = spritePrototipo;
            sr.color = new Color(0.48f, 0.32f, 0.18f);
            visual.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
        }
        sr.sortingOrder = 4;

#if UNITY_EDITOR
        var ctrlFangoso = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/FangosoAnimator.controller");
        if (ctrlFangoso != null)
        {
            Animator anim = visual.AddComponent<Animator>();
            anim.runtimeAnimatorController = ctrlFangoso;
        }
#endif

        EnemyManager em = enemigo.AddComponent<EnemyManager>();
        em.ConfigurarEscalaVisual(visual.transform.localScale);

#if UNITY_EDITOR
        UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(em);
        so.FindProperty("tipo").enumValueIndex = (int)tipo;
        so.FindProperty("capaObstaculosSuelo").intValue = LayerMask.GetMask("Ground");
        
        var prefabBala = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BalaBarro.prefab");
        if (prefabBala == null) prefabBala = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BalaEnemiga.prefab");
        if (prefabBala != null) so.FindProperty("prefabBalaEnemiga").objectReferenceValue = prefabBala;

        var prefabBotella = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BotellaPlastico.prefab");
        if (prefabBotella != null) so.FindProperty("prefabBotellaDrop").objectReferenceValue = prefabBotella;

        so.ApplyModifiedProperties();
#endif
    }

    private void ConstruirContenedorMeta(Transform padre)
    {
        GameObject meta = new GameObject("Contenedor_Meta_Reciclaje");
        meta.transform.SetParent(padre);
        meta.transform.position = new Vector2(132f, -3.8f);
        meta.transform.localScale = Vector3.one;

        BoxCollider2D col = meta.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(2.0f, 2.3f);

        ContenedorMeta cm = meta.AddComponent<ContenedorMeta>();

#if UNITY_EDITOR
        UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(cm);
        so.FindProperty("botellasRequeridas").intValue = 15;
        so.ApplyModifiedProperties();
#endif

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(meta.transform);
        visual.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        if (spriteEstacionReciclaje != null)
        {
            sr.sprite = spriteEstacionReciclaje;
            AjustarEscalaVisual(visual, sr, 2.0f, 2.3f);
        }
        else
        {
            sr.sprite = spritePrototipo;
            sr.color = new Color(0.2f, 0.8f, 0.3f);
            visual.transform.localScale = new Vector3(2f, 2.3f, 1f);
        }
        sr.sortingOrder = 3;
    }

    private void ConstruirKillZone(Transform padre)
    {
        GameObject kz = new GameObject("KillZone_Pozos");
        kz.transform.SetParent(padre);
        kz.transform.position = new Vector2(65f, -12f);
        kz.transform.localScale = Vector3.one;

        BoxCollider2D col = kz.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(170f, 4f);
    }

    private void ActualizarSpriteIllari()
    {
        GameObject player = GameObject.Find("Illari_Player");
        if (player != null)
        {
            Transform visual = player.transform.Find("Visual");
            if (visual != null)
            {
                SpriteRenderer sr = visual.GetComponent<SpriteRenderer>();
                if (sr != null && spriteIyari != null)
                {
                    sr.sprite = spriteIyari;
                    sr.color = Color.white;
                    visual.localScale = Vector3.one;
                    sr.sortingOrder = 6;
                }
            }

            BoxCollider2D col = player.GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.size = new Vector2(0.75f, 1.55f);
                col.offset = Vector2.zero;
            }

            IllariPlatformer ip = player.GetComponent<IllariPlatformer>();
            if (ip != null && visual != null)
            {
                ip.ConfigurarEscalaVisual(visual.localScale);
            }

#if UNITY_EDITOR
            var ctrl = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/IyariAnimator.controller");
            if (ctrl != null)
            {
                Animator anim = player.GetComponent<Animator>();
                if (anim == null) anim = player.AddComponent<Animator>();
                anim.runtimeAnimatorController = ctrl;
            }
#endif
        }
    }
}
