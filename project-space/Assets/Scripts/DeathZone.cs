using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckAndTriggerDeath(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckAndTriggerDeath(other.gameObject);
    }

    private void CheckAndTriggerDeath(GameObject obj)
    {
        // If the player touches this hazard, kill them!
        Player.PlayerController pc = obj.GetComponent<Player.PlayerController>();
        if (pc == null && obj.attachedRigidbody != null)
        {
            pc = obj.attachedRigidbody.GetComponent<Player.PlayerController>();
        }

        if (pc != null)
        {
            SessionManager.Instance.Die();
        }
    }
}
