using UnityEngine;

/// <summary>
/// The one character: run + jump with coyote time and a jump buffer.
/// Built entirely in code (sprite, capsule, rigidbody) so the project needs
/// no prefabs or art assets. Rigidbody2D velocity-based movement.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 7.5f;
    public float jumpVelocity = 15f;
    public float coyoteTime = 0.10f;
    public float jumpBufferTime = 0.12f;

    public bool ControlsEnabled = true;

    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private float _coyote;
    private float _buffer;

    /// <summary>Create a fully wired player at the spawn position.</summary>
    public static PlayerController Create(Vector3 spawn)
    {
        var go = new GameObject("Player");
        go.tag = "Player";

        var rend = go.AddComponent<SpriteRenderer>();
        rend.sprite = SpriteFactory.Square;
        rend.color = Palette.Player;
        rend.sortingOrder = 5;

        var col = go.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(0.8f, 0.95f);
        col.sharedMaterial = new PhysicsMaterial2D("PlayerPhysics") { friction = 0f, bounciness = 0f };

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 2.8f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        var pc = go.AddComponent<PlayerController>();
        go.transform.position = spawn;
        return pc;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();
    }

    private void Update()
    {
        if (ControlsEnabled &&
            (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
            _buffer = jumpBufferTime;
        }
        else
        {
            _buffer -= Time.deltaTime;
        }

        // Safety net: never stay lost below the level.
        if (transform.position.y < -10f && GameManager.Instance != null)
        {
            GameManager.Instance.RespawnPlayer();
        }
    }

    private void FixedUpdate()
    {
        float move = ControlsEnabled ? Input.GetAxisRaw("Horizontal") : 0f;

        bool grounded = CheckGrounded();
        if (grounded) _coyote = coyoteTime;
        else _coyote -= Time.fixedDeltaTime;

        _rb.velocity = new Vector2(move * moveSpeed, _rb.velocity.y);

        if (_buffer > 0f && (grounded || _coyote > 0f))
        {
            _rb.velocity = new Vector2(_rb.velocity.x, jumpVelocity);
            _buffer = 0f;
            _coyote = 0f;
        }
    }

    private bool CheckGrounded()
    {
        Bounds b = _col.bounds;
        var origin = new Vector2(b.center.x, b.min.y - 0.03f);
        Collider2D[] hits = Physics2D.OverlapBoxAll(origin, new Vector2(0.7f, 0.08f), 0f);
        foreach (Collider2D h in hits)
        {
            if (h != null && !h.isTrigger && h.gameObject != gameObject) return true;
        }
        return false;
    }

    public void Respawn(Vector3 pos)
    {
        transform.position = pos;
        if (_rb != null) _rb.velocity = Vector2.zero;
        _buffer = 0f;
        _coyote = 0f;
    }
}
