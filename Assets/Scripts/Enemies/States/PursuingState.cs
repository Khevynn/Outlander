using UnityEngine;

public class PursuingState : EnemyState
{
    public PursuingState() : base()
    {
        Name = State.Pursuing;
    }
    protected override void Update()
    {
        base.Update();
        if (!CanSeePlayer())
        {
            ChangeToState(new PatrollingState());
        }

        if (Vector3.Distance(NpcGameObject.transform.position, PlayerTransform.position) <= NpcController.GetAttackRange())
        {
            CallAttack();
        }
 
        Agent.SetDestination(PlayerTransform.position);
    }

    private void CallAttack()
    {
        NpcController.Attack();
    }
}
