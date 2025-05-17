using System.Collections.Generic;
using UnityEngine;

public class EnemiesPool : MonoBehaviour
{
    public static EnemiesPool Instance { get; private set; }
    
    [SerializeField] private List<GameObject> _enemiesPrefabs;
    [SerializeField] private float minimumSpawnOfEachEnemy = 5;
    private List<EnemyController> _availableEnemiesInPool = new List<EnemyController>();
    private List<EnemyController> _nonAvailableEnemiesInPool = new List<EnemyController>();
    
    
    Vector3 spawnPosition = Vector3.zero;
    
    private void Awake()
    {
        if(Instance != null)
        {
            Debug.Log("EnemiesPool already exists, destroying new one");
            Destroy(this);
        }
        else
            Instance = this;

        SortPrefabsById();
    }
    private void Start()
    {
        SpawnInitialEnemies();
    }

    private void SpawnInitialEnemies()
    {
        var ray = new Ray(transform.position, -transform.up);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            spawnPosition = hit.transform.position;
        }
        
        for(int i = 0; i < _enemiesPrefabs.Count; ++i)
        {
            for (int numberOfEnemies = 0; numberOfEnemies < minimumSpawnOfEachEnemy; numberOfEnemies++)
            {
                EnemyController enemy = Instantiate(_enemiesPrefabs[i], spawnPosition, Quaternion.identity, transform).GetComponent<EnemyController>();
                _availableEnemiesInPool.Add(enemy);
                enemy.gameObject.SetActive(false);
            }
        }
    }

    public EnemyController GetEnemyOfType(EnemyType type)
    {
        EnemyController enemyToReturn = null;
        bool couldFindEnemy = false;
        for(int i = 0; i < _availableEnemiesInPool.Count; ++i)
        {
            if(_availableEnemiesInPool[i].GetEnemyType() == type)
            {
                couldFindEnemy = true;
                enemyToReturn = _availableEnemiesInPool[i];
            }
        }

        if (!couldFindEnemy)
        {
            enemyToReturn = Instantiate(_enemiesPrefabs[(int)type], spawnPosition, Quaternion.identity, transform).GetComponent<EnemyController>();
        }
        enemyToReturn.gameObject.SetActive(true);
        enemyToReturn.ActivateEnemy();
        
        _availableEnemiesInPool.Remove(enemyToReturn);
        _nonAvailableEnemiesInPool.Add(enemyToReturn);
        
        return enemyToReturn;
    }
    public void ReturnEnemyToPool(EnemyController enemy)
    {
        enemy.gameObject.SetActive(false);
        _availableEnemiesInPool.Add(enemy);
        _nonAvailableEnemiesInPool.Remove(enemy);
    }

    public int GetNumberOfCurrentlyActiveEnemies()
    {
        return _nonAvailableEnemiesInPool.Count;
    }
    
    private void SortPrefabsById()
    {
        _enemiesPrefabs.Sort((GameObject a, GameObject b) => 
            a.GetComponent<EnemyController>().GetEnemyType().CompareTo(b.GetComponent<EnemyController>().GetEnemyType())
        );
    }
}
