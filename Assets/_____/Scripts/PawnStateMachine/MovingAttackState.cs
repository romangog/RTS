using UnityEngine;

public class MovingAttackState : PawnState
{
    private readonly PawnView _view;
    private readonly PawnController.Settings _settings;
    private readonly EnemiesLocator _enemiesLocator;
    private readonly PawnInterStateData _interStateData;

    public override PawnStateType Type => PawnStateType.MovingAttack;

    public MovingAttackState(PawnFacade facade)
    {
        _interStateData = facade.InterStateData;
        _view = facade.View;
        _settings = facade.Settings;
        _enemiesLocator = facade.EnemiesLocator;
    }

    private void OnFoundClosestEnemy(PawnController closestEnemy)
    {
        if (_interStateData.TargetEnemyPawn != null)
        {
            _interStateData.TargetEnemyPawn.InterStateData.ApproachingEnemies--;
        }
        closestEnemy.InterStateData.ApproachingEnemies++;
        _interStateData.TargetEnemyPawn = closestEnemy;
    }

    public override void Start()
    {
        _enemiesLocator.FoundClosestEnemyEvent += OnFoundClosestEnemy;
        _interStateData.PawnStateType = this.Type;
    }


    public override void Stop()
    {
        _enemiesLocator.FoundClosestEnemyEvent -= OnFoundClosestEnemy;

    }

    public override void Update()
    {
        if (_interStateData.TargetEnemyPawn.IsDead)
        {
            CalledForStateChangeEvent(PawnStateType.Idle);
            return;
        }
        _enemiesLocator.Update();
        _view.Agent.destination = _interStateData.TargetEnemyPawn.Position;

        // UpdateClosestEnemy
        float distanceToEnemy = Vector3.Distance(_view.transform.position, _interStateData.TargetEnemyPawn.Position);
        if (distanceToEnemy < _settings.AttackRadius)
        {
            CalledForStateChangeEvent(PawnStateType.Attacking);
        }
    }
}
