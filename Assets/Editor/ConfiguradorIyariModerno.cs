using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// Script de Editor integral para 'Illari 2026':
/// 1. Configura todas las texturas nuevas como Sprites 2D nítidos y tileables (FullRect + Repeat).
/// 2. Genera los AnimationClip nativos de Illari (Idle, Run, Jump acrobático) con transiciones instantáneas.
/// 3. Genera el AnimationClip y AnimatorController de caminata para los Fangosos (Monstruos de barro).
/// 4. Configura prefabs (BalaBarro a 0.3m, BotellaPlastico).
/// 5. Regenera el Nivel 1 del Parque de la Identidad Huanca con plataformas tiled y sin deformaciones.
/// </summary>
public class ConfiguradorIyariModerno
{
    private static readonly string[] RutasSpritesGenerales = new string[]
    {
        "Assets/Sprites/MonstruoBarro.png",
        "Assets/Sprites/EstacionReciclaje.png",
        "Assets/Sprites/BolaBarro.png",
        "Assets/Sprites/BotellaPlastico.png",
        "Assets/Sprites/LagoToxico.png",
        "Assets/Sprites/CartelInformativo.png",
        "Assets/Sprites/FondoParqueModernoSeamless.png"
    };

    private static readonly string[] RutasSpritesTiled = new string[]
    {
        "Assets/Sprites/PisoParqueHuanca.png",
        "Assets/Sprites/PlataformaParque.png"
    };

    [MenuItem("Iyari/Configurar Iyari 2026 y Animaciones")]
    public static void EjecutarTodo()
    {
        ConfigurarTexturas();
        ConfigurarTexturasTiled();
        ConfigurarSpritesAnimacionIllari();
        ConfigurarSpritesAnimacionFangoso();

        // Clips e Illari Animator Controller
        AnimationClip clipIdle = CrearClipIdle();
        AnimationClip clipRun = CrearClipRun();
        AnimationClip clipJump = CrearClipJump();
        AnimatorController controllerIllari = CrearOActualizarAnimatorController(clipIdle, clipRun, clipJump);

        // Clip y Animator Controller de Fangoso
        AnimationClip clipFangosoWalk = CrearClipFangosoWalk();
        AnimationClip clipFangosoIdle = CrearClipFangosoIdle();
        CrearAnimatorFangoso(clipFangosoWalk, clipFangosoIdle);

        // Prefabs
        CrearPrefabBalaBarro();
        CrearPrefabBotellaPlastico();

        // Actualizar jugador
        ActualizarJugadorIyari(controllerIllari);

        // Regenerar el nivel completo
        GeneradorNivelColonial generador = Object.FindFirstObjectByType<GeneradorNivelColonial>();
        if (generador != null)
        {
            generador.GenerarNivel();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("<color=#00FF88><b>[ILLARI]</b> ¡Nivel 1 (Parque de la Identidad Huanca) regenerado con plataformas TILED y animaciones pulidas!</color>");
        }
    }

    private static void ConfigurarTexturas()
    {
        foreach (string ruta in RutasSpritesGenerales)
        {
            TextureImporter importer = AssetImporter.GetAtPath(ruta) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }
    }

    private static void ConfigurarTexturasTiled()
    {
        foreach (string ruta in RutasSpritesTiled)
        {
            TextureImporter importer = AssetImporter.GetAtPath(ruta) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;

                TextureImporterSettings tis = new TextureImporterSettings();
                importer.ReadTextureSettings(tis);
                tis.spriteMeshType = SpriteMeshType.FullRect; // OBLIGATORIO para Tiled SpriteRenderer
                importer.SetTextureSettings(tis);

                importer.wrapMode = TextureWrapMode.Repeat;         // OBLIGATORIO para repetición continua
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;

                if (ruta.Contains("Piso") || ruta.Contains("Plataforma"))
                {
                    importer.spritePixelsPerUnit = 200f; // 464px / 200 = 2.32m (escala humana exacta para muros y plataformas)
                }
                else if (ruta.Contains("Piedra") || ruta.Contains("Tierra"))
                {
                    importer.spritePixelsPerUnit = 200f;
                }
                else if (ruta.Contains("Pasto"))
                {
                    importer.spritePixelsPerUnit = 200f;
                }

                importer.SaveAndReimport();
            }
        }
    }

    private static void ConfigurarSpritesAnimacionIllari()
    {
        string dirAnim = "Assets/Sprites/Iyari";
        if (!Directory.Exists(dirAnim)) return;

        string[] archivos = Directory.GetFiles(dirAnim, "*.png");
        foreach (string archivo in archivos)
        {
            string rutaRelativa = archivo.Replace("\\", "/");
            TextureImporter importer = AssetImporter.GetAtPath(rutaRelativa) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Point;
                importer.spritePivot = new Vector2(0.5f, 0.05f); // Pivote en los pies
                importer.spritePixelsPerUnit = 100f;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }
        AssetDatabase.Refresh();
    }

    private static void ConfigurarSpritesAnimacionFangoso()
    {
        string dirAnim = "Assets/Sprites/Fangoso";
        if (!Directory.Exists(dirAnim)) return;

        string[] archivos = Directory.GetFiles(dirAnim, "*.png");
        foreach (string archivo in archivos)
        {
            string rutaRelativa = archivo.Replace("\\", "/");
            TextureImporter importer = AssetImporter.GetAtPath(rutaRelativa) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Point;
                importer.spritePivot = new Vector2(0.5f, 0.1f);
                importer.spritePixelsPerUnit = 100f;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }
        AssetDatabase.Refresh();
    }

    private static AnimationClip CrearClipIdle()
    {
        string path = "Assets/Animations/Iyari/Iyari_Idle.anim";
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 8;

        EditorCurveBinding binding = new EditorCurveBinding();
        binding.type = typeof(SpriteRenderer);
        binding.path = "Visual";
        binding.propertyName = "m_Sprite";

        Sprite[] sprites = new Sprite[8];
        for (int i = 0; i < 8; i++)
        {
            sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Iyari/Idle_{i}.png");
        }

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[8];
        for (int i = 0; i < 8; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe();
            keyframes[i].time = i / 8f;
            keyframes[i].value = sprites[i];
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static AnimationClip CrearClipRun()
    {
        string path = "Assets/Animations/Iyari/Iyari_Run.anim";
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 12;

        EditorCurveBinding binding = new EditorCurveBinding();
        binding.type = typeof(SpriteRenderer);
        binding.path = "Visual";
        binding.propertyName = "m_Sprite";

        Sprite[] sprites = new Sprite[10];
        for (int i = 0; i < 10; i++)
        {
            sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Iyari/Run_{i}.png");
        }

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[10];
        for (int i = 0; i < 10; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe();
            keyframes[i].time = i / 12f;
            keyframes[i].value = sprites[i];
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static AnimationClip CrearClipJump()
    {
        string path = "Assets/Animations/Iyari/Iyari_Jump.anim";
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 14; // Giro aéreo dinámico y ágil

        EditorCurveBinding binding = new EditorCurveBinding();
        binding.type = typeof(SpriteRenderer);
        binding.path = "Visual";
        binding.propertyName = "m_Sprite";

        Sprite[] sprites = new Sprite[10];
        for (int i = 0; i < 10; i++)
        {
            sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Iyari/Jump_{i}.png");
        }

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[10];
        for (int i = 0; i < 10; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe();
            keyframes[i].time = i / 14f;
            keyframes[i].value = sprites[i];
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true; // Permite giro continuo si permanece en el aire
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static AnimatorController CrearOActualizarAnimatorController(AnimationClip idle, AnimationClip run, AnimationClip jump)
    {
        string path = "Assets/Animations/IyariAnimator.controller";
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);

        controller.AddParameter("estaEnAire", AnimatorControllerParameterType.Bool);
        controller.AddParameter("velocidadX", AnimatorControllerParameterType.Float);

        var sm = controller.layers[0].stateMachine;

        var stateIdle = sm.AddState("Iyari_Idle");
        stateIdle.motion = idle;

        var stateRun = sm.AddState("Iyari_Run");
        stateRun.motion = run;

        var stateJump = sm.AddState("Iyari_Jump");
        stateJump.motion = jump;

        sm.defaultState = stateIdle;

        // Idle -> Run (Instantáneo: duration = 0)
        var tIdleToRun = stateIdle.AddTransition(stateRun);
        tIdleToRun.hasExitTime = false;
        tIdleToRun.duration = 0f;
        tIdleToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "velocidadX");
        tIdleToRun.AddCondition(AnimatorConditionMode.IfNot, 0, "estaEnAire");

        // Run -> Idle (Instantáneo: duration = 0)
        var tRunToIdle = stateRun.AddTransition(stateIdle);
        tRunToIdle.hasExitTime = false;
        tRunToIdle.duration = 0f;
        tRunToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "velocidadX");

        // AnyState -> Jump (Instantáneo: duration = 0)
        var tAnyToJump = sm.AddAnyStateTransition(stateJump);
        tAnyToJump.hasExitTime = false;
        tAnyToJump.duration = 0f;
        tAnyToJump.canTransitionToSelf = false;
        tAnyToJump.AddCondition(AnimatorConditionMode.If, 0, "estaEnAire");

        // Jump -> Idle (Aterrizaje en reposo)
        var tJumpToIdle = stateJump.AddTransition(stateIdle);
        tJumpToIdle.hasExitTime = false;
        tJumpToIdle.duration = 0f;
        tJumpToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "estaEnAire");
        tJumpToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "velocidadX");

        // Jump -> Run (Aterrizaje en carrera)
        var tJumpToRun = stateJump.AddTransition(stateRun);
        tJumpToRun.hasExitTime = false;
        tJumpToRun.duration = 0f;
        tJumpToRun.AddCondition(AnimatorConditionMode.IfNot, 0, "estaEnAire");
        tJumpToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "velocidadX");

        AssetDatabase.SaveAssets();
        return controller;
    }

    private static AnimationClip CrearClipFangosoWalk()
    {
        string path = "Assets/Animations/Fangoso/Fangoso_Walk.anim";
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 8;

        EditorCurveBinding binding = new EditorCurveBinding();
        binding.type = typeof(SpriteRenderer);
        binding.path = "";
        binding.propertyName = "m_Sprite";

        Sprite[] sprites = new Sprite[6];
        for (int i = 0; i < 6; i++)
        {
            sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Fangoso/Walk_{i}.png");
            if (sprites[i] == null) sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/MonstruoBarro.png");
        }

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[6];
        for (int i = 0; i < 6; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe();
            keyframes[i].time = i / 8f;
            keyframes[i].value = sprites[i];
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static AnimationClip CrearClipFangosoIdle()
    {
        string path = "Assets/Animations/Fangoso/Fangoso_Idle.anim";
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 2;

        EditorCurveBinding binding = new EditorCurveBinding();
        binding.type = typeof(SpriteRenderer);
        binding.path = "";
        binding.propertyName = "m_Sprite";

        Sprite spriteIdle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Fangoso/Idle_0.png");
        if (spriteIdle == null) spriteIdle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/MonstruoBarro.png");

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[1];
        keyframes[0] = new ObjectReferenceKeyframe();
        keyframes[0].time = 0f;
        keyframes[0].value = spriteIdle;

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static void CrearAnimatorFangoso(AnimationClip walkClip, AnimationClip idleClip)
    {
        string path = "Assets/Animations/FangosoAnimator.controller";
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);

        controller.AddParameter("velocidadX", AnimatorControllerParameterType.Float);

        var sm = controller.layers[0].stateMachine;

        var stateIdle = sm.AddState("Fangoso_Idle");
        stateIdle.motion = idleClip;

        var stateWalk = sm.AddState("Fangoso_Walk");
        stateWalk.motion = walkClip;

        sm.defaultState = stateIdle;

        // Idle -> Walk (al moverse)
        var tIdleToWalk = stateIdle.AddTransition(stateWalk);
        tIdleToWalk.hasExitTime = false;
        tIdleToWalk.duration = 0f;
        tIdleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.05f, "velocidadX");

        // Walk -> Idle (al detenerse o voltear)
        var tWalkToIdle = stateWalk.AddTransition(stateIdle);
        tWalkToIdle.hasExitTime = false;
        tWalkToIdle.duration = 0f;
        tWalkToIdle.AddCondition(AnimatorConditionMode.Less, 0.05f, "velocidadX");

        AssetDatabase.SaveAssets();
    }

    private static void CrearPrefabBalaBarro()
    {
        string path = "Assets/Prefabs/BalaBarro.prefab";
        GameObject go = new GameObject("BalaBarro");

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.2f;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        Sprite spriteBarro = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/BolaBarro.png");
        sr.sprite = spriteBarro;
        sr.color = Color.white;
        sr.sortingOrder = 8;
        go.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

        Bala bala = go.AddComponent<Bala>();
        SerializedObject so = new SerializedObject(bala);
        so.FindProperty("esBalaEnemiga").boolValue = true;
        so.FindProperty("velocidad").floatValue = 11f;
        so.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
    }

    private static void CrearPrefabBotellaPlastico()
    {
        string path = "Assets/Prefabs/BotellaPlastico.prefab";
        GameObject go = new GameObject("BotellaPlastico");
        go.tag = "Collectible";

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.5f, 0.7f);

        BotellaVidrio bv = go.AddComponent<BotellaVidrio>();

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(go.transform);
        visual.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        Sprite spBot = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/BotellaPlastico.png");
        if (spBot == null) spBot = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/BotellaVidrio.png");
        sr.sprite = spBot;
        sr.sortingOrder = 5;

        GeneradorNivelColonial.AjustarEscalaVisual(visual, sr, 0.45f, 0.7f);
        visual.AddComponent<RotadorBotella>();

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
    }

    private static void ActualizarJugadorIyari(AnimatorController controller)
    {
        GameObject player = GameObject.Find("Illari_Player");
        if (player == null) return;

        Animator anim = player.GetComponent<Animator>();
        if (anim == null) anim = player.AddComponent<Animator>();
        anim.runtimeAnimatorController = controller;

        Transform visual = player.transform.Find("Visual");
        if (visual != null)
        {
            SpriteRenderer sr = visual.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Iyari/Idle_0.png");
                sr.sprite = sp;
                sr.color = Color.white;
                sr.sortingOrder = 6;
            }
            visual.localScale = Vector3.one;
        }

        BoxCollider2D boxCol = player.GetComponent<BoxCollider2D>();
        if (boxCol != null)
        {
            boxCol.size = new Vector2(0.75f, 1.55f);
            boxCol.offset = new Vector2(0f, 0f);
        }

        IllariPlatformer ip = player.GetComponent<IllariPlatformer>();
        if (ip != null && visual != null)
        {
            ip.ConfigurarEscalaVisual(Vector3.one);
        }
    }
}
