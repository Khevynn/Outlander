using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class IDamageable : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    
    [Header("Hp Bar Settings")]
    [SerializeField] private Transform hpBar;
    [SerializeField] private TMP_Text hpText;
    public Slider hpSlider;
    
    private Transform _playerTransform;

    public void Start()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        currentHealth = maxHealth;
    }
    
    private void FixedUpdate()
    {
        UpdateHpBarRotation();
    }

    protected void UpdateHpBarInfo()
    {
        hpText.text = $"{currentHealth}/{maxHealth}";
        hpSlider.value = GetHealthPercentage();
    }
    protected void UpdateHpBarRotation()
    {
        if(_playerTransform && hpBar)
        {
            hpBar.transform.rotation = Quaternion.LookRotation(hpBar.transform.position - _playerTransform.position);
        }
    }

    public void TakeDamage(int damage)
    {
        if(currentHealth <= 0) { Die(); }
        currentHealth -= damage;
        UpdateHpBarInfo();
    }
    protected virtual void Die()
    {
        
    }
    
    public float GetMaxHealth() { return maxHealth; }
    public float GetCurrentHealth() { return currentHealth; }
    public float GetHealthPercentage() { return (float)currentHealth / maxHealth; }
}
