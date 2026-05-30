using UnityEngine;

public class PlanetGravity : MonoBehaviour
{
    [Header("Gravity Settings")]
    [Tooltip("The strength of the gravitational acceleration.")]
    public float gravityStrength = 9.81f;

    [Tooltip("The range of the gravitational field.")]
    public float gravityRadius = 20f;

    [Tooltip("If true, gravity becomes weaker the further away you are from the planet center.")]
    public bool useFalloff = true;

    private void Awake()
    {
        // Automatically disable global gravity so only local gravity pulls objects
        Physics2D.gravity = Vector2.zero;
    }

    private void FixedUpdate()
    {
        // Find all colliders within the gravity radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, gravityRadius);
        
        foreach (Collider2D col in colliders)
        {
            Rigidbody2D rb = col.attachedRigidbody;
            
            // Apply force to non-kinematic rigidbodies (excluding the planet itself)
            if (rb != null && !rb.isKinematic && rb.gameObject != gameObject)
            {
                Vector2 direction = ((Vector2)transform.position - rb.position).normalized;
                float distance = Vector2.Distance(transform.position, rb.position);

                float force = gravityStrength;
                if (useFalloff)
                {
                    // Linear decay: full strength at center, 0 at gravityRadius edge
                    float factor = Mathf.Clamp01(1f - (distance / gravityRadius));
                    force *= factor;
                }

                // Apply central pulling force
                rb.AddForce(direction * force * rb.mass);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Render the gravity field boundary in the Unity editor editor when selected
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, gravityRadius);
    }
}
