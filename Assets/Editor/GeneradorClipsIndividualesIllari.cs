using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.IO;

public static class GeneradorClipsIndividualesIllari
{
    private const string RUTA_SPRITES = "Assets/Sprites/Iyari/Individual";
    private const string RUTA_CLIPS = "Assets/Animations/Iyari/";
    private const string RUTA_ANIMATOR = "Assets/Animations/IyariAnimator.controller";

    [MenuItem("Iyari/Generar Clips Individuales (36 Sprites)")]
    public static void GenerarTodo()
    {
        if (!Directory.Exists(RUTA_CLIPS))
            Directory.CreateDirectory(RUTA_CLIPS);

        // 1. Configurar TextureImporter de cada sprite individual
        string[] archivos = Directory.GetFiles(RUTA_SPRITES, "*.png");
        foreach (string file in archivos)
        {
            string unityPath = file.Replace("\\", "/");
            TextureImporter imp = AssetImporter.GetAtPath(unityPath) as TextureImporter;
            if (imp != null)
            {
                imp.textureType = TextureImporterType.Sprite;
                imp.spriteImportMode = SpriteImportMode.Single;
                imp.filterMode = FilterMode.Bilinear;
                imp.mipmapEnabled = false;
                imp.alphaIsTransparency = true;
                imp.maxTextureSize = 512;

                imp.spritePixelsPerUnit = 125;
                TextureImporterSettings tis = new TextureImporterSettings();
                imp.ReadTextureSettings(tis);
                tis.spriteAlignment = (int)SpriteAlignment.Custom;
                tis.spritePivot = new Vector2(0.5f, 0.00f); // Pivote en los pies
                imp.SetTextureSettings(tis);

                imp.SaveAndReimport();
            }
        }
        AssetDatabase.Refresh();

        // 2. Crear los AnimationClips
        CrearClipDesdeArchivos("Iyari_Idle",       "Idle_",       4,  8f, true);
        CrearClipDesdeArchivos("Iyari_Run",        "Run_",        7, 14f, true);
        CrearClipDesdeArchivos("Iyari_Jump",       "Jump_",       5, 12f, false);
        CrearClipDesdeArchivos("Iyari_DoubleJump", "DoubleJump_", 4, 16f, false);
        CrearClipDesdeArchivos("Iyari_Shoot",      "Shoot_",      4, 18f, false);
        CrearClipDesdeArchivos("Iyari_ShootAir",   "ShootAir_",   4, 18f, false);
        CrearClipDesdeArchivos("Iyari_ShootUp",    "ShootUp_",    4, 18f, false);
        CrearClipDesdeArchivos("Iyari_ShootAirUp", "ShootAirUp_", 4, 18f, false);
        CrearClipDesdeArchivos("Iyari_Hurt",       "Hurt_",       3, 12f, false);
        CrearClipDesdeArchivos("Iyari_Death",      "Death_",      5,  8f, false);

        // 3. Conectar al AnimatorController
        ConfigurarAnimator();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("<color=#00FFAA><b>[ILLARI CLIPS]</b> ¡Sprites y clips de disparo vertical vinculados al Animator con éxito!</color>");
    }

    private static void CrearClipDesdeArchivos(string nombreClip, string prefijo, int cantidad, float fps, bool loop)
    {
        List<Sprite> sprites = new List<Sprite>();
        for (int i = 0; i < cantidad; i++)
        {
            string path = $"{RUTA_SPRITES}/{prefijo}{i:D2}.png";
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null)
            {
                sprites.Add(s);
            }
            else
            {
                Debug.LogWarning($"[ILLARI] No se encontró sprite: {path}");
            }
        }

        if (sprites.Count == 0) return;

        string rutaClip = $"{RUTA_CLIPS}{nombreClip}.anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(rutaClip);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, rutaClip);
        }

        clip.frameRate = fps;

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        // Curva PPtr hacia el objeto hijo "Visual"
        EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("Visual", typeof(SpriteRenderer), "m_Sprite");
        ObjectReferenceKeyframe[] frames = new ObjectReferenceKeyframe[sprites.Count];
        for (int i = 0; i < sprites.Count; i++)
        {
            frames[i] = new ObjectReferenceKeyframe
            {
                time = i / fps,
                value = sprites[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, frames);
        EditorUtility.SetDirty(clip);
    }

    private static void ConfigurarAnimator()
    {
        AnimatorController ac = AssetDatabase.LoadAssetAtPath<AnimatorController>(RUTA_ANIMATOR);
        if (ac == null) return;

        AnimatorStateMachine sm = ac.layers[0].stateMachine;

        // Asegurar parámetros
        AsegurarParametro(ac, "velocidadX",       AnimatorControllerParameterType.Float);
        AsegurarParametro(ac, "estaEnAire",       AnimatorControllerParameterType.Bool);
        AsegurarParametro(ac, "disparar",         AnimatorControllerParameterType.Trigger);
        AsegurarParametro(ac, "apuntandoArriba",  AnimatorControllerParameterType.Bool);
        AsegurarParametro(ac, "dobleJump",        AnimatorControllerParameterType.Trigger);
        AsegurarParametro(ac, "recibirDanio",     AnimatorControllerParameterType.Trigger);
        AsegurarParametro(ac, "morir",            AnimatorControllerParameterType.Trigger);

        // Clips
        AnimationClip cIdle       = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{RUTA_CLIPS}Iyari_Idle.anim");
        AnimationClip cRun        = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{RUTA_CLIPS}Iyari_Run.anim");
        AnimationClip cJump       = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{RUTA_CLIPS}Iyari_Jump.anim");
        AnimationClip cDoubleJump = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{RUTA_CLIPS}Iyari_DoubleJump.anim");
        AnimationClip cShoot      = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{RUTA_CLIPS}Iyari_Shoot.anim");
        AnimationClip cShootAir   = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{RUTA_CLIPS}Iyari_ShootAir.anim");
        AnimationClip cShootUp    = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{RUTA_CLIPS}Iyari_ShootUp.anim");
        AnimationClip cShootAirUp = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{RUTA_CLIPS}Iyari_ShootAirUp.anim");
        AnimationClip cHurt       = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{RUTA_CLIPS}Iyari_Hurt.anim");
        AnimationClip cDeath      = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{RUTA_CLIPS}Iyari_Death.anim");

        // Asignar a estados
        var stIdle       = AsignarClipAEstado(sm, "Iyari_Idle",       cIdle);
        var stRun        = AsignarClipAEstado(sm, "Iyari_Run",        cRun);
        var stJump       = AsignarClipAEstado(sm, "Iyari_Jump",       cJump);
        var stDoubleJump = AsignarClipAEstado(sm, "Iyari_DoubleJump", cDoubleJump);
        var stShoot      = AsignarClipAEstado(sm, "Iyari_Shoot",      cShoot);
        var stShootAir   = AsignarClipAEstado(sm, "Iyari_ShootAir",   cShootAir);
        var stShootUp    = AsignarClipAEstado(sm, "Iyari_ShootUp",    cShootUp);
        var stShootAirUp = AsignarClipAEstado(sm, "Iyari_ShootAirUp", cShootAirUp);
        var stHurt       = AsignarClipAEstado(sm, "Iyari_Hurt",       cHurt);
        var stDeath      = AsignarClipAEstado(sm, "Iyari_Death",      cDeath);

        // Transiciones de salida automáticas para disparo vertical
        if (stShootUp != null && stIdle != null)
        {
            bool transExiste = false;
            foreach (var t in stShootUp.transitions)
            {
                if (t.destinationState == stIdle) { transExiste = true; break; }
            }
            if (!transExiste)
            {
                var tr = stShootUp.AddTransition(stIdle);
                tr.hasExitTime = true;
                tr.exitTime = 0.85f;
                tr.duration = 0.05f;
            }
        }

        if (stShootAirUp != null && stJump != null)
        {
            bool transExiste = false;
            foreach (var t in stShootAirUp.transitions)
            {
                if (t.destinationState == stJump) { transExiste = true; break; }
            }
            if (!transExiste)
            {
                var tr = stShootAirUp.AddTransition(stJump);
                tr.hasExitTime = true;
                tr.exitTime = 0.85f;
                tr.duration = 0.05f;
            }
        }

        EditorUtility.SetDirty(ac);
    }

    private static void AsegurarParametro(AnimatorController ac, string nombre, AnimatorControllerParameterType tipo)
    {
        foreach (var p in ac.parameters)
            if (p.name == nombre) return;
        ac.AddParameter(nombre, tipo);
    }

    private static AnimatorState AsignarClipAEstado(AnimatorStateMachine sm, string nombreEstado, AnimationClip clip)
    {
        if (clip == null) return null;
        foreach (var cs in sm.states)
        {
            if (cs.state.name == nombreEstado)
            {
                cs.state.motion = clip;
                return cs.state;
            }
        }
        var st = sm.AddState(nombreEstado);
        st.motion = clip;
        return st;
    }
}
