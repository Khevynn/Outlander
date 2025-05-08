using UnityEngine;
using UnityEngine.InputSystem;

public class CamController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform target;
    [SerializeField] private Transform player;
    
    [Header("Inputs")]
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private float minY;
    [SerializeField] private float maxY;
    private InputAction _lookAction;
    
    private float _xRotation = 0f;
    private float _yRotation = 0f;

    private void Start()
    {
        _lookAction = InputSystem.actions.FindAction("Look");
        Cursor.lockState = CursorLockMode.Locked;
        
        transform.rotation = target.rotation;
    }

    private void Update()
    {
        transform.position = target.position;
        
        RotateCam();
    }

    private void RotateCam()
    {
        _yRotation += GetLookInput().x * (mouseSensitivity / 10f);
        
        _xRotation -= GetLookInput().y * (mouseSensitivity / 10f);
        _xRotation = Mathf.Clamp(_xRotation, minY, maxY);

        transform.rotation = Quaternion.Euler(_xRotation,_yRotation, 0f);
        player.rotation = Quaternion.Euler(0f, _yRotation + 20f, 0f);
    }
    private Vector2 GetLookInput()
    {
        return _lookAction.ReadValue<Vector2>();
    }
}
