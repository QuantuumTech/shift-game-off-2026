using UnityEngine;

/// <summary>
/// The goal door. Touch = level complete. Pulses gently so it reads as the
/// objective in both palettes.
/// </summary>
public class Goal : MonoBehaviour
{
    private float _t;

    private void Update()
    {
        _t += Time.deltaTime;
        float s = 0.9f + Mathf.Sin(_t * 3f) * 0.07f;
        transform.localScale = new Vector3(s, s, 1f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && GameManager.Instance != null)
        {
            GameManager.Instance.LevelComplete();
        }
    }
}
