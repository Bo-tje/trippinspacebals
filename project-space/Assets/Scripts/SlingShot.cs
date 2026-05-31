using UnityEngine;
using UnityEngine.InputSystem;
using Player;

public class SlingShot : MonoBehaviour
{
    [Header("Components")]
    public TrajectoryLine trajectoryLine;
   [SerializeField] private LineRenderer bandRenderer;
    private Collider2D triggerCollider;

    [Header("Slingshot Settings")]
    public float maxLength = 10f;
    public float force = 10f;
    public float bandWidthOffset = 0.5f; // Left/right anchor distance from center
    public float playerPositionOffset = 0.2f;

    [Header("State")]
    public Rigidbody2D playerRb;
    public Collider2D playerCollider;
    public Vector3 currentPosition;

    private Vector3 CenterPosition => transform.position;

    private void Awake()
    {
        bandRenderer = GetComponent<LineRenderer>();
        triggerCollider = GetComponent<Collider2D>();
        
        if (trajectoryLine == null)
        {
            trajectoryLine = GetComponentInChildren<TrajectoryLine>();
        }

        // Configure the band LineRenderer
        if (bandRenderer != null)
        {
            bandRenderer.positionCount = 3;
        }

        // Ensure collider is marked as trigger
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    void Start()
    {
        ResetStrips();
    }

    public void LoadPlayer(GameObject player)
    {
        playerRb = player.GetComponent<Rigidbody2D>();
        playerCollider = player.GetComponent<Collider2D>();
        if (playerCollider != null) playerCollider.enabled = false;
        
        playerRb.isKinematic = true;
        playerRb.linearVelocity = Vector2.zero;
        playerRb.angularVelocity = 0f;
        
        // Snap player to center position and reset rotation
        playerRb.transform.position = CenterPosition;
        playerRb.transform.rotation = Quaternion.identity;
        
        // Disable player controllers/inputs
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;
        
        PlayerInputHandler input = player.GetComponent<PlayerInputHandler>();
        if (input != null) input.enabled = false;

        SlingshotPlacer placer = player.GetComponent<SlingshotPlacer>();
        if (placer != null) placer.enabled = false;

        ResetStrips();
    }

    void Update()
    {
        if (playerRb == null) return;

        bool mouseDown = GetMouseButton();
        bool mouseDownThisFrame = GetMouseButtonDown();

        if (mouseDownThisFrame)
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);
            if (hit != null && (hit == triggerCollider || hit.transform.IsChildOf(transform)))
            {
                _isDragging = true;
            }
        }

        if (_isDragging)
        {
            if (mouseDown)
            {
                Vector3 mouseWorldPos = GetMouseWorldPosition();
                currentPosition = CenterPosition + Vector3.ClampMagnitude(mouseWorldPos - CenterPosition, maxLength);
                
                Vector3 launchVelocity = (CenterPosition - currentPosition) * force;
                float gravityScale = playerRb ? playerRb.gravityScale : 1f;
                trajectoryLine.ShowTrajectory(playerRb ? playerRb.transform.position : currentPosition, launchVelocity, gravityScale);
                
                SetStrips(currentPosition);
            }
            else
            {
                _isDragging = false;
                trajectoryLine.EndLine();
                Shoot();
            }
        }
    }

    private bool _isDragging;

    public bool IsDragging => _isDragging;
    public Vector3 Center => CenterPosition;
    public Vector3 CurrentPullPosition => currentPosition;

    /// <summary>
    /// Returns a value between 0 (not stretched) and 1 (fully stretched to maxLength)
    /// </summary>
    public float StretchRatio
    {
        get
        {
            if (!_isDragging || maxLength <= 0f) return 0f;
            float dist = Vector3.Distance(CenterPosition, currentPosition);
            return Mathf.Clamp01(dist / maxLength);
        }
    }

    private bool GetMouseButton()
    {
        if (Mouse.current != null)
        {
            return Mouse.current.leftButton.isPressed;
        }
        return Input.GetMouseButton(0);
    }

    private bool GetMouseButtonDown()
    {
        if (Mouse.current != null)
        {
            return Mouse.current.leftButton.wasPressedThisFrame;
        }
        return Input.GetMouseButtonDown(0);
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos;
        if (Mouse.current != null)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            mousePos = new Vector3(screenPos.x, screenPos.y, 10f);
        }
        else
        {
            mousePos = Input.mousePosition;
            mousePos.z = 10f;
        }
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    void Shoot()
    {
        if (playerRb == null) return;
        
        Vector3 pullVector = CenterPosition - currentPosition;
        if (pullVector.magnitude > 0.2f)
        {
            playerRb.isKinematic = false;
            Vector3 playerForce = pullVector * force;
            playerRb.linearVelocity = playerForce;

            // Delayed re-enabling of player's collider to prevent launching collision blocks
            if (playerCollider != null)
            {
                StartCoroutine(EnablePlayerColliderAfterDelay(playerCollider, 0.25f));
            }

            // Re-enable player controllers/inputs
            PlayerController controller = playerRb.GetComponent<PlayerController>();
            if (controller != null) controller.enabled = true;
            
            PlayerInputHandler input = playerRb.GetComponent<PlayerInputHandler>();
            if (input != null) input.enabled = true;

            SlingshotPlacer placer = playerRb.GetComponent<SlingshotPlacer>();
            if (placer != null) placer.enabled = true;

            playerRb = null;
            playerCollider = null;
            ResetStrips();
        }
        else
        {
            ResetPlayerToIdle();
        }
    }

    private System.Collections.IEnumerator EnablePlayerColliderAfterDelay(Collider2D col, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (col != null)
        {
            col.enabled = true;
        }
    }

    void ResetPlayerToIdle()
    {
        if (playerRb)
        {
            playerRb.transform.position = CenterPosition;
            playerRb.transform.rotation = Quaternion.identity;
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }
        ResetStrips();
    }

    void ResetStrips()
    {
        currentPosition = CenterPosition;
        SetStrips(currentPosition);
    }

    void SetStrips(Vector3 centerPullPosition)
    {
        if (bandRenderer == null) return;

        // Dynamic local positions for left/right anchors relative to slingshot rotation
        Vector3 leftAnchor = CenterPosition + transform.right * -bandWidthOffset;
        Vector3 rightAnchor = CenterPosition + transform.right * bandWidthOffset;

        bandRenderer.SetPosition(0, leftAnchor);
        bandRenderer.SetPosition(1, centerPullPosition);
        bandRenderer.SetPosition(2, rightAnchor);

        if (playerRb)
        {
            Vector3 direction = centerPullPosition - CenterPosition;
            if (direction.sqrMagnitude > 0.001f)
            {
                playerRb.transform.position = centerPullPosition + direction.normalized * playerPositionOffset;
                playerRb.transform.right = -direction.normalized;
            }
            else
            {
                playerRb.transform.position = CenterPosition;
                playerRb.transform.right = transform.right; // Facing forward default
            }
        }
    }
}
