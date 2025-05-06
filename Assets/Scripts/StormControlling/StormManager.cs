using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using VolumetricFogAndMist2;
using Random = UnityEngine.Random;

public class StormManager : MonoBehaviour
{
    private struct EnemyWeights
    {
        public float Dog;
        public float Spider;
        public float LandBeetle;
        public float FlyingBeetle;
    }

    public static StormManager Instance;

    #region References
    [Header("References")] 
    [SerializeField] private VolumetricFogProfile stormProfile;
    #endregion

    #region Storm Settings
    [Header("Storm Generation")]
    [SerializeField] private float maxTimeBetweenStorms = 60f;
    [SerializeField] private float timeToStartStorm = 10f;
    [SerializeField] private float timeToFinishStorm = 8f;
    [SerializeField] private float maxStormDuration = 45f;

    private float lastStartedStormTime;
    private float currentTimeBetweenStorms;
    private bool startingStorm;
    private bool currentlyInStorm;
    private bool finishingStorm;
    private Coroutine startStormCoroutine;
    #endregion

    #region Enemy Wave Settings
    [Header("Enemy Groups Control")]
    [SerializeField] private List<EnemyGroupSize> groupSizes = new();

    [Header("Enemy Spawning")]
    [SerializeField] private int numberOfWavesToSpawnIfSkipped = 2;
    [SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private int maxActiveEnemies = 20;
    [SerializeField] private float spawnRadius = 20;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Minimum waves for each enemy")]
    [SerializeField] private int minWaveForDogs = 0;
    [SerializeField] private int minWaveForSpiders = 2;
    [SerializeField] private int minWaveForLandBeetles = 3;
    [SerializeField] private int minWaveForFlyingBeetles = 5;

    [Header("Wave Settings")]
    [SerializeField] private int currentWave = 0;
    [SerializeField] private float baseEnemiesPerWave = 3f;
    [SerializeField] private float enemyGrowthRate = 1.15f; // 15% more enemies per wave
    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance)
        {
            Debug.Log("StormManager already exists, destroying new one");
            Destroy(this);
        }
        else
            Instance = this;
    }

    private void Start()
    {
        stormProfile.noiseFinalMultiplier = 0f;
        currentTimeBetweenStorms = maxTimeBetweenStorms;
    }
    private void FixedUpdate()
    {
        // Count down to the next storm
        if (!currentlyInStorm && !finishingStorm && !startingStorm && currentTimeBetweenStorms > 0f)
            currentTimeBetweenStorms -= Time.fixedDeltaTime;

        // Start a storm when the timer reaches zero
        if (!currentlyInStorm && !startingStorm && !finishingStorm && currentTimeBetweenStorms <= 0f)
            startStormCoroutine = StartCoroutine(StartStorm());
    }
    #endregion

    #region Storm Logic
    /// <summary>
    /// Starts the storm sequence by increasing fog, enabling waves, and scheduling storm end.
    /// </summary>
    private IEnumerator StartStorm()
    {
        startingStorm = true;
        lastStartedStormTime = Time.time;

        // Ramp up storm intensity visually
        yield return LerpStormMultiplier(stormProfile.noiseFinalMultiplier, 1.3f, timeToStartStorm);

        startingStorm = false;
        currentlyInStorm = true;

        MainWaveLoop(); // Begin spawning enemy waves

        yield return new WaitForSeconds(maxStormDuration); // Wait for storm to end

        StartCoroutine(FinishStorm());
    }
    /// <summary>
    /// Finishes the storm by reducing fog and resetting timers.
    /// </summary>
    private IEnumerator FinishStorm()
    {
        print("Finishing");
        currentlyInStorm = false;
        finishingStorm = true;

        // Ramp down storm visuals
        yield return LerpStormMultiplier(stormProfile.noiseFinalMultiplier, 0f, timeToFinishStorm);

        // Reset storm cycle
        currentTimeBetweenStorms = maxTimeBetweenStorms;
        finishingStorm = false;
    }

    public  void SkipStorm()
    {
        if (!startingStorm || Time.time - lastStartedStormTime > timeToStartStorm / 2)
            return;
        
        //TODO: Run an UI animation and then do all that stuff
        startingStorm = false;
        if (startStormCoroutine != null)
        {
            StopCoroutine(startStormCoroutine);
            startStormCoroutine = null;
        }
        
        for (int i = 0; i < numberOfWavesToSpawnIfSkipped; ++i)
        {
            CreateEnemyWave(Mathf.Pow(enemyGrowthRate, currentWave - 1));
        }

        stormProfile.noiseFinalMultiplier = 0;
    }

    /// <summary>
    /// Gradually interpolates the storm fog intensity over time.
    /// </summary>
    /// <param name="from">Starting multiplier value.</param>
    /// <param name="to">Target multiplier value.</param>
    /// <param name="duration">Duration of the transition in seconds.</param>
    private IEnumerator LerpStormMultiplier(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            stormProfile.noiseFinalMultiplier = Mathf.Lerp(from, to, t); // Smooth transition
            yield return null;
        }

        stormProfile.noiseFinalMultiplier = to;
    }
    
    /// <summary>
    /// Returns the current normalized intensity of the storm (0 to 1).
    /// </summary>
    private float GetStormIntensity()
    {
        return Mathf.InverseLerp(0f, 1.3f, stormProfile.noiseFinalMultiplier);
    }
    #endregion

    #region Wave Logic
    /// <summary>
    /// Initiates a new wave if conditions allow, and schedules the next wave.
    /// </summary>
    private void MainWaveLoop()
    {
        // Stop spawning if storm is off or if the enemies are at max, it just calls the function again after a few seconds
        if (!currentlyInStorm)
            return;
        if (EnemiesPool.Instance.GetNumberOfCurrentlyActiveEnemies() >= maxActiveEnemies)
        {
            Invoke(nameof(MainWaveLoop), timeBetweenWaves); // Schedule next wave
            return;
        }

        float scaleFactor = Mathf.Pow(enemyGrowthRate, currentWave); // Progressive difficulty
        CreateEnemyWave(scaleFactor); // Spawn enemies for this wave

        Invoke(nameof(MainWaveLoop), timeBetweenWaves); // Schedule next wave
    }
    
    /// <summary>
    /// Spawns enemies in a wave based on a scaling factor and group logic.
    /// </summary>
    /// <param name="scaleFactor">The number of enemies to spawn is multiplied by this value.</param>
    private void CreateEnemyWave(float scaleFactor)
    {
        int totalToSpawn = Mathf.CeilToInt(baseEnemiesPerWave * scaleFactor); // Scale wave size

        while (totalToSpawn > 0)
        {
            EnemyType type = GetRandomEnemyType();
            int groupSize = Mathf.Min(GetGroupSizeForType(type), totalToSpawn); // Clamp to remaining quota
            
            Vector3 spawnPoint = GetRandomSpawnPoint();
            
            for (int i = 0; i < groupSize; i++)
            {
                EnemyController enemy = EnemiesPool.Instance.GetEnemyOfType(type);
                var randomSpawnPointOscillation = new Vector3(Random.Range(-2, 2), spawnPoint.y, Random.Range(-2, 2));

                enemy.transform.position = spawnPoint + randomSpawnPointOscillation;
                enemy.transform.rotation = Quaternion.identity;
            }

            totalToSpawn -= groupSize;
        }

        currentWave++; // Track current wave number
        print($"Wave {currentWave}: Enemies spawned.");
    }
    #endregion

    #region Enemy Spawn Logic
    /// <summary>
    /// Returns a random enemy type based on weighted probabilities affected by storm intensity and wave progression.
    /// </summary>
    private EnemyType GetRandomEnemyType()
    {
        float intensity = GetStormIntensity();
        float difficultyProgress = Mathf.Clamp01(currentWave / 20f);

        var weights = CalculateEnemyWeights(intensity, difficultyProgress);
        
        // Disable certain enemies early in the game
        if (currentWave < minWaveForDogs) weights.Dog = 0f;
        if (currentWave < minWaveForSpiders) weights.Spider = 0f;
        if (currentWave < minWaveForLandBeetles) weights.LandBeetle = 0f;
        if (currentWave < minWaveForFlyingBeetles) weights.FlyingBeetle = 0f;
        
        NormalizeWeights(ref weights);

        return SelectEnemyFromWeights(weights);
    }
    
    /// <summary>
    /// Calculates raw enemy type weights based on current storm intensity and wave progress.
    /// </summary>
    /// <param name="intensity">Current storm intensity (0 to 1).</param>
    /// <param name="progress">Progress through the waves (0 to 1).</param>
    /// <returns>Struct containing unnormalized enemy weights.</returns>
    private EnemyWeights CalculateEnemyWeights(float intensity, float progress)
    {
        return new EnemyWeights
        {
            Dog = Mathf.Lerp(0.5f, 0.2f, progress),                           // Fewer dogs later
            Spider = Mathf.Lerp(0.2f, 0.3f, progress),                        // More spiders later
            LandBeetle = Mathf.Lerp(0.15f, 0.3f, intensity),                  // Stronger storms = more beetles
            FlyingBeetle = Mathf.Lerp(0.15f, 0.2f + progress * 0.1f, intensity) // Harder & stormier = more flyers
        };
    }
    
    /// <summary>
    /// Normalizes the weight values so their total equals 1.
    /// </summary>
    /// <param name="weights">Reference to enemy weights to normalize.</param>
    private void NormalizeWeights(ref EnemyWeights weights)
    {
        float total = weights.Dog + weights.Spider + weights.LandBeetle + weights.FlyingBeetle;

        weights.Dog /= total;
        weights.Spider /= total;
        weights.LandBeetle /= total;
        weights.FlyingBeetle /= total;
    }
    
    /// <summary>
    /// Selects a random enemy type using the provided normalized weights.
    /// </summary>
    /// <param name="weights">Normalized weights representing spawn probability of each enemy type.</param>
    /// <returns>Chosen EnemyType based on weighted random selection.</returns>
    private EnemyType SelectEnemyFromWeights(EnemyWeights weights)
    {
        float rand = Random.value;

        if (rand < weights.Dog) return EnemyType.Dog;
        if (rand < weights.Dog + weights.Spider) return EnemyType.Spider;
        if (rand < weights.Dog + weights.Spider + weights.FlyingBeetle) return EnemyType.FlyingBeetle;
        return EnemyType.LandBeetle;
    }
    
    /// <summary>
    /// Returns a random group size for the specified enemy type based on inspector-defined ranges.
    /// </summary>
    /// <param name="type">The enemy type to evaluate.</param>
    /// <returns>A random integer between min and max group size.</returns>
    private int GetGroupSizeForType(EnemyType type)
    {
        EnemyGroupSize group = groupSizes.Find(g => g.type == type);
        if (group != null)
            return Random.Range(group.min, group.max + 1); // Unity's Range max is exclusive

        Debug.LogWarning($"No group size defined for {type}, defaulting to 1.");
        return 1;
    }

    /// <summary>
    /// Calculates and returns a random spawn point on the ground within the spawn radius.
    /// </summary>
    /// <returns>A valid world position on the ground where an enemy can be spawned.</returns>
    private Vector3 GetRandomSpawnPoint()
    {
        const int maxAttempts = 30;
        const float minDistance = 5f;

        List<Vector3> spawnedPositions = new();  // Track accepted spawn points

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Generate a random point above the ground within the spawn radius
            float offsetX = Random.Range(-spawnRadius, spawnRadius);
            float offsetZ = Random.Range(-spawnRadius, spawnRadius);
            Vector3 rayOrigin = new(transform.position.x + offsetX, transform.position.y + 10f, transform.position.z + offsetZ);

            // Cast a ray down to find the ground
            if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit groundHit, 100f, groundLayer))
                continue;

            Vector3 candidatePoint = groundHit.point;

            // Check if the candidate is far enough from previous spawn points
            bool tooCloseToOthers = spawnedPositions.Any(pos => Vector3.Distance(candidatePoint, pos) < minDistance);
            if (tooCloseToOthers)
                continue;

            // Ensure the point is on the NavMesh
            if (!NavMesh.SamplePosition(candidatePoint, out NavMeshHit navHit, 1f, NavMesh.AllAreas))
                continue;

            spawnedPositions.Add(candidatePoint);  // Remember this point
            return navHit.position;
        }

        Debug.LogError("Could not find valid spawn point after multiple attempts.");
        return transform.position;
    }
    #endregion
}