using UnityEngine;
using Player;

public class HomePlanet : MonoBehaviour
{
    [Header("Refill Settings")]
    [Tooltip("The capacity to refill the player's slingshot count to.")]
    public int refillAmount = 5;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Try to find the SlingshotPlacer on the colliding object or parent
        SlingshotPlacer placer = collision.gameObject.GetComponentInParent<SlingshotPlacer>();

        if (placer != null)
        {
            placer.RefillSlingshots(refillAmount);
            SessionManager.Instance.CommitSessionProgress();
        }
    }
}
