using UnityEngine;

/// <summary>
/// Fixed one-screen camera: frames the whole level (levels are designed to
/// fit a single screen) with a small margin, whatever the window aspect.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraFit : MonoBehaviour
{
    private Camera _cam;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        _cam.orthographic = true;
    }

    public void Fit(int levelWidth, int levelHeight)
    {
        if (_cam == null) _cam = GetComponent<Camera>();
        const float margin = 0.6f;
        float sizeForHeight = levelHeight * 0.5f + margin;
        float sizeForWidth = (levelWidth * 0.5f + margin) / Mathf.Max(0.1f, _cam.aspect);
        _cam.orthographicSize = Mathf.Max(sizeForHeight, sizeForWidth);
        transform.position = new Vector3(levelWidth * 0.5f - 0.5f, levelHeight * 0.5f - 0.5f, -10f);
    }
}
