using UnityEngine;

public class SlingShot : MonoBehaviour
{
    
    public TrajectoryLine trajectoryLine;
    
    public LineRenderer[] lineRenderers;
    public Transform[] stripPositions;
    public Transform center;
    public Transform idlePosition;

    public Vector3 currentPosition;
    public float maxLength = 10f;
    public float bottomBoundary = 10f;
    public float force = 10f;
    
    public GameObject playerPrefab;
    public Rigidbody2D playerRb;
    public Collider2D playerCollider;

    public float playerPositionOffset;
    
    private bool _isMouseDown;
    private Vector3 _startPoint;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderers[0].positionCount = 2;
        lineRenderers[1].positionCount = 2;
        lineRenderers[0].SetPosition(0, stripPositions[0].position);
        lineRenderers[1].SetPosition(0, stripPositions[1].position);
        
        CreatePlayer();
    }

    // Update is called once per frame
    void Update()
    {
        if (_isMouseDown)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10;
            
            currentPosition = Camera.main.ScreenToWorldPoint(mousePos);
            currentPosition = center.position + Vector3.ClampMagnitude(currentPosition - center.position, maxLength);
            currentPosition = ClampBounds(currentPosition);
            trajectoryLine.RenderLine(_startPoint, currentPosition);
            
            SetStrips(currentPosition);

            
            if (playerCollider)
            {
                playerCollider.enabled = true;
            }
        }
        else
        {
            ResetStrips();
        }
    }

    void CreatePlayer()
    {
        playerRb = Instantiate(playerPrefab).GetComponent<Rigidbody2D>();
        playerCollider = playerRb.GetComponent<Collider2D>();
        playerCollider.enabled = false;
    }
    
    void OnMouseDown()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10;
        
        _isMouseDown = true;
        _startPoint = Camera.main.ScreenToWorldPoint(mousePos);
    }
    
    void OnMouseUp()
    {
        _isMouseDown = false;
        trajectoryLine.EndLine();
        Shoot();
    }

    void Shoot()
    {
        playerRb.isKinematic = false;
        Vector3 playerForce = (currentPosition - center.position) * force * -1;
        playerRb.linearVelocity = playerForce;

        playerRb = null;
        playerCollider = null;
    }



    void ResetStrips()
    {
        currentPosition = idlePosition.position;
        SetStrips(currentPosition);
    }

    void SetStrips(Vector3 positions)
    {
        lineRenderers[0].SetPosition(1, positions);
        lineRenderers[1].SetPosition(1, positions);

        if (playerRb)
        {
            Vector3 direction = positions - center.position;
            playerRb.transform.position = positions + direction.normalized * playerPositionOffset;
            playerRb.transform.right = -direction.normalized;
        }
    }
    
    Vector3 ClampBounds(Vector3 vector)
    {
        vector.y = Mathf.Clamp(vector.y, bottomBoundary, 1000);
        return vector;
    }
}
