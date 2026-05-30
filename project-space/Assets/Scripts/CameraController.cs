using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    public float followSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10);

    [Header("Zoom Settings")]
    public float baseZoom = 5f;
    public float speedZoomMultiplier = 0.15f;
    public float pullZoomMultiplier = 0.5f;
    public float zoomSpeed = 2f;
    private Camera _camera;

    [Header("Rotation Settings")]
    public float rotationSpeed = 3f;
    public float maxPlanetDetectionDistance = 40f;

    [Header("Slingshot Pan Settings")]
    public float panMultiplier = 0.4f;

    // Screen shake variables
    private float _shakeIntensity = 0f;
    private float _shakeDuration = 0f;
    private Vector3 _shakeOffset = Vector3.zero;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_camera == null)
        {
            _camera = GetComponentInChildren<Camera>();
        }
    }

    private void Start()
    {
        UpdateTarget();
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }

    private void LateUpdate()
    {
        UpdateTarget();
        if (target == null) return;

        // 1. Follow position
        Vector3 targetPosition = target.position + offset;
        
        // Exclude Z from calculations
        targetPosition.z = transform.position.z;

        // 2. Slingshot Pan
        SlingShot activeSling = FindFirstObjectByType<SlingShot>();
        float pullOffsetZoom = 0f;
        if (activeSling != null && activeSling.IsDragging)
        {
            Vector3 pullDirection = activeSling.Center - activeSling.CurrentPullPosition;
            targetPosition += pullDirection * panMultiplier;
            pullOffsetZoom = pullDirection.magnitude * pullZoomMultiplier;
        }

        // 3. Dynamic Zoom
        float targetZoom = baseZoom + pullOffsetZoom;
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb != null && !targetRb.isKinematic)
        {
            // Zoom out at high speeds
            targetZoom += targetRb.linearVelocity.magnitude * speedZoomMultiplier;
        }
        
        if (_camera != null)
        {
            if (_camera.orthographic)
            {
                _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, targetZoom, Time.deltaTime * zoomSpeed);
            }
        }

        // 4. Proximity-Based Rotation
        RotateTowardsNearestPlanet();

        // 5. Apply Position with Smooth Follow
        Vector3 newPos = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
        
        // 6. Apply Screen Shake
        HandleScreenShake();
        newPos += _shakeOffset;
        
        transform.position = newPos;
    }

    private void UpdateTarget()
    {
        if (target == null)
        {
            Player.PlayerController pc = FindFirstObjectByType<Player.PlayerController>();
            if (pc != null)
            {
                target = pc.transform;
            }
        }
    }

    private void RotateTowardsNearestPlanet()
    {
        Player.PlayerController pc = target.GetComponent<Player.PlayerController>();
        bool isGrounded = pc != null && pc.IsGrounded;

        float targetAngle = 0f;

        if (isGrounded)
        {
            GameObject[] planets = GameObject.FindGameObjectsWithTag("Planet");
            GameObject nearestPlanet = null;
            float minDistance = float.MaxValue;

            foreach (GameObject planet in planets)
            {
                float dist = Vector3.Distance(target.position, planet.transform.position);
                if (dist < minDistance && dist < maxPlanetDetectionDistance)
                {
                    minDistance = dist;
                    nearestPlanet = planet;
                }
            }

            if (nearestPlanet != null)
            {
                // Point the bottom of the camera towards the planet's center (gravity direction)
                Vector3 gravityDirection = (nearestPlanet.transform.position - target.position).normalized;
                // Align local "down" vector (-up) with gravity direction
                targetAngle = Mathf.Atan2(gravityDirection.y, gravityDirection.x) * Mathf.Rad2Deg + 90f;
            }
        }

        // Smoothly rotate the camera (to either the planet alignment or upright 0)
        float angle = Mathf.LerpAngle(transform.eulerAngles.z, targetAngle, Time.deltaTime * rotationSpeed);
        transform.eulerAngles = new Vector3(0, 0, angle);
    }

    private float _totalShakeDuration = 0f;

    public void TriggerShake(float intensity, float duration)
    {
        _shakeIntensity = intensity;
        _shakeDuration = duration;
        _totalShakeDuration = duration;
    }

    private void HandleScreenShake()
    {
        if (_shakeDuration > 0 && _totalShakeDuration > 0)
        {
            // Smoothly decay shake intensity over time
            float currentIntensity = _shakeIntensity * (_shakeDuration / _totalShakeDuration);
            _shakeOffset = Random.insideUnitCircle * currentIntensity;
            _shakeDuration -= Time.deltaTime;
        }
        else
        {
            _shakeOffset = Vector3.zero;
        }
    }
}
