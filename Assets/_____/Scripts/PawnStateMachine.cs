using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum PawnStateType
{
    Idle,
    Moving,
    Attacking,
    MovingAttack
}
public class PawnStateMachine
{
    private PawnState _currentState;

    private PawnState[] _states;

    public PawnStateMachine(PawnFacade facade)
    {
        IdleState idleState = new IdleState(
            facade);
        MovingState movingState = new MovingState(
    facade);
        AttackingState attackingState = new AttackingState(
    facade);
        MovingAttackState movingAttackState = new MovingAttackState(
    facade);

        _states = new PawnState[]
        {
            idleState,
            movingState,
            attackingState,
            movingAttackState
        };

        foreach (var state in _states)
        {
            state.CalledForStateChangeEvent += ChangeState;
        }
    }

    public void ChangeState(PawnStateType stateType)
    {
        if (_currentState != null && _currentState.Type != stateType)
        {
            _currentState.Stop();
        }
        _currentState = _states[(int)stateType];

        _currentState.Start();
    }

    public void Tick()
    {
        _currentState?.Update();
    }


}

public class PawnFacade
{
    public PawnFacade(
        List<PawnController> enemies,
        PawnController.Settings settings,
        PawnView view,
        PawnInterStateData interStateData)
    {
        Enemies = enemies;
        Settings = settings;
        View = view;
        InterStateData = interStateData;
        EnemiesLocator = new EnemiesLocator(enemies, settings, view, interStateData);
    }

    public List<PawnController> Enemies { get; }
    public PawnController.Settings Settings { get; }
    public PawnView View { get; }
    public EnemiesLocator EnemiesLocator { get; }

    public PawnInterStateData InterStateData { get; }

    public void Update()
    {
        EnemiesLocator.Update();
    }
}

public class PawnInterStateData
{
    public Action<PawnController> SpottedEnemyEvent;
    public PawnController TargetEnemyPawn { get; set; }
    public Vector3 TargetPosition { get; set; }
    public PawnStateType PawnStateType { get; set; }
    public int ApproachingEnemies { get; set; }
}

public class EnemiesLocator
{
    private readonly List<PawnController> _enemies;
    private readonly PawnController.Settings _pawnSettings;
    private readonly PawnView _view;
    private readonly PawnInterStateData _interStateData;
    private float _detectionCD;

    public Action<PawnController> FoundClosestEnemyEvent;

    public EnemiesLocator(
        List<PawnController> enemies,
        PawnController.Settings pawnSettings,
        PawnView view,
        PawnInterStateData interStateData)
    {
        _enemies = enemies;
        _pawnSettings = pawnSettings;
        _view = view;
        _interStateData = interStateData;
    }

    public PawnController GetClosestEnemyPawn()
    {
        if (_enemies.Count == 0) return null;
        float maxPrefertence = 0f;
        PawnController closestEnemyPawn = null;
        for (int i = 0; i < _enemies.Count; i++)
        {
            var enemy = _enemies[i];
            float distance = Vector3.Distance(enemy.Position, _view.transform.position);
            if (distance >= _pawnSettings.EnemiesDetectionRadius) continue;
            float approachingEnemiesDivider =
                (_interStateData.TargetEnemyPawn == enemy)
                ? (enemy.InterStateData.ApproachingEnemies)
                : (enemy.InterStateData.ApproachingEnemies + 1);

            float preference = (1f - distance / _pawnSettings.EnemiesDetectionRadius) / approachingEnemiesDivider;

            if (preference > maxPrefertence)
            {
                maxPrefertence = preference;
                closestEnemyPawn = enemy;
            }
        }
        return closestEnemyPawn;
    }

    internal void Update()
    {
        _detectionCD = Mathf.MoveTowards(_detectionCD, 0f, Time.deltaTime);
        if (_detectionCD == 0f)
        {
            _detectionCD = _pawnSettings.CheckEnemiesAroundCD;
            PawnController closestPawn = GetClosestEnemyPawn();
            if (closestPawn != null)
                FoundClosestEnemyEvent?.Invoke(closestPawn);
        }
    }
}
public class IdleState : PawnState
{
    public override PawnStateType Type => PawnStateType.Idle;

    private readonly PawnInterStateData _interStateData;
    private readonly PawnView _view;
    private readonly EnemiesLocator _enemiesLocator;
    private float _checkEnemiesAroundCd;

    public IdleState(PawnFacade facade)
    {
        _interStateData = facade.InterStateData;
        _view = facade.View;
        _enemiesLocator = facade.EnemiesLocator;
    }



    public override void Start()
    {
        _enemiesLocator.FoundClosestEnemyEvent += OnFoundClosestEnemy;
        _view.Agent.destination = _view.transform.position;
        _interStateData.PawnStateType = this.Type;
    }
    private void OnFoundClosestEnemy(PawnController closestEnemy)
    {
        if (_interStateData.TargetEnemyPawn != null)
        {
            _interStateData.TargetEnemyPawn.InterStateData.ApproachingEnemies--;
        }
        closestEnemy.InterStateData.ApproachingEnemies++;
        _interStateData.TargetEnemyPawn = closestEnemy;

        _interStateData.SpottedEnemyEvent?.Invoke(closestEnemy);
        CalledForStateChangeEvent?.Invoke(PawnStateType.MovingAttack);
    }
    public override void Stop()
    {
        _enemiesLocator.FoundClosestEnemyEvent -= OnFoundClosestEnemy;
    }

    public override void Update()
    {
        _enemiesLocator.Update();
    }


}

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
        _view.Agent.enabled = true;
        _view.Obstacle.enabled = false;
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

public class MovingState : PawnState
{
    private readonly PawnInterStateData _interStateData;
    private readonly PawnView _view;
    private readonly PawnFacade _facade;

    public override PawnStateType Type => PawnStateType.Moving;

    public MovingState(PawnFacade facade)
    {
        _interStateData = facade.InterStateData;
        _view = facade.View;
    }

    public override void Start()
    {
        _view.Agent.destination = _interStateData.TargetPosition;
        _interStateData.PawnStateType = this.Type;
    }

    public override void Stop()
    {

    }

    public override void Update()
    {
        float distanceToDestinationPoint = Vector3.Distance(_view.transform.position, _interStateData.TargetPosition);
        if (distanceToDestinationPoint < 0.1f)
        {
            CalledForStateChangeEvent(PawnStateType.Idle);
        }
    }
}


public abstract class PawnState
{
    public Action<PawnStateType> CalledForStateChangeEvent;
    public abstract PawnStateType Type { get; }
    public abstract void Start();
    public abstract void Update();
    public abstract void Stop();
}
