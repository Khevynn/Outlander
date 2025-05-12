using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[Serializable]
public enum EnemyType { Dog = 0, LandBeetle = 1, FlyingBeetle = 2, Spider = 3 }

[Serializable]
public class EnemyGroupSize
{
    public EnemyType type;
    public int min = 1;
    public int max = 1;
}

public class EnemyController : MonoBehaviour
{
    #region Fields & Components

    [Header("References")]
    [SerializeField] private NavMeshAgent meshAgent;
    [SerializeField] private Animator animator;
    [SerializeField] private Collider enemyCollider;
    [SerializeField] private LayerMask groundLayer;

    private Transform playerTransform;
    private HealthComponent playerStats;
    private HealthComponent enemyStats;
    private DropItemsComponent _dropItemsComponent;
    private EnemyState currentState;
    
    [Header("Hit Effect")]
    [SerializeField] private List<SkinnedMeshRenderer> meshRenderers;
    [SerializeField] private Color hitEffectColor;
    [SerializeField] private float hitEffectDuration;
    [SerializeField] private float hitEffectMultiplier;

    [Header("Grouping Control")]
    [SerializeField] private EnemyType enemyType;
    public bool isGroupable;
    [HideInInspector] public bool isEnemyBeingCalled;
    [HideInInspector] public bool isAttacking;

    [SerializeField] private float groupingRadius;
    [SerializeField] private LayerMask enemiesLayer;
    [SerializeField] private List<EnemyController> currentGroup = new();

    [Header("Velocity Control")]
    [SerializeField] private float patrollingVelocity = 2f;
    [SerializeField] private float pursueVelocity = 4f;
    [SerializeField] private float fleeVelocity = 3f;

    [Header("Fleeing Control")]
    [SerializeField] private float minFleeingDistance = 3f;

    [Header("FOV Control")]
    [SerializeField] private float maxVisionRange;
    [SerializeField] private float maxVisionAngle;

    [Header("Attack Control")]
    [SerializeField] private float attackMaxCooldown = 2f;
    [SerializeField] private float attackDamage = 2f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private Transform projectileSpawnLocation;

    private GameObject generatedProjectile;
    private float _currentAttackCooldown = 0f;

    #endregion

    #region Unity Events

    private void Awake()
    {
        meshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        _dropItemsComponent = GetComponent<DropItemsComponent>();
        enemyStats = GetComponent<HealthComponent>();
        enemyCollider = GetComponent<Collider>();

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        playerStats = playerTransform.GetComponent<HealthComponent>();

        NavMesh.avoidancePredictionTime = 3;
        SetupCurrentState();
        ActivateEnemy();
    }

    private void Update()
    {
        currentState = currentState.Process();

        if (_currentAttackCooldown > 0)
            _currentAttackCooldown -= Time.deltaTime;
    }

    #endregion

    #region Activation & Grouping

    /// <summary>
    /// Prepares the enemy for activation, including health and grouping logic.
    /// </summary>
    public void ActivateEnemy()
    {
        if (isGroupable)
            GenerateGroup();

        currentGroup.Clear();
        enemyStats.Revive();
    }

    /// <summary>
    /// Creates a group with nearby similar enemies.
    /// </summary>
    private void GenerateGroup()
    {
        var enemiesAround = Physics.OverlapSphere(transform.position, groupingRadius, enemiesLayer);

        foreach (var enemy in enemiesAround)
        {
            if (enemy.gameObject == this.gameObject || !enemy.TryGetComponent(out EnemyController enemyController))
                continue;

            if (enemyController.isGroupable && enemyController.GetEnemyType() == this.enemyType)
            {
                currentGroup.Add(enemyController);
                enemyController.AddEnemyToGroup(this);
            }
        }
    }

    /// <summary>
    /// Adds an enemy to this enemy's group.
    /// </summary>
    public void AddEnemyToGroup(EnemyController enemy)
    {
        if (!currentGroup.Contains(enemy))
            currentGroup.Add(enemy);
    }

    #endregion

    #region Combat & Attack

    /// <summary>
    /// Initiates the attack animation and cooldown.
    /// </summary>
    public void Attack()
    {
        _currentAttackCooldown = attackMaxCooldown;
        isAttacking = true;
        animator.Play("Attack");
    }
    /// <summary>
    /// Resets attack state after animation.
    /// </summary>
    public void FinishAttack() => isAttacking = false;

    /// <summary>
    /// Deals damage to the player.
    /// </summary>
    private void DealDamage() => playerStats.TakeDamage(attackDamage);

    /// <summary>
    /// Spawns a projectile at the shoot point.
    /// </summary>
    private void GenerateProjectile()
    {
        generatedProjectile = EnemyProjectilesPool.Instance.GetProjectileFromPool();
        generatedProjectile.transform.position = projectileSpawnLocation.position;
    }
    /// <summary>
    /// Shoots projectile toward player.
    /// </summary>
    private void PerformRangedAttack()
    {
        if (generatedProjectile.TryGetComponent(out Rigidbody rb))
        {
            var direction = playerTransform.position - projectileSpawnLocation.position;
            rb.AddForce(direction * 500);
        }

        Invoke("ReturnProjectile", 3);
    }
    /// <summary>
    /// Returns projectile to pool.
    /// </summary>
    private void ReturnProjectile()
    {
        EnemyProjectilesPool.Instance.ReturnProjectileToPool(generatedProjectile);
        generatedProjectile = null;
    }

    public void PlayHitEffect()
    {
        StartCoroutine(HitEffect());
    }
    private IEnumerator HitEffect()
    {
        var block = new MaterialPropertyBlock();

        foreach (var skinnedMesh in meshRenderers)
        {
            skinnedMesh.GetPropertyBlock(block);
            block.SetColor("_BaseColor", hitEffectColor * hitEffectMultiplier);
            skinnedMesh.SetPropertyBlock(block);
        }

        yield return new WaitForSeconds(hitEffectDuration);

        foreach (var skinnedMesh in meshRenderers)
        {
            skinnedMesh.GetPropertyBlock(block);
            block.SetColor("_BaseColor", Color.white); // or original color if you store it
            skinnedMesh.SetPropertyBlock(block);
        }
    }

    #endregion

    #region Fleeing & Vision

    /// <summary>
    /// Sends the enemy fleeing away from the player.
    /// </summary>
    public void Flee()
    {
        float randomX = Random.Range(-attackRange * 2, -attackRange);
        float randomZ = Random.Range(-attackRange * 2, -attackRange);

        var direction = transform.position - playerTransform.position;
        meshAgent.SetDestination(transform.position + (direction * 2) + new Vector3(randomX, 0, randomZ));
    }

    /// <summary>
    /// Determines whether the enemy can currently see the player.
    /// </summary>
    public bool CanSeePlayer()
    {
        Vector3 direction = playerTransform.position - transform.position;
        float angle = Vector3.Angle(direction, transform.forward);

        return direction.magnitude < GetMaxVisionRange() && angle < GetMaxVisionAngle();
    }

    #endregion

    #region Death
    /// <summary>
    /// Triggers enemy death logic and starts return-to-pool coroutine.
    /// </summary>
    public void CallDeath()
    {
        meshAgent.ResetPath();
        animator.SetBool("isDead", true);
        enemyCollider.enabled = false;

        if (generatedProjectile)
        {
            ReturnProjectile();
        }

        CallDropItems();
        StartCoroutine(ReturnToPool());
    }

    private void CallDropItems()
    {
        if (_dropItemsComponent != null)
            _dropItemsComponent.DropItems();
    }

    private IEnumerator ReturnToPool()
    {
        enemyCollider.enabled = true;
        yield return new WaitForSeconds(2f);
        EnemiesPool.Instance.ReturnEnemyToPool(this);
    }
    #endregion

    #region Group Call

    /// <summary>
    /// Notifies grouped enemies to pursue the player.
    /// </summary>
    public void CallGroup()
    {
        if (enemyStats.IsDead) return;

        foreach (var enemy in currentGroup)
        {
            if (enemy.enemyStats.IsDead) continue;

            enemy.isEnemyBeingCalled = true;
            enemy.meshAgent.SetDestination(playerTransform.position);
        }
    }

    #endregion

    #region Setup & Getters

    private void SetupCurrentState()
    {
        currentState = new IdleState();
        currentState.SetPlayerTransform(playerTransform);
        currentState.SetStatsController(enemyStats);
        currentState.SetAnimator(animator);
        currentState.SetNavMeshAgent(meshAgent);
        currentState.SetNpcGameObject(gameObject);
        currentState.SetGroundLayer(groundLayer);
    }

    public EnemyType GetEnemyType() => enemyType;
    public float GetMaxVisionRange() => maxVisionRange;
    public float GetMaxVisionAngle() => maxVisionAngle;
    public float GetAttackRange() => attackRange;
    public float GetAttackCooldown() => _currentAttackCooldown;
    public float GetMinFleeDistance() => minFleeingDistance;
    public float GetPatrollingVelocity() => patrollingVelocity;
    public float GetPursueVelocity() => pursueVelocity;
    public float GetFleeVelocity() => fleeVelocity;
    public bool GetIsDead() => enemyStats.IsDead;

    #endregion
}
