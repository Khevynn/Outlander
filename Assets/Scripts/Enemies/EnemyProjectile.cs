using System;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float projectileDamage;

    private void OnEnable()
    {
        Invoke("ReturnProjectile", 5);
    }
    
    /// <summary>
    /// Returns projectile to pool.
    /// </summary>
    private void ReturnProjectile()
    {
        EnemyProjectilesPool.Instance.ReturnProjectileToPool(this.gameObject);
    }
}
