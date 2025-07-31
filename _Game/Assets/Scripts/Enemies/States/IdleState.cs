using UnityEngine;

public class IdleState : EnemyState
{
    [Header("Random State Changing")]
    private float _maxStateChangingCooldown = 2f;
    private float _currentStateChangingCooldown = 0f;
    private int _chanceOfChangingToPatrol = 50;
    
    public IdleState() : base()
    {
        Name = State.Idle;
    }

    protected override void Enter()
    {
        base.Enter();
        _currentStateChangingCooldown = _maxStateChangingCooldown;
        
        Agent.ResetPath();
        Agent.speed = 0;
    }
    protected override void Update()
    {
        NpcAnimator.SetBool("isWalking", false);
        if (NpcController.CanSeePlayer() || NpcController.isEnemyBeingCalled)
        { 
            ChangeToState(new PursuingState());
        }
        
        if (_currentStateChangingCooldown <= 0)
        {
            TryToChangeState(State.Patrolling, _chanceOfChangingToPatrol);
            _currentStateChangingCooldown = _maxStateChangingCooldown;
        }
        else
        {
            _currentStateChangingCooldown -= Time.fixedDeltaTime;
        }
    }
}
