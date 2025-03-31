using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance;
    
    [SerializeField] private GameObject bulletPrefab;
    private List<GameObject> _availableBulletList = new List<GameObject>();
    private List<GameObject> _notAvailableBulletList = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        SpawnBullets();
    }

    private void SpawnBullets()
    {
        for (int i = 0; i < 10; ++i)
        {
            var bullet = Instantiate(bulletPrefab, transform);
            _availableBulletList.Add(bullet);
            bullet.SetActive(false);
        }
    }
    
    public GameObject GetBullet()
    {
        var bullet = _availableBulletList[0];

        if (!bullet)
        {
            bullet = Instantiate(bulletPrefab, transform);
        }
        
        _availableBulletList.RemoveAt(0);
        _notAvailableBulletList.Add(bullet);
        return bullet;
    }
    public void ReturnBullet(GameObject bullet)
    {
        _notAvailableBulletList.Remove(bullet);
        _availableBulletList.Add(bullet);
    }
}
