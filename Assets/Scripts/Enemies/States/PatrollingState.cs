using UnityEngine;

public class PatrollingState : EnemyState
{
    [Header("Walk point Control")]
    private Vector3 walkPoint;
    private float walkPointRange = 8;
    private bool walkPointSet = false;
    
    private int _chanceOfChangingToIdle = 60;
    
    public PatrollingState() : base()
    {
        Name = State.Patrolling;
    }

    protected override void Enter()
    {
        base.Enter();
        SearchWalkPoint();
        Debug.Log("Entered Patrol State");
    }
    protected override void Update()
    {
        base.Update();
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet && !Agent.hasPath)
            Agent.SetDestination(walkPoint);
        
        if (Vector3.Distance(NpcGameObject.transform.position, walkPoint) < Agent.stoppingDistance)
        {
            if (TryToChangeState(State.Idle, _chanceOfChangingToIdle))
                return;
            
            SearchWalkPoint();
            walkPointSet = false;
        }

        if (CanSeePlayer())
        {
            ChangeToState(new PursuingState());
        }
    }
    
    private void SearchWalkPoint()
    {
        Agent.ResetPath();
        
        // Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(NpcGameObject.transform.position.x + randomX, NpcGameObject.transform.position.y, NpcGameObject.transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -NpcGameObject.transform.up, 2f, GroundLayer))
            walkPointSet = true;
    }
}
