using UnityEngine;

public class FleeingState : EnemyState
{
    public FleeingState() : base()
    {
        Name = State.Fleeing;
    }

    protected override void Enter()
    {
        base.Enter();
        Agent.speed = NpcController.GetFleeVelocity();
    }

    protected override void Update()
    {
        base.Update();
        
        NpcController.Flee();
        
        if (NpcController.GetAttackCooldown() <= 0f || Vector3.Distance(PlayerTransform.position, NpcGameObject.transform.position) > NpcController.GetMinFleeDistance())
        {
            ChangeToState(new PursuingState());
        }
    }
}
