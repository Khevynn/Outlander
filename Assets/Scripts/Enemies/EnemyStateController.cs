using UnityEngine;
using UnityEngine.AI;

public enum State
{
    Idle,
    Patrolling,
    Pursuing,
    Attacking
}
public enum StateEvent
{
    Enter,
    Update,
    Exit
}

public class EnemyState
{
    [Header("State Control")]
    public State Name;
    protected StateEvent CurrentEvent;
    protected EnemyState NextState;

    [Header("References")] 
    protected NavMeshAgent Agent { get; private set; }
    protected GameObject NpcGameObject { get; private set; }
    protected EnemyController NpcController { get; private set; }
    protected LayerMask GroundLayer { get; private set; }
    protected Transform PlayerTransform { get; private set; }

    //Constructors
    public EnemyState()
    {
        CurrentEvent = StateEvent.Enter;
    }
    
    public EnemyState Process()
    {
        if (CurrentEvent == StateEvent.Enter) Enter();
        if (CurrentEvent == StateEvent.Update) Update();
        if (CurrentEvent == StateEvent.Exit)
        {
            Exit();
            return NextState;
        }
        return this;
    }
    
    protected virtual void Enter() { CurrentEvent = StateEvent.Update; }
    protected virtual void Update() {  }
    protected virtual void Exit() { CurrentEvent = StateEvent.Enter; }
    
    public void SetPlayerTransform(Transform player)
    {
        PlayerTransform = player;
    }
    public void SetNpcGameObject(GameObject npc)
    {
        NpcGameObject = npc;
        NpcGameObject.TryGetComponent(out EnemyController enemyController);
        NpcController = enemyController;
    }
    public void SetNavMeshAgent(NavMeshAgent agent)
    {
        Agent = agent;
    }
    public void SetGroundLayer(LayerMask layer)
    {
        GroundLayer = layer;
    }
    
    protected bool TryToChangeState(State nextState, float chanceOfChangingState)
    {
        bool changeState = Random.Range(0, 100) < chanceOfChangingState;
        if(!changeState)
            return false;

        switch (nextState)
        {
            case State.Idle:
                ChangeToState(new IdleState());
                break;
            case State.Patrolling:
                ChangeToState(new PatrollingState());
                break;
            case State.Pursuing:
                ChangeToState(new PursuingState());
                break;
            case State.Attacking:
                break;
            default:
                break;
        }

        return true;
    }
    protected void ChangeToState(EnemyState state)
    {
        state.SetPlayerTransform(PlayerTransform);
        state.SetNpcGameObject(NpcGameObject);
        state.SetNavMeshAgent(Agent);
        state.SetGroundLayer(GroundLayer);
        
        NextState = state;
        CurrentEvent = StateEvent.Exit;
    }

    protected bool CanSeePlayer()
    {
        Vector3 direction = PlayerTransform.position - NpcGameObject.transform.position;
        float angle = Vector3.Angle(direction, NpcGameObject.transform.forward);

        return direction.magnitude < NpcController.GetMaxVisionRange() && angle < NpcController.GetMaxVisionAngle();
    }
}
