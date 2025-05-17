using System;
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
    public bool IsDead { get; private set; }
    
    [Header("Hp Bar Settings")]
    [SerializeField] private ComponentOwner owner;
    [SerializeField] protected Transform hpBar;
    [SerializeField] protected TMP_Text hpText;
    public Slider hpSlider;
    
    public UnityEvent onGetHit;
    public UnityEvent onDie;
    private Transform _playerTransform;
    
    public void Start()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        currentHealth = maxHealth;
        UpdateHpBarInfo();
    }
    private void FixedUpdate()
    {
        if(owner == ComponentOwner.Enemy)
            UpdateHpBarRotation();
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
        if(_playerTransform && hpBar)
        {
            hpBar.transform.rotation = Quaternion.LookRotation(hpBar.transform.position - _playerTransform.position);
        }
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0)
            return;
        
        if (currentHealth - amount <= 0)
        {
            currentHealth = 0;
            UpdateHpBarInfo();
            Die();
            return;
        }
        
        currentHealth -= amount;
        UpdateHpBarInfo();

        if (owner == ComponentOwner.Player)
        {
            CamController.Instance.ShakeCamera(.2f, .1f);
            InGamePopupsController.Instance.CallDamageAlert(.2f);
        }
        
        onGetHit.Invoke();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("EnemyProjectile") && owner == ComponentOwner.Player & other.TryGetComponent(out EnemyProjectile projectile))
        {
            TakeDamage(projectile.projectileDamage);
        }
    }
    public void Die()
    {
        IsDead = true;
        onDie.Invoke();
    }
    public void Revive()
    {
        IsDead = false;
        currentHealth = maxHealth;
        UpdateHpBarInfo();
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}
