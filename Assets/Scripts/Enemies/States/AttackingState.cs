using UnityEngine;

public class AttackingState : EnemyState
{
    public AttackingState() : base()
    {
        Name = State.Attacking;
    }

    protected override void Enter()
    {
        base.Enter();
        Agent.speed = 0;        
        
        CallAttack();
    }

    protected override void Update()
    {
        base.Update();
        Agent.SetDestination(PlayerTransform.position);
        TurnEnemyToTarget();
        
        if(!NpcController.isAttacking)
            ChangeToState(new FleeingState());
    }
    
    private void CallAttack()
    {
        NpcController.Attack();
    }
}
