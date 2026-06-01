using UnityEngine;

namespace Player
{
    public class SlingshotPlacer : MonoBehaviour
    {
        [SerializeField] private GameObject slingshotPrefab;
        [SerializeField] private int slingshotsRemaining = 5;
        [SerializeField] private float spawnVerticalOffset = 1.5f;

        public int SlingshotsRemaining => slingshotsRemaining;
        public bool IsNearSlingshot => _nearbySlingshot != null;

        private SlingShot _nearbySlingshot;
        private PlayerController _playerController;

        private float _timeUngrounded = 0f;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
        }

        private void Update()
        {
            if (_playerController != null)
            {
                if (_playerController.IsGrounded)
                {
                    _timeUngrounded = 0f;
                }
                else
                {
                    _timeUngrounded += Time.deltaTime;
                }
            }

            if (GetReturnHomeKeyPressed())
            {
                if (_playerController != null)
                {
                    // Only trigger Die() if they are actually floating in space (ungrounded for > 0.5s)
                    if (!_playerController.IsGrounded && _timeUngrounded >= 0.5f)
                    {
                        Debug.Log("Player manually chose to die/reset in space with R after floating.");
                        SessionManager.Instance.Die();
                    }
                    // If grounded or in a micro-hop, treat it as a safe hitchhike back home if out of ammo
                    else if ((_playerController.IsGrounded || _timeUngrounded < 0.5f) && slingshotsRemaining <= 0)
                    {
                        ReturnHome();
                    }
                }
            }
        }

        private bool GetReturnHomeKeyPressed()
        {
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                return UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame;
            }
            return Input.GetKeyDown(KeyCode.R);
        }

        public void RefillSlingshots(int amount)
        {
            slingshotsRemaining = amount;
            Debug.Log($"Slingshots refilled to: {slingshotsRemaining}");
        }

        private void ReturnHome()
        {
            HomePlanet home = FindFirstObjectByType<HomePlanet>();
            if (home != null)
            {
                Debug.Log("Out of slingshots! Teleporting back to Home Planet.");
                // Teleport player slightly above the home planet
                transform.position = home.transform.position + transform.up * spawnVerticalOffset;
                // Reset linear/angular velocities to prevent sliding on spawn
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }
                SessionManager.Instance.CommitSessionProgress();
                RefillSlingshots(5);
            }
        }

        public void Interact()
        {
            // If standing near an existing slingshot, load into it
            if (_nearbySlingshot != null)
            {
                _nearbySlingshot.LoadPlayer(gameObject);
                _nearbySlingshot = null; // Clear reference since we are now loaded inside
                return;
            }

            // Otherwise, place a new one at our feet if we are grounded on a planet
            if (_playerController != null && _playerController.IsGrounded)
            {
                PlaceSlingshot();
            }
            else
            {
                Debug.Log("Must be grounded on a planet to place a slingshot!");
            }
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

            // Register with the planet and session manager
            SlingShot ss = newSlingshot.GetComponent<SlingShot>();
            if (ss != null)
            {
                SessionManager.Instance.RegisterSessionSlingshot(ss);
                if (planetInfo != null)
                {
                    planetInfo.RegisterSlingshot(ss);
                }
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
