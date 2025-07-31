using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class CamController : MonoBehaviour
{
    public static CamController Instance { get; private set; }

    [Header("Components")]
    [SerializeField] private Transform target;
    [FormerlySerializedAs("player")] [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform cameraVisual; // Assign in inspector

    [Header("Inputs")]
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private float minY;
    [SerializeField] private float maxY;
    private InputAction _lookAction;

    private float _xRotation = 0f;
    private float _yRotation = 0f;
    private bool _canControlCam = true;

    private void Awake()
    {
        if (Instance)
        {
            Debug.Log("CamController already exists, destroying new one");
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        _lookAction = InputSystem.actions.FindAction("Look");
        Cursor.lockState = CursorLockMode.Locked;

        transform.rotation = target.rotation;
    }

    private void Update()
    {
        if (_canControlCam)
        {
            transform.position = target.position;
            RotateCam();
        }
    }

    public void ShakeCamera(float duration, float magnitude)
    {
        StartCoroutine(ShakeAction(duration, magnitude));
    }

    private IEnumerator ShakeAction(float duration, float magnitude)
    {
        var originalLocalPos = cameraVisual.localPosition;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            var offsetX = Random.Range(-1f, 1f) * magnitude;
            var offsetY = Random.Range(-1f, 1f) * magnitude;

            cameraVisual.localPosition = originalLocalPos + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraVisual.localPosition = originalLocalPos;
    }

    private void RotateCam()
    {
        _yRotation += GetLookInput().x * (mouseSensitivity / 10f);

        _xRotation -= GetLookInput().y * (mouseSensitivity / 10f);
        _xRotation = Mathf.Clamp(_xRotation, minY, maxY);

        transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
        playerTransform.rotation = Quaternion.Euler(0f, _yRotation + 20f, 0f);
    }

    public void ResetCam()
    {
        transform.rotation = Quaternion.identity;
        playerTransform.rotation = Quaternion.identity;
    }

    private Vector2 GetLookInput()
    {
        return _lookAction.ReadValue<Vector2>();
    }

    // DEATH CAMERA FEATURE
    public void TriggerDeathCamera(float heightOffset, float duration)
    {
        _canControlCam = false;
        StartCoroutine(DeathCamRoutine(heightOffset, duration));
    }

    private IEnumerator DeathCamRoutine(float heightOffset, float duration)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        // End position: directly above the player
        Vector3 endPos = playerTransform.position + Vector3.up * heightOffset;

        // End rotation: straight down
        Quaternion endRot = Quaternion.Euler(90f, 0f, 0f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        transform.rotation = endRot;
    }

    public void SetMouseSensitivity()
    {
        mouseSensitivity = PlayerPrefs.HasKey("MouseSensitivity") ? PlayerPrefs.GetFloat("MouseSensitivity") : 1;
    }
}
