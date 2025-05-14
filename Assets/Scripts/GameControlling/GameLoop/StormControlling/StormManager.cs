using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using VolumetricFogAndMist2;

public class StormManager : MonoBehaviour
{
    public static StormManager Instance { get; private set; }

    [Header("Storm Visuals")]
    [SerializeField] private VolumetricFogProfile stormProfile;

    [Header("Storm Timings")]
    [SerializeField] private float maxTimeBetweenStorms = 60f;
    [SerializeField] private float maxTimeToSkipStorm = 30f;
    [SerializeField] private float timeToStartStorm = 10f;
    [SerializeField] private float timeToFinishStorm = 8f;
    [SerializeField] private float maxStormDuration = 45f;
    [SerializeField] private float screenFadeTime = 2f;

    [Header("Enemy Waves")]
    [SerializeField] private int numberOfWavesToSpawnIfSkipped = 2;
    [SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private int maxActiveEnemies = 20;
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private List<EnemyGroupSize> groupSizes = new();

    [Header("Enemy Wave Requirements")]
    [SerializeField] private int minWaveForDogs = 0;
    [SerializeField] private int minWaveForSpiders = 2;
    [SerializeField] private int minWaveForLandBeetles = 3;
    [SerializeField] private int minWaveForFlyingBeetles = 5;
    [SerializeField] private float baseEnemiesPerWave = 3f;
    [SerializeField] private float enemyGrowthRate = 1.15f;

    private float currentTimeBetweenStorms;
    private float lastStartedStormTime;
    private float currentTimeSinceStartOfStorm;

    private int currentWave = 0;

    private bool skippedStorm;
    private bool startingStorm;
    private bool currentlyInStorm;
    private bool finishingStorm;

    private Coroutine startStormCoroutine;
    private Transform playerTransform;

    private readonly List<Vector3> tempSpawnPositions = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        playerTransform = GameObject.FindWithTag("Player")?.transform;
        if (!playerTransform)
        {
            Debug.LogError("Player not found in scene. Make sure the player has the 'Player' tag.");
        }

        stormProfile.noiseFinalMultiplier = 0f;
        currentTimeBetweenStorms = maxTimeBetweenStorms;
    }

    private void FixedUpdate()
    {
        if (!currentlyInStorm && !finishingStorm && !startingStorm)
        {
            currentTimeBetweenStorms -= Time.fixedDeltaTime;
            if (currentTimeBetweenStorms <= 0f)
                startStormCoroutine = StartCoroutine(StartStorm());
        }

        if (startingStorm)
        {
            currentTimeSinceStartOfStorm = Time.time - lastStartedStormTime;
            if (currentTimeSinceStartOfStorm <= maxTimeToSkipStorm)
                InGamePopupsController.Instance.SetStormAlertTimer(maxTimeToSkipStorm - currentTimeSinceStartOfStorm);
            else
                InGamePopupsController.Instance.HideStormAlert();
        }
    }

    public void CallSkipStorm()
    {
        if (skippedStorm || !startingStorm || currentTimeSinceStartOfStorm > maxTimeToSkipStorm)
        {
            Debug.LogWarning("SkipStorm called outside valid window.");
            return;
        }

        InGamePopupsController.Instance.CallFadeIn(screenFadeTime, true);
        StartCoroutine(SkipStorm());
    }

    private IEnumerator StartStorm()
    {
        startingStorm = true;
        skippedStorm = false;
        lastStartedStormTime = Time.time;

        InGamePopupsController.Instance.ShowStormAlert();
        yield return LerpStormMultiplier(0f, 1.3f, timeToStartStorm);

        startingStorm = false;
        currentlyInStorm = true;

        MainWaveLoop();

        yield return new WaitForSeconds(maxStormDuration);
        StartCoroutine(FinishStorm());
    }

    private IEnumerator SkipStorm()
    {
        yield return new WaitForSeconds(screenFadeTime);

        InGamePopupsController.Instance.HideStormAlert();
        skippedStorm = true;
        startingStorm = false;

        if (startStormCoroutine != null)
        {
            StopCoroutine(startStormCoroutine);
            startStormCoroutine = null;
        }

        for (int i = 0; i < numberOfWavesToSpawnIfSkipped; i++)
            CreateEnemyWave(Mathf.Pow(enemyGrowthRate, currentWave - 1));

        currentTimeBetweenStorms = maxTimeBetweenStorms;
        stormProfile.noiseFinalMultiplier = 0f;
    }

    private IEnumerator FinishStorm()
    {
        currentlyInStorm = false;
        finishingStorm = true;

        yield return LerpStormMultiplier(stormProfile.noiseFinalMultiplier, 0f, timeToFinishStorm);

        currentTimeBetweenStorms = maxTimeBetweenStorms;
        finishingStorm = false;
    }

    private IEnumerator LerpStormMultiplier(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            stormProfile.noiseFinalMultiplier = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        stormProfile.noiseFinalMultiplier = to;
    }

    private float GetStormIntensity() => Mathf.InverseLerp(0f, 1.3f, stormProfile.noiseFinalMultiplier);

    private void MainWaveLoop()
    {
        if (!currentlyInStorm) return;

        if (EnemiesPool.Instance.GetNumberOfCurrentlyActiveEnemies() >= maxActiveEnemies)
        {
            Invoke(nameof(MainWaveLoop), timeBetweenWaves);
            return;
        }

        CreateEnemyWave(Mathf.Pow(enemyGrowthRate, currentWave));
        Invoke(nameof(MainWaveLoop), timeBetweenWaves);
    }

    private void CreateEnemyWave(float scaleFactor)
    {
        int toSpawn = Mathf.CeilToInt(baseEnemiesPerWave * scaleFactor);

        while (toSpawn > 0)
        {
            EnemyType type = GetRandomEnemyType();
            int groupSize = Mathf.Min(GetGroupSizeForType(type), toSpawn);

            Vector3 spawnPoint = GetRandomSpawnPoint();
            for (int i = 0; i < groupSize; i++)
            {
                EnemyController enemy = EnemiesPool.Instance.GetEnemyOfType(type);
                enemy.gameObject.SetActive(false);
                enemy.transform.position = spawnPoint;
                enemy.transform.rotation = Quaternion.identity;
                enemy.gameObject.SetActive(true);
            }

            toSpawn -= groupSize;
        }

        currentWave++;
    }

    private EnemyType GetRandomEnemyType()
    {
        float intensity = GetStormIntensity();
        float progress = Mathf.Clamp01(currentWave / 20f);
        var weights = new Dictionary<EnemyType, float>
        {
            [EnemyType.Dog] = currentWave >= minWaveForDogs ? Mathf.Lerp(0.5f, 0.2f, progress) : 0f,
            [EnemyType.Spider] = currentWave >= minWaveForSpiders ? Mathf.Lerp(0.2f, 0.3f, progress) : 0f,
            [EnemyType.LandBeetle] = currentWave >= minWaveForLandBeetles ? Mathf.Lerp(0.15f, 0.3f, intensity) : 0f,
            [EnemyType.FlyingBeetle] = currentWave >= minWaveForFlyingBeetles ? Mathf.Lerp(0.15f, 0.2f + progress * 0.1f, intensity) : 0f
        };

        float total = weights.Values.Sum();
        if (total == 0f) return EnemyType.Dog; // fallback

        float rand = Random.value;
        float cumulative = 0f;

        foreach (var kvp in weights)
        {
            cumulative += kvp.Value / total;
            if (rand <= cumulative) return kvp.Key;
        }

        return EnemyType.Dog; // fallback
    }

    private int GetGroupSizeForType(EnemyType type)
    {
        var group = groupSizes.Find(g => g.type == type);
        return group != null ? Random.Range(group.min, group.max + 1) : 1;
    }

    private Vector3 GetRandomSpawnPoint()
    {
        const int maxAttempts = 30;
        const float minDistance = 5f;
        tempSpawnPositions.Clear();

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Generate a random point above the ground within the spawn radius
            float offsetX = Random.Range(-spawnRadius, spawnRadius);
            float offsetZ = Random.Range(-spawnRadius, spawnRadius);
            Vector3 rayOrigin = new(playerTransform.position.x + offsetX, playerTransform.position.y + 10f, playerTransform.position.z + offsetZ);

            // Cast a ray down to find the ground
            if (!Physics.Raycast(rayOrigin, -transform.up, out RaycastHit groundHit, 100f, groundLayer))
                continue;

            Vector3 candidatePoint = groundHit.point;

            // Check if the candidate is far enough from previous spawn points
            bool tooCloseToOthers = tempSpawnPositions.Any(pos => Vector3.Distance(candidatePoint, pos) < minDistance);
            if (tooCloseToOthers)
                continue;

            // Ensure the point is on the NavMesh
            if (!NavMesh.SamplePosition(candidatePoint, out NavMeshHit navHit, 1f, NavMesh.AllAreas))
                continue;

            tempSpawnPositions.Add(candidatePoint);  // Remember this point
            return navHit.position;
        }

        Debug.LogError("Could not find valid spawn point after multiple attempts.");
        return transform.position;
    }
}
