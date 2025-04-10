using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance;
    
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float nOfBulletsToSpawnAtStart;
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
        for (int i = 0; i < nOfBulletsToSpawnAtStart; ++i)
        {
            var bullet = Instantiate(bulletPrefab, transform);
            _availableBulletList.Add(bullet);
            bullet.SetActive(false);
        }
    }
    
    public GameObject GetBullet()
    {
        if (_availableBulletList.Count <= 0)
        {
            _availableBulletList.Add(Instantiate(bulletPrefab, transform));
        }
        
        var bullet = _availableBulletList[0];

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
