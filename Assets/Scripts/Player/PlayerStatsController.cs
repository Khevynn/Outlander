using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsController : MonoBehaviour, IDamageable
{
    [Header("Stats")] 
    [SerializeField] protected float maxHealth;
    [SerializeField] protected float currentHealth;
    
    [Header("Hp Bar Settings")]
    [SerializeField] protected TMP_Text hpText;
    public Slider hpSlider;
    
    public void Start()
    {
        currentHealth = maxHealth;
    }
    
    private void UpdateHpBarInfo()
    {
        hpText.text = $"{currentHealth}";
        hpSlider.value = GetHealthPercentage();
    }

    public void TakeDamage(float amount)
    {
        if(currentHealth <= 0) { Die(); }
        currentHealth -= amount;
        UpdateHpBarInfo();
    }
    public void Die()
    {
        print("Player Died");
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
