using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class EnemyController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private NavMeshAgent meshAgent;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("FOV Control")]
    [SerializeField] private float maxVisionRange;
    [SerializeField] private float maxVisionAngle;
    
    [Header("Attack Control")]
    [SerializeField] private float attackMaxCooldown = 2f;
    private float _attackCurrentCooldown = 0f;
    [SerializeField] private float attackDamage = 2f;
    [SerializeField] private float attackRange = 2f;
    
    private Transform playerTransform;
    private PlayerController playerController;
    private EnemyState currentState;

    void Start()
    {
        meshAgent = GetComponent<NavMeshAgent>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = playerTransform.gameObject.GetComponent<PlayerController>();
        SetupCurrentState();
    }

    void Update()
    {
        currentState = currentState.Process();
    }

    public void Attack()
    {
        if (_attackCurrentCooldown > 0)
            return;
        
        meshAgent.ResetPath();
        playerController.TakeDamage(attackDamage);
        _attackCurrentCooldown = attackMaxCooldown;
    }
    
    private void SetupCurrentState()
    {
        currentState = new IdleState();
        currentState.SetPlayerTransform(playerTransform);
        currentState.SetNavMeshAgent(meshAgent);
        currentState.SetNpcGameObject(this.gameObject);
        currentState.SetGroundLayer(groundLayer);
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
}
