using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EnemyHpStatsControlller : MonoBehaviour, IDamageable
{
    [Header("References")] 
    private EnemyController _enemyController;
    
    [Header("Stats")] 
    [SerializeField] protected float maxHealth;
    [SerializeField] protected float currentHealth;
    
    [Header("Hp Bar Settings")]
    [SerializeField] protected Transform hpBar;
    [SerializeField] protected TMP_Text hpText;
    public Slider hpSlider;
    
    public UnityEvent onGetHit;
    private Transform _playerTransform;
    
    public void Start()
    {
        _enemyController = GetComponent<EnemyController>();
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        currentHealth = maxHealth;
        UpdateHpBarInfo();
    }
    private void FixedUpdate()
    {
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
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            UpdateHpBarInfo();
            Die();
            return;
        }
        
        UpdateHpBarInfo();
        onGetHit.Invoke();
    }
    public void Die()
    {
        _enemyController.CallDeath();
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
