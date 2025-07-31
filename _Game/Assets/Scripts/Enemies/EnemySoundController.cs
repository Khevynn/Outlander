using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class EnemySoundController : MonoBehaviour
{
    [Header("References")]
    private EnemyController _enemyController;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource continuousAudioSource;
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private AudioClip[] attackClips;
    [SerializeField] private AudioClip growlingClip;
    [SerializeField] private AudioClip dyingClip;
    [SerializeField] private float maxDistance = 30;

    [Header("Continuous Audio Settings")] 
    [SerializeField] private bool playOnIdle;
    [SerializeField] private bool playOnAttack;
    
    [Header("Growling Settings")]
    [SerializeField] private Vector2 growlingIntervalRange = new Vector2(0.95f, 1.05f);
    private float growlingInterval;
    private float growlingTimer;
    
    [Header("Variation")]
    [SerializeField] private Vector2 footStepsPitchRange = new Vector2(0.95f, 1.05f);
    [SerializeField] private Vector2 attackPitchRange = new Vector2(0.95f, 1.05f);

    private void Awake()
    {
        _enemyController = GetComponent<EnemyController>();
        audioSource.maxDistance = maxDistance;
    }

    private void FixedUpdate()
    {
        VerifyContinuousSound();

        if (_enemyController.GetCurrentState().Name != State.Pursuing)
            return;
        
        if (growlingTimer > growlingInterval)
        {
            growlingTimer = 0;
            PlayGrowling();
        }
        else
        {
            growlingTimer += Time.fixedDeltaTime;
        }
    }

    private void VerifyContinuousSound()
    {
        if (!continuousAudioSource)
            return;
        
        if (!playOnIdle && _enemyController.GetCurrentState().Name == State.Idle)
        {
            continuousAudioSource.Stop();
        }else if (!playOnAttack && _enemyController.GetCurrentState().Name == State.Attacking)
        {
            continuousAudioSource.Stop();
        }
        if (!continuousAudioSource.isPlaying)
        {
            PlayContinuousSound();
        }
        if (_enemyController.GetIsDead() && continuousAudioSource.isPlaying)
        {
            continuousAudioSource.Stop();
        }
    }

    public void PlayDeath()
    {
        audioSource.pitch = 2;
        audioSource.PlayOneShot(dyingClip);
    }
    public void PlayFootstep()
    {
        if (footstepClips.Length == 0) return;
        
        var clip = footstepClips[Random.Range(0, footstepClips.Length)];
        audioSource.pitch = Random.Range(footStepsPitchRange.x, footStepsPitchRange.y);
        audioSource.PlayOneShot(clip);
    }
    public void PlayGrowling()
    {
        if (!growlingClip) return;
        
        growlingInterval = Random.Range(growlingIntervalRange.x, growlingIntervalRange.y);
        audioSource.PlayOneShot(growlingClip);
    }
    public void PlayAttack()
    {
        if (attackClips.Length == 0) return;

        var clip = attackClips[Random.Range(0, attackClips.Length)];
        audioSource.pitch = Random.Range(attackPitchRange.x, attackPitchRange.y);
        audioSource.PlayOneShot(clip);
    }
    public void PlayContinuousSound()
    {
        if (!playOnAttack && _enemyController.GetCurrentState().Name == State.Attacking)
            return;
        if (!playOnIdle && _enemyController.GetCurrentState().Name == State.Idle)
            return;
        
        continuousAudioSource.Play();
    }
}
