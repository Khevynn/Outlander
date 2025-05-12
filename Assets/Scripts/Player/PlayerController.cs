using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody _rb;
    private Animator _animator;
    
    [Header("References")]
    private Camera _mainCamera;
    private CamController camController;
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject pauseMenuUI;
    
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
    private GameObject _currentHoveredObj;
    
    [Header("Inputs")]
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _shootAction;
    private InputAction _jumpAction;
    private InputAction _sprintAction;
    private InputAction _interactAction;
    private InputAction _inventoryAction;
    private InputAction _pauseAction;
    
    private bool _onGround;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _mainCamera = Camera.main;
        camController = _mainCamera.transform.parent.GetComponent<CamController>();
        
        _moveAction = InputSystem.actions.FindAction("Move");
        _lookAction = InputSystem.actions.FindAction("Look");
        _shootAction = InputSystem.actions.FindAction("Shoot");
        _sprintAction = InputSystem.actions.FindAction("Sprint");
        _jumpAction = InputSystem.actions.FindAction("Jump");
        _interactAction = InputSystem.actions.FindAction("Interact");
        _inventoryAction = InputSystem.actions.FindAction("Inventory");
        _pauseAction = InputSystem.actions.FindAction("Pause");
    }
    private void Update()
    {
        UpdateAnimations();
    }
    private void FixedUpdate()
    {
        CheckForInputs();
        CheckIsGrounded();
        CheckForHoverable();
        
        Movement();
    }

    private void CheckForInputs()
    {
        _interactAction.performed += ctx => CallInteraction();
        _inventoryAction.performed += ctx => OpenOrCloseInventory();
        _pauseAction.performed += ctx => PauseOrUnpauseGame();
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

        // Rotate direction 20 degrees around Y axis to fit with the camera direction
        Quaternion rotation = Quaternion.AngleAxis(-20f, Vector3.up);
        Vector3 rotatedDirection = rotation * _moveDirection.normalized;

        _rb.AddForce(rotatedDirection * (_currentMaxSpeed * 10f), ForceMode.Force);

        _rb.linearDamping = _onGround ? groundLinearDamping : 0f;
        
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
    public void PauseOrUnpauseGame()
    {
        inventoryUI.SetActive(false);
        pauseMenuUI.SetActive(!pauseMenuUI.activeSelf);
        Cursor.lockState = pauseMenuUI.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
        if (pauseMenuUI.activeSelf)
        {
            DisableAllActions();
            Time.timeScale = 0f;
        }
        else
        {
            EnableAllActions();
            Time.timeScale = 1f;
        }
    }
    public void BackToMainMenu()
    {
        EnableAllActions();
        Time.timeScale = 1f;
        GameManager.Instance.BackToMainMenu();
    }

    private void CallInteraction()
    {
        var ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.TransformDirection(Vector3.forward));
        
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactionRange, interactionLayer))
        {
            IInteract interact = hit.transform.gameObject.GetComponent<IInteract>();
            if(interact != null)
                interact.OnInteract();
        }
    }
    private void CheckForHoverable()
    {
        var ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.TransformDirection(Vector3.forward));

        if (!Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionLayer))
        {
            CallHoverExit();
            return;
        }

        if (_currentHoveredObj != hit.transform.gameObject)
        {
            CallHoverExit();

            var hoverables = hit.transform.GetComponents<IHover>();
            if (hoverables.Length == 0)
                return;

            _currentHoveredObj = hit.transform.gameObject;
            CallHover(hoverables);
        }
    }
    private void CallHover(IHover[] hoverableObjs)
    {
        foreach (var hoverable in hoverableObjs)
            hoverable.OnHoverEnter();
    }
    private void CallHoverExit()
    {
        if (_currentHoveredObj == null)
            return;

        var hoverables = _currentHoveredObj.GetComponents<IHover>();
        foreach (var hoverable in hoverables)
            hoverable.OnHoverExit();

        _currentHoveredObj = null;
    }

    public void CallDeathAnimation()
    {
        GetComponentInChildren<Renderer>().shadowCastingMode = ShadowCastingMode.On;
        DisableAllActions();
        
        _animator.SetBool("isDead", true);
        camController.TriggerDeathCamera(2f, 1f);

        InGamePopupsController.Instance.CallFadeIn(3f, false);
        StartCoroutine(CallLoseScreen());
    }
    public IEnumerator CallLoseScreen()
    {
        yield return new WaitForSeconds(3f);
        EnableAllActions();
        GameManager.Instance.Lose();
    }
    
    public void EnableAllActions()
    {
        _moveAction.Enable();
        _lookAction.Enable();
        _shootAction.Enable();
        _jumpAction.Enable();
        _sprintAction.Enable();
        _interactAction.Enable();
    }
    public void DisableAllActions()
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
