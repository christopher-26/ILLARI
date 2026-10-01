using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.Linq;
using System.IO;

/// <summary>
/// Importa el sprite sheet de Illari, lo corta en 30 sprites y genera
/// los 8 AnimationClips conectados al IyariAnimator.controller.
/// Ejecutar desde menú: Iyari / Importar Animaciones Illari
/// </summary>
public static class ImportadorAnimacionesIllari
{
    // Ruta fuente del sprite sheet subido
    private const string RUTA_FUENTE = @"C:\Users\15-CW1008LA\.gemini\antigravity\brain\9925b625-5273-45db-980a-a567d2017904\.user_uploaded\media_1790300538798.jpg";
    private const string RUTA_DESTINO = "Assets/Sprites/Iyari/IllariSpriteSheet.jpg";
    private const string RUTA_CLIPS = "Assets/Animations/Iyari/";
    private const string RUTA_ANIMATOR = "Assets/Animations/IyariAnimator.controller";

    [MenuItem("Iyari/Importar Animaciones Illari (Sprite Sheet)")]
    public static void ImportarAnimaciones()
    {
        // === PASO 1: Copiar y configurar sprite sheet ===
        if (!Directory.Exists("Assets/Sprites/Iyari"))
            Directory.CreateDirectory("Assets/Sprites/Iyari");
        if (!Directory.Exists(RUTA_CLIPS))
            Directory.CreateDirectory(RUTA_CLIPS);

        File.Copy(RUTA_FUENTE, RUTA_DESTINO, true);
        AssetDatabase.Refresh();

        TextureImporter imp = AssetImporter.GetAtPath(RUTA_DESTINO) as TextureImporter;
        imp.GetSourceTextureWidthAndHeight(out int W, out int H);
        // W=1024, H=768

        // === PASO 2: Definir rects proporcionales (imagen 1024x768) ===
        // Layout: col izq = 46% ancho, col der = 54% ancho; 4 filas iguales; label 28px
        float lW  = W * 0.455f;  // ~466px col izq
        float rx  = lW;
        float rW  = W - lW;      // ~558px col der
        float rowH = H * 0.25f;  // 192px por fila
        float labelH = 28f;
        float sprH = rowH - labelH - 4f; // ~160px sprite util
        float pad = 3f;

        // Y base de cada fila en espacio imagen (0=arriba)
        float r1y = labelH;
        float r2y = rowH + labelH;
        float r3y = 2f * rowH + labelH;
        float r4y = 3f * rowH + labelH;

        var meta = new List<SpriteMetaData>();

        // Convierte y-imagen (top=0) → y-Unity (bottom=0)
        // ImgRect(x, imageY, w, h) → Rect(x, H-imageY-h, w, h)

        // FILA 1 — IDLE (4) y RUN (6)
        float idleW = (lW - pad) / 4f;
        for (int i = 0; i < 4; i++)
            meta.Add(MakeSpr("Idle_" + i.ToString("00"),
                0f + i * idleW + pad, r1y, idleW - pad, sprH, W, H));

        float runW = (rW - pad) / 6f;
        for (int i = 0; i < 6; i++)
            meta.Add(MakeSpr("Run_" + i.ToString("00"),
                rx + i * runW + pad, r1y, runW - pad, sprH, W, H));

        // FILA 2 — SALTO (4) y DOBLE SALTO (4)
        float saltW = (lW - pad) / 4f;
        for (int i = 0; i < 4; i++)
            meta.Add(MakeSpr("Salto_" + i.ToString("00"),
                0f + i * saltW + pad, r2y, saltW - pad, sprH, W, H));

        float dsW = (rW - pad) / 4f;
        for (int i = 0; i < 4; i++)
            meta.Add(MakeSpr("DobleSalto_" + i.ToString("00"),
                rx + i * dsW + pad, r2y, dsW - pad, sprH, W, H));

        // FILA 3 — DISPARO (4) y DISPARO EN AIRE (4)
        float dispW = (lW - pad) / 4f;
        for (int i = 0; i < 4; i++)
            meta.Add(MakeSpr("Disparo_" + i.ToString("00"),
                0f + i * dispW + pad, r3y, dispW - pad, sprH, W, H));

        float daW = (rW - pad) / 4f;
        for (int i = 0; i < 4; i++)
            meta.Add(MakeSpr("DisparoAire_" + i.ToString("00"),
                rx + i * daW + pad, r3y, daW - pad, sprH, W, H));

        // FILA 4 — DAÑO (3) y MUERTE (5)
        float danW = (lW - pad) / 3f;
        for (int i = 0; i < 3; i++)
            meta.Add(MakeSpr("Danio_" + i.ToString("00"),
                0f + i * danW + pad, r4y, danW - pad, sprH, W, H));

        float muerteW = (rW - pad) / 5f;
        for (int i = 0; i < 5; i++)
            meta.Add(MakeSpr("Muerte_" + i.ToString("00"),
                rx + i * muerteW + pad, r4y, muerteW - pad, sprH, W, H));

        // Aplicar configuración base del importer
        imp.textureType  = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Multiple;
        imp.mipmapEnabled = false;
        imp.filterMode   = FilterMode.Bilinear;
        imp.maxTextureSize = 2048;
        imp.alphaIsTransparency = false;
        
#pragma warning disable 0618  // suprimir obsoleto spritesheet
        imp.spritesheet = meta.ToArray();
#pragma warning restore 0618
        
        imp.SaveAndReimport();
        AssetDatabase.Refresh();

        // === PASO 3: Cargar sprites en orden ===
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(RUTA_DESTINO);
        var spriteMap = new Dictionary<string, Sprite>();
        foreach (Object a in assets)
        {
            Sprite s = a as Sprite;
            if (s != null) spriteMap[s.name] = s;
        }

        // === PASO 4: Crear AnimationClips ===
        CrearClip("Iyari_Idle",      ObtenerSprites(spriteMap, "Idle_",       4),  8, true);
        CrearClip("Iyari_Run",       ObtenerSprites(spriteMap, "Run_",        6), 14, true);
        CrearClip("Iyari_Jump",      ObtenerSprites(spriteMap, "Salto_",      4), 12, false);
        CrearClip("Iyari_DoubleJump",ObtenerSprites(spriteMap, "DobleSalto_", 4), 16, false);
        CrearClip("Iyari_Shoot",     ObtenerSprites(spriteMap, "Disparo_",    4), 18, false);
        CrearClip("Iyari_ShootAir",  ObtenerSprites(spriteMap, "DisparoAire_",4), 18, false);
        CrearClip("Iyari_Hurt",      ObtenerSprites(spriteMap, "Danio_",      3), 14, false);
        CrearClip("Iyari_Death",     ObtenerSprites(spriteMap, "Muerte_",     5),  8, false);

        // === PASO 5: Actualizar Animator Controller ===
        ActualizarAnimator();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("<color=#00FF88><b>[ILLARI ANIM]</b> ¡8 animaciones importadas y conectadas al Animator!</color>");
    }

    private static SpriteMetaData MakeSpr(string nombre, float x, float iy, float w, float h, int W, int H)
    {
        float uy = H - iy - h; // flip Y
        return new SpriteMetaData
        {
            name      = nombre,
            rect      = new Rect(Mathf.Max(0, x), Mathf.Max(0, uy), w, h),
            alignment = (int)SpriteAlignment.Custom,
            pivot     = new Vector2(0.5f, 0.08f),
            border    = Vector4.zero
        };
    }

    private static Sprite[] ObtenerSprites(Dictionary<string, Sprite> mapa, string prefijo, int cantidad)
    {
        var lista = new List<Sprite>();
        for (int i = 0; i < cantidad; i++)
        {
            string key = prefijo + i.ToString("00");
            if (mapa.ContainsKey(key)) lista.Add(mapa[key]);
        }
        return lista.ToArray();
    }

    private static void CrearClip(string nombre, Sprite[] sprites, float fps, bool loop)
    {
        if (sprites == null || sprites.Length == 0) return;

        string ruta = RUTA_CLIPS + nombre + ".anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ruta);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, ruta);
        }

        clip.frameRate = fps;

        // Configurar loop
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        // Crear curva de sprite (en hijo "Visual")
        EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("Visual", typeof(SpriteRenderer), "m_Sprite");

        ObjectReferenceKeyframe[] frames = new ObjectReferenceKeyframe[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            frames[i] = new ObjectReferenceKeyframe
            {
                time  = i / fps,
                value = sprites[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, frames);
        EditorUtility.SetDirty(clip);
    }

    private static void ActualizarAnimator()
    {
        AnimatorController ac = AssetDatabase.LoadAssetAtPath<AnimatorController>(RUTA_ANIMATOR);
        if (ac == null)
        {
            Debug.LogWarning("[ILLARI ANIM] No se encontró IyariAnimator.controller en " + RUTA_ANIMATOR);
            return;
        }

        AnimatorStateMachine sm = ac.layers[0].stateMachine;

        // Cargar clips nuevos
        AnimationClip clipIdle       = AssetDatabase.LoadAssetAtPath<AnimationClip>(RUTA_CLIPS + "Iyari_Idle.anim");
        AnimationClip clipRun        = AssetDatabase.LoadAssetAtPath<AnimationClip>(RUTA_CLIPS + "Iyari_Run.anim");
        AnimationClip clipJump       = AssetDatabase.LoadAssetAtPath<AnimationClip>(RUTA_CLIPS + "Iyari_Jump.anim");
        AnimationClip clipDoubleJump = AssetDatabase.LoadAssetAtPath<AnimationClip>(RUTA_CLIPS + "Iyari_DoubleJump.anim");
        AnimationClip clipShoot      = AssetDatabase.LoadAssetAtPath<AnimationClip>(RUTA_CLIPS + "Iyari_Shoot.anim");
        AnimationClip clipShootAir   = AssetDatabase.LoadAssetAtPath<AnimationClip>(RUTA_CLIPS + "Iyari_ShootAir.anim");
        AnimationClip clipHurt       = AssetDatabase.LoadAssetAtPath<AnimationClip>(RUTA_CLIPS + "Iyari_Hurt.anim");
        AnimationClip clipDeath      = AssetDatabase.LoadAssetAtPath<AnimationClip>(RUTA_CLIPS + "Iyari_Death.anim");

        // Agregar parámetros nuevos si no existen
        AgregarParametro(ac, "dobleJump",    AnimatorControllerParameterType.Trigger);
        AgregarParametro(ac, "recibirDanio", AnimatorControllerParameterType.Trigger);
        AgregarParametro(ac, "morir",        AnimatorControllerParameterType.Trigger);

        // Actualizar/crear estados
        AnimatorState stIdle       = ObtenerOCrearEstado(sm, "Iyari_Idle",       clipIdle);
        AnimatorState stRun        = ObtenerOCrearEstado(sm, "Iyari_Run",        clipRun);
        AnimatorState stJump       = ObtenerOCrearEstado(sm, "Iyari_Jump",       clipJump);
        AnimatorState stDoubleJump = ObtenerOCrearEstado(sm, "Iyari_DoubleJump", clipDoubleJump);
        AnimatorState stShoot      = ObtenerOCrearEstado(sm, "Iyari_Shoot",      clipShoot);
        AnimatorState stShootAir   = ObtenerOCrearEstado(sm, "Iyari_ShootAir",   clipShootAir);
        AnimatorState stHurt       = ObtenerOCrearEstado(sm, "Iyari_Hurt",       clipHurt);
        AnimatorState stDeath      = ObtenerOCrearEstado(sm, "Iyari_Death",      clipDeath);

        // Posicionar estados en el grafo (ChildAnimatorState es struct, requiere índice)
        var childStates = sm.states;
        for (int ci = 0; ci < childStates.Length; ci++)
        {
            var child = childStates[ci];
            if      (child.state == stDoubleJump) { var c = child; c.position = new Vector3(520, 80, 0);  sm.states[ci] = c; }
            else if (child.state == stShootAir)   { var c = child; c.position = new Vector3(520, 140, 0); sm.states[ci] = c; }
            else if (child.state == stHurt)        { var c = child; c.position = new Vector3(250, 200, 0); sm.states[ci] = c; }
            else if (child.state == stDeath)       { var c = child; c.position = new Vector3(520, 200, 0); sm.states[ci] = c; }
        }

        // Transiciones desde cualquier estado: Hurt y Death tienen prioridad máxima
        AgregarTransicionAny(sm, stHurt,  "recibirDanio", ac);
        AgregarTransicionAny(sm, stDeath, "morir",        ac);

        // Jump → DoubleJump
        AgregarTransicion(stJump, stDoubleJump, "dobleJump", false);

        // Jump → ShootAir
        AgregarTransicionDisparoAire(stJump, stShootAir);

        // ShootAir → Jump
        AgregarSalida(stShootAir, stJump);

        // Hurt → Idle (al terminar)
        AgregarSalidaConExitTime(stHurt, stIdle, 0.9f);

        // Death no regresa (es estado final)

        EditorUtility.SetDirty(ac);
    }

    private static void AgregarParametro(AnimatorController ac, string nombre, AnimatorControllerParameterType tipo)
    {
        foreach (var p in ac.parameters)
            if (p.name == nombre) return;
        ac.AddParameter(nombre, tipo);
    }

    private static AnimatorState ObtenerOCrearEstado(AnimatorStateMachine sm, string nombre, AnimationClip clip)
    {
        foreach (var cs in sm.states)
            if (cs.state.name == nombre)
            {
                if (clip != null) cs.state.motion = clip;
                return cs.state;
            }

        AnimatorState st = sm.AddState(nombre);
        if (clip != null) st.motion = clip;
        return st;
    }

    private static void AgregarTransicion(AnimatorState origen, AnimatorState destino, string trigger, bool hasExit)
    {
        if (origen == null || destino == null) return;
        foreach (var t in origen.transitions)
            if (t.destinationState == destino) return;

        var tr = origen.AddTransition(destino);
        tr.hasExitTime = hasExit;
        tr.duration = 0f;
        tr.AddCondition(AnimatorConditionMode.If, 0, trigger);
    }

    private static void AgregarTransicionDisparoAire(AnimatorState origen, AnimatorState destino)
    {
        if (origen == null || destino == null) return;
        foreach (var t in origen.transitions)
            if (t.destinationState == destino) return;

        var tr = origen.AddTransition(destino);
        tr.hasExitTime = false;
        tr.duration = 0f;
        tr.AddCondition(AnimatorConditionMode.If, 0, "disparar");
        tr.AddCondition(AnimatorConditionMode.If, 0, "estaEnAire");
    }

    private static void AgregarSalida(AnimatorState origen, AnimatorState destino)
    {
        if (origen == null || destino == null) return;
        foreach (var t in origen.transitions)
            if (t.destinationState == destino) return;

        var tr = origen.AddTransition(destino);
        tr.hasExitTime = true;
        tr.exitTime = 1.0f;
        tr.duration = 0.05f;
    }

    private static void AgregarSalidaConExitTime(AnimatorState origen, AnimatorState destino, float exitTime)
    {
        if (origen == null || destino == null) return;
        foreach (var t in origen.transitions)
            if (t.destinationState == destino) return;

        var tr = origen.AddTransition(destino);
        tr.hasExitTime = true;
        tr.exitTime = exitTime;
        tr.duration = 0.05f;
    }

    private static void AgregarTransicionAny(AnimatorStateMachine sm, AnimatorState destino, string trigger, AnimatorController ac)
    {
        if (destino == null) return;
        foreach (var t in sm.anyStateTransitions)
            if (t.destinationState == destino) return;

        var tr = sm.AddAnyStateTransition(destino);
        tr.hasExitTime = false;
        tr.duration = 0.03f;
        tr.canTransitionToSelf = false;
        tr.AddCondition(AnimatorConditionMode.If, 0, trigger);
    }
}
