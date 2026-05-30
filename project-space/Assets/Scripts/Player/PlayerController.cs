using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
    
        [SerializeField] private float movementSpeed = 5f, jumpPower = 5f;

        private Rigidbody2D _rigidbody;
        private Vector2 _moveVector;
        private bool _jumpRequested;

        
        private bool _isGrounded;
        
        public bool IsGrounded => _isGrounded;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            GameObject nearestPlanet = GetNearestPlanet();

            if (_isGrounded && nearestPlanet != null)
            {
                // Gravity direction (points towards planet center)
                Vector2 gravityDir = ((Vector2)nearestPlanet.transform.position - _rigidbody.position).normalized;
                
                // Align player rotation (up points away from planet center)
                float targetAngle = Mathf.Atan2(gravityDir.y, gravityDir.x) * Mathf.Rad2Deg + 90f;
                _rigidbody.MoveRotation(Mathf.LerpAngle(_rigidbody.rotation, targetAngle, Time.fixedDeltaTime * 15f));

                // Movement tangent (perpendicular to gravity)
                Vector2 tangent = new Vector2(-gravityDir.y, gravityDir.x);
                
                // Project current velocity along the local up axis (radial velocity)
                Vector2 localUp = -gravityDir;
                float verticalSpeed = Vector2.Dot(_rigidbody.linearVelocity, localUp);
                
                // Grounding Snap: Apply a strong downward pull to glue the player to the curved shell
                if (!_jumpRequested)
                {
                    _rigidbody.AddForce(gravityDir * 15f, ForceMode2D.Force);
                    // Dampen any radial bouncy speed to keep them snapped
                    if (verticalSpeed > 0f) verticalSpeed *= 0.5f;
                }

                // Horizontal speed along tangent
                Vector2 horizontalVelocity = tangent * (_moveVector.x * movementSpeed);
                
                // Update velocity
                _rigidbody.linearVelocity = horizontalVelocity + (localUp * verticalSpeed);

                if (_jumpRequested)
                {
                    // Add jump velocity along local up
                    _rigidbody.linearVelocity = horizontalVelocity + (localUp * jumpPower);
                    _jumpRequested = false;
                }
            }
            else
            {
                // If in space (not grounded), let physics completely control velocity
                _jumpRequested = false;
            }
        }
        
        public void Move(Vector2 moveVector)
        {
            _moveVector = moveVector;
        }
        
        public void Jump()
        {
            _jumpRequested = true;
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

        private void OnCollisionEnter2D(Collision2D collision)
        {
            _isGrounded = true;
            
            // Try to collect a postcard if landing on a custom planet
            PlanetInfo planetInfo = collision.gameObject.GetComponentInParent<PlanetInfo>();
            if (planetInfo != null)
            {
                planetInfo.CollectPostcard();
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            _isGrounded = true;
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            _isGrounded = false;
        }
    }
}
