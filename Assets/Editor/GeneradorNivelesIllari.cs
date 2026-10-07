using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Animations;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// Generador maestro para la campaña completa de 9 niveles de 'Illari' (Huancayo 2026):
/// Nivel 1: Parque de la Identidad Huanca (Parkour, Cantutas y Charcos Radiactivos)
/// Nivel 2: Cerrito de la Libertad (Laderas Escalonadas, Miradores y Fangosos Voladores)
/// Nivel 3: Parque Túpac Amaru (Laguna Ácida, Tablones Inestables, Trampolines y Géysers)
/// Nivel 4: Parque de los Sombreros (Cintas Transportadoras, Sombreros Giratorios y Jefe Titán)
/// Nivel 5: Plaza Constitución & Huamanmarca (Relojes Móviles, Drenajes y Guardias Blindados)
/// Nivel 6: Bosque Dorado de Paccha (Quenuales, Tablones Inestables y Fangosos Aéreos)
/// Nivel 7: Ribera del Río Shullcas & Parque Peñaloza (Corrientes Rápidas y Fangosos Cañoneros)
/// Nivel 8: Formaciones Rocosas de Torre Torre (Agujas de Arcilla, Súper Trampolines y Plataformas Giratorias)
/// Nivel 9: Cumbres del Nevado Huaytapallana (Abismos Glaciales, Hielo Inestable y Jefe Supremo: Coloso Huaytapallana)
/// 
/// Garantiza:
/// - Matemática inamovible de EXACTAMENTE 15 botellas por nivel (9 en terreno + 6 de enemigos/jefe).
/// - Fondos 16:9 a proporción nativa exacta sin distorsión.
/// - Cámaras con auto-seguimiento suave al jugador, encuadre calibrado y límites perimetrales.
/// - Animaciones completas de Illari (Caminar, Saltar, Disparo con retroceso) y proyectil de agua (gotas fluidas y salpicadura).
/// - HUD completo: Barra de vida visual con fill gradiente y corazones, contador de botellas ODS 15, título del nivel.
/// - Menú de Pausa accesible por botón en pantalla y teclas ESC/P, con Guardar Partida, Cargar, Reiniciar y Salir.
/// - Registro automático de las 9 escenas en EditorBuildSettings.
/// </summary>
public class GeneradorNivelesIllari
{
    private static Sprite sprPiso;
    private static Sprite sprPlat;
    private static Sprite sprLago;
    private static Sprite sprBotella;
    private static Sprite sprCartel;
    private static Sprite sprMeta;
    private static Sprite sprBalaBarro;
    private static GameObject prefabBalaBarro;
    private static GameObject prefabBotella;

    [MenuItem("Illari/1. RECONSTRUIR TODOS LOS NIVELES (Fondos Unicos y Suelo sin Vacios) %#r")]
    [MenuItem("Iyari/Generar Campaña Completa 9 Niveles (Huancayo 2026)")]
    public static void GenerarTodosLosNiveles()
    {
        ConfigurarNuevasTexturas();
        ConfigurarAnimatorIyari();
        CargarRecursosComunes();

        ConstruirNivel1();
        ConstruirNivel2();
        ConstruirNivel3();
        ConstruirNivel4();
        ConstruirNivel5();
        ConstruirNivel6();
        ConstruirNivel7();
        ConstruirNivel8();
        ConstruirNivel9();

        ActualizarBuildSettings();

        // Volver a abrir el Nivel 1 como escena principal activa
        EditorSceneManager.OpenScene("Assets/Scenes/NivelParqueColonial.unity");
        Debug.Log("<color=#00FF88><b>[ILLARI 2026]</b> ¡Campaña completa de 9 niveles generada con éxito con Fondo Único seguidor, suelos masivos sin vacíos y HUD!</color>");
    }

    [MenuItem("Illari/2. Reconstruir Solo Nivel 1 (Parque Identidad Huanca)")]
    public static void ReconstruirSoloNivel1()
    {
        ConfigurarNuevasTexturas();
        ConfigurarAnimatorIyari();
        CargarRecursosComunes();
        ConstruirNivel1();
        EditorSceneManager.OpenScene("Assets/Scenes/NivelParqueColonial.unity");
        Debug.Log("<color=#00FF88><b>[ILLARI 2026]</b> ¡Nivel 1 regenerado con Fondo Único seguidor y suelo macizo sin vacíos!</color>");
    }

    private static void ConfigurarNuevasTexturas()
    {
        string[] rutasSprites = new string[]
        {
            "Assets/Sprites/PisoParqueHuanca.png",
            "Assets/Sprites/PlataformaParque.png",
            "Assets/Sprites/FondoParqueModernoSeamless.png",
            "Assets/Sprites/FondoCerritoLibertad.png",
            "Assets/Sprites/FondoTupacAmaru.png",
            "Assets/Sprites/FondoParqueSombreros.png",
            "Assets/Sprites/FondoHuamanmarca.png",
            "Assets/Sprites/FondoBosqueDorado.png",
            "Assets/Sprites/FondoRioShullcas.png",
            "Assets/Sprites/FondoTorreTorre.png",
            "Assets/Sprites/FondoHuaytapallana.png",
            "Assets/Sprites/FangosoVolador.png",
            "Assets/Sprites/FangosoBlindado.png",
            "Assets/Sprites/FangosoTitan.png",
            "Assets/Sprites/FangosoCanionero.png",
            "Assets/Sprites/ColosoHuaytapallana.png",
            "Assets/Sprites/ProyectilAgua/GotaAgua_0.png",
            "Assets/Sprites/ProyectilAgua/GotaAgua_1.png",
            "Assets/Sprites/ProyectilAgua/GotaAgua_2.png",
            "Assets/Sprites/ProyectilAgua/GotaAgua_3.png",
            "Assets/Sprites/ProyectilAgua/SplashAgua_0.png",
            "Assets/Sprites/ProyectilAgua/SplashAgua_1.png",
            "Assets/Sprites/ProyectilAgua/SplashAgua_2.png",
            "Assets/Sprites/ProyectilAgua/SplashAgua_3.png",
            "Assets/Sprites/Iyari/Shoot_0.png",
            "Assets/Sprites/Iyari/Shoot_1.png",
            "Assets/Sprites/Iyari/Shoot_2.png",
            "Assets/Sprites/Iyari/Shoot_3.png"
        };

        // Añadir sprites de Fangoso patrullero
        for (int i = 0; i < 6; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/Fangoso/Walk_{i}.png");
        for (int i = 0; i < 4; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/Fangoso/Idle_{i}.png");

        // Añadir sprites de Fangoso tirador
        for (int i = 0; i < 4; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/FangosoTirador/Idle_{i}.png");
        for (int i = 0; i < 5; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/FangosoTirador/Shoot_{i}.png");
        for (int i = 0; i < 2; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/FangosoTirador/Hurt_{i}.png");
        for (int i = 0; i < 5; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/FangosoTirador/Death_{i}.png");

        // Añadir sprites de Fangoso volador
        for (int i = 0; i < 6; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/FangosoVolador/Fly_{i}.png");
        for (int i = 0; i < 4; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/FangosoVolador/Dive_{i}.png");
        for (int i = 0; i < 2; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/FangosoVolador/Hurt_{i}.png");
        for (int i = 0; i < 5; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/FangosoVolador/Death_{i}.png");

        // Añadir sprites de Fangoso blindado
        rutasSprites = AnadirRuta(rutasSprites, "Assets/Sprites/FangosoBlindado.png");
        for (int i = 0; i < 4; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/FangosoBlindado/Idle_{i}.png");
        for (int i = 0; i < 6; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/FangosoBlindado/Walk_{i}.png");
        for (int i = 0; i < 4; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/FangosoBlindado/ShieldBash_{i}.png");
        for (int i = 0; i < 2; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/FangosoBlindado/Hurt_{i}.png");
        for (int i = 0; i < 6; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/FangosoBlindado/Death_{i}.png");

        // Añadir sprites de Fangoso cañonero
        rutasSprites = AnadirRuta(rutasSprites, "Assets/Sprites/FangosoCanionero.png");
        for (int i = 0; i < 4; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/FangosoCanionero/Idle_{i}.png");
        for (int i = 0; i < 6; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/FangosoCanionero/Mortar_{i}.png");
        for (int i = 0; i < 2; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/FangosoCanionero/Hurt_{i}.png");
        for (int i = 0; i < 5; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/FangosoCanionero/Death_{i}.png");

        // Añadir sprites de Fangoso titán
        rutasSprites = AnadirRuta(rutasSprites, "Assets/Sprites/FangosoTitan.png");
        for (int i = 0; i < 4; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/FangosoTitan/Idle_{i}.png");
        for (int i = 0; i < 6; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/FangosoTitan/GroundSlam_{i}.png");
        for (int i = 0; i < 4; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/FangosoTitan/Roar_{i}.png");
        for (int i = 0; i < 3; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/FangosoTitan/Hurt_{i}.png");
        for (int i = 0; i < 6; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/FangosoTitan/Death_{i}.png");

        // Añadir sprites de Coloso Huaytapallana
        rutasSprites = AnadirRuta(rutasSprites, "Assets/Sprites/ColosoHuaytapallana.png");
        for (int i = 0; i < 4; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/ColosoHuaytapallana/Stance_{i}.png");
        for (int i = 0; i < 6; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/ColosoHuaytapallana/IceSpikes_{i}.png");
        for (int i = 0; i < 6; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/ColosoHuaytapallana/AcidBeam_{i}.png");
        for (int i = 0; i < 3; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/ColosoHuaytapallana/Hurt_{i}.png");
        for (int i = 0; i < 6; i++) rutasSprites = AnadirRuta(rutasSprites, $"Assets/Sprites/ColosoHuaytapallana/Death_{i}.png");

        // Fondos
        rutasSprites = AnadirRuta(rutasSprites, "Assets/Sprites/FondoPlazaConstitucion.png");
        rutasSprites = AnadirRuta(rutasSprites, "Assets/Sprites/FondoHuamanmarca.png");

        bool reimported = false;
        foreach (string r in rutasSprites)
        {
            TextureImporter imp = AssetImporter.GetAtPath(r) as TextureImporter;
            if (imp != null)
            {
                bool needSave = false;
                if (imp.textureType != TextureImporterType.Sprite) { imp.textureType = TextureImporterType.Sprite; needSave = true; }
                if (imp.spriteImportMode != SpriteImportMode.Single) { imp.spriteImportMode = SpriteImportMode.Single; needSave = true; }
                if (!imp.alphaIsTransparency) { imp.alphaIsTransparency = true; needSave = true; }
                if (imp.mipmapEnabled) { imp.mipmapEnabled = false; needSave = true; }
                if (imp.filterMode != FilterMode.Point) { imp.filterMode = FilterMode.Point; needSave = true; }
                if (imp.textureCompression != TextureImporterCompression.Uncompressed) { imp.textureCompression = TextureImporterCompression.Uncompressed; needSave = true; }

                if (r.Contains("Piso") || r.Contains("Plataforma"))
                {
                    if (imp.wrapMode != TextureWrapMode.Repeat) { imp.wrapMode = TextureWrapMode.Repeat; needSave = true; }
                    if (r.Contains("Piso"))
                    {
                        if (imp.spriteBorder != new Vector4(0, 0, 0, 85))
                        {
                            imp.spriteBorder = new Vector4(0, 0, 0, 85);
                            needSave = true;
                        }
                    }
                    TextureImporterSettings tis = new TextureImporterSettings();
                    imp.ReadTextureSettings(tis);
                    if (tis.spriteMeshType != SpriteMeshType.FullRect)
                    {
                        tis.spriteMeshType = SpriteMeshType.FullRect;
                        imp.SetTextureSettings(tis);
                        needSave = true;
                    }
                }

                if (needSave)
                {
                    imp.SaveAndReimport();
                    reimported = true;
                }
            }
        }
        if (reimported)
        {
            AssetDatabase.Refresh();
        }
    }

    private static string[] AnadirRuta(string[] arr, string nueva)
    {
        string[] res = new string[arr.Length + 1];
        arr.CopyTo(res, 0);
        res[arr.Length] = nueva;
        return res;
    }

    private static void ConfigurarAnimatorIyari()
    {
        string pathController = "Assets/Animations/IyariAnimator.controller";
        var ac = AssetDatabase.LoadAssetAtPath<AnimatorController>(pathController);
        if (ac == null) return;

        // 1. Asegurar parámetro 'disparar' (Trigger)
        bool tieneDisparar = false;
        foreach (var p in ac.parameters)
        {
            if (p.name == "disparar") { tieneDisparar = true; break; }
        }
        if (!tieneDisparar)
        {
            ac.AddParameter("disparar", AnimatorControllerParameterType.Trigger);
        }

        // 2. Cargar clip Iyari_Shoot
        AnimationClip clipShoot = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/Iyari/Iyari_Shoot.anim");

        // 3. Revisar capa base
        var sm = ac.layers[0].stateMachine;
        AnimatorState stateShoot = null;
        AnimatorState stateIdle = null;
        AnimatorState stateRun = null;

        foreach (var cs in sm.states)
        {
            if (cs.state.name == "Iyari_Shoot") stateShoot = cs.state;
            else if (cs.state.name == "Iyari_Idle") stateIdle = cs.state;
            else if (cs.state.name == "Iyari_Run") stateRun = cs.state;
        }

        if (stateShoot == null)
        {
            stateShoot = sm.AddState("Iyari_Shoot", new Vector3(520, 20, 0));
        }
        if (clipShoot != null)
        {
            stateShoot.motion = clipShoot;
        }

        // Transición Idle -> Shoot
        if (stateIdle != null && stateShoot != null)
        {
            bool transExiste = false;
            foreach (var t in stateIdle.transitions)
            {
                if (t.destinationState == stateShoot) { transExiste = true; break; }
            }
            if (!transExiste)
            {
                var tr = stateIdle.AddTransition(stateShoot);
                tr.hasExitTime = false;
                tr.duration = 0f;
                tr.AddCondition(AnimatorConditionMode.If, 0, "disparar");
            }
        }

        // Transición Run -> Shoot
        if (stateRun != null && stateShoot != null)
        {
            bool transExiste = false;
            foreach (var t in stateRun.transitions)
            {
                if (t.destinationState == stateShoot) { transExiste = true; break; }
            }
            if (!transExiste)
            {
                var tr = stateRun.AddTransition(stateShoot);
                tr.hasExitTime = false;
                tr.duration = 0f;
                tr.AddCondition(AnimatorConditionMode.If, 0, "disparar");
            }
        }

        // Transición Shoot -> Idle
        if (stateShoot != null && stateIdle != null)
        {
            bool transExiste = false;
            foreach (var t in stateShoot.transitions)
            {
                if (t.destinationState == stateIdle) { transExiste = true; break; }
            }
            if (!transExiste)
            {
                var tr = stateShoot.AddTransition(stateIdle);
                tr.hasExitTime = true;
                tr.exitTime = 0.9f;
                tr.duration = 0.05f;
            }
        }

        EditorUtility.SetDirty(ac);
        AssetDatabase.SaveAssets();
    }

    private static void CargarRecursosComunes()
    {
        sprPiso = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PisoParqueHuanca.png");
        sprPlat = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PlataformaParque.png");
        if (sprPlat == null) sprPlat = sprPiso;

        sprLago = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/LagoToxico.png");
        sprBotella = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/BotellaPlastico.png");
        sprCartel = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/CartelInformativo.png");
        sprMeta = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/EstacionReciclaje.png");
        sprBalaBarro = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/BolaBarro.png");

        prefabBalaBarro = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BalaBarro.prefab");
        prefabBotella = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BotellaPlastico.prefab");

        // Configurar proyectil de agua animado
        GameObject prefabBala = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BalaAgua.prefab");
        if (prefabBala != null)
        {
            Bala b = prefabBala.GetComponent<Bala>();
            if (b != null)
            {
                SerializedObject soBala = new SerializedObject(b);
                SerializedProperty propGota = soBala.FindProperty("spritesGotaAgua");
                if (propGota != null)
                {
                    propGota.arraySize = 4;
                    for (int i = 0; i < 4; i++)
                    {
                        propGota.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/ProyectilAgua/GotaAgua_{i}.png");
                    }
                }
                SerializedProperty propSplash = soBala.FindProperty("spritesSplash");
                if (propSplash != null)
                {
                    propSplash.arraySize = 4;
                    for (int i = 0; i < 4; i++)
                    {
                        propSplash.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/ProyectilAgua/SplashAgua_{i}.png");
                    }
                }
                soBala.ApplyModifiedProperties();
                EditorUtility.SetDirty(prefabBala);
            }
        }
    }

    // ==========================================
    // NIVEL 1: PARQUE DE LA IDENTIDAD HUANCA
    // ==========================================
    private static void ConstruirNivel1()
    {
        string scenePath = "Assets/Scenes/NivelParqueColonial.unity";
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Sprite bg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/FondoParqueIdentidad.png");
        if (bg == null) bg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/FondoParqueModernoSeamless.png");
        GameObject root = new GameObject("--- NIVEL_1_IDENTIDAD_HUANCA ---");

        CrearEntornoBase(bg, 8, 32.25f, 26.0f, new Vector3(-6f, -1.8f, -10f), 6.0f, -6f, 135f, -3.5f, 6.0f, "Assets/Audio/Music/Musica_NivelIdentidad.wav");

        Transform grpSuelos = CrearGrupo(root.transform, "01_Superficies_Y_Pozos");
        Transform grpPlataformas = CrearGrupo(root.transform, "02_Plataformas_Parkour");
        Transform grpLagos = CrearGrupo(root.transform, "03_Lagos_Toxicos");
        Transform grpCarteles = CrearGrupo(root.transform, "04_Carteles_ODS15");
        Transform grpBotellas = CrearGrupo(root.transform, "05_Botellas_Plastico_9");
        Transform grpEnemigos = CrearGrupo(root.transform, "06_Enemigos_6");

        // 1. Suelos base con pozos
        CrearBloqueSuelo(grpSuelos, "Suelo_Entrada", new Vector2(4f, -5.5f), new Vector2(30f, 2f));
        CrearBloqueSuelo(grpSuelos, "Suelo_Plaza", new Vector2(39.5f, -5.5f), new Vector2(25f, 2f));
        CrearBloqueSuelo(grpSuelos, "Suelo_Callejon", new Vector2(74.5f, -5.5f), new Vector2(27f, 2f));
        CrearBloqueSuelo(grpSuelos, "Suelo_PatioFinal", new Vector2(117f, -5.5f), new Vector2(38f, 2f));

        CrearMuroTope(grpSuelos, -11f, 22f);
        CrearMuroTope(grpSuelos, 137f, 22f);

        // 2. Lagos tóxicos
        CrearLagoToxico(grpLagos, new Vector2(23f, -6.6f), new Vector2(7.5f, 1.2f));
        CrearLagoToxico(grpLagos, new Vector2(56.5f, -6.6f), new Vector2(8.5f, 1.2f));
        CrearLagoToxico(grpLagos, new Vector2(93f, -6.6f), new Vector2(9.5f, 1.2f));
        CrearLagoToxico(grpLagos, new Vector2(70f, -4.3f), new Vector2(3.5f, 0.6f));

        // 3. Plataformas parkour
        CrearPlataformaSimple(grpPlataformas, "Plat_Plaza_Baja", new Vector2(30f, -2.5f), new Vector2(5f, 0.6f));
        CrearPlataformaSimple(grpPlataformas, "Plat_Plaza_Alta", new Vector2(38f, 0.2f), new Vector2(4.5f, 0.6f));
        CrearPlataformaSimple(grpPlataformas, "Plat_Callejon_1", new Vector2(64f, -2.0f), new Vector2(4f, 0.6f));
        CrearPlataformaSimple(grpPlataformas, "Plat_Callejon_2", new Vector2(73f, 0.8f), new Vector2(5f, 0.6f));
        CrearPlataformaSimple(grpPlataformas, "Plat_Patio_1", new Vector2(102f, -2.2f), new Vector2(4.5f, 0.6f));
        CrearPlataformaSimple(grpPlataformas, "Plat_Patio_2", new Vector2(111f, 0.5f), new Vector2(5.5f, 0.6f));

        // 4. Carteles ecológicos ODS 15
        CrearCartel(grpCarteles, new Vector2(10f, -4.0f), "En Huancayo se generan más de 180 toneladas de residuos diarios. ¡La mitad termina contaminando parques y el río Shullcas!", "Parque de la Identidad Huanca");
        CrearCartel(grpCarteles, new Vector2(48f, -4.0f), "Una sola botella de plástico tarda hasta 500 años en degradarse en los valles del Mantaro.", "Parque de la Identidad Huanca");
        CrearCartel(grpCarteles, new Vector2(82f, -4.0f), "ODS 15: Vida de Ecosistemas Terrestres. Proteger la biodiversidad de Junín comienza limpiando nuestros suelos.", "Parque de la Identidad Huanca");

        // 5. 9 Botellas en escenario
        CrearBotella(grpBotellas, new Vector2(8f, -3.5f));
        CrearBotella(grpBotellas, new Vector2(30f, -1.2f));
        CrearBotella(grpBotellas, new Vector2(38f, 1.5f));
        CrearBotella(grpBotellas, new Vector2(45f, -3.5f));
        CrearBotella(grpBotellas, new Vector2(64f, -0.7f));
        CrearBotella(grpBotellas, new Vector2(73f, 2.1f));
        CrearBotella(grpBotellas, new Vector2(80f, -3.5f));
        CrearBotella(grpBotellas, new Vector2(102f, -0.9f));
        CrearBotella(grpBotellas, new Vector2(111f, 1.8f));

        // 6. 6 Monstruos Fangosos — posiciones seguras sobre suelo y plataformas (6 botellas drop)
        CrearEnemigoPatrullero(grpEnemigos, "Fangoso_Patrulla_1", new Vector2(10f, -4.50f));   // Suelo Entrada [-11, 19]
        CrearEnemigoTirador(grpEnemigos, "Fangoso_Tirador_1", new Vector2(38f, 0.40f));        // Plataforma alta
        CrearEnemigoPatrullero(grpEnemigos, "Fangoso_Patrulla_2", new Vector2(44f, -4.50f));   // Suelo Plaza [27, 52]
        CrearEnemigoTirador(grpEnemigos, "Fangoso_Tirador_2", new Vector2(73f, 1.00f));        // Plataforma callejon
        CrearEnemigoPatrullero(grpEnemigos, "Fangoso_Patrulla_3", new Vector2(80f, -4.50f));   // Suelo Callejon [61, 88]
        CrearEnemigoPatrullero(grpEnemigos, "Fangoso_Patrulla_4", new Vector2(118f, -4.50f));  // Suelo Patio Final [98, 136]

        // 7. Cápsulas de Poder y Mejoras
        Transform grpPoderes = CrearGrupo(root.transform, "07_Poderes_Mejoras");
        CrearItemPoder(grpPoderes, new Vector2(30f, -1.8f), ItemMejora.TipoMejora.DisparoTriple);
        CrearItemPoder(grpPoderes, new Vector2(64f, 1.2f), ItemMejora.TipoMejora.EscudoAcuatico);
        CrearItemPoder(grpPoderes, new Vector2(88f, 1.5f), ItemMejora.TipoMejora.LaserAgua);
        CrearItemPoder(grpPoderes, new Vector2(102f, 0.8f), ItemMejora.TipoMejora.CuracionVida);

        CrearMeta(root.transform, new Vector2(132f, -3.8f));
        CrearJugadorYUI(new Vector2(-8f, -4.50f));

        EditorSceneManager.SaveScene(scene, scenePath);
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Nivel1.unity");
    }

    // ==========================================
    // NIVEL 2: CERRITO DE LA LIBERTAD
    // ==========================================
    private static void ConstruirNivel2()
    {
        string scenePath = "Assets/Scenes/Nivel2_CerritoLibertad.unity";
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Sprite bg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/FondoCerritoLibertad.png");
        GameObject root = new GameObject("--- NIVEL_2_CERRITO_LIBERTAD ---");

        CrearEntornoBase(bg, 8, 32.25f, 26.0f, new Vector3(-6f, -1.5f, -10f), 6.5f, -6f, 138f, -3.5f, 8.5f, "Assets/Audio/Music/Musica_NivelCerrito.wav");

        Transform grpSuelos = CrearGrupo(root.transform, "01_Terreno_Laderas");
        Transform grpPlataformas = CrearGrupo(root.transform, "02_Plataformas_Mirador");
        Transform grpBotellas = CrearGrupo(root.transform, "03_Botellas_Plastico_9");
        Transform grpEnemigos = CrearGrupo(root.transform, "04_Enemigos_6");

        // Terrenos escalonados ascendentes
        CrearBloqueSuelo(grpSuelos, "Ladera_Inicio", new Vector2(4f, -5.5f), new Vector2(28f, 2f));
        CrearBloqueSuelo(grpSuelos, "Ladera_Media", new Vector2(42f, -3.5f), new Vector2(24f, 2f));
        CrearBloqueSuelo(grpSuelos, "Ladera_Alta", new Vector2(80f, -1.0f), new Vector2(26f, 2f));
        CrearBloqueSuelo(grpSuelos, "Mirador_Cumbre", new Vector2(120f, 1.5f), new Vector2(32f, 2f));

        CrearMuroTope(grpSuelos, -11f, 25f);
        CrearMuroTope(grpSuelos, 140f, 25f);

        // Lagos tóxicos en las depresiones
        CrearLagoToxico(grpSuelos, new Vector2(23f, -6.6f), new Vector2(9f, 1.2f));
        CrearLagoToxico(grpSuelos, new Vector2(60f, -5.0f), new Vector2(10f, 1.2f));
        CrearLagoToxico(grpSuelos, new Vector2(99f, -2.5f), new Vector2(10f, 1.2f));

        // Plataformas escalonadas One-Way
        CrearPlataformaOneWay(grpPlataformas, "Plat_Escalon_1", new Vector2(23f, -3.0f), new Vector2(4.5f, 0.5f));
        CrearPlataformaOneWay(grpPlataformas, "Plat_Escalon_2", new Vector2(33f, -1.0f), new Vector2(4.5f, 0.5f));
        CrearPlataformaOneWay(grpPlataformas, "Plat_Mirador_1", new Vector2(58f, -1.5f), new Vector2(5.0f, 0.5f));
        CrearPlataformaOneWay(grpPlataformas, "Plat_Mirador_2", new Vector2(66f, 1.0f), new Vector2(5.0f, 0.5f));
        CrearPlataformaOneWay(grpPlataformas, "Plat_Cumbre_1", new Vector2(97f, 1.2f), new Vector2(5.0f, 0.5f));
        CrearPlataformaOneWay(grpPlataformas, "Plat_Cumbre_2", new Vector2(107f, 3.5f), new Vector2(5.5f, 0.5f));

        // Carteles
        Transform grpCarteles = CrearGrupo(root.transform, "05_Carteles");
        CrearCartel(grpCarteles, new Vector2(8f, -4.0f), "El Cerrito de la Libertad es el mirador histórico de Huancayo. ¡Protejamos su ladera de residuos plásticos!", "Cerrito de la Libertad");
        CrearCartel(grpCarteles, new Vector2(46f, -2.0f), "El plástico no degradado en las colinas arrastra sedimentos que aceleran la erosión del suelo.", "Cerrito de la Libertad");
        CrearCartel(grpCarteles, new Vector2(84f, 0.5f), "ODS 15: Promover la reforestación urbana en Junín con queñuales y flores de Cantuta.", "Cerrito de la Libertad");

        // 9 Botellas en Escenario
        CrearBotella(grpBotellas, new Vector2(8f, -3.5f));
        CrearBotella(grpBotellas, new Vector2(23f, -1.8f));
        CrearBotella(grpBotellas, new Vector2(33f, 0.3f));
        CrearBotella(grpBotellas, new Vector2(46f, -1.8f));
        CrearBotella(grpBotellas, new Vector2(58f, -0.2f));
        CrearBotella(grpBotellas, new Vector2(66f, 2.3f));
        CrearBotella(grpBotellas, new Vector2(84f, 0.5f));
        CrearBotella(grpBotellas, new Vector2(97f, 2.5f));
        CrearBotella(grpBotellas, new Vector2(107f, 4.8f));

        // 6 Fangosos — posicionados en laderas escalonadas (6 botellas drop)
        CrearEnemigoPatrullero(grpEnemigos, "Fangoso_Patrulla_1", new Vector2(10f, -4.50f));   // Ladera inicio [-10, 18]
        CrearEnemigoVolador(grpEnemigos, "Fangoso_Volador_1", new Vector2(23f, 1.0f));          // Vuela sobre lago 1
        CrearEnemigoPatrullero(grpEnemigos, "Fangoso_Patrulla_2", new Vector2(42f, -2.50f));   // Ladera media [30, 54]
        CrearEnemigoVolador(grpEnemigos, "Fangoso_Volador_2", new Vector2(60f, 2.5f));          // Vuela sobre lago 2
        CrearEnemigoPatrullero(grpEnemigos, "Fangoso_Patrulla_3", new Vector2(80f, 0.00f));    // Ladera alta [67, 93]
        CrearEnemigoPatrullero(grpEnemigos, "Fangoso_Patrulla_4", new Vector2(120f, 2.50f));   // Mirador cumbre [104, 136]

        // 7. Cápsulas de Poder y Mejoras
        Transform grpPoderesN2 = CrearGrupo(root.transform, "07_Poderes_Mejoras");
        CrearItemPoder(grpPoderesN2, new Vector2(23f, -1.2f), ItemMejora.TipoMejora.SuperVelocidad);
        CrearItemPoder(grpPoderesN2, new Vector2(58f, 0.8f), ItemMejora.TipoMejora.LaserAgua);
        CrearItemPoder(grpPoderesN2, new Vector2(66f, 3.2f), ItemMejora.TipoMejora.EscudoAcuatico);
        CrearItemPoder(grpPoderesN2, new Vector2(97f, 3.5f), ItemMejora.TipoMejora.CuracionVida);

        CrearMeta(root.transform, new Vector2(134f, 3.2f));
        CrearJugadorYUI(new Vector2(-8f, -4.50f));

        EditorSceneManager.SaveScene(scene, scenePath);
    }

    // ==========================================
    // NIVEL 3: PARQUE TÚPAC AMARU
    // ==========================================
    private static void ConstruirNivel3()
    {
        string scenePath = "Assets/Scenes/Nivel3_ParqueTupacAmaru.unity";
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Sprite bg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/FondoTupacAmaru.png");
        GameObject root = new GameObject("--- NIVEL_3_PARQUE_TUPAC_AMARU ---");

        CrearEntornoBase(bg, 8, 32.25f, 26.0f, new Vector3(-6f, -1.8f, -10f), 6.5f, -6f, 148f, -4.0f, 7.5f, "Assets/Audio/Music/Musica_NivelBosqueDorado.wav");

        Transform grpSuelos = CrearGrupo(root.transform, "01_Islas_Terrestres");
        Transform grpPlataformas = CrearGrupo(root.transform, "02_Tablones_Y_Trampolines");
        Transform grpPeligros = CrearGrupo(root.transform, "03_Lagunas_Y_Geysers");
        Transform grpBotellas = CrearGrupo(root.transform, "04_Botellas_Plastico_9");
        Transform grpEnemigos = CrearGrupo(root.transform, "05_Enemigos_6");

        // Islas de tierra separadas por una gran laguna tóxica
        CrearBloqueSuelo(grpSuelos, "Muelle_Entrada", new Vector2(4f, -5.5f), new Vector2(26f, 2f));
        CrearBloqueSuelo(grpSuelos, "Isla_Central_1", new Vector2(46f, -5.5f), new Vector2(20f, 2f));
        CrearBloqueSuelo(grpSuelos, "Isla_Sauces_2", new Vector2(88f, -5.5f), new Vector2(22f, 2f));
        CrearBloqueSuelo(grpSuelos, "Plaza_Estatua_Final", new Vector2(130f, -5.5f), new Vector2(32f, 2f));

        CrearMuroTope(grpSuelos, -11f, 25f);
        CrearMuroTope(grpSuelos, 150f, 25f);

        // Fosos inundados (Gran Laguna Ácida)
        CrearLagoToxico(grpPeligros, new Vector2(22f, -6.6f), new Vector2(10f, 1.2f));
        CrearLagoToxico(grpPeligros, new Vector2(67f, -6.6f), new Vector2(21f, 1.2f));
        CrearLagoToxico(grpPeligros, new Vector2(109f, -6.6f), new Vector2(18f, 1.2f));

        // Géysers tóxicos
        CrearGeyser(grpPeligros, "Geyser_1", new Vector2(22f, -5.5f), 2f, 1.5f);
        CrearGeyser(grpPeligros, "Geyser_2", new Vector2(72f, -5.5f), 1.8f, 1.2f);
        CrearGeyser(grpPeligros, "Geyser_3", new Vector2(110f, -5.5f), 2.2f, 1.0f);

        // Tablones inestables que caen
        CrearTablonInestable(grpPlataformas, "Tablon_1", new Vector2(20f, -2.5f), new Vector2(4f, 0.5f));
        CrearTablonInestable(grpPlataformas, "Tablon_2", new Vector2(26f, -0.5f), new Vector2(4f, 0.5f));
        CrearTablonInestable(grpPlataformas, "Tablon_Laguna_A", new Vector2(63f, -2.0f), new Vector2(4.5f, 0.5f));
        CrearTablonInestable(grpPlataformas, "Tablon_Laguna_B", new Vector2(72f, 0.5f), new Vector2(4.5f, 0.5f));
        CrearTablonInestable(grpPlataformas, "Tablon_Final", new Vector2(106f, -1.8f), new Vector2(5f, 0.5f));

        // Trampolines de llantas recicladas
        CrearTrampolin(grpPlataformas, "Trampolin_Llantas_1", new Vector2(42f, -4.0f), 17.5f);
        CrearTrampolin(grpPlataformas, "Trampolin_Llantas_2", new Vector2(84f, -4.0f), 18f);

        // Carteles
        Transform grpCarteles = CrearGrupo(root.transform, "06_Carteles");
        CrearCartel(grpCarteles, new Vector2(8f, -4.0f), "Parque Túpac Amaru: pulmón verde de San Carlos. El estanque sufre por botellas arrojadas.", "Parque Túpac Amaru");
        CrearCartel(grpCarteles, new Vector2(50f, -4.0f), "Las llantas recicladas pueden utilizarse para crear parques infantiles y amortiguadores ecológicos.", "Parque Túpac Amaru");
        CrearCartel(grpCarteles, new Vector2(92f, -4.0f), "ODS 15: Cuidemos los ecosistemas acuáticos y terrestres de los parques urbanos.", "Parque Túpac Amaru");

        // 9 Botellas en Escenario
        CrearBotella(grpBotellas, new Vector2(8f, -3.5f));
        CrearBotella(grpBotellas, new Vector2(20f, -1.0f));
        CrearBotella(grpBotellas, new Vector2(26f, 1.2f));
        CrearBotella(grpBotellas, new Vector2(42f, 1.5f));
        CrearBotella(grpBotellas, new Vector2(63f, -0.3f));
        CrearBotella(grpBotellas, new Vector2(72f, 2.3f));
        CrearBotella(grpBotellas, new Vector2(84f, 2.0f));
        CrearBotella(grpBotellas, new Vector2(100f, -3.5f));
        CrearBotella(grpBotellas, new Vector2(125f, -3.5f));

        // 6 Enemigos (3 Voladores + 3 Tiradores = 6 botellas drop)
        CrearEnemigoTirador(grpEnemigos, "Tirador_Muelle", new Vector2(10f, -4.50f));           // Muelle entrada [-9, 17]
        CrearEnemigoVolador(grpEnemigos, "Volador_Laguna1", new Vector2(22f, 1.5f));           // Vuela sobre lago 1
        CrearEnemigoTirador(grpEnemigos, "Tirador_IslaCentral", new Vector2(46f, -4.50f));      // Isla central [36, 56]
        CrearEnemigoVolador(grpEnemigos, "Volador_Laguna2", new Vector2(67f, 1.8f));           // Vuela sobre lago 2
        CrearEnemigoVolador(grpEnemigos, "Volador_Laguna3", new Vector2(108f, 1.8f));          // Vuela sobre lago 3
        CrearEnemigoTirador(grpEnemigos, "Tirador_Sauces", new Vector2(126f, -4.50f));          // Plaza final [114, 146]

        // 07. Cápsulas de Poder y Mejoras
        Transform grpPoderesN3 = CrearGrupo(root.transform, "07_Poderes_Mejoras");
        CrearItemPoder(grpPoderesN3, new Vector2(26f, 1.5f), ItemMejora.TipoMejora.BombaAgua);
        CrearItemPoder(grpPoderesN3, new Vector2(42f, 0.5f), ItemMejora.TipoMejora.DisparoTriple);
        CrearItemPoder(grpPoderesN3, new Vector2(72f, 2.5f), ItemMejora.TipoMejora.SuperVelocidad);
        CrearItemPoder(grpPoderesN3, new Vector2(106f, 0.5f), ItemMejora.TipoMejora.CuracionVida);

        CrearMeta(root.transform, new Vector2(144f, -3.8f));
        CrearJugadorYUI(new Vector2(-8f, -4.50f));

        EditorSceneManager.SaveScene(scene, scenePath);
    }

    // ==========================================
    // NIVEL 4: PARQUE DE LOS SOMBREROS (EL TAMBO)
    // ==========================================
    private static void ConstruirNivel4()
    {
        string scenePath = "Assets/Scenes/Nivel4_ParqueSombreros.unity";
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Sprite bg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/FondoParqueSombreros.png");
        GameObject root = new GameObject("--- NIVEL_4_PARQUE_SOMBREROS ---");

        CrearEntornoBase(bg, 8, 32.25f, 26.0f, new Vector3(-6f, -1.8f, -10f), 6.0f, -6f, 156f, -4.0f, 7.5f, "Assets/Audio/Music/Musica_NivelCerrito.wav");

        Transform grpSuelos = CrearGrupo(root.transform, "01_Explanada_Industrial");
        Transform grpMecanicas = CrearGrupo(root.transform, "02_Cintas_Y_Sombreros");
        Transform grpBotellas = CrearGrupo(root.transform, "03_Botellas_Plastico_9");
        Transform grpEnemigos = CrearGrupo(root.transform, "04_Enemigos_Y_Jefe");
        Transform grpCartelesN4 = CrearGrupo(root.transform, "05_Carteles_ODS15");

        CrearBloqueSuelo(grpSuelos, "Entrada_Parque", new Vector2(4f, -5.5f), new Vector2(28f, 2f));
        CrearBloqueSuelo(grpSuelos, "Plaza_Huaconada", new Vector2(46f, -5.5f), new Vector2(24f, 2f));
        CrearBloqueSuelo(grpSuelos, "Plaza_Chonguinada", new Vector2(90f, -5.5f), new Vector2(26f, 2f));
        CrearBloqueSuelo(grpSuelos, "Arena_Jefe_Final", new Vector2(138f, -5.5f), new Vector2(38f, 2f));

        CrearMuroTope(grpSuelos, -11f, 25f);
        CrearMuroTope(grpSuelos, 160f, 25f);

        CrearLagoToxico(grpSuelos, new Vector2(24f, -6.6f), new Vector2(10f, 1.2f));
        CrearLagoToxico(grpSuelos, new Vector2(67f, -6.6f), new Vector2(16f, 1.2f));
        CrearLagoToxico(grpSuelos, new Vector2(110f, -6.6f), new Vector2(12f, 1.2f));

        // Cintas transportadoras
        CrearCintaTransportadora(grpMecanicas, "Cinta_Reciclaje_1", new Vector2(24f, -2.5f), new Vector2(8f, 0.6f), 3.5f);
        CrearCintaTransportadora(grpMecanicas, "Cinta_Reciclaje_2", new Vector2(67f, -2.0f), new Vector2(10f, 0.6f), -3.0f);

        // Plataformas giratorias (sombreros andinos)
        CrearPlataformaGiratoria(grpMecanicas, "Sombrero_Giratorio_1", new Vector2(72f, 1.5f), 35f);
        CrearPlataformaGiratoria(grpMecanicas, "Sombrero_Giratorio_2", new Vector2(110f, -1.0f), -40f);

        // Carteles ODS 12 & 15
        CrearCartel(grpCartelesN4, new Vector2(8f, -4.0f), "El Parque de los Sombreros rinde homenaje a la rica tradición artesanal huanca. ¡Cuidemos este espacio cultural libre de plástico!", "Parque de los Sombreros");
        CrearCartel(grpCartelesN4, new Vector2(50f, -4.0f), "Las cintas transportadoras recicladas y la economía circular son clave para reducir la basura urbana en Junín.", "Parque de los Sombreros");
        CrearCartel(grpCartelesN4, new Vector2(92f, -4.0f), "ODS 12: Producción y Consumo Responsables. Reutilizar materiales evita la saturación de los botaderos.", "Parque de los Sombreros");

        // 9 Botellas en Escenario
        CrearBotella(grpBotellas, new Vector2(8f, -3.5f));
        CrearBotella(grpBotellas, new Vector2(24f, -1.0f));
        CrearBotella(grpBotellas, new Vector2(42f, -3.5f));
        CrearBotella(grpBotellas, new Vector2(50f, -3.5f));
        CrearBotella(grpBotellas, new Vector2(67f, -0.5f));
        CrearBotella(grpBotellas, new Vector2(72f, 3.2f));
        CrearBotella(grpBotellas, new Vector2(88f, -3.5f));
        CrearBotella(grpBotellas, new Vector2(110f, 0.8f));
        CrearBotella(grpBotellas, new Vector2(124f, -3.5f));

        // 4 Fangosos Blindados (1 botella c/u = 4) + 1 Jefe Titan (2 botellas) = 6 botellas drop total (Exactamente 15 en el nivel)
        CrearEnemigoBlindado(grpEnemigos, "Blindado_Entrada", new Vector2(10f, -3.85f));        // Entrada Parque [-10, 18]
        CrearEnemigoBlindado(grpEnemigos, "Blindado_Cinta1", new Vector2(38f, 1.15f));          // Cinta transportadora 1
        CrearEnemigoBlindado(grpEnemigos, "Blindado_Plaza1", new Vector2(48f, -3.85f));         // Plaza Huaconada [34, 58]
        CrearEnemigoBlindado(grpEnemigos, "Blindado_Plaza2", new Vector2(90f, -3.85f));         // Plaza Chonguinada [77, 103]

        // 1 Jefe Titán de Lodo (2 botellas de drop)
        CrearJefeTitan(grpEnemigos, "Jefe_Fangoso_Titan", new Vector2(140f, -3.20f));          // Arena Jefe Final [119, 157]

        // 06. Cápsulas de Poder y Mejoras
        Transform grpPoderesN4 = CrearGrupo(root.transform, "06_Poderes_Mejoras");
        CrearItemPoder(grpPoderesN4, new Vector2(24f, -0.8f), ItemMejora.TipoMejora.DisparoTriple);
        CrearItemPoder(grpPoderesN4, new Vector2(67f, 1.2f), ItemMejora.TipoMejora.LaserAgua);
        CrearItemPoder(grpPoderesN4, new Vector2(72f, 3.8f), ItemMejora.TipoMejora.EscudoAcuatico);
        CrearItemPoder(grpPoderesN4, new Vector2(110f, 1.2f), ItemMejora.TipoMejora.BombaAgua);
        CrearItemPoder(grpPoderesN4, new Vector2(132f, -3.5f), ItemMejora.TipoMejora.CuracionVida);

        CrearMeta(root.transform, new Vector2(154f, -3.8f));
        CrearJugadorYUI(new Vector2(-8f, -4.50f));

        EditorSceneManager.SaveScene(scene, scenePath);
    }

    // ==========================================
    // NIVEL 5: PLAZA CONSTITUCIÓN & HUAMANMARCA
    // ==========================================
    private static void ConstruirNivel5()
    {
        string scenePath = "Assets/Scenes/Nivel5_PlazaConstitucion.unity";
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Sprite bg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/FondoPlazaConstitucion.png");
        if (bg == null) bg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/FondoHuamanmarca.png");
        GameObject root = new GameObject("--- NIVEL_5_PLAZA_CONSTITUCION ---");

        CrearEntornoBase(bg, 8, 32.25f, 26.0f, new Vector3(-6f, -1.8f, -10f), 6.5f, -6f, 145f, -3.5f, 7.5f, "Assets/Audio/Music/Musica_NivelIdentidad.wav");

        Transform grpSuelos = CrearGrupo(root.transform, "01_Paseos_Urbanos");
        Transform grpPlataformas = CrearGrupo(root.transform, "02_Relojes_Moviles");
        Transform grpDrenajes = CrearGrupo(root.transform, "03_Drenajes_Toxicos");
        Transform grpCarteles = CrearGrupo(root.transform, "04_Carteles_ODS15");
        Transform grpBotellas = CrearGrupo(root.transform, "05_Botellas_Plastico_9");
        Transform grpEnemigos = CrearGrupo(root.transform, "06_Enemigos_6");

        // Terrenos de mármol y concreto
        CrearBloqueSuelo(grpSuelos, "Suelo_Constitucion", new Vector2(4f, -5.5f), new Vector2(28f, 2f));
        CrearBloqueSuelo(grpSuelos, "Suelo_CalleReal", new Vector2(46f, -5.5f), new Vector2(24f, 2f));
        CrearBloqueSuelo(grpSuelos, "Suelo_Catedral", new Vector2(88f, -5.5f), new Vector2(26f, 2f));
        CrearBloqueSuelo(grpSuelos, "Suelo_HuamanmarcaFinal", new Vector2(130f, -5.5f), new Vector2(34f, 2f));

        CrearMuroTope(grpSuelos, -11f, 25f);
        CrearMuroTope(grpSuelos, 150f, 25f);

        // Canales de drenaje taponados por basura
        CrearLagoToxico(grpDrenajes, new Vector2(24f, -6.6f), new Vector2(10f, 1.2f));
        CrearLagoToxico(grpDrenajes, new Vector2(65f, -6.6f), new Vector2(12f, 1.2f));
        CrearLagoToxico(grpDrenajes, new Vector2(108f, -6.6f), new Vector2(12f, 1.2f));

        // Mecánicas: Plataformas móviles verticales (Relojes Municipales)
        CrearPlataformaMovil(grpPlataformas, "PlatMovil_Reloj1", new Vector2(24f, -2.5f), new Vector2(24f, 2.5f), new Vector2(4.5f, 0.6f), 2.2f);
        CrearPlataformaMovil(grpPlataformas, "PlatMovil_Balcon", new Vector2(63f, -1.2f), new Vector2(69f, -1.2f), new Vector2(4.5f, 0.6f), 2.5f);
        CrearPlataformaSimple(grpPlataformas, "Plat_Catedral1", new Vector2(82f, 0.5f), new Vector2(4.5f, 0.6f));
        CrearPlataformaSimple(grpPlataformas, "Plat_Catedral2", new Vector2(94f, 2.8f), new Vector2(4.5f, 0.6f));

        // Carteles ODS 15
        CrearCartel(grpCarteles, new Vector2(10f, -4.0f), "En el centro histórico de Huancayo, más de 40 toneladas de plástico taponan los sumideros pluviales cada temporada.", "Plaza Constitución");
        CrearCartel(grpCarteles, new Vector2(50f, -4.0f), "Los residuos no recolectados en la Plaza Constitución son arrastrados directamente al río Shullcas sin tratamiento.", "Plaza Constitución");
        CrearCartel(grpCarteles, new Vector2(92f, -4.0f), "ODS 15: Ciudades Sostenibles. Reducir plásticos de un solo uso protege la salubridad y los suelos urbanos de Junín.", "Plaza Constitución");

        // 9 Botellas en escenario
        CrearBotella(grpBotellas, new Vector2(8f, -3.5f));
        CrearBotella(grpBotellas, new Vector2(24f, 4.0f));
        CrearBotella(grpBotellas, new Vector2(38f, -3.5f));
        CrearBotella(grpBotellas, new Vector2(46f, -3.5f));
        CrearBotella(grpBotellas, new Vector2(66f, 0.5f));
        CrearBotella(grpBotellas, new Vector2(82f, 2.0f));
        CrearBotella(grpBotellas, new Vector2(94f, 4.3f));
        CrearBotella(grpBotellas, new Vector2(115f, -3.5f));
        CrearBotella(grpBotellas, new Vector2(128f, -3.5f));

        // 6 Enemigos (2 Patrulleros + 2 Tiradores + 1 Volador + 1 Blindado = 6 botellas drop)
        CrearEnemigoPatrullero(grpEnemigos, "Fangoso_Patrulla_1", new Vector2(8f, -3.85f));    // Plaza Constitucion [-10, 18]
        CrearEnemigoBlindado(grpEnemigos, "Fangoso_Blindado_1", new Vector2(46f, -3.85f));      // Calle Real [34, 58]
        CrearEnemigoVolador(grpEnemigos, "Fangoso_Volador_1", new Vector2(65f, 2.5f));          // Vuela sobre drenaje
        CrearEnemigoTirador(grpEnemigos, "Fangoso_Tirador_1", new Vector2(82f, 1.45f));         // Plataforma Catedral 1
        CrearEnemigoTirador(grpEnemigos, "Fangoso_Tirador_2", new Vector2(94f, 3.75f));         // Plataforma Catedral 2
        CrearEnemigoPatrullero(grpEnemigos, "Fangoso_Patrulla_2", new Vector2(128f, -3.85f));   // Huamanmarca Final [113, 147]

        // 07. Cápsulas de Poder y Mejoras
        Transform grpPoderesN5 = CrearGrupo(root.transform, "07_Poderes_Mejoras");
        CrearItemPoder(grpPoderesN5, new Vector2(24f, 4.5f), ItemMejora.TipoMejora.LaserAgua);
        CrearItemPoder(grpPoderesN5, new Vector2(63f, 0.8f), ItemMejora.TipoMejora.SuperVelocidad);
        CrearItemPoder(grpPoderesN5, new Vector2(82f, 2.8f), ItemMejora.TipoMejora.BombaAgua);
        CrearItemPoder(grpPoderesN5, new Vector2(94f, 5.0f), ItemMejora.TipoMejora.EscudoAcuatico);
        CrearItemPoder(grpPoderesN5, new Vector2(118f, -3.5f), ItemMejora.TipoMejora.CuracionVida);

        CrearMeta(root.transform, new Vector2(140f, -3.8f));
        CrearJugadorYUI(new Vector2(-8f, -4.50f));

        EditorSceneManager.SaveScene(scene, scenePath);
    }

    // ==========================================
    // NIVEL 6: BOSQUE DORADO DE PACCHA
    // ==========================================
    private static void ConstruirNivel6()
    {
        string scenePath = "Assets/Scenes/Nivel6_BosqueDorado.unity";
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Sprite bg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/FondoBosqueDorado.png");
        GameObject root = new GameObject("--- NIVEL_6_BOSQUE_DORADO ---");

        CrearEntornoBase(bg, 8, 32.25f, 26.0f, new Vector3(-6f, -1.8f, -10f), 6.5f, -6f, 145f, -3.5f, 8.0f, "Assets/Audio/Music/Musica_NivelBosqueDorado.wav");

        Transform grpSuelos = CrearGrupo(root.transform, "01_Senderos_Paccha");
        Transform grpPlataformas = CrearGrupo(root.transform, "02_Ramas_Inestables");
        Transform grpPeligros = CrearGrupo(root.transform, "03_Mulch_Toxico");
        Transform grpCarteles = CrearGrupo(root.transform, "04_Carteles_ODS15");
        Transform grpBotellas = CrearGrupo(root.transform, "05_Botellas_Plastico_9");
        Transform grpEnemigos = CrearGrupo(root.transform, "06_Enemigos_6");

        // Terrenos naturales
        CrearBloqueSuelo(grpSuelos, "Suelo_SenderoPaccha", new Vector2(4f, -5.5f), new Vector2(28f, 2f));
        CrearBloqueSuelo(grpSuelos, "Suelo_BosqueQuinual", new Vector2(46f, -5.5f), new Vector2(22f, 2f));
        CrearBloqueSuelo(grpSuelos, "Suelo_ClaroDorada", new Vector2(88f, -5.5f), new Vector2(24f, 2f));
        CrearBloqueSuelo(grpSuelos, "Suelo_MiradorPaccha", new Vector2(130f, -5.5f), new Vector2(34f, 2f));

        CrearMuroTope(grpSuelos, -11f, 25f);
        CrearMuroTope(grpSuelos, 150f, 25f);

        // Mulch y fango tóxico
        CrearLagoToxico(grpPeligros, new Vector2(24f, -6.6f), new Vector2(10f, 1.2f));
        CrearLagoToxico(grpPeligros, new Vector2(64f, -6.6f), new Vector2(12f, 1.2f));
        CrearLagoToxico(grpPeligros, new Vector2(107f, -6.6f), new Vector2(12f, 1.2f));

        // Tablones inestables simulando ramas frágiles de quenual
        CrearTablonInestable(grpPlataformas, "Rama_Paccha_1", new Vector2(20f, -2.0f), new Vector2(4.0f, 0.6f));
        CrearTablonInestable(grpPlataformas, "Rama_Paccha_2", new Vector2(28f, 0.5f), new Vector2(4.0f, 0.6f));
        CrearTablonInestable(grpPlataformas, "Rama_Paccha_3", new Vector2(60f, -1.8f), new Vector2(4.5f, 0.6f));
        CrearTablonInestable(grpPlataformas, "Rama_Paccha_4", new Vector2(68f, 1.2f), new Vector2(4.5f, 0.6f));
        CrearPlataformaSimple(grpPlataformas, "Plat_QuinualAlta", new Vector2(88f, 2.5f), new Vector2(5.0f, 0.6f));
        CrearTablonInestable(grpPlataformas, "Rama_Paccha_5", new Vector2(103f, -1.5f), new Vector2(4.5f, 0.6f));
        CrearTablonInestable(grpPlataformas, "Rama_Paccha_6", new Vector2(111f, 1.0f), new Vector2(4.5f, 0.6f));

        // Carteles ODS 15
        CrearCartel(grpCarteles, new Vector2(10f, -4.0f), "El Bosque Dorado de Paccha alberga bosques de quenuales y quinuales nativos, reguladores vitales del agua andina.", "Bosque Dorado de Paccha");
        CrearCartel(grpCarteles, new Vector2(50f, -4.0f), "Los residuos plásticos arrojados por turistas tardan siglos en degradarse y contaminan las raíces de flora milenaria.", "Bosque Dorado de Paccha");
        CrearCartel(grpCarteles, new Vector2(90f, -4.0f), "ODS 15: Cuidar los ecosistemas forestales de altura previene la desertificación y protege la fauna silvestre de Junín.", "Bosque Dorado de Paccha");

        // 9 Botellas en escenario
        CrearBotella(grpBotellas, new Vector2(8f, -3.5f));
        CrearBotella(grpBotellas, new Vector2(20f, -0.5f));
        CrearBotella(grpBotellas, new Vector2(28f, 2.0f));
        CrearBotella(grpBotellas, new Vector2(44f, -3.5f));
        CrearBotella(grpBotellas, new Vector2(60f, -0.3f));
        CrearBotella(grpBotellas, new Vector2(68f, 2.6f));
        CrearBotella(grpBotellas, new Vector2(88f, 4.0f));
        CrearBotella(grpBotellas, new Vector2(103f, 0.0f));
        CrearBotella(grpBotellas, new Vector2(111f, 2.5f));

        // 6 Enemigos (2 Patrulleros + 2 Voladores + 1 Blindado + 1 Tirador = 6 botellas drop)
        CrearEnemigoPatrullero(grpEnemigos, "Fangoso_Patrulla_1", new Vector2(8f, -3.85f));     // Sendero Paccha [-10, 18]
        CrearEnemigoVolador(grpEnemigos, "Fangoso_Volador_1", new Vector2(24f, 2.5f));          // Vuela sobre lago 1
        CrearEnemigoBlindado(grpEnemigos, "Fangoso_Blindado_1", new Vector2(46f, -3.85f));      // Bosque Quinual [35, 57]
        CrearEnemigoVolador(grpEnemigos, "Fangoso_Volador_2", new Vector2(64f, 2.5f));          // Vuela sobre lago 2
        CrearEnemigoTirador(grpEnemigos, "Fangoso_Tirador_1", new Vector2(88f, -3.85f));        // Claro Dorada [76, 100]
        CrearEnemigoPatrullero(grpEnemigos, "Fangoso_Patrulla_2", new Vector2(126f, -3.85f));   // Mirador Paccha [113, 147]

        // 07. Cápsulas de Poder y Mejoras
        Transform grpPoderesN6 = CrearGrupo(root.transform, "07_Poderes_Mejoras");
        CrearItemPoder(grpPoderesN6, new Vector2(28f, 2.5f), ItemMejora.TipoMejora.DisparoTriple);
        CrearItemPoder(grpPoderesN6, new Vector2(60f, 0.5f), ItemMejora.TipoMejora.SuperVelocidad);
        CrearItemPoder(grpPoderesN6, new Vector2(68f, 3.2f), ItemMejora.TipoMejora.LaserAgua);
        CrearItemPoder(grpPoderesN6, new Vector2(88f, 4.5f), ItemMejora.TipoMejora.EscudoAcuatico);
        CrearItemPoder(grpPoderesN6, new Vector2(115f, -3.5f), ItemMejora.TipoMejora.CuracionVida);

        CrearMeta(root.transform, new Vector2(140f, -3.8f));
        CrearJugadorYUI(new Vector2(-8f, -4.50f));

        EditorSceneManager.SaveScene(scene, scenePath);
    }

    // ==========================================
    // NIVEL 7: RIBERA DEL RÍO SHULLCAS & PEÑALOZA
    // ==========================================
    private static void ConstruirNivel7()
    {
        string scenePath = "Assets/Scenes/Nivel7_RioShullcas.unity";
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Sprite bg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/FondoRioShullcas.png");
        GameObject root = new GameObject("--- NIVEL_7_RIO_SHULLCAS ---");

        CrearEntornoBase(bg, 8, 32.25f, 26.0f, new Vector3(-6f, -1.8f, -10f), 6.5f, -6f, 150f, -4.0f, 8.0f, "Assets/Audio/Music/Musica_NivelBosqueDorado.wav");

        Transform grpSuelos = CrearGrupo(root.transform, "01_Riberas_Y_Pozas");
        Transform grpMecanicas = CrearGrupo(root.transform, "02_Cintas_Corrientes");
        Transform grpPeligros = CrearGrupo(root.transform, "03_Geysers_Toxicos");
        Transform grpCarteles = CrearGrupo(root.transform, "04_Carteles_ODS15");
        Transform grpBotellas = CrearGrupo(root.transform, "05_Botellas_Plastico_9");
        Transform grpEnemigos = CrearGrupo(root.transform, "06_Enemigos_6");

        // Riberas rocosas
        CrearBloqueSuelo(grpSuelos, "Suelo_RiberaOeste", new Vector2(4f, -5.5f), new Vector2(28f, 2f));
        CrearBloqueSuelo(grpSuelos, "Suelo_IsloteCentral", new Vector2(48f, -5.5f), new Vector2(26f, 2f));
        CrearBloqueSuelo(grpSuelos, "Suelo_MuroContencion", new Vector2(94f, -5.5f), new Vector2(26f, 2f));
        CrearBloqueSuelo(grpSuelos, "Suelo_ParquePenaloza", new Vector2(136f, -5.5f), new Vector2(34f, 2f));

        CrearMuroTope(grpSuelos, -11f, 25f);
        CrearMuroTope(grpSuelos, 155f, 25f);

        // Rápidos contaminados del río
        CrearLagoToxico(grpSuelos, new Vector2(25f, -6.6f), new Vector2(12f, 1.2f));
        CrearLagoToxico(grpSuelos, new Vector2(70f, -6.6f), new Vector2(16f, 1.2f));
        CrearLagoToxico(grpSuelos, new Vector2(114f, -6.6f), new Vector2(12f, 1.2f));

        // Corrientes rápidas (Cintas Transportadoras)
        CrearCintaTransportadora(grpMecanicas, "Corriente_Rio1", new Vector2(25f, -2.5f), new Vector2(9f, 0.6f), 4.0f);
        CrearCintaTransportadora(grpMecanicas, "Corriente_Rio2", new Vector2(70f, -2.0f), new Vector2(11f, 0.6f), -3.5f);

        // Géysers fluviales de lodo
        CrearGeyser(grpPeligros, "Geyser_Shullcas1", new Vector2(58f, -4.5f), 1.8f, 1.5f);
        CrearGeyser(grpPeligros, "Geyser_Shullcas2", new Vector2(104f, -4.5f), 2.0f, 1.8f);

        // Plataformas rocosas
        CrearPlataformaSimple(grpMecanicas, "Roca_Penaloza1", new Vector2(65f, 1.5f), new Vector2(4.5f, 0.6f));
        CrearPlataformaSimple(grpMecanicas, "Roca_Penaloza2", new Vector2(75f, 3.5f), new Vector2(4.5f, 0.6f));

        // Carteles ODS 15
        CrearCartel(grpCarteles, new Vector2(10f, -4.0f), "El río Shullcas nace en el Huaytapallana y brinda agua a Huancayo. ¡No dejemos que los desechos plásticos lo asfixien!", "Ribera del Río Shullcas");
        CrearCartel(grpCarteles, new Vector2(50f, -4.0f), "Los plásticos que flotan en el Shullcas se fragmentan en micropartículas que dañan la fauna acuática y la agricultura del Mantaro.", "Ribera del Río Shullcas");
        CrearCartel(grpCarteles, new Vector2(96f, -4.0f), "ODS 15: Limpiar las cuencas hidrográficas de Junín garantiza agua limpia y tierras fértiles para futuras generaciones.", "Ribera del Río Shullcas");

        // 9 Botellas en escenario
        CrearBotella(grpBotellas, new Vector2(8f, -3.5f));
        CrearBotella(grpBotellas, new Vector2(25f, -1.0f));
        CrearBotella(grpBotellas, new Vector2(38f, -3.5f));
        CrearBotella(grpBotellas, new Vector2(48f, -3.5f));
        CrearBotella(grpBotellas, new Vector2(65f, 3.0f));
        CrearBotella(grpBotellas, new Vector2(75f, 5.0f));
        CrearBotella(grpBotellas, new Vector2(94f, -3.5f));
        CrearBotella(grpBotellas, new Vector2(105f, 0.5f));
        CrearBotella(grpBotellas, new Vector2(124f, -3.5f));

        // 6 Enemigos (2 Cañoneros + 2 Blindados + 1 Tirador + 1 Patrullero = 6 botellas drop)
        CrearEnemigoCanionero(grpEnemigos, "Fangoso_Canionero_1", new Vector2(8f, -3.85f));     // Ribera Oeste [-10, 18]
        CrearEnemigoBlindado(grpEnemigos, "Fangoso_Blindado_1", new Vector2(42f, -3.85f));       // Islote Central [35, 61]
        CrearEnemigoTirador(grpEnemigos, "Fangoso_Tirador_1", new Vector2(52f, 2.45f));          // Plataforma Desembocadura
        CrearEnemigoCanionero(grpEnemigos, "Fangoso_Canionero_2", new Vector2(90f, -3.85f));     // Muro Contencion [81, 107]
        CrearEnemigoBlindado(grpEnemigos, "Fangoso_Blindado_2", new Vector2(100f, -3.85f));      // Muro Contencion [81, 107]
        CrearEnemigoPatrullero(grpEnemigos, "Fangoso_Patrulla_1", new Vector2(132f, -3.85f));    // Parque Penaloza [119, 153]

        // 07. Cápsulas de Poder y Mejoras
        Transform grpPoderesN7 = CrearGrupo(root.transform, "07_Poderes_Mejoras");
        CrearItemPoder(grpPoderesN7, new Vector2(25f, 1.0f), ItemMejora.TipoMejora.BombaAgua);
        CrearItemPoder(grpPoderesN7, new Vector2(65f, 3.5f), ItemMejora.TipoMejora.LaserAgua);
        CrearItemPoder(grpPoderesN7, new Vector2(75f, 5.5f), ItemMejora.TipoMejora.SuperVelocidad);
        CrearItemPoder(grpPoderesN7, new Vector2(105f, 2.5f), ItemMejora.TipoMejora.EscudoAcuatico);
        CrearItemPoder(grpPoderesN7, new Vector2(126f, -3.5f), ItemMejora.TipoMejora.CuracionVida);

        CrearMeta(root.transform, new Vector2(145f, -3.8f));
        CrearJugadorYUI(new Vector2(-8f, -4.50f));

        EditorSceneManager.SaveScene(scene, scenePath);
    }

    // ==========================================
    // NIVEL 8: FORMACIONES ROCOSAS DE TORRE TORRE
    // ==========================================
    private static void ConstruirNivel8()
    {
        string scenePath = "Assets/Scenes/Nivel8_TorreTorre.unity";
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Sprite bg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/FondoTorreTorre.png");
        GameObject root = new GameObject("--- NIVEL_8_TORRE_TORRE ---");

        CrearEntornoBase(bg, 8, 32.25f, 26.0f, new Vector3(-6f, -1.8f, -10f), 7.0f, -6f, 150f, -4.0f, 13.0f, "Assets/Audio/Music/Musica_NivelTorreTorre.wav");

        Transform grpSuelos = CrearGrupo(root.transform, "01_Caniones_Rocosos");
        Transform grpAgujas = CrearGrupo(root.transform, "02_Agujas_Y_Trampolines");
        Transform grpPeligros = CrearGrupo(root.transform, "03_Fosos_Arcilla");
        Transform grpCarteles = CrearGrupo(root.transform, "04_Carteles_ODS15");
        Transform grpBotellas = CrearGrupo(root.transform, "05_Botellas_Plastico_9");
        Transform grpEnemigos = CrearGrupo(root.transform, "06_Enemigos_6");

        // Terrenos y cañones
        CrearBloqueSuelo(grpSuelos, "Suelo_BaseCanion", new Vector2(4f, -5.5f), new Vector2(26f, 2f));
        CrearBloqueSuelo(grpSuelos, "Suelo_MesetaMedia", new Vector2(48f, -4.0f), new Vector2(22f, 2f));
        CrearBloqueSuelo(grpSuelos, "Suelo_TorreonCentral", new Vector2(92f, -2.5f), new Vector2(24f, 2f));
        CrearBloqueSuelo(grpSuelos, "Suelo_CrestaFinal", new Vector2(136f, -3.0f), new Vector2(34f, 2f));

        CrearMuroTope(grpSuelos, -11f, 30f);
        CrearMuroTope(grpSuelos, 155f, 30f);

        // Fosos de arcilla radiactiva
        CrearLagoToxico(grpPeligros, new Vector2(23f, -6.6f), new Vector2(10f, 1.2f));
        CrearLagoToxico(grpPeligros, new Vector2(66f, -6.6f), new Vector2(12f, 1.2f));
        CrearLagoToxico(grpPeligros, new Vector2(111f, -6.6f), new Vector2(12f, 1.2f));

        // Trampolines de alto impulso hacia las agujas de piedra
        CrearTrampolin(grpAgujas, "SuperTrampolin_1", new Vector2(20f, -4.0f), 20f);
        CrearPlataformaOneWay(grpAgujas, "Aguja_OneWay_1", new Vector2(24f, 1.5f), new Vector2(4.5f, 0.5f));
        CrearPlataformaOneWay(grpAgujas, "Aguja_OneWay_2", new Vector2(28f, 5.0f), new Vector2(4.5f, 0.5f));

        CrearPlataformaGiratoria(grpAgujas, "Aguja_Giratoria", new Vector2(66f, 1.0f), 45f);

        CrearTrampolin(grpAgujas, "SuperTrampolin_2", new Vector2(85f, -1.2f), 21f);
        CrearPlataformaOneWay(grpAgujas, "Aguja_OneWay_3", new Vector2(92f, 4.0f), new Vector2(5.0f, 0.5f));
        CrearPlataformaOneWay(grpAgujas, "Aguja_OneWay_4", new Vector2(98f, 7.5f), new Vector2(5.0f, 0.5f));

        CrearPlataformaMovil(grpAgujas, "PlatMovil_Torre", new Vector2(108f, 0.0f), new Vector2(116f, 5.0f), new Vector2(4.5f, 0.6f), 3.0f);

        // Carteles ODS 15
        CrearCartel(grpCarteles, new Vector2(10f, -4.0f), "Torre Torre es una maravilla geológica de torres de arcilla roja moldeadas por el viento y el agua durante milenios.", "Torre Torre");
        CrearCartel(grpCarteles, new Vector2(50f, -2.5f), "El plástico abandonado entre las hendiduras rocosas daña la frágil estabilidad del suelo arcilloso.", "Torre Torre");
        CrearCartel(grpCarteles, new Vector2(92f, -1.0f), "ODS 15: Proteger el patrimonio geológico y los paisajes naturales de Huancayo es responsabilidad de todos.", "Torre Torre");

        // 9 Botellas en escenario (ubicadas a gran altura)
        CrearBotella(grpBotellas, new Vector2(8f, -3.5f));
        CrearBotella(grpBotellas, new Vector2(24f, 3.0f));
        CrearBotella(grpBotellas, new Vector2(28f, 6.5f));
        CrearBotella(grpBotellas, new Vector2(46f, -2.5f));
        CrearBotella(grpBotellas, new Vector2(66f, 3.2f));
        CrearBotella(grpBotellas, new Vector2(85f, 2.5f));
        CrearBotella(grpBotellas, new Vector2(92f, 5.5f));
        CrearBotella(grpBotellas, new Vector2(98f, 9.0f));
        CrearBotella(grpBotellas, new Vector2(124f, -1.5f));

        // 6 Enemigos (2 Cañoneros + 2 Voladores + 1 Blindado + 1 Tirador = 6 botellas drop)
        CrearEnemigoCanionero(grpEnemigos, "Fangoso_Canionero_1", new Vector2(8f, -3.85f));     // Base Cañon [-9, 17]
        CrearEnemigoVolador(grpEnemigos, "Fangoso_Volador_1", new Vector2(23f, 1.0f));          // Vuela sobre hendidura 1
        CrearEnemigoBlindado(grpEnemigos, "Fangoso_Blindado_1", new Vector2(48f, -2.35f));      // Meseta Media [37, 59]
        CrearEnemigoVolador(grpEnemigos, "Fangoso_Volador_2", new Vector2(66f, 1.5f));          // Vuela sobre hendidura 2
        CrearEnemigoCanionero(grpEnemigos, "Fangoso_Canionero_2", new Vector2(92f, -0.85f));    // Torreon Central [80, 104]
        CrearEnemigoTirador(grpEnemigos, "Fangoso_Tirador_1", new Vector2(130f, -1.35f));       // Cresta Final [119, 153]

        // 07. Cápsulas de Poder y Mejoras
        Transform grpPoderesN8 = CrearGrupo(root.transform, "07_Poderes_Mejoras");
        CrearItemPoder(grpPoderesN8, new Vector2(24f, 3.5f), ItemMejora.TipoMejora.SuperVelocidad);
        CrearItemPoder(grpPoderesN8, new Vector2(28f, 7.2f), ItemMejora.TipoMejora.DisparoTriple);
        CrearItemPoder(grpPoderesN8, new Vector2(66f, 3.8f), ItemMejora.TipoMejora.LaserAgua);
        CrearItemPoder(grpPoderesN8, new Vector2(98f, 9.5f), ItemMejora.TipoMejora.BombaAgua);
        CrearItemPoder(grpPoderesN8, new Vector2(126f, -1.0f), ItemMejora.TipoMejora.CuracionVida);

        CrearMeta(root.transform, new Vector2(145f, -1.5f));
        CrearJugadorYUI(new Vector2(-8f, -4.50f));

        EditorSceneManager.SaveScene(scene, scenePath);
    }

    // ==========================================
    // NIVEL 9: CUMBRES DEL NEVADO HUAYTAPALLANA (GRAN FINAL)
    // ==========================================
    private static void ConstruirNivel9()
    {
        string scenePath = "Assets/Scenes/Nivel9_NevadoHuaytapallana.unity";
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Sprite bg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/FondoHuaytapallana.png");
        GameObject root = new GameObject("--- NIVEL_9_NEVADO_HUAYTAPALLANA ---");

        CrearEntornoBase(bg, 8, 32.25f, 26.0f, new Vector3(-6f, -1.8f, -10f), 7.0f, -6f, 160f, -4.0f, 8.5f, "Assets/Audio/Music/Musica_NivelBoss.wav");

        Transform grpSuelos = CrearGrupo(root.transform, "01_Morrenas_Y_Glaciares");
        Transform grpPlataformas = CrearGrupo(root.transform, "02_Hielo_Y_Mecanicas");
        Transform grpPeligros = CrearGrupo(root.transform, "03_Abismos_Slush");
        Transform grpCarteles = CrearGrupo(root.transform, "04_Carteles_ODS15");
        Transform grpBotellas = CrearGrupo(root.transform, "05_Botellas_Plastico_9");
        Transform grpEnemigos = CrearGrupo(root.transform, "06_Enemigos_Y_JefeSupremo");

        // Morrenas y rocas glaciales
        CrearBloqueSuelo(grpSuelos, "Suelo_MorrenaGlacial", new Vector2(4f, -5.5f), new Vector2(28f, 2f));
        CrearBloqueSuelo(grpSuelos, "Suelo_LagunaGlacial", new Vector2(48f, -5.5f), new Vector2(26f, 2f));
        CrearBloqueSuelo(grpSuelos, "Suelo_CrestaDeNieve", new Vector2(94f, -5.5f), new Vector2(26f, 2f));
        CrearBloqueSuelo(grpSuelos, "Arena_Coloso_Huaytapallana", new Vector2(142f, -5.5f), new Vector2(42f, 2f));

        CrearMuroTope(grpSuelos, -11f, 30f);
        CrearMuroTope(grpSuelos, 165f, 30f);

        // Abismos de slush tóxico subcero
        CrearLagoToxico(grpPeligros, new Vector2(25f, -6.6f), new Vector2(12f, 1.2f));
        CrearLagoToxico(grpPeligros, new Vector2(70f, -6.6f), new Vector2(16f, 1.2f));
        CrearLagoToxico(grpPeligros, new Vector2(114f, -6.6f), new Vector2(12f, 1.2f));

        // Mecánicas: Hielo móvil y géysers glaciales
        CrearPlataformaMovil(grpPlataformas, "PlatMovil_Glaciar1", new Vector2(21f, -2.5f), new Vector2(29f, -2.5f), new Vector2(4.5f, 0.6f), 2.5f);
        CrearTablonInestable(grpPlataformas, "Tablon_Hielo1", new Vector2(66f, -1.5f), new Vector2(4.5f, 0.6f));
        CrearPlataformaMovil(grpPlataformas, "PlatMovil_Glaciar2", new Vector2(74f, -1.0f), new Vector2(74f, 3.5f), new Vector2(4.5f, 0.6f), 2.0f);
        CrearTablonInestable(grpPlataformas, "Tablon_Hielo2", new Vector2(108f, -1.8f), new Vector2(4.5f, 0.6f));
        CrearPlataformaSimple(grpPlataformas, "Plat_CimaHielo", new Vector2(116f, 1.2f), new Vector2(5.0f, 0.6f));

        CrearGeyser(grpPeligros, "Geyser_Glacial1", new Vector2(42f, -4.5f), 1.8f, 1.4f);
        CrearGeyser(grpPeligros, "Geyser_Glacial2", new Vector2(88f, -4.5f), 2.2f, 1.6f);

        // Carteles ODS 15
        CrearCartel(grpCarteles, new Vector2(10f, -4.0f), "El Nevado Huaytapallana (5,557 msnm) es el apu sagrado y la principal fuente de agua dulce para más de medio millón de personas.", "Nevado Huaytapallana");
        CrearCartel(grpCarteles, new Vector2(52f, -4.0f), "El calentamiento y los microplásticos aceleran el deshielo del glaciar. Su protección es urgente e innegociable.", "Nevado Huaytapallana");
        CrearCartel(grpCarteles, new Vector2(96f, -4.0f), "ODS 15: Salvaguardar los glaciares andinos es garantizar la vida, la agricultura y el futuro de Junín.", "Nevado Huaytapallana");

        // 9 Botellas en escenario
        CrearBotella(grpBotellas, new Vector2(8f, -3.5f));
        CrearBotella(grpBotellas, new Vector2(25f, -1.0f));
        CrearBotella(grpBotellas, new Vector2(38f, -3.5f));
        CrearBotella(grpBotellas, new Vector2(55f, -3.5f));
        CrearBotella(grpBotellas, new Vector2(66f, 0.2f));
        CrearBotella(grpBotellas, new Vector2(74f, 5.0f));
        CrearBotella(grpBotellas, new Vector2(94f, -3.5f));
        CrearBotella(grpBotellas, new Vector2(116f, 2.8f));
        CrearBotella(grpBotellas, new Vector2(128f, -3.5f));

        // 3 Enemigos Élite (1 botella c/u = 3) + 1 Jefe Supremo (3 botellas) = 6 botellas drop total (Exactamente 15 en el nivel)
        CrearEnemigoCanionero(grpEnemigos, "Fangoso_Canionero_Glacial", new Vector2(8f, -3.85f));   // Morrena Glacial [-10, 18]
        CrearEnemigoVolador(grpEnemigos, "Fangoso_Volador_Glacial", new Vector2(25f, 2.0f));       // Vuela sobre lago glacial 1
        CrearEnemigoBlindado(grpEnemigos, "Fangoso_Blindado_Glacial", new Vector2(48f, -3.85f));    // Laguna Glacial [35, 61]

        // Jefe Supremo de Campaña (20 HP, ráfagas espirales, azote sísmico y 3 botellas de drop)
        CrearBossSupremeHuaytapallana(grpEnemigos, "JefeSupremo_ColosoHuaytapallana", new Vector2(144f, -2.70f)); // Arena Coloso [121, 163]

        // 07. Cápsulas de Poder y Mejoras
        Transform grpPoderesN9 = CrearGrupo(root.transform, "07_Poderes_Mejoras");
        CrearItemPoder(grpPoderesN9, new Vector2(25f, 1.0f), ItemMejora.TipoMejora.LaserAgua);
        CrearItemPoder(grpPoderesN9, new Vector2(48f, -3.5f), ItemMejora.TipoMejora.SuperVelocidad);
        CrearItemPoder(grpPoderesN9, new Vector2(74f, 4.5f), ItemMejora.TipoMejora.BombaAgua);
        CrearItemPoder(grpPoderesN9, new Vector2(116f, 3.2f), ItemMejora.TipoMejora.EscudoAcuatico);
        CrearItemPoder(grpPoderesN9, new Vector2(134f, -3.5f), ItemMejora.TipoMejora.CuracionVida);

        CrearMeta(root.transform, new Vector2(156f, -3.8f));
        CrearJugadorYUI(new Vector2(-8f, -4.50f));

        EditorSceneManager.SaveScene(scene, scenePath);
    }

    // ==========================================
    // HELPERS DE CONSTRUCCIÓN COMUNES
    // ==========================================
    private static void CrearEntornoBase(Sprite spriteFondo, int cantidadPaneles, float anchoPanel, float altoPanel, Vector3 camPos, float camSize, float minX, float maxX, float minY, float maxY, string rutaMusicaNivel = "Assets/Audio/Music/Musica_NivelIdentidad.wav")
    {
        // Sistema de Audio centralizado y persistente con banda sonora de Huancayo
        ConfigurarSistemaAudio(rutaMusicaNivel);

        // Luz 2D
        GameObject light = new GameObject("Global Light 2D");
        var l2d = light.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
        l2d.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Global;
        l2d.intensity = 1.0f;

        // Cámara
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.35f, 0.65f, 0.90f);
        cam.orthographic = true;
        cam.orthographicSize = camSize > 0.1f ? camSize : 5.4f;
        camObj.transform.position = camPos;
        camObj.AddComponent<AudioListener>(); // Garantiza que toda la música y SFX se escuchen con claridad

        CameraFollow cf = camObj.AddComponent<CameraFollow>();
        cf.offset = new Vector3(2.0f, 1.6f, -10f);
        cf.ConfigurarLimites(minX, maxX, minY, maxY);

        // UN SOLO FONDO PANORÁMICO QUE SIGUE AL JUGADOR / CÁMARA
        // Cero repeticiones, cero espejados: estampa auténtica única que sigue horizontalmente al jugador con parallax suave.
        GameObject fondoObj = new GameObject("--- FONDO_PANORAMICO_UNICO ---");
        fondoObj.transform.position = new Vector3(camPos.x, -2.5f, 12f);

        SpriteRenderer sr = fondoObj.AddComponent<SpriteRenderer>();
        sr.sprite = spriteFondo;
        sr.sortingOrder = -20;
        // Fondo más opaco / contrastado para resaltar el plano jugable (directiva usuario)
        sr.color = new Color(0.68f, 0.68f, 0.72f, 1.0f);

        // Relación de aspecto uniforme exacta sin distorsión (directiva usuario)
        GeneradorNivelColonial.EscalarFondoManteniendoAspecto(fondoObj, sr, 46.0f, 26.0f);

        var seguidor = fondoObj.AddComponent<FondoSigueJugador>();
        seguidor.camaraPrincipal = cam;
        seguidor.posYBase = -2.5f;
        seguidor.camYReferencia = -2.9f;
        seguidor.offsetZ = 12f;
        seguidor.usarParallaxNivel = true;
        seguidor.recorridoParallaxX = 6.0f;
        seguidor.limiteNivelMinX = minX;
        seguidor.limiteNivelMaxX = maxX;
        seguidor.factorSeguimientoY = 0.25f;
    }

    public static void ConfigurarSistemaAudio(string rutaMusicaNivel)
    {
        AudioClip clipMusicaNivel = AssetDatabase.LoadAssetAtPath<AudioClip>(rutaMusicaNivel);
        AudioClip clipMusicaMenu = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/MenuPrincipal.mp3");
        if (clipMusicaMenu == null) clipMusicaMenu = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Musica_Menu.wav");

        AudioClip clipMusicaNivel1 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Nivel1.mp3");
        if (clipMusicaNivel1 == null) clipMusicaNivel1 = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Musica_NivelBosqueDorado.wav");

        AudioClip clipMusicaIdentidad = clipMusicaNivel1;
        if (clipMusicaIdentidad == null) clipMusicaIdentidad = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Musica_NivelIdentidad.wav");

        AudioClip clipMusicaCerrito = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Musica_NivelCerrito.wav");

        AudioClip clipMusicaBosque = clipMusicaNivel1;
        if (clipMusicaBosque == null) clipMusicaBosque = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Musica_NivelBosqueDorado.wav");

        AudioClip clipMusicaTorre = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Musica_NivelTorreTorre.wav");
        AudioClip clipMusicaBoss = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Musica_NivelBoss.wav");

        AudioClip sfxDisparo = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/SFX_DisparoAgua.wav");
        AudioClip sfxSalto = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/SFX_SaltoArcade.wav");
        AudioClip sfxBotella = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/SFX_BotellaCristal.wav");
        AudioClip sfxDanio = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/SFX_DanioHit.wav");
        AudioClip sfxClick = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/SFX_ClickUI.wav");
        AudioClip sfxPurif = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/SFX_VictoriaPurificacion.wav");

        GameObject amObj = GameObject.Find("AudioManager");
        if (amObj == null) amObj = new GameObject("AudioManager");

        AudioManager am = amObj.GetComponent<AudioManager>();
        if (am == null) am = amObj.AddComponent<AudioManager>();

        SerializedObject so = new SerializedObject(am);
        so.FindProperty("musicaMenu").objectReferenceValue = clipMusicaMenu;
        var propNivel1 = so.FindProperty("musicaNivel1");
        if (propNivel1 != null) propNivel1.objectReferenceValue = clipMusicaNivel1;
        so.FindProperty("musicaNivelIdentidad").objectReferenceValue = clipMusicaIdentidad;
        so.FindProperty("musicaNivelCerrito").objectReferenceValue = clipMusicaCerrito;
        so.FindProperty("musicaNivelBosqueDorado").objectReferenceValue = clipMusicaBosque;
        so.FindProperty("musicaNivelTorreTorre").objectReferenceValue = clipMusicaTorre;
        so.FindProperty("musicaNivelBoss").objectReferenceValue = clipMusicaBoss;

        so.FindProperty("sfxDisparoAgua").objectReferenceValue = sfxDisparo;
        so.FindProperty("sfxSalto").objectReferenceValue = sfxSalto;
        so.FindProperty("sfxRecogerBotella").objectReferenceValue = sfxBotella;
        so.FindProperty("sfxDanio").objectReferenceValue = sfxDanio;
        so.FindProperty("sfxBotonUI").objectReferenceValue = sfxClick;
        so.FindProperty("sfxPurificacion").objectReferenceValue = sfxPurif;

        // AudioSource de Música pre-cargado con la pista del parque
        Transform trMusica = amObj.transform.Find("AudioSource_Musica");
        GameObject goMusica = (trMusica != null) ? trMusica.gameObject : new GameObject("AudioSource_Musica");
        goMusica.transform.SetParent(amObj.transform);
        AudioSource asMusica = goMusica.GetComponent<AudioSource>();
        if (asMusica == null) asMusica = goMusica.AddComponent<AudioSource>();
        asMusica.clip = (clipMusicaNivel != null) ? clipMusicaNivel : clipMusicaIdentidad;
        asMusica.loop = true;
        asMusica.playOnAwake = true;
        asMusica.volume = 0.80f;
        asMusica.spatialBlend = 0f;

        Transform trSFX = amObj.transform.Find("AudioSource_SFX");
        GameObject goSFX = (trSFX != null) ? trSFX.gameObject : new GameObject("AudioSource_SFX");
        goSFX.transform.SetParent(amObj.transform);
        AudioSource asSFX = goSFX.GetComponent<AudioSource>();
        if (asSFX == null) asSFX = goSFX.AddComponent<AudioSource>();
        asSFX.loop = false;
        asSFX.playOnAwake = false;
        asSFX.volume = 0.90f;
        asSFX.spatialBlend = 0f;

        so.FindProperty("audioSourceMusica").objectReferenceValue = asMusica;
        so.FindProperty("audioSourceSFX").objectReferenceValue = asSFX;

        so.ApplyModifiedProperties();
    }

    private static Transform CrearGrupo(Transform padre, string nombre)
    {
        GameObject grp = new GameObject(nombre);
        grp.transform.SetParent(padre);
        return grp.transform;
    }

    private static void CrearBloqueSuelo(Transform padre, string nombre, Vector2 centro, Vector2 tamano)
    {
        // El nivel superior del suelo (caminar) se preserva exactamente:
        float ySuperficie = centro.y + (tamano.y / 2f);
        // Profundidad de cimientos sólidos hasta -26.0f para conectar sin ninguna fisura con la base de piedra:
        float yProfundidad = -26.0f;
        float altoSolido = ySuperficie - yProfundidad;
        float nuevoCentroY = yProfundidad + (altoSolido / 2f);

        Vector2 centroSolido = new Vector2(centro.x, nuevoCentroY);
        Vector2 tamanoSolido = new Vector2(tamano.x, altoSolido);

        GameObject bloque = new GameObject(nombre);
        bloque.transform.SetParent(padre);
        bloque.transform.position = centroSolido;
        bloque.tag = "Ground";
        int layerGround = LayerMask.NameToLayer("Ground");
        if (layerGround != -1) bloque.layer = layerGround;

        BoxCollider2D col = bloque.AddComponent<BoxCollider2D>();
        col.size = tamanoSolido;

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(bloque.transform);
        visual.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = sprPiso;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        sr.size = tamanoSolido;
        sr.sortingOrder = 1;
    }

    private static void CrearMuroTope(Transform padre, float posX, float alto)
    {
        GameObject muro = new GameObject("Muro_Tope");
        muro.transform.SetParent(padre);
        muro.transform.position = new Vector3(posX, 2f, 0f);
        muro.tag = "Ground";
        int layerGround = LayerMask.NameToLayer("Ground");
        if (layerGround != -1) muro.layer = layerGround;

        BoxCollider2D col = muro.AddComponent<BoxCollider2D>();
        col.size = new Vector2(2f, alto + 40f);
    }

    private static void CrearLagoToxico(Transform padre, Vector2 posicion, Vector2 tamano)
    {
        // 1. Superficie de agua/lodo tóxico interactivo
        GameObject lago = new GameObject("Lago_Toxico");
        lago.transform.SetParent(padre);
        lago.transform.position = posicion;

        // Tag para detección de colisiones tóxicas
        lago.tag = "ToxicWater";

        BoxCollider2D col = lago.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = tamano;
        col.offset = new Vector2(0f, 0.05f);

        lago.AddComponent<CharcoToxico>();

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(lago.transform);
        visual.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = sprLago;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        sr.size = tamano;
        sr.sortingOrder = 2;
        sr.color = Color.white;

        // 2. Lecho profundo del pozo (Roca/Fango oscuro) que llega hasta -26.0f sin dejar vacíos:
        float yFondoLago = posicion.y - (tamano.y / 2f);
        float yProfundidad = -26.0f;
        float altoLecho = yFondoLago - yProfundidad;
        if (altoLecho > 0.4f)
        {
            GameObject lecho = new GameObject("Lecho_Pozo_Profundo");
            lecho.transform.SetParent(lago.transform);
            lecho.transform.position = new Vector3(posicion.x, yProfundidad + (altoLecho / 2f), 0f);
            lecho.tag = "Ground";
            int layerGround = LayerMask.NameToLayer("Ground");
            if (layerGround != -1) lecho.layer = layerGround;

            BoxCollider2D colLecho = lecho.AddComponent<BoxCollider2D>();
            colLecho.size = new Vector2(tamano.x, altoLecho);

            GameObject visualLecho = new GameObject("VisualLecho");
            visualLecho.transform.SetParent(lecho.transform);
            visualLecho.transform.localPosition = Vector3.zero;

            SpriteRenderer srLecho = visualLecho.AddComponent<SpriteRenderer>();
            srLecho.sprite = sprPiso;
            srLecho.color = new Color(0.20f, 0.18f, 0.16f); // Tinte oscuro de lecho pantanoso
            srLecho.drawMode = SpriteDrawMode.Tiled;
            srLecho.tileMode = SpriteTileMode.Continuous;
            srLecho.size = new Vector2(tamano.x, altoLecho);
            srLecho.sortingOrder = 0;
        }
    }

    private static void CrearPlataformaSimple(Transform padre, string nombre, Vector2 centro, Vector2 tamano)
    {
        GameObject plat = new GameObject(nombre);
        plat.transform.SetParent(padre);
        plat.transform.position = centro;
        plat.tag = "Ground";
        int layerGround = LayerMask.NameToLayer("Ground");
        if (layerGround != -1) plat.layer = layerGround;

        float altoVisual = 1.20f;
        float altoSolido = 0.40f;

        BoxCollider2D col = plat.AddComponent<BoxCollider2D>();
        col.size = new Vector2(tamano.x * 0.98f, altoSolido);
        col.offset = new Vector2(0f, (altoVisual - altoSolido) / 2f); // Losa superior donde pisa el jugador

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(plat.transform);
        visual.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = sprPlat;
        sr.drawMode = SpriteDrawMode.Simple;
        sr.sortingOrder = 3;
        GeneradorNivelColonial.AjustarEscalaVisual(visual, sr, tamano.x, altoVisual);
    }

    private static void CrearPlataformaOneWay(Transform padre, string nombre, Vector2 centro, Vector2 tamano)
    {
        GameObject plat = new GameObject(nombre);
        plat.transform.SetParent(padre);
        plat.transform.position = centro;
        plat.tag = "Ground";
        int layerGround = LayerMask.NameToLayer("Ground");
        if (layerGround != -1) plat.layer = layerGround;

        float altoVisual = 1.20f;
        float altoSolido = 0.35f;

        BoxCollider2D col = plat.AddComponent<BoxCollider2D>();
        col.size = new Vector2(tamano.x * 0.98f, altoSolido);
        col.offset = new Vector2(0f, (altoVisual - altoSolido) / 2f);
        col.usedByEffector = true;

        PlatformEffector2D eff = plat.AddComponent<PlatformEffector2D>();
        eff.useOneWay = true;
        eff.useOneWayGrouping = true;
        eff.surfaceArc = 170f;

        plat.AddComponent<PlataformaOneWay>();

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(plat.transform);
        visual.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = sprPlat;
        sr.drawMode = SpriteDrawMode.Simple;
        sr.sortingOrder = 3;
        GeneradorNivelColonial.AjustarEscalaVisual(visual, sr, tamano.x, altoVisual);
    }

    private static void CrearTablonInestable(Transform padre, string nombre, Vector2 pos, Vector2 tamano)
    {
        GameObject tab = new GameObject(nombre);
        tab.transform.SetParent(padre);
        tab.transform.position = pos;
        tab.tag = "Ground";
        int layerGround = LayerMask.NameToLayer("Ground");
        if (layerGround != -1) tab.layer = layerGround;

        float altoVisual = 1.20f;
        float altoSolido = 0.40f;

        BoxCollider2D col = tab.AddComponent<BoxCollider2D>();
        col.size = new Vector2(tamano.x * 0.98f, altoSolido);
        col.offset = new Vector2(0f, (altoVisual - altoSolido) / 2f);
        tab.AddComponent<PlataformaInestable>();

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(tab.transform);
        visual.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = sprPlat;
        sr.drawMode = SpriteDrawMode.Simple;
        sr.sortingOrder = 3;
        GeneradorNivelColonial.AjustarEscalaVisual(visual, sr, tamano.x, altoVisual);
    }

    private static void CrearTrampolin(Transform padre, string nombre, Vector2 pos, float impulso)
    {
        GameObject tr = new GameObject(nombre);
        tr.transform.SetParent(padre);
        tr.transform.position = pos;
        tr.tag = "Ground";

        BoxCollider2D col = tr.AddComponent<BoxCollider2D>();
        col.size = new Vector2(2.5f, 0.8f);

        var comp = tr.AddComponent<TrampolinLlantas>();
        SerializedObject so = new SerializedObject(comp);
        so.FindProperty("fuerzaRebote").floatValue = impulso;
        so.ApplyModifiedProperties();

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(tr.transform);
        visual.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = sprPiso;
        sr.color = new Color(0.2f, 0.2f, 0.25f);
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(2.5f, 0.8f);
        sr.sortingOrder = 2;
    }

    private static void CrearGeyser(Transform padre, string nombre, Vector2 pos, float tActivo, float tInactivo)
    {
        GameObject g = new GameObject(nombre);
        g.transform.SetParent(padre);
        g.transform.position = pos;

        BoxCollider2D col = g.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.8f, 3.5f);

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(g.transform);
        visual.transform.localPosition = new Vector3(0f, 1.75f, 0f);

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = sprLago;
        sr.color = new Color(0.1f, 1f, 0.3f, 0.85f);
        sr.sortingOrder = 3;
        GeneradorNivelColonial.AjustarEscalaVisual(visual, sr, 1.8f, 3.5f);

        var gComp = g.AddComponent<GeyserToxico>();
        SerializedObject so = new SerializedObject(gComp);
        so.FindProperty("tiempoActivo").floatValue = tActivo;
        so.FindProperty("tiempoInactivo").floatValue = tInactivo;
        so.ApplyModifiedProperties();
    }

    private static void CrearCintaTransportadora(Transform padre, string nombre, Vector2 pos, Vector2 tamano, float vel)
    {
        GameObject cinta = new GameObject(nombre);
        cinta.transform.SetParent(padre);
        cinta.transform.position = pos;
        cinta.tag = "Ground";
        int layerGround = LayerMask.NameToLayer("Ground");
        if (layerGround != -1) cinta.layer = layerGround;

        float altoVisual = 1.20f;
        float altoSolido = 0.40f;

        BoxCollider2D col = cinta.AddComponent<BoxCollider2D>();
        col.size = new Vector2(tamano.x * 0.98f, altoSolido);
        col.offset = new Vector2(0f, (altoVisual - altoSolido) / 2f);

        var ct = cinta.AddComponent<CintaTransportadora>();
        SerializedObject so = new SerializedObject(ct);
        so.FindProperty("velocidadCinta").floatValue = vel;
        so.ApplyModifiedProperties();

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(cinta.transform);
        visual.transform.localPosition = Vector3.zero;
        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = sprPlat;
        sr.drawMode = SpriteDrawMode.Simple;
        sr.sortingOrder = 3;
        GeneradorNivelColonial.AjustarEscalaVisual(visual, sr, tamano.x, altoVisual);
    }

    private static void CrearPlataformaGiratoria(Transform padre, string nombre, Vector2 pos, float velGiro)
    {
        GameObject plat = new GameObject(nombre);
        plat.transform.SetParent(padre);
        plat.transform.position = pos;
        plat.tag = "Ground";
        int layerGround = LayerMask.NameToLayer("Ground");
        if (layerGround != -1) plat.layer = layerGround;

        float altoVisual = 1.20f;
        float altoSolido = 0.40f;

        BoxCollider2D col = plat.AddComponent<BoxCollider2D>();
        col.size = new Vector2(5.2f, altoSolido);
        col.offset = new Vector2(0f, (altoVisual - altoSolido) / 2f);

        var pg = plat.AddComponent<PlataformaGiratoria>();
        SerializedObject so = new SerializedObject(pg);
        so.FindProperty("velocidadRotacion").floatValue = velGiro;
        so.ApplyModifiedProperties();

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(plat.transform);
        visual.transform.localPosition = Vector3.zero;
        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = sprPlat;
        sr.drawMode = SpriteDrawMode.Simple;
        sr.sortingOrder = 3;
        GeneradorNivelColonial.AjustarEscalaVisual(visual, sr, 5.5f, altoVisual);
    }

    private static void CrearPlataformaMovil(Transform padre, string nombre, Vector2 posA, Vector2 posB, Vector2 tamano, float velocidad)
    {
        GameObject plat = new GameObject(nombre);
        plat.transform.SetParent(padre);
        plat.transform.position = posA;
        plat.tag = "Ground";
        int layerGround = LayerMask.NameToLayer("Ground");
        if (layerGround != -1) plat.layer = layerGround;

        float altoVisual = 1.20f;
        float altoSolido = 0.40f;

        BoxCollider2D col = plat.AddComponent<BoxCollider2D>();
        col.size = new Vector2(tamano.x * 0.98f, altoSolido);
        col.offset = new Vector2(0f, (altoVisual - altoSolido) / 2f);

        var pm = plat.AddComponent<PlataformaMovil>();
        SerializedObject so = new SerializedObject(pm);
        so.FindProperty("puntoA").vector2Value = posA;
        so.FindProperty("puntoB").vector2Value = posB;
        so.FindProperty("velocidad").floatValue = velocidad;
        so.ApplyModifiedProperties();

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(plat.transform);
        visual.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = sprPlat;
        sr.drawMode = SpriteDrawMode.Simple;
        sr.sortingOrder = 3;
        GeneradorNivelColonial.AjustarEscalaVisual(visual, sr, tamano.x, altoVisual);
    }

    private static void CrearCartel(Transform padre, Vector2 pos, string textoInfo, string tituloParque = "PARQUE DE HUANCAYO")
    {
        GameObject c = new GameObject("Cartel_Informativo_ODS15");
        c.transform.SetParent(padre);
        c.transform.position = pos;

        BoxCollider2D col = c.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(3.5f, 3.2f); // Zona amplia para leer con comodidad

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(c.transform);
        visual.transform.localPosition = Vector3.zero;
        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = sprCartel;
        sr.sortingOrder = 2;
        GeneradorNivelColonial.AjustarEscalaVisual(visual, sr, 1.8f, 2.3f); // Cartel más grande y visible

        var cartelComp = c.AddComponent<CartelInformativo>();
        cartelComp.ConfigurarMensaje(textoInfo, tituloParque);
        SerializedObject so = new SerializedObject(cartelComp);
        so.FindProperty("mensajeEcológico").stringValue = textoInfo;
        so.FindProperty("tituloParque").stringValue = tituloParque;
        so.ApplyModifiedProperties();
    }

    private static void CrearBotella(Transform padre, Vector2 pos)
    {
        GameObject b = new GameObject("Botella_Plastico");
        b.transform.SetParent(padre);
        b.transform.position = pos;
        b.tag = "Collectible";

        BoxCollider2D col = b.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.5f, 0.7f);

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(b.transform);
        visual.transform.localPosition = Vector3.zero;
        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = sprBotella;
        sr.sortingOrder = 5;
        GeneradorNivelColonial.AjustarEscalaVisual(visual, sr, 0.45f, 0.7f);

        visual.AddComponent<RotadorBotella>();
        var bv = b.AddComponent<BotellaVidrio>();
        SerializedObject soB = new SerializedObject(bv);
        AudioClip sfxBotella = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/SFX_BotellaCristal.wav");
        if (sfxBotella != null) soB.FindProperty("sonidoRecoleccion").objectReferenceValue = sfxBotella;
        soB.ApplyModifiedProperties();
    }

    private static void CrearItemPoder(Transform padre, Vector2 pos, ItemMejora.TipoMejora tipo)
    {
        GameObject item = new GameObject($"PowerUp_{tipo}");
        item.transform.SetParent(padre);
        item.transform.position = pos;

        var mej = item.AddComponent<ItemMejora>();
        mej.ConfigurarTipo(tipo);
    }

    private static void CrearEnemigoPatrullero(Transform padre, string nombre, Vector2 pos)
    {
        GameObject en = new GameObject(nombre);
        en.transform.SetParent(padre);
        en.transform.position = pos;
        en.tag = "Enemy";

        BoxCollider2D col = en.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.10f, 1.30f);
        col.offset = new Vector2(0f, 0.65f);

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(en.transform);
        visual.transform.localPosition = new Vector3(0f, 0.65f, 0f);

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        Sprite sprM = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Fangoso/Walk_0.png");
        sr.sprite = (sprM != null) ? sprM : sprBalaBarro;
        sr.sortingOrder = 4;
        GeneradorNivelColonial.AjustarEscalaVisual(visual, sr, 1.45f, 1.45f);

        var em = en.AddComponent<EnemyManager>();
        em.ConfigurarEscalaVisual(visual.transform.localScale);

        SerializedObject so = new SerializedObject(em);
        so.FindProperty("tipo").enumValueIndex = (int)EnemyManager.TipoEnemigo.Patrullero;
        so.FindProperty("capaObstaculosSuelo").intValue = LayerMask.GetMask("Ground");
        so.FindProperty("prefabBalaEnemiga").objectReferenceValue = prefabBalaBarro;
        so.FindProperty("prefabBotellaDrop").objectReferenceValue = prefabBotella;

        SerializedProperty pWalk = so.FindProperty("spritesCaminar");
        if (pWalk != null)
        {
            pWalk.arraySize = 6;
            for (int i = 0; i < 6; i++)
                pWalk.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Fangoso/Walk_{i}.png");
        }

        SerializedProperty pIdle = so.FindProperty("spritesReposo");
        if (pIdle != null)
        {
            pIdle.arraySize = 4;
            for (int i = 0; i < 4; i++)
                pIdle.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Fangoso/Idle_{i}.png");
        }

        so.ApplyModifiedProperties();
    }

    private static void CrearEnemigoTirador(Transform padre, string nombre, Vector2 pos)
    {
        GameObject en = new GameObject(nombre);
        en.transform.SetParent(padre);
        en.transform.position = pos;
        en.tag = "Enemy";

        BoxCollider2D col = en.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.10f, 1.30f);
        col.offset = new Vector2(0f, 0.65f);

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(en.transform);
        visual.transform.localPosition = new Vector3(0f, 0.65f, 0f);

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        Sprite sprM = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/FangosoTirador/Idle_0.png");
        if (sprM == null) sprM = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Fangoso/Idle_0.png");
        sr.sprite = (sprM != null) ? sprM : sprBalaBarro;
        sr.sortingOrder = 4;
        GeneradorNivelColonial.AjustarEscalaVisual(visual, sr, 1.45f, 1.45f);

        var em = en.AddComponent<EnemyManager>();
        em.ConfigurarEscalaVisual(visual.transform.localScale);

        SerializedObject so = new SerializedObject(em);
        so.FindProperty("tipo").enumValueIndex = (int)EnemyManager.TipoEnemigo.Tirador;
        so.FindProperty("capaObstaculosSuelo").intValue = LayerMask.GetMask("Ground");
        so.FindProperty("prefabBalaEnemiga").objectReferenceValue = prefabBalaBarro;
        so.FindProperty("prefabBotellaDrop").objectReferenceValue = prefabBotella;

        SerializedProperty pIdle = so.FindProperty("spritesReposo");
        if (pIdle != null)
        {
            pIdle.arraySize = 4;
            for (int i = 0; i < 4; i++)
                pIdle.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/FangosoTirador/Idle_{i}.png");
        }

        SerializedProperty pShoot = so.FindProperty("spritesDisparo");
        if (pShoot != null)
        {
            pShoot.arraySize = 5;
            for (int i = 0; i < 5; i++)
                pShoot.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/FangosoTirador/Shoot_{i}.png");
        }

        SerializedProperty pHurt = so.FindProperty("spritesDanio");
        if (pHurt != null)
        {
            pHurt.arraySize = 2;
            for (int i = 0; i < 2; i++)
                pHurt.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/FangosoTirador/Hurt_{i}.png");
        }

        SerializedProperty pDeath = so.FindProperty("spritesMuerte");
        if (pDeath != null)
        {
            pDeath.arraySize = 5;
            for (int i = 0; i < 5; i++)
                pDeath.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/FangosoTirador/Death_{i}.png");
        }

        so.ApplyModifiedProperties();
    }

    private static void CrearEnemigoVolador(Transform padre, string nombre, Vector2 pos)
    {
        GameObject en = new GameObject(nombre);
        en.transform.SetParent(padre);
        en.transform.position = pos;
        en.tag = "Enemy";

        CircleCollider2D col = en.AddComponent<CircleCollider2D>();
        col.radius = 0.8f;

        Rigidbody2D rb = en.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(en.transform);
        visual.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        Sprite sprV = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/FangosoVolador/Fly_0.png");
        if (sprV == null) sprV = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/FangosoVolador.png");
        sr.sprite = (sprV != null) ? sprV : sprBalaBarro;
        sr.sortingOrder = 4;
        GeneradorNivelColonial.AjustarEscalaVisual(visual, sr, 1.9f, 1.9f);

        var fly = en.AddComponent<EnemyFlyingManager>();
        SerializedObject so = new SerializedObject(fly);
        so.FindProperty("prefabBalaBarro").objectReferenceValue = prefabBalaBarro;
        so.FindProperty("prefabBotellaDrop").objectReferenceValue = prefabBotella;

        SerializedProperty pFly = so.FindProperty("spritesVuelo");
        if (pFly != null)
        {
            pFly.arraySize = 6;
            for (int i = 0; i < 6; i++)
                pFly.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/FangosoVolador/Fly_{i}.png");
        }

        SerializedProperty pDive = so.FindProperty("spritesPicada");
        if (pDive != null)
        {
            pDive.arraySize = 4;
            for (int i = 0; i < 4; i++)
                pDive.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/FangosoVolador/Dive_{i}.png");
        }

        SerializedProperty pHurt = so.FindProperty("spritesDanio");
        if (pHurt != null)
        {
            pHurt.arraySize = 2;
            for (int i = 0; i < 2; i++)
                pHurt.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/FangosoVolador/Hurt_{i}.png");
        }

        SerializedProperty pDeath = so.FindProperty("spritesMuerte");
        if (pDeath != null)
        {
            pDeath.arraySize = 5;
            for (int i = 0; i < 5; i++)
                pDeath.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/FangosoVolador/Death_{i}.png");
        }

        so.ApplyModifiedProperties();
    }

    private static void CrearEnemigoBlindado(Transform padre, string nombre, Vector2 pos)
    {
        GameObject en = new GameObject(nombre);
        en.transform.SetParent(padre);
        en.transform.position = pos;
        en.tag = "Enemy";

        BoxCollider2D col = en.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.30f, 1.60f);
        col.offset = new Vector2(0f, 0.80f);

        Rigidbody2D rb = en.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(en.transform);
        visual.transform.localPosition = new Vector3(0f, 0.80f, 0f);

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        Sprite sprB = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/FangosoBlindado/Walk_0.png");
        sr.sprite = (sprB != null) ? sprB : sprBalaBarro;
        sr.sortingOrder = 4;
        GeneradorNivelColonial.AjustarEscalaVisual(visual, sr, 1.70f, 1.70f);

        var arm = en.AddComponent<EnemyArmored>();
        arm.ConfigurarEscalaVisual(visual.transform.localScale);

        SerializedObject so = new SerializedObject(arm);
        so.FindProperty("prefabBotellaDrop").objectReferenceValue = prefabBotella;

        SerializedProperty pIdle = so.FindProperty("spritesIdle");
        if (pIdle != null)
        {
            pIdle.arraySize = 4;
            for (int i = 0; i < 4; i++)
                pIdle.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/FangosoBlindado/Idle_{i}.png");
        }

        SerializedProperty pWalk = so.FindProperty("spritesWalk");
        if (pWalk != null)
        {
            pWalk.arraySize = 6;
            for (int i = 0; i < 6; i++)
                pWalk.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/FangosoBlindado/Walk_{i}.png");
        }

        SerializedProperty pShield = so.FindProperty("spritesShieldBash");
        if (pShield != null)
        {
            pShield.arraySize = 4;
            for (int i = 0; i < 4; i++)
                pShield.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/FangosoBlindado/ShieldBash_{i}.png");
        }

        SerializedProperty pHurt = so.FindProperty("spritesHurt");
        if (pHurt != null)
        {
            pHurt.arraySize = 2;
            for (int i = 0; i < 2; i++)
                pHurt.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/FangosoBlindado/Hurt_{i}.png");
        }

        SerializedProperty pDeath = so.FindProperty("spritesDeath");
        if (pDeath != null)
        {
            pDeath.arraySize = 6;
            for (int i = 0; i < 6; i++)
                pDeath.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/FangosoBlindado/Death_{i}.png");
        }

        so.ApplyModifiedProperties();
    }

    private static void CrearEnemigoCanionero(Transform padre, string nombre, Vector2 pos)
    {
        GameObject en = new GameObject(nombre);
        en.transform.SetParent(padre);
        en.transform.position = pos;
        en.tag = "Enemy";

        BoxCollider2D col = en.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.7f, 2.0f);

        Rigidbody2D rb = en.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(en.transform);
        visual.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        Sprite sprC = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/FangosoCanionero/Idle_0.png");
        sr.sprite = (sprC != null) ? sprC : sprBalaBarro;
        sr.sortingOrder = 4;
        GeneradorNivelColonial.AjustarEscalaVisual(visual, sr, 2.2f, 2.2f);

        var can = en.AddComponent<EnemyCannoneer>();
        can.ConfigurarEscalaVisual(visual.transform.localScale);

        SerializedObject so = new SerializedObject(can);
        so.FindProperty("prefabBalaBarro").objectReferenceValue = prefabBalaBarro;
        so.FindProperty("prefabBotellaDrop").objectReferenceValue = prefabBotella;

        SerializedProperty pIdle = so.FindProperty("spritesIdle");
        if (pIdle != null)
        {
            pIdle.arraySize = 4;
            for (int i = 0; i < 4; i++)
                pIdle.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/FangosoCanionero/Idle_{i}.png");
        }

        SerializedProperty pMortar = so.FindProperty("spritesMortar");
        if (pMortar != null)
        {
            pMortar.arraySize = 6;
            for (int i = 0; i < 6; i++)
                pMortar.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/FangosoCanionero/Mortar_{i}.png");
        }

        SerializedProperty pHurt = so.FindProperty("spritesHurt");
        if (pHurt != null)
        {
            pHurt.arraySize = 2;
            for (int i = 0; i < 2; i++)
                pHurt.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/FangosoCanionero/Hurt_{i}.png");
        }

        SerializedProperty pDeath = so.FindProperty("spritesDeath");
        if (pDeath != null)
        {
            pDeath.arraySize = 5;
            for (int i = 0; i < 5; i++)
                pDeath.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/FangosoCanionero/Death_{i}.png");
        }

        so.ApplyModifiedProperties();
    }

    private static void CrearJefeTitan(Transform padre, string nombre, Vector2 pos)
    {
        GameObject boss = new GameObject(nombre);
        boss.transform.SetParent(padre);
        boss.transform.position = pos;
        boss.tag = "Enemy";

        BoxCollider2D col = boss.AddComponent<BoxCollider2D>();
        col.size = new Vector2(2.8f, 3.4f);

        Rigidbody2D rb = boss.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(boss.transform);
        visual.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        Sprite sprT = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/FangosoTitan/Idle_0.png");
        sr.sprite = (sprT != null) ? sprT : sprBalaBarro;
        sr.sortingOrder = 4;
        GeneradorNivelColonial.AjustarEscalaVisual(visual, sr, 3.6f, 3.6f);

        var titan = boss.AddComponent<BossTitanManager>();
        titan.ConfigurarEscalaVisual(visual.transform.localScale);

        SerializedObject so = new SerializedObject(titan);
        so.FindProperty("prefabBalaBarro").objectReferenceValue = prefabBalaBarro;
        so.FindProperty("prefabBotellaDrop").objectReferenceValue = prefabBotella;

        SerializedProperty pIdle = so.FindProperty("spritesIdle");
        if (pIdle != null)
        {
            pIdle.arraySize = 4;
            for (int i = 0; i < 4; i++)
                pIdle.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/FangosoTitan/Idle_{i}.png");
        }

        SerializedProperty pSlam = so.FindProperty("spritesGroundSlam");
        if (pSlam != null)
        {
            pSlam.arraySize = 6;
            for (int i = 0; i < 6; i++)
                pSlam.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/FangosoTitan/GroundSlam_{i}.png");
        }

        SerializedProperty pRoar = so.FindProperty("spritesRoar");
        if (pRoar != null)
        {
            pRoar.arraySize = 4;
            for (int i = 0; i < 4; i++)
                pRoar.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/FangosoTitan/Roar_{i}.png");
        }

        SerializedProperty pHurt = so.FindProperty("spritesHurt");
        if (pHurt != null)
        {
            pHurt.arraySize = 3;
            for (int i = 0; i < 3; i++)
                pHurt.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/FangosoTitan/Hurt_{i}.png");
        }

        SerializedProperty pDeath = so.FindProperty("spritesDeath");
        if (pDeath != null)
        {
            pDeath.arraySize = 6;
            for (int i = 0; i < 6; i++)
                pDeath.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/FangosoTitan/Death_{i}.png");
        }

        so.ApplyModifiedProperties();
    }

    private static void CrearBossSupremeHuaytapallana(Transform padre, string nombre, Vector2 pos)
    {
        GameObject boss = new GameObject(nombre);
        boss.transform.SetParent(padre);
        boss.transform.position = pos;
        boss.tag = "Enemy";

        BoxCollider2D col = boss.AddComponent<BoxCollider2D>();
        col.size = new Vector2(3.0f, 3.8f);

        Rigidbody2D rb = boss.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(boss.transform);
        visual.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        Sprite sprH = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ColosoHuaytapallana/Stance_0.png");
        sr.sprite = (sprH != null) ? sprH : sprBalaBarro;
        sr.sortingOrder = 4;
        GeneradorNivelColonial.AjustarEscalaVisual(visual, sr, 4.0f, 4.0f);

        var supreme = boss.AddComponent<BossSupremeHuaytapallana>();
        supreme.ConfigurarEscalaVisual(visual.transform.localScale);

        SerializedObject so = new SerializedObject(supreme);
        so.FindProperty("prefabBalaBarro").objectReferenceValue = prefabBalaBarro;
        so.FindProperty("prefabBotellaDrop").objectReferenceValue = prefabBotella;

        SerializedProperty pStance = so.FindProperty("spritesStance");
        if (pStance != null)
        {
            pStance.arraySize = 4;
            for (int i = 0; i < 4; i++)
                pStance.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/ColosoHuaytapallana/Stance_{i}.png");
        }

        SerializedProperty pSpikes = so.FindProperty("spritesIceSpikes");
        if (pSpikes != null)
        {
            pSpikes.arraySize = 6;
            for (int i = 0; i < 6; i++)
                pSpikes.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/ColosoHuaytapallana/IceSpikes_{i}.png");
        }

        SerializedProperty pBeam = so.FindProperty("spritesAcidBeam");
        if (pBeam != null)
        {
            pBeam.arraySize = 6;
            for (int i = 0; i < 6; i++)
                pBeam.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/ColosoHuaytapallana/AcidBeam_{i}.png");
        }

        SerializedProperty pHurt = so.FindProperty("spritesHurt");
        if (pHurt != null)
        {
            pHurt.arraySize = 3;
            for (int i = 0; i < 3; i++)
                pHurt.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/ColosoHuaytapallana/Hurt_{i}.png");
        }

        SerializedProperty pDeath = so.FindProperty("spritesDeath");
        if (pDeath != null)
        {
            pDeath.arraySize = 6;
            for (int i = 0; i < 6; i++)
                pDeath.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/ColosoHuaytapallana/Death_{i}.png");
        }

        so.ApplyModifiedProperties();
    }

    private static void CrearMeta(Transform padre, Vector2 pos)
    {
        GameObject meta = new GameObject("Contenedor_Meta_Reciclaje");
        meta.transform.SetParent(padre);
        meta.transform.position = pos;

        BoxCollider2D col = meta.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(2.0f, 2.3f);

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(meta.transform);
        visual.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = sprMeta;
        sr.sortingOrder = 2;
        GeneradorNivelColonial.AjustarEscalaVisual(visual, sr, 2.0f, 2.3f);

        var cm = meta.AddComponent<ContenedorMeta>();
        SerializedObject soM = new SerializedObject(cm);
        soM.FindProperty("botellasRequeridas").intValue = 15;
        AudioClip sfxPurif = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/SFX_VictoriaPurificacion.wav");
        if (sfxPurif != null) soM.FindProperty("sonidoVictoria").objectReferenceValue = sfxPurif;
        soM.ApplyModifiedProperties();
    }

    private static void CrearJugadorYUI(Vector2 posInicial)
    {
        // 1. Instanciar jugador
        GameObject jugador = new GameObject("Illari_Player");
        jugador.transform.position = posInicial;
        jugador.tag = "Player";

        BoxCollider2D col = jugador.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.55f, 1.36f);
        col.offset = new Vector2(0f, 0.68f); // Base del collider alineada exactamente con los pies en Y=0

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(jugador.transform);
        visual.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        Sprite sprIyari = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Iyari/Individual/Idle_00.png");
        if (sprIyari == null) sprIyari = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Iyari/Idle_0.png");
        sr.sprite = sprIyari;
        sr.sortingOrder = 5;

        // Escala 0.88 para altura de 1.38m (proporción atlética y ágil para plataformas arcade)
        visual.transform.localScale = new Vector3(0.88f, 0.88f, 1.0f);

        // Animator asignado al jugador (padre) para que las animaciones con binding 'Visual' afecten al hijo Visual
        Animator anim = jugador.AddComponent<Animator>();
        anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/IyariAnimator.controller");

        GameObject pDisparo = new GameObject("PuntoDisparo");
        pDisparo.transform.SetParent(jugador.transform);
        pDisparo.transform.localPosition = new Vector3(0.50f, 0.65f, 0f);

        IllariPlatformer platformer = jugador.AddComponent<IllariPlatformer>();
        platformer.ConfigurarEscalaVisual(visual.transform.localScale);

        SerializedObject so = new SerializedObject(platformer);
        so.FindProperty("capaSuelo").intValue = LayerMask.GetMask("Ground");
        so.FindProperty("prefabProyectilAgua").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BalaAgua.prefab");
        so.FindProperty("puntoDisparo").objectReferenceValue = pDisparo.transform;
        so.FindProperty("visualTransform").objectReferenceValue = visual.transform;
        so.FindProperty("animator").objectReferenceValue = anim;
        so.ApplyModifiedProperties();

        // 2. Vincular Cámara al Jugador
        Camera cam = Camera.main;
        if (cam != null)
        {
            CameraFollow cf = cam.GetComponent<CameraFollow>();
            if (cf != null)
            {
                cf.target = jugador.transform;
                cf.CentrarInmediato();
            }
        }

        // 3. Crear GameManager y construir UI completa (Barra de vida, Botellas, Pausa, Guardado)
        GameObject gmObj = new GameObject("GameManagerIyari");
        var gm = gmObj.AddComponent<GameManagerIyari>();
        gm.GarantizarUI();
    }

    private static void ActualizarBuildSettings()
    {
        string[] escenas = new string[]
        {
            "Assets/Scenes/MenuPrincipal.unity",
            "Assets/Scenes/NivelParqueColonial.unity",
            "Assets/Scenes/Nivel2_CerritoLibertad.unity",
            "Assets/Scenes/Nivel3_ParqueTupacAmaru.unity",
            "Assets/Scenes/Nivel4_ParqueSombreros.unity",
            "Assets/Scenes/Nivel5_PlazaConstitucion.unity",
            "Assets/Scenes/Nivel6_BosqueDorado.unity",
            "Assets/Scenes/Nivel7_RioShullcas.unity",
            "Assets/Scenes/Nivel8_TorreTorre.unity",
            "Assets/Scenes/Nivel9_NevadoHuaytapallana.unity"
        };

        EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[escenas.Length];
        for (int i = 0; i < escenas.Length; i++)
        {
            buildScenes[i] = new EditorBuildSettingsScene(escenas[i], true);
        }

        EditorBuildSettings.scenes = buildScenes;
        Debug.Log($"<color=#00FF88><b>[BUILD SETTINGS]</b> Las {escenas.Length} escenas de la campaña completa de Huancayo 2026 configuradas correctamente.</color>");
    }
}


[InitializeOnLoad]
public class AutoEjecutorGeneradorNiveles
{
    static AutoEjecutorGeneradorNiveles()
    {
        EditorApplication.delayCall += VerificarTrigger;
    }

    public static void VerificarTrigger()
    {
        string triggerPath = "Temp/trigger_generate_levels.txt";
        if (File.Exists(triggerPath))
        {
            try { File.Delete(triggerPath); } catch {}
            Debug.Log("[TRIGGER] Ejecutando GeneradorNivelesIllari.GenerarTodosLosNiveles()...");
            GeneradorNivelesIllari.GenerarTodosLosNiveles();
        }
    }
}
