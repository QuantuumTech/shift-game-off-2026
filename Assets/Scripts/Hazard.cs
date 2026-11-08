using UnityEngine;

/// <summary>
/// Spikes. Deadly in exactly one world state — the LevelBuilder enables /
/// disables this trigger collider and ghosts the sprite per state.
/// Touch = respawn at the level start.
/// </summary>
public class Hazard : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryKill(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Also fires when a flip activates these spikes under the player.
        TryKill(other);
    }

    private void TryKill(Collider2D other)
    {
        if (other.CompareTag("Player") && GameManager.Instance != null)
        {
            GameManager.Instance.RespawnPlayer();
        }
    }
}
