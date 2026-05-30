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

        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            _rigidbody.linearVelocity = new Vector2(_moveVector.x * movementSpeed, _rigidbody.linearVelocity.y);
            
            if (_jumpRequested)
            {
                _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, jumpPower);
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
    }
}
