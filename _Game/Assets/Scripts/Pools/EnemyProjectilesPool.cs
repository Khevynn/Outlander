using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectilesPool : MonoBehaviour
{
    public static EnemyProjectilesPool Instance { get; private set; }

    [Header("Projectile Spawning")]
    [SerializeField] private int startNumberOfProjectiles;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField]private List<GameObject> _availableProjectiles;
    [SerializeField]private List<GameObject> _nonAvailableProjectiles;
    
    void Awake()
    {
        if(Instance != null)
        {
            Debug.Log("EnemyProjectilesPool already exists, destroying new one");
            Destroy(this);
        }
        else
            Instance = this;
    }

    private void Start()
    {
        SpawnInitialProjectiles();
    }

    private void SpawnInitialProjectiles()
    {
        for(int i = 0; i < startNumberOfProjectiles; ++i)
        {
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity, transform);
            _availableProjectiles.Add(projectile);
            projectile.SetActive(false);
        }
    }
    
    public GameObject GetProjectileFromPool()
    {
        var projectileToReturn = _availableProjectiles[0];
        if (projectileToReturn)
        {
            _availableProjectiles.Remove(projectileToReturn);
            _nonAvailableProjectiles.Add(projectileToReturn);
        }
        else
        {
            projectileToReturn = Instantiate(projectilePrefab, transform.position, Quaternion.identity, transform);
            _nonAvailableProjectiles.Add(projectileToReturn);
        }

        projectileToReturn.SetActive(true);
        return projectileToReturn;
    }
    public void ReturnProjectileToPool(GameObject projectile)
    {
        projectile.TryGetComponent(out Rigidbody rb);
        rb.linearVelocity = Vector3.zero;
        
        projectile.SetActive(false);
        _availableProjectiles.Add(projectile);
        _nonAvailableProjectiles.Remove(projectile);
    }
}
