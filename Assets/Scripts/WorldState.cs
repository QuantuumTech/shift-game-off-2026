using UnityEngine;

/// <summary>
/// The two world states. Pressing SHIFT flips between them.
/// </summary>
public enum WorldState
{
    A,
    B
}

/// <summary>
/// Flat two-palette art direction: one palette per world state so a flip
/// reads instantly. Sprites are plain white shapes tinted with these colors.
/// </summary>
public static class Palette
{
    public static readonly Color BackgroundA = new Color(0.05f, 0.07f, 0.16f); // deep indigo
    public static readonly Color BackgroundB = new Color(0.16f, 0.05f, 0.12f); // dark plum

    public static readonly Color SolidA = new Color(0.35f, 0.42f, 0.62f); // always-solid, state A tint
    public static readonly Color SolidB = new Color(0.55f, 0.38f, 0.45f); // always-solid, state B tint

    public static readonly Color OnlyA = new Color(0.25f, 0.85f, 1.00f); // cyan: exists in A
    public static readonly Color OnlyB = new Color(1.00f, 0.55f, 0.20f); // orange: exists in B

    public static readonly Color SpikeA = new Color(1.00f, 0.30f, 0.45f); // hot pink-red
    public static readonly Color SpikeB = new Color(0.85f, 0.40f, 1.00f); // violet

    public static readonly Color Player = new Color(1.00f, 0.95f, 0.55f); // warm yellow
    public static readonly Color Goal = new Color(0.45f, 1.00f, 0.55f);   // bright green

    /// <summary>Alpha for objects that belong to the inactive state (shown as ghosts).</summary>
    public const float GhostAlpha = 0.15f;
}
