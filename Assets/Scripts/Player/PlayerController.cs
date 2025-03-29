using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody _rb;
    
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float groundLinearDamping = 10f;
    private float _currentMaxSpeed;
    private Vector3 _moveDirection;
    
    [Header("Inputs")]
    private InputAction _moveAction;
    private InputAction _sprintAction;
    
    private bool _onGround;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _moveAction = InputSystem.actions.FindAction("Move");
        _sprintAction = InputSystem.actions.FindAction("Sprint");}
    private void FixedUpdate()
    {
        CheckIsGrounded();
        Movement();
        
        if(_sprintAction.IsPressed())
            _currentMaxSpeed = sprintSpeed;
        else
            _currentMaxSpeed = walkSpeed;
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

    private void CheckIsGrounded()
    {
        if (Physics.Raycast(transform.position, transform.up * -1, out RaycastHit hit, 0.5f))
        {
            _onGround = true;
        }
    }
    private Vector2 GetMoveInput()
    {
        return _moveAction.ReadValue<Vector2>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 0.5f);
    }
}
