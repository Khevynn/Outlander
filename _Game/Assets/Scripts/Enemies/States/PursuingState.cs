using UnityEngine;

public class PursuingState : EnemyState
{
    private float _maxCooldownToPerformCallGroup = 1f;
    private float _currentCooldownToPerformCallGroup;

    private Vector3 lastRandomPoint;
    private float _maxTimeBetweenRandomPoints = 1;
    private float _currentTimeBetweenRandomPoints;
    
    public PursuingState() : base()
    {
        Name = State.Pursuing;
    }

    protected override void Enter()
    {
        base.Enter();
        _currentCooldownToPerformCallGroup = 0f;
        Agent.speed = NpcController.GetPursueVelocity();
        
        Agent.SetDestination(PlayerTransform.position);
        TryCallGroup();
    }

    protected override void Update()
    {
        base.Update();
        _currentCooldownToPerformCallGroup -= Time.deltaTime;
        _currentTimeBetweenRandomPoints -= Time.deltaTime;
        
        if (Vector3.Distance(NpcGameObject.transform.position, PlayerTransform.position) <= NpcController.GetAttackRange() && NpcController.GetAttackCooldown() <= 0)
        {
            ChangeToState(new AttackingState());
        }
        
        if (NpcController.CanSeePlayer())
        {
            NpcController.isEnemyBeingCalled = false;
            
            lastRandomPoint = GetRandomPath();
            Agent.SetDestination(lastRandomPoint);
            
            TryCallGroup();
        }
        else if (Vector3.Distance(NpcGameObject.transform.position, Agent.pathEndPosition) <= Agent.stoppingDistance)
        {
            NpcController.isEnemyBeingCalled = false;
            ChangeToState(new PatrollingState());
        }
    }

    private void TryCallGroup()
    {
        if (_currentCooldownToPerformCallGroup > 0)
            return;
        
        NpcController.CallGroup();
        _currentCooldownToPerformCallGroup = _maxCooldownToPerformCallGroup;
    }
    
    private Vector3 GetRandomPath()
    {
        if (_currentTimeBetweenRandomPoints > 0 && Vector3.Distance(NpcGameObject.transform.position, Agent.pathEndPosition) > Agent.stoppingDistance)
            return lastRandomPoint;
        
        // Calculate random point in range
        float randomX = Random.Range(-2, 2);
        float randomZ = Random.Range(-2, 2);
        _currentTimeBetweenRandomPoints = _maxTimeBetweenRandomPoints;
        return PlayerTransform.position + new Vector3(randomX, 0, randomZ);
    }
}
