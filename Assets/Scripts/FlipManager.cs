using UnityEngine;

/// <summary>
/// Owns the world state and the SHIFT flip:
///  - Left or Right Shift toggles State A &lt;-&gt; State B.
///  - The player's position and momentum are untouched by a flip.
///  - A-blocks/hazards materialize, B ones ghost out, and vice versa.
///  - The camera background lerps to the new palette and a brief flash
///    sells the swap.
///  - A flip is DENIED (dull red flash) if a block would materialize inside
///    the player, so the player can never be crushed or soft-locked.
///  - A short cooldown stops per-frame spam.
/// </summary>
public class FlipManager : MonoBehaviour
{
    public WorldState State { get; private set; } = WorldState.A;
    public float flipCooldown = 0.22f;

    private float _lastFlip = -10f;
    private LevelBuilder _level;
    private PlayerController _player;
    private Camera _cam;
    private SpriteRenderer _flash;
    private float _flashT = 1f;
    private Color _bgTarget = Palette.BackgroundA;

    public void Init(Camera cam, PlayerController player)
    {
        _cam = cam;
        _player = player;
        if (_cam != null && _flash == null) BuildFlash();
        if (_cam != null) _cam.backgroundColor = _bgTarget;
    }

    public void SetLevel(LevelBuilder level)
    {
        _level = level;
    }

    public void ResetState()
    {
        State = WorldState.A;
        _bgTarget = Palette.BackgroundA;
    }

    private void BuildFlash()
    {
        var go = new GameObject("FlipFlash");
        go.transform.SetParent(_cam.transform, false);
        go.transform.localPosition = new Vector3(0f, 0f, 1f);
        var r = go.AddComponent<SpriteRenderer>();
        r.sprite = SpriteFactory.Square;
        r.sortingOrder = 1000;
        r.color = new Color(1f, 1f, 1f, 0f);
        _flash = r;
    }

    private void Update()
    {
        // Palette-swap background, eased.
        if (_cam != null)
        {
            _cam.backgroundColor = Color.Lerp(_cam.backgroundColor, _bgTarget, Time.deltaTime * 10f);
        }

        // Keep the flash quad covering the view; fade it out.
        if (_flash != null && _cam != null)
        {
            float h = _cam.orthographicSize * 2f + 2f;
            float w = h * _cam.aspect + 2f;
            _flash.transform.localScale = new Vector3(w, h, 1f);
            if (_flashT < 1f)
            {
                _flashT = Mathf.Min(1f, _flashT + Time.deltaTime / 0.14f);
                Color c = _flash.color;
                c.a = (1f - _flashT) * 0.35f;
                _flash.color = c;
            }
        }

        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            TryFlip();
        }
    }

    public void TryFlip()
    {
        if (Time.time - _lastFlip < flipCooldown) return;
        if (_level == null || _player == null) return;

        WorldState target = State == WorldState.A ? WorldState.B : WorldState.A;

        Collider2D col = _player.GetComponent<Collider2D>();
        if (col != null)
        {
            Bounds b = col.bounds;
            var rect = new Rect(b.min.x + 0.06f, b.min.y + 0.06f,
                                b.size.x - 0.12f, b.size.y - 0.12f);
            if (_level.WouldActivatingBlockOverlap(rect, target))
            {
                _lastFlip = Time.time;
                Flash(new Color(1f, 0.25f, 0.2f)); // denied
                return;
            }
        }

        _lastFlip = Time.time;
        State = target;
        _level.ApplyState(State);
        _bgTarget = State == WorldState.A ? Palette.BackgroundA : Palette.BackgroundB;
        Flash(Color.white);

        if (GameManager.Instance != null) GameManager.Instance.OnFlipped(State);
    }

    private void Flash(Color c)
    {
        if (_flash == null) return;
        c.a = 0.35f;
        _flash.color = c;
        _flashT = 0f;
    }
}
