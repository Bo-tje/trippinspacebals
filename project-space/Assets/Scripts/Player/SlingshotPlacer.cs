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

            // Check if we are near a planet and if it allows more placements
            GameObject nearestPlanet = GetNearestPlanet();
            PlanetInfo planetInfo = null;
            if (nearestPlanet != null)
            {
                planetInfo = nearestPlanet.GetComponent<PlanetInfo>();
                if (planetInfo != null && !planetInfo.CanPlaceSlingshot)
                {
                    Debug.Log($"Cannot place slingshot! This planet is limited to {planetInfo.maxSlingshotsAllowed} slingshots.");
                    return;
                }
            }

            // Place slingshot at player's current position plus local vertical offset, aligned to planet surface
            Vector3 spawnPosition = transform.position + transform.up * spawnVerticalOffset;
            GameObject newSlingshot = Instantiate(slingshotPrefab, spawnPosition, transform.rotation);
            slingshotsRemaining--;

            // Register with the planet
            if (planetInfo != null)
            {
                SlingShot ss = newSlingshot.GetComponent<SlingShot>();
                planetInfo.RegisterSlingshot(ss);
            }

            Debug.Log($"Placed a slingshot! Slingshots remaining: {slingshotsRemaining}");
        }

        private GameObject GetNearestPlanet()
        {
            GameObject[] planets = GameObject.FindGameObjectsWithTag("Planet");
            GameObject nearestPlanet = null;
            float minDistance = float.MaxValue;

            foreach (GameObject planet in planets)
            {
                float dist = Vector3.Distance(transform.position, planet.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearestPlanet = planet;
                }
            }
            return nearestPlanet;
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
