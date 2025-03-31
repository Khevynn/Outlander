using System;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyDummyStats : IDamageable
{
    protected override void Die()
    {
        base.Die();
        currentHealth = maxHealth;
        UpdateHpBarInfo();
    }
}
