using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody _rb;
    private Animator _animator;
    
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float groundLinearDamping = 10f;
    private float _currentMaxSpeed;
    private Vector3 _moveDirection;
    
    [Header("Jumping")]
    [SerializeField] private float jumpForce = 10f;
    
    [Header("Inputs")]
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _sprintAction;
    
    private bool _onGround;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        
        _moveAction = InputSystem.actions.FindAction("Move");
        _sprintAction = InputSystem.actions.FindAction("Sprint");
        _jumpAction = InputSystem.actions.FindAction("Jump");
    }
    private void Update()
    {
        UpdateAnimations();
    }
    private void FixedUpdate()
    {
        CheckIsGrounded();
        Movement();

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
