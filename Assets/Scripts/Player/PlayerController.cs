using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody _rb;
    private Animator _animator;
    
    [Header("References")]
    private Camera _mainCamera;
    [SerializeField] private GameObject inventoryUI;
    
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float groundLinearDamping = 10f;
    private float _currentMaxSpeed;
    private Vector3 _moveDirection;
    
    [Header("Jumping")]
    [SerializeField] private float jumpForce = 10f;
    
    [Header("Interaction")]
    [SerializeField] private float interactionRange = 10f;
    [SerializeField] private LayerMask interactionLayer;
    private GameObject _currentHoveredInteractable;
    
    [Header("Inputs")]
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _shootAction;
    private InputAction _jumpAction;
    private InputAction _sprintAction;
    private InputAction _interactAction;
    private InputAction _inventoryAction;
    
    private bool _onGround;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _mainCamera = Camera.main;
        
        _moveAction = InputSystem.actions.FindAction("Move");
        _lookAction = InputSystem.actions.FindAction("Look");
        _shootAction = InputSystem.actions.FindAction("Shoot");
        _sprintAction = InputSystem.actions.FindAction("Sprint");
        _jumpAction = InputSystem.actions.FindAction("Jump");
        _interactAction = InputSystem.actions.FindAction("Interact");
        _inventoryAction = InputSystem.actions.FindAction("Inventory");
    }
    private void Update()
    {
        UpdateAnimations();
    }
    private void FixedUpdate()
    {
        CheckForInputs();
        CheckIsGrounded();
        CheckForInteractable();
        
        Movement();
    }

    private void CheckForInputs()
    {
        _interactAction.performed += ctx => CallInteraction();
        _inventoryAction.performed += ctx => OpenOrCloseInventory();
        if (_jumpAction.IsPressed())
        {
            Jump();
        }
        if (_sprintAction.IsPressed() && _onGround)
        {
            _currentMaxSpeed = sprintSpeed;
        }
        else
        {
            _currentMaxSpeed = walkSpeed;
        }
    }

    private void Movement()
    {
        _moveDirection = transform.forward * GetMoveInput().y + transform.right * GetMoveInput().x;
        _rb.AddForce(_moveDirection.normalized * (_currentMaxSpeed * 10), ForceMode.Force);
        
        if(_onGround)
            _rb.linearDamping = groundLinearDamping;
        else
            _rb.linearDamping = 0;
        
        LimitVelocity();
    }
    private void LimitVelocity()
    {
        var flatVel = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
        
        if (flatVel.magnitude > _currentMaxSpeed)
        {
            var limitedVel = flatVel.normalized * _currentMaxSpeed;
            _rb.linearVelocity = new Vector3(limitedVel.x, _rb.linearVelocity.y, limitedVel.z);
        }
    }
    private void Jump()
    {
        if (!_onGround)
            return;
        
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
    
    private void UpdateAnimations()
    {
        _animator.SetBool("isMoving", _moveDirection != Vector3.zero);
        _animator.SetBool("isJumping", !_onGround);
        _animator.SetFloat("MoveX", GetMoveInput().x);
        _animator.SetFloat("MoveZ", GetMoveInput().y);
    }
    
    private void OpenOrCloseInventory()
    {
        inventoryUI.SetActive(!inventoryUI.activeSelf);
        Cursor.lockState = inventoryUI.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
        if (inventoryUI.activeSelf)
        {
            DisableAllActions();
        }
        else
        {
            EnableAllActions();
        }
    }

    private void CallInteraction()
    {
        var ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.TransformDirection(Vector3.forward));
        
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactionRange, interactionLayer))
        {
            IInteractable interactable = hit.transform.gameObject.GetComponent<IInteractable>();
            if(interactable != null)
                interactable.Interact();
        }
    }
    private void CheckForInteractable()
    {
        var ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.TransformDirection(Vector3.forward));
        
        if (!Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionLayer))
        {
            CallHoverExit();
            return;
        }
        
        if (_currentHoveredInteractable != hit.transform.gameObject && hit.transform.TryGetComponent(out IInteractable interactable))
        {
            _currentHoveredInteractable = hit.transform.gameObject;
            CallHover(interactable);
        }
    }
    
    private void CallHover(IInteractable interactable)
    {
        interactable.OnHover();
    }
    private void CallHoverExit()
    {
        if (_currentHoveredInteractable == null)
            return;
        
        _currentHoveredInteractable.GetComponent<IInteractable>().OnHoverExit();
        _currentHoveredInteractable = null;
    }
    
    private void EnableAllActions()
    {
        _moveAction.Enable();
        _lookAction.Enable();
        _shootAction.Enable();
        _jumpAction.Enable();
        _sprintAction.Enable();
        _interactAction.Enable();
    }
    private void DisableAllActions()
    {
        _moveAction.Disable();
        _lookAction.Disable();
        _shootAction.Disable();
        _jumpAction.Disable();
        _sprintAction.Disable();
        _interactAction.Disable();
    }

    private void CheckIsGrounded()
    {
        _onGround = Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.up * -1, out RaycastHit hit, 0.6f);
    }
    private Vector2 GetMoveInput()
    {
        return _moveAction.ReadValue<Vector2>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, transform.position + Vector3.down * 0.1f);
    }
}
