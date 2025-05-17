using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerController))]
public class PlayerSoundsController : MonoBehaviour
{
    [Header("References")]
    private Rigidbody _rb;
    private PlayerController _playerController;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private AudioClip landingClip;

    [Header("Step Settings")]
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float runStepInterval = 0.3f;

    [Header("Variation")]
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);
    
    private float stepTimer;
    private bool _wasGroundedLastFrame;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _playerController = GetComponent<PlayerController>();
        _wasGroundedLastFrame = _playerController.OnGround();
    }

    private void Update()
    {
        bool isGrounded = _playerController.OnGround();

        // Landing detection: just transitioned from air to ground
        if (!_wasGroundedLastFrame && isGrounded)
        {
            PlayLanding();
        }
        _wasGroundedLastFrame = isGrounded;

        // Footsteps only when moving on ground
        if (isGrounded && _playerController.IsMoving())
        {
            stepTimer += Time.deltaTime;
            float interval = _playerController.IsRunning() ? runStepInterval : walkStepInterval;

            if (stepTimer >= interval)
            {
                PlayFootstep();
                stepTimer = 0f;
            }
        }
        else if (!isGrounded)
        {
            // reset timer when in air so you don't get an instant footstep on landing
            stepTimer = 0f;
        }
    }

    private void PlayFootstep()
    {
        if (footstepClips.Length == 0) return;

        var clip = footstepClips[Random.Range(0, footstepClips.Length)];
        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(clip);
    }

    private void PlayLanding()
    {
        if (landingClip == null) return;

        // Optionally vary pitch on landing as well
        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(landingClip);
    }
}
