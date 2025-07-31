using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HealthComponent : MonoBehaviour, IDamageable
{
    [Serializable]
    private enum ComponentOwner
    {
        Player,
        Enemy
    }

    [Header("Stats")]
    [SerializeField] protected float maxHealth;
    [SerializeField] protected float currentHealth;

    private float _lastDamageTime;
    private bool _isHealing;

    public bool IsDead { get; private set; }

    [Header("HP Bar Settings")]
    [SerializeField] private ComponentOwner owner;
    [SerializeField] protected Transform hpBar;
    [SerializeField] protected TMP_Text hpText;
    public Slider hpSlider;

    public UnityEvent onGetHit;
    public UnityEvent onDie;

    private Transform _playerTransform;

    #region Unity Lifecycle

    private void Start()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        currentHealth = maxHealth;
        UpdateHpBarInfo();

        _lastDamageTime = Time.time;
    }

    private void FixedUpdate()
    {
        if (owner == ComponentOwner.Enemy)
        {
            UpdateHpBarRotation();
        }

        if (owner == ComponentOwner.Player && !_isHealing && !IsDead)
        {
            if (Time.time - _lastDamageTime >= 30f)
            {
                StartCoroutine(HealOverTime());
                _isHealing = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (owner != ComponentOwner.Player)
            return;

        if (!other.CompareTag("EnemyProjectile"))
            return;

        if (other.TryGetComponent(out EnemyProjectile projectile))
        {
            TakeDamage(projectile.projectileDamage);
        }
    }

    #endregion

    #region Public Methods

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0 || IsDead)
            return;

        _lastDamageTime = Time.time;

        if (_isHealing)
        {
            StopCoroutine(HealOverTime());
            _isHealing = false;
        }

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            UpdateHpBarInfo();
            Die();
            return;
        }

        UpdateHpBarInfo();

        if (owner == ComponentOwner.Player)
        {
            CamController.Instance.ShakeCamera(0.2f, 0.1f);
            InGamePopupsController.Instance.CallDamageAlert(0.2f);
        }

        onGetHit?.Invoke();
    }

    public void Die()
    {
        if (IsDead)
            return;

        IsDead = true;
        onDie?.Invoke();
    }
    public void Revive()
    {
        IsDead = false;
        currentHealth = maxHealth;
        UpdateHpBarInfo();
    }

    public float GetMaxHealth() => maxHealth;
    public float GetCurrentHealth() => currentHealth;
    public float GetHealthPercentage() => currentHealth / maxHealth;

    #endregion

    #region Private Helpers

    private IEnumerator HealOverTime()
    {
        while (currentHealth < maxHealth && !IsDead)
        {
            currentHealth += 5f;
            if (currentHealth > maxHealth)
                currentHealth = maxHealth;

            UpdateHpBarInfo();

            yield return new WaitForSeconds(5f);
        }

        _isHealing = false;
    }

    private void UpdateHpBarInfo()
    {
        if (!hpBar)
            return;

        hpText.text = $"{currentHealth}/{maxHealth}";
        hpSlider.value = GetHealthPercentage();
    }

    private void UpdateHpBarRotation()
    {
        if (!_playerTransform || !hpBar)
            return;

        Vector3 directionToPlayer = hpBar.position - _playerTransform.position;
        hpBar.rotation = Quaternion.LookRotation(directionToPlayer);
    }

    #endregion
}
