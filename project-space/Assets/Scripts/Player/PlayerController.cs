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
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (_isGrounded)
            {
                _rigidbody.linearVelocity = new Vector2(_moveVector.x * movementSpeed, _rigidbody.linearVelocity.y);
            }
            
            if (_jumpRequested && _isGrounded)
            {
                _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, jumpPower);
                _jumpRequested = false;
            }
            else if (_jumpRequested)
            {
                // Clear jump request if we pressed jump but weren't grounded
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
