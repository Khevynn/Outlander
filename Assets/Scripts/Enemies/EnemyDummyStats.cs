using System;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyDummyStats : EnemyHpStatsControlller
{
    public new void Die()
    {
        base.Die();
        currentHealth = maxHealth;
    }
}
