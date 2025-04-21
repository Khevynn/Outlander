using UnityEngine;

public class IdleState : EnemyState
{
    [Header("Random State Changing")]
    private float _maxStateChangingCooldown = 2f;
    private float _currentStateChangingCooldown = 0f;
    private int _chanceOfChangingToPatrol = 20;
    
    public IdleState() : base()
    {
        Name = State.Idle;
    }

    protected override void Enter()
    {
        base.Enter();
        Agent.ResetPath();
        _currentStateChangingCooldown = _maxStateChangingCooldown;
    }
    protected override void Update()
    {
        base.Update();
        
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
