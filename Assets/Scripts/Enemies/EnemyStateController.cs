using UnityEngine;
using UnityEngine.AI;

public enum State
{
    Idle,
    Patrolling,
    Pursuing,
    Attacking,
    Fleeing
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
    protected Animator NpcAnimator { get; private set; }
    protected GameObject NpcGameObject { get; private set; }
    protected EnemyController NpcController { get; private set; }
    protected LayerMask GroundLayer { get; private set; }
    protected Transform PlayerTransform { get; private set; }

    protected HealthComponent StatsController;
    //Constructors
    public EnemyState()
    {
        CurrentEvent = StateEvent.Enter;
    }
    
    public EnemyState Process()
    {
        if (NpcController.GetIsDead() || !NpcGameObject.activeSelf)
            return this;
        
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
    protected virtual void Update()
    {
        if(Agent.hasPath)
            TurnEnemyToTarget();
        
        if(Agent.velocity.magnitude >= 0.2f)
            NpcAnimator.SetBool("isWalking", true);
        
        NpcAnimator.SetFloat("CurrentSpeed", Agent.speed);
    }
    protected virtual void Exit() { CurrentEvent = StateEvent.Enter; }
    
    public void SetPlayerTransform(Transform player)
    {
        PlayerTransform = player;
    }
    public void SetStatsController(HealthComponent controller)
    {
        StatsController = controller;
        StatsController.onGetHit.RemoveAllListeners();
        StatsController.onGetHit.AddListener(OnGetHit);
    }
    public void SetAnimator(Animator anim)
    {
        NpcAnimator = anim;
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

    protected void OnGetHit()
    {
        if(Name == State.Idle || Name == State.Patrolling)
            ChangeToState(new PursuingState());
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
                ChangeToState(new AttackingState());
                break;
            case State.Fleeing:
                ChangeToState(new FleeingState());
                break;
            default:
                break;
        }

        return true;
    }
    protected void ChangeToState(EnemyState state)
    {
        state.SetPlayerTransform(PlayerTransform);
        state.SetStatsController(StatsController);
        state.SetAnimator(NpcAnimator);
        state.SetNpcGameObject(NpcGameObject);
        state.SetNavMeshAgent(Agent);
        state.SetGroundLayer(GroundLayer);
        
        NextState = state;
        CurrentEvent = StateEvent.Exit;
    }
    
    protected void TurnEnemyToTarget()
    {
        Vector3 direction = Agent.pathEndPosition - NpcGameObject.transform.position; //Calculate Direction
        direction.y = 0;

        var rotation = Quaternion.LookRotation(direction); //Gets the angle

        NpcGameObject.transform.rotation = Quaternion.Slerp(NpcGameObject.transform.rotation, rotation, Time.deltaTime * 30); //Rotate towards the player
    }
}
