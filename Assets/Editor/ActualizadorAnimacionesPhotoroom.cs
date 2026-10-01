using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.IO;

public static class ActualizadorAnimacionesPhotoroom
{
    private const string RUTA_INDIVIDUAL = "Assets/Sprites/Iyari/Individual";
    private const string RUTA_CLIPS = "Assets/Animations/Iyari/";
    private const string RUTA_ANIMATOR = "Assets/Animations/IyariAnimator.controller";

    [MenuItem("Iyari/Aplicar Animaciones Photoroom (36 Frames HD)")]
    public static void AplicarTodo()
    {
        // 1. Configurar cada TextureImporter con PPU = 125 (altura 1.57m) y pivote en los pies
        string[] archivos = Directory.GetFiles(RUTA_INDIVIDUAL, "*.png");
        foreach (string file in archivos)
        {
            string unityPath = file.Replace("\\", "/");
            string fileName = Path.GetFileNameWithoutExtension(unityPath);

            TextureImporter imp = AssetImporter.GetAtPath(unityPath) as TextureImporter;
            if (imp == null) continue;

            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = 125; // 196px / 125 = 1.57m de altura perfecta
            imp.filterMode = FilterMode.Bilinear;
            imp.mipmapEnabled = false;
            imp.alphaIsTransparency = true;
            imp.maxTextureSize = 512;

            TextureImporterSettings tis = new TextureImporterSettings();
            imp.ReadTextureSettings(tis);
            tis.spriteMode = (int)SpriteImportMode.Single;
            tis.spriteAlignment = (int)SpriteAlignment.Custom;

            // Pivote horizontal y vertical (Y=0.0f = la suela de los zapatos exactamente en el suelo)
            float pivoteX = 0.50f;
            float pivoteY = 0.00f;

            if (fileName.StartsWith("Run_00")) pivoteX = 0.42f;
            else if (fileName.StartsWith("Run_01")) pivoteX = 0.76f;
            else if (fileName.StartsWith("Run_02")) pivoteX = 0.62f;
            else if (fileName.StartsWith("Run_03")) pivoteX = 0.40f;
            else if (fileName.StartsWith("Run_04")) pivoteX = 0.45f;
            else if (fileName.StartsWith("Run_05")) pivoteX = 0.63f;
            else if (fileName.StartsWith("Run_06")) pivoteX = 0.45f;
            else if (fileName.StartsWith("Shoot_01")) pivoteX = 0.30f; // Chorro de agua hacia la derecha
            else if (fileName.StartsWith("Shoot_02")) pivoteX = 0.70f; // Retroceso
            else if (fileName.StartsWith("Shoot_00")) pivoteX = 0.48f;
            else if (fileName.StartsWith("ShootAir_01")) pivoteX = 0.15f; // Chorro aéreo
            else if (fileName.StartsWith("ShootAir_00")) pivoteX = 0.20f;
            else if (fileName.StartsWith("ShootAir_02")) pivoteX = 0.35f;
            else if (fileName.StartsWith("ShootAir_03")) pivoteX = 0.22f;
            else if (fileName.StartsWith("Death_04")) pivoteX = 0.35f;

            tis.spritePivot = new Vector2(pivoteX, pivoteY);
            imp.SetTextureSettings(tis);

            imp.SaveAndReimport();
        }
        AssetDatabase.Refresh();

        // 2. Crear los 8 AnimationClips con las duraciones y loops correctos
        CrearClip("Iyari_Idle",       "Idle_",       4,  8f, true);
        CrearClip("Iyari_Run",        "Run_",        7, 14f, true);
        CrearClip("Iyari_Jump",       "Jump_",       5, 12f, false);
        CrearClip("Iyari_DoubleJump", "DoubleJump_", 4, 16f, false);
        CrearClip("Iyari_Shoot",      "Shoot_",      4, 16f, false);
        CrearClip("Iyari_ShootAir",   "ShootAir_",   4, 16f, false);
        CrearClip("Iyari_Hurt",       "Hurt_",       3, 12f, false);
        CrearClip("Iyari_Death",      "Death_",      5,  8f, false);

        // 3. Vincular al AnimatorController
        VincularAnimator();

        // 4. Actualizar jugador en la escena activa si existe
        ActualizarJugadorEnEscena();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("<color=#00FFAA><b>[ILLARI HD]</b> ¡36 fotogramas Photoroom importados, 8 clips creados y Animator actualizado exitosamente!</color>");
    }

    private static void CrearClip(string nombreClip, string prefijo, int cantidad, float fps, bool loop)
    {
        List<Sprite> sprites = new List<Sprite>();
        for (int i = 0; i < cantidad; i++)
        {
            string path = $"{RUTA_INDIVIDUAL}/{prefijo}{i:D2}.png";
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null) sprites.Add(s);
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

    private static void VincularAnimator()
    {
        AnimatorController ac = AssetDatabase.LoadAssetAtPath<AnimatorController>(RUTA_ANIMATOR);
        if (ac == null) return;

        var sm = ac.layers[0].stateMachine;

        Dictionary<string, string> mapeo = new Dictionary<string, string>
        {
            { "Iyari_Idle",       "Iyari_Idle.anim" },
            { "Iyari_Run",        "Iyari_Run.anim" },
            { "Iyari_Jump",       "Iyari_Jump.anim" },
            { "Iyari_DoubleJump", "Iyari_DoubleJump.anim" },
            { "Iyari_Shoot",      "Iyari_Shoot.anim" },
            { "Iyari_ShootAir",   "Iyari_ShootAir.anim" },
            { "Iyari_Hurt",       "Iyari_Hurt.anim" },
            { "Iyari_Death",      "Iyari_Death.anim" }
        };

        foreach (var cs in sm.states)
        {
            if (mapeo.ContainsKey(cs.state.name))
            {
                AnimationClip c = AssetDatabase.LoadAssetAtPath<AnimationClip>(RUTA_CLIPS + mapeo[cs.state.name]);
                if (c != null)
                {
                    cs.state.motion = c;
                }
            }
        }

        EditorUtility.SetDirty(ac);
    }

    private static void ActualizarJugadorEnEscena()
    {
        var player = GameObject.Find("Illari_Player");
        if (player == null) return;

        var visual = player.transform.Find("Visual");
        if (visual != null)
        {
            // Escala 0.88 para altura de 1.38m (proporción ágil y equilibrada)
            visual.localScale = new Vector3(0.88f, 0.88f, 1.0f);
            visual.localPosition = Vector3.zero;

            var sr = visual.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                var s = AssetDatabase.LoadAssetAtPath<Sprite>($"{RUTA_INDIVIDUAL}/Idle_00.png");
                if (s != null) sr.sprite = s;
            }
        }

        var col = player.GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.size = new Vector2(0.55f, 1.36f);
            col.offset = new Vector2(0f, 0.68f);
        }

        var platformer = player.GetComponent<IllariPlatformer>();
        if (platformer != null)
        {
            platformer.ConfigurarEscalaVisual(new Vector3(0.88f, 0.88f, 1.0f));
        }

        player.transform.position = new Vector3(player.transform.position.x, -4.50f, 0f);
        EditorUtility.SetDirty(player);
    }
}
