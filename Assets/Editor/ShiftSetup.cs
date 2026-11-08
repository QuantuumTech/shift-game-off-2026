#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// One-click project setup (menu: SHIFT &gt; Setup Project):
/// creates Assets/Scenes/Main.unity with a Main Camera + a Bootstrap object,
/// assigns the 8 level TextAssets, registers the scene in Build Settings,
/// and applies the WebGL player settings used for the itch.io upload.
/// Run this once after opening the project. See SETUP.md.
/// </summary>
public static class ShiftSetup
{
    private const string ScenePath = "Assets/Scenes/Main.unity";

    [MenuItem("SHIFT/Setup Project (Scene + Bootstrap + WebGL)")]
    public static void SetupProject()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Palette.BackgroundA;
        camGo.AddComponent<AudioListener>();
        camGo.AddComponent<CameraFit>();
        camGo.transform.position = new Vector3(13.5f, 7.5f, -10f);

        var bootGo = new GameObject("Bootstrap");
        var boot = bootGo.AddComponent<Bootstrap>();

        var levels = new TextAsset[8];
        for (int i = 0; i < levels.Length; i++)
        {
            string path = "Assets/Levels/level" + (i + 1) + ".txt";
            levels[i] = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (levels[i] == null)
            {
                Debug.LogWarning("SHIFT setup: could not find " + path);
            }
        }
        boot.levelFiles = levels;

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

        PlayerSettings.companyName = "QuantuumTech";
        PlayerSettings.productName = "SHIFT";
        PlayerSettings.defaultScreenWidth = 960;
        PlayerSettings.defaultScreenHeight = 540;
        PlayerSettings.runInBackground = true;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

        AssetDatabase.SaveAssets();
        Debug.Log("SHIFT setup complete. Press Play to test the game.");
        EditorUtility.DisplayDialog("SHIFT",
            "Setup complete.\n\nPress Play to test.\nFor the itch.io build: File > Build Settings > Build (WebGL). See SETUP.md.",
            "OK");
    }
}
#endif
