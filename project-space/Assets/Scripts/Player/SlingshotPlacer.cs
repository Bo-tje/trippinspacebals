using UnityEngine;

namespace Player
{
    public class SlingshotPlacer : MonoBehaviour
    {
        [SerializeField] private GameObject slingshotPrefab;
        [SerializeField] private int slingshotsRemaining = 5;
        [SerializeField] private float spawnVerticalOffset = 1.5f;

        public int SlingshotsRemaining => slingshotsRemaining;

        private SlingShot _nearbySlingshot;

        public void Interact()
        {
            // If standing near an existing slingshot, load into it
            if (_nearbySlingshot != null)
            {
                _nearbySlingshot.LoadPlayer(gameObject);
                _nearbySlingshot = null; // Clear reference since we are now loaded inside
                return;
            }

            // Otherwise, place a new one at our feet
            PlaceSlingshot();
        }

        private void PlaceSlingshot()
        {
            if (slingshotsRemaining <= 0)
            {
                Debug.Log("No slingshots remaining!");
                return;
            }

            if (slingshotPrefab == null)
            {
                Debug.LogError("Slingshot prefab is not assigned in the SlingshotPlacer component!");
                return;
            }

            // Place slingshot at player's current position plus vertical offset
            Vector3 spawnPosition = transform.position + Vector3.up * spawnVerticalOffset;
            Instantiate(slingshotPrefab, spawnPosition, Quaternion.identity);
            slingshotsRemaining--;

            Debug.Log($"Placed a slingshot! Slingshots remaining: {slingshotsRemaining}");
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            SlingShot slingshot = other.GetComponent<SlingShot>();
            if (slingshot == null && other.attachedRigidbody != null)
            {
                slingshot = other.attachedRigidbody.GetComponent<SlingShot>();
            }

            if (slingshot != null)
            {
                _nearbySlingshot = slingshot;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            SlingShot slingshot = other.GetComponent<SlingShot>();
            if (slingshot == null && other.attachedRigidbody != null)
            {
                slingshot = other.attachedRigidbody.GetComponent<SlingShot>();
            }

            if (slingshot != null && _nearbySlingshot == slingshot)
            {
                _nearbySlingshot = null;
            }
        }
    }
}
