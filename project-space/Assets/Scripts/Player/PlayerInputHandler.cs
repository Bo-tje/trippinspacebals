using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerInputHandler : MonoBehaviour
    {
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference jumpAction;
        
        private PlayerController _playerController;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
        }

        private void OnEnable()
        {
            if (moveAction != null) moveAction.action.Enable();
            if (jumpAction != null)
            {
                jumpAction.action.Enable();
                jumpAction.action.performed += OnJumpPerformed;
            }
        }

        private void OnDisable()
        {
            if (moveAction != null) moveAction.action.Disable();
            if (jumpAction != null)
            {
                jumpAction.action.Disable();
                jumpAction.action.performed -= OnJumpPerformed;
            }
        }
    
        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            SlingshotPlacer placer = GetComponent<SlingshotPlacer>();
            if (placer != null && placer.enabled)
            {
                placer.Interact();
            }
            else if (_playerController != null)
            {
                _playerController.Jump();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (moveAction != null && _playerController != null)
            {
                Vector2 moveVector = moveAction.action.ReadValue<Vector2>();
                _playerController.Move(moveVector);
            }
        }
    }
}
