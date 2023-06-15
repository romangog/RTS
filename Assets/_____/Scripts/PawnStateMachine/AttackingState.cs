using UnityEngine;

public class AttackingState : PawnState
{
    private readonly PawnInterStateData _interStateData;
    private readonly PawnController.Settings _settings;
    private readonly PawnView _view;

    public override PawnStateType Type => PawnStateType.Attacking;

    private float _attackCD;

    public AttackingState(PawnFacade facade)
    {
        _interStateData = facade.InterStateData;
        _settings = facade.Settings;
        _view = facade.View;
    }

    public override void Start()
    {
        _interStateData.PawnStateType = this.Type;
        _view.Agent.destination = _view.transform.position;
        _view.Agent.enabled = false;
        _view.Obstacle.enabled = true;
    }

    public override void Stop()
    {
        _view.Obstacle.enabled = false;
        _view.Agent.enabled = true;
    }

    public override void Update()
    {
        if (_interStateData.TargetEnemyPawn.IsDead)
        {
            CalledForStateChangeEvent(PawnStateType.Idle);
            return;
        }

        float distanceToEnemy = Vector3.Distance(_view.transform.position, _interStateData.TargetEnemyPawn.Position);
        if (distanceToEnemy >= _settings.AttackRadius)
        {
            CalledForStateChangeEvent(PawnStateType.MovingAttack);
            return;
        }

        _view.transform.rotation = Quaternion.LookRotation(_interStateData.TargetEnemyPawn.Position - _view.transform.position);
        _attackCD = Mathf.MoveTowards(_attackCD, 0f, Time.deltaTime);
        if (_attackCD == 0f)
        {
            AttackClosestEnemy();
            _attackCD = _settings.AttackCD;
        }

    }

    private void AttackClosestEnemy()
    {
        _interStateData.TargetEnemyPawn.RecieveDamage(_settings.AttackDamage);
        _view.AttackParticles.Play();
    }
}
