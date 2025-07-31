using UnityEngine;

public interface IDamageable
{
    public void TakeDamage(float amount);
    public void Die();
    
    public float GetMaxHealth();
    public float GetCurrentHealth();
}
