using System;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyDummyStats : HealthComponent
{
    public new void Die()
    {
        base.Die();
        currentHealth = maxHealth;
    }
}
