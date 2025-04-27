using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[Serializable]
public enum EnemyType {Dogs, LandBeetles, FlyingBeetles, Spiders}

public class EnemyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent meshAgent;
    [SerializeField] private Animator animator;
    [SerializeField] private LayerMask groundLayer;
    private Transform playerTransform;
    private PlayerStatsController playerStats;
    private EnemyState currentState;
    
    [Header("Grouping Control")]
    [SerializeField] private EnemyType enemyType;

    public bool isGroupable;
    [HideInInspector] public bool isEnemyBeingCalled;
    [HideInInspector] public bool isAttacking;

    [SerializeField] private float groupingRadius;
    [SerializeField] private LayerMask enemiesLayer;
    [SerializeField] private List<EnemyController> currentGroup = new List<EnemyController>();
    
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

    public bool IsDead { get; private set; }

    private void Start()
    {
        meshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        playerStats = playerTransform.gameObject.GetComponent<PlayerStatsController>();
        
        NavMesh.avoidancePredictionTime = 3;

        if(isGroupable)
            GenerateGroup();
        SetupCurrentState();
    }
    private void Update() 
    {
        currentState = currentState.Process();

        if (_currentAttackCooldown > 0)
        {
            _currentAttackCooldown -= Time.deltaTime;
        }
    }

    public void Attack()
    {
        _currentAttackCooldown = attackMaxCooldown;
        isAttacking = true;
        
        animator.Play("Attack");
    }
    public void FinishAttack()
    {
        isAttacking = false;
    }

    private void DealDamage()
    {
        playerStats.TakeDamage(attackDamage);
    }
    
    private void GenerateProjectile()
    {
        generatedProjectile = EnemyProjectilesPool.Instance.GetProjectileFromPool();
        generatedProjectile.transform.position = projectileSpawnLocation.position;
    }
    private void PerformRangedAttack()
    {
        generatedProjectile.TryGetComponent(out Rigidbody rb);
        var direction = playerTransform.position - projectileSpawnLocation.position;
        rb.AddForce(direction * 500);
        
        Invoke("ReturnProjectile", 3);
    }
    private void ReturnProjectile()
    {
        EnemyProjectilesPool.Instance.ReturnProjectileToPool(generatedProjectile);
        generatedProjectile = null;
    }
    
    public void Flee()
    {
        // Calculate random point in range
        float randomX = Random.Range(-attackRange * 2, -attackRange);
        float randomZ = Random.Range(-attackRange * 2, -attackRange);

        var direction = transform.position - playerTransform.position;
        meshAgent.SetDestination( transform.position + (direction * 2) + new Vector3(randomX, 0, randomZ));
    }
    
    public EnemyType GetEnemyType()
    {
        return enemyType;
    }
    
    public float GetMaxVisionRange()
    {
        return maxVisionRange;
    }
    public float GetMaxVisionAngle()
    {
        return maxVisionAngle;
    }
    public float GetAttackRange()
    {
        return attackRange;
    }
    public float GetAttackCooldown()
    {
        return _currentAttackCooldown;
    }
    public float GetMinFleeDistance()
    {
        return minFleeingDistance;
    }
    
    public float GetPatrollingVelocity()
    {
        return patrollingVelocity;
    }
    public float GetPursueVelocity()
    {
        return pursueVelocity;
    }
    public float GetFleeVelocity()
    {
        return fleeVelocity;
    }
    
    public void CallDeath()
    {
        IsDead = true;
        
        meshAgent.ResetPath();
        meshAgent.enabled = false;
        
        animator.SetBool("isDead", true);
        StartCoroutine(ReturnToPool());
    }
    private IEnumerator ReturnToPool()
    {
        yield return new WaitForSeconds(2f);
        meshAgent.enabled = true;
        gameObject.SetActive(false);
    }
    
    private void SetupCurrentState()
    {
        currentState = new IdleState();
        currentState.SetPlayerTransform(playerTransform);
        currentState.SetStatsController(GetComponent<EnemyHpStatsControlller>());
        currentState.SetAnimator(animator);
        currentState.SetNavMeshAgent(meshAgent);
        currentState.SetNpcGameObject(this.gameObject);
        currentState.SetGroundLayer(groundLayer);
    }
    private void GenerateGroup()
    {
        var enemiesAround = Physics.OverlapSphere(transform.position, groupingRadius, enemiesLayer);
        foreach (var enemy in enemiesAround)
        {
            if (enemy.gameObject == this.gameObject || !enemy.gameObject.TryGetComponent(out EnemyController enemyController))
                continue;
            
            if(enemyController.isGroupable && enemyController.GetEnemyType() == this.enemyType)
                currentGroup.Add(enemyController);
        }
    }
    public bool CanSeePlayer()
    {
        Vector3 direction = playerTransform.position - transform.position;
        float angle = Vector3.Angle(direction, transform.forward);

        return direction.magnitude < GetMaxVisionRange() && angle < GetMaxVisionAngle();
    }
    public void CallGroup()
    {
        foreach (var enemy in currentGroup)
        {
            enemy.isEnemyBeingCalled = true;
            enemy.meshAgent.SetDestination(playerTransform.position);
        }
    }
}
