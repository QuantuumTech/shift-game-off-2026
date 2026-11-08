using UnityEngine;

/// <summary>
/// Generates the game's sprites procedurally at runtime (a white square and a
/// spike triangle). The project ships with zero art assets, so nothing can be
/// missing in the editor or in a build.
/// </summary>
public static class SpriteFactory
{
    private static Sprite _square;
    private static Sprite _triangle;

    /// <summary>1x1 unit white square.</summary>
    public static Sprite Square
    {
        get
        {
            if (_square == null)
            {
                const int s = 16;
                var tex = new Texture2D(s, s);
                var px = new Color32[s * s];
                for (int i = 0; i < px.Length; i++) px[i] = new Color32(255, 255, 255, 255);
                tex.SetPixels32(px);
                tex.filterMode = FilterMode.Point;
                tex.Apply();
                _square = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
            }
            return _square;
        }
    }

    /// <summary>1x1 unit upward-pointing triangle (spikes).</summary>
    public static Sprite Triangle
    {
        get
        {
            if (_triangle == null)
            {
                const int s = 32;
                var tex = new Texture2D(s, s);
                var px = new Color32[s * s];
                for (int y = 0; y < s; y++)
                {
                    // Texture y=0 is the bottom row: wide base at the bottom,
                    // narrowing to a point at the top.
                    float half = (s - y) * 0.5f;
                    for (int x = 0; x < s; x++)
                    {
                        bool inside = Mathf.Abs(x - (s - 1) * 0.5f) <= half;
                        px[y * s + x] = inside
                            ? new Color32(255, 255, 255, 255)
                            : new Color32(0, 0, 0, 0);
                    }
                }
                tex.SetPixels32(px);
                tex.filterMode = FilterMode.Point;
                tex.Apply();
                _triangle = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
            }
            return _triangle;
        }
    }
}
