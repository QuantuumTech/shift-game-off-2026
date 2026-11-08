using UnityEngine;

/// <summary>
/// The single entry point. Put ONE GameObject with this component in the
/// scene (the SHIFT &gt; Setup menu does it for you), press Play, and the
/// whole game builds itself at runtime: camera fit, UI, player, levels.
///
/// The 8 level TextAssets are assigned into <see cref="levelFiles"/> by the
/// editor setup menu (SHIFT &gt; Setup Project). If they are missing, the
/// title screen shows a reminder instead of crashing.
/// </summary>
public class Bootstrap : MonoBehaviour
{
    [Header("Level maps (Assets/Levels/level1.txt ... level8.txt)")]
    public TextAsset[] levelFiles;

    private void Awake()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
        }
        cam.orthographic = true;
        if (cam.GetComponent<CameraFit>() == null)
        {
            cam.gameObject.AddComponent<CameraFit>();
        }

        var gmGo = new GameObject("GameManager");
        var gm = gmGo.AddComponent<GameManager>();
        gm.Init(levelFiles, cam);
    }
}
