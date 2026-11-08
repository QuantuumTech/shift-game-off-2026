using System.Collections.Generic;
using UnityEngine;

/// <summary>Parsed ASCII level map.</summary>
public class LevelData
{
    public int Width;
    public int Height;
    public char[,] Grid; // indexed [x, y], y = row from the TOP of the file
    public Vector2Int SpawnCell;
    public Vector2Int GoalCell;

    /// <summary>World position of a cell's center. Row 0 of the file is the top.</summary>
    public Vector3 CellToWorld(Vector2Int cell)
    {
        return new Vector3(cell.x, Height - 1 - cell.y, 0f);
    }
}

/// <summary>
/// Builds a level from an ASCII TextAsset at runtime.
///
/// Legend:
///   '#'  solid in both states
///   'A'  solid only in State A
///   'B'  solid only in State B
///   '^'  spikes, deadly only in State A
///   'v'  spikes, deadly only in State B
///   'P'  player start
///   'G'  goal
///   '.'  empty
///
/// Objects belonging to the inactive state stay visible as faint "ghosts"
/// (no collider) so the player can plan the next flip.
/// </summary>
public class LevelBuilder : MonoBehaviour
{
    private class BlockEntry
    {
        public char Kind; // '#', 'A' or 'B'
        public Vector3 WorldPos;
        public BoxCollider2D Collider;
        public SpriteRenderer Renderer;
    }

    private class HazardEntry
    {
        public char Kind; // '^' (State A) or 'v' (State B)
        public BoxCollider2D Collider;
        public SpriteRenderer Renderer;
    }

    private readonly List<BlockEntry> _blocks = new List<BlockEntry>();
    private readonly List<HazardEntry> _hazards = new List<HazardEntry>();

    public LevelData Data { get; private set; }
    public Vector3 SpawnWorld { get; private set; }

    public void Build(TextAsset asset)
    {
        Data = Parse(asset);
        SpawnWorld = Data.CellToWorld(Data.SpawnCell);

        for (int y = 0; y < Data.Height; y++)
        {
            for (int x = 0; x < Data.Width; x++)
            {
                char c = Data.Grid[x, y];
                var cell = new Vector2Int(x, y);
                Vector3 pos = Data.CellToWorld(cell);
                switch (c)
                {
                    case '#':
                    case 'A':
                    case 'B':
                        CreateBlock(pos, c);
                        break;
                    case '^':
                    case 'v':
                        CreateHazard(pos, c);
                        break;
                    case 'G':
                        CreateGoal(pos);
                        break;
                }
            }
        }
    }

    public static LevelData Parse(TextAsset asset)
    {
        string[] raw = asset.text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        var rows = new List<string>();
        foreach (string line in raw) rows.Add(line.TrimEnd());
        while (rows.Count > 0 && rows[rows.Count - 1].Length == 0) rows.RemoveAt(rows.Count - 1);

        var data = new LevelData();
        data.Height = rows.Count;
        data.Width = rows[0].Length;
        data.Grid = new char[data.Width, data.Height];

        for (int y = 0; y < data.Height; y++)
        {
            string row = rows[y];
            for (int x = 0; x < data.Width && x < row.Length; x++)
            {
                char c = row[x];
                data.Grid[x, y] = c;
                if (c == 'P') data.SpawnCell = new Vector2Int(x, y);
                else if (c == 'G') data.GoalCell = new Vector2Int(x, y);
            }
        }
        return data;
    }

    /// <summary>Apply a world state: toggle colliders and ghost/tint visuals.</summary>
    public void ApplyState(WorldState state)
    {
        foreach (BlockEntry b in _blocks)
        {
            if (b.Kind == '#')
            {
                b.Renderer.color = state == WorldState.A ? Palette.SolidA : Palette.SolidB;
            }
            else
            {
                bool active = (b.Kind == 'A') == (state == WorldState.A);
                b.Collider.enabled = active;
                Color c = b.Kind == 'A' ? Palette.OnlyA : Palette.OnlyB;
                c.a = active ? 1f : Palette.GhostAlpha;
                b.Renderer.color = c;
            }
        }

        foreach (HazardEntry h in _hazards)
        {
            bool active = (h.Kind == '^') == (state == WorldState.A);
            h.Collider.enabled = active;
            Color c = h.Kind == '^' ? Palette.SpikeA : Palette.SpikeB;
            c.a = active ? 1f : Palette.GhostAlpha;
            h.Renderer.color = c;
        }
    }

    /// <summary>
    /// True if flipping TO <paramref name="target"/> would materialize a solid
    /// block inside <paramref name="rect"/> (the player's bounds). The flip is
    /// denied in that case so the player can never be crushed or stuck.
    /// </summary>
    public bool WouldActivatingBlockOverlap(Rect rect, WorldState target)
    {
        char kind = target == WorldState.A ? 'A' : 'B';
        foreach (BlockEntry b in _blocks)
        {
            if (b.Kind != kind) continue;
            var blockRect = new Rect(b.WorldPos.x - 0.5f, b.WorldPos.y - 0.5f, 1f, 1f);
            if (blockRect.Overlaps(rect)) return true;
        }
        return false;
    }

    private void CreateBlock(Vector3 pos, char kind)
    {
        var go = new GameObject("Block_" + kind);
        go.transform.SetParent(transform, false);
        go.transform.position = pos;

        var rend = go.AddComponent<SpriteRenderer>();
        rend.sprite = SpriteFactory.Square;
        rend.sortingOrder = 0;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;

        _blocks.Add(new BlockEntry { Kind = kind, WorldPos = pos, Collider = col, Renderer = rend });
    }

    private void CreateHazard(Vector3 pos, char kind)
    {
        var go = new GameObject("Spikes_" + kind);
        go.transform.SetParent(transform, false);
        go.transform.position = pos;

        var rend = go.AddComponent<SpriteRenderer>();
        rend.sprite = SpriteFactory.Triangle;
        rend.sortingOrder = 1;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 0.45f);
        col.offset = new Vector2(0f, -0.25f);

        go.AddComponent<Hazard>();

        _hazards.Add(new HazardEntry { Kind = kind, Collider = col, Renderer = rend });
    }

    private void CreateGoal(Vector3 pos)
    {
        var go = new GameObject("Goal");
        go.transform.SetParent(transform, false);
        go.transform.position = pos;

        var rend = go.AddComponent<SpriteRenderer>();
        rend.sprite = SpriteFactory.Square;
        rend.color = Palette.Goal;
        rend.sortingOrder = 2;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.9f, 0.9f);

        go.AddComponent<Goal>();
    }
}
