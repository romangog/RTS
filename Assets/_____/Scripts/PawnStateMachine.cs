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
        PawnView view)
    {
        Enemies = enemies;
        Settings = settings;
        View = view;
        EnemiesLocator = new EnemiesLocator(enemies, settings, view);
    }

    public List<PawnController> Enemies { get; }
    public PawnController.Settings Settings { get; }
    public PawnView View { get; }
    public EnemiesLocator EnemiesLocator { get; }

    public PawnController TargetEnemyPawn { get; set; }
    public Vector3 TargetPosition { get; set; }

    public void Update()
    {
        EnemiesLocator.Update();
    }
}

public class EnemiesLocator
{
    private readonly List<PawnController> _enemies;
    private readonly PawnController.Settings _pawnSettings;
    private readonly PawnView _view;
    private float _detectionCD;

    public Action<PawnController> FoundClosestEnemyEvent;

    public EnemiesLocator(
        List<PawnController> enemies,
        PawnController.Settings pawnSettings,
        PawnView view)
    {
        _enemies = enemies;
        _pawnSettings = pawnSettings;
        _view = view;
    }

    public PawnController GetClosestEnemyPawn()
    {
        if (_view.Debug) Debug.Log("Enemeis: " + _enemies.Count);
        if (_enemies.Count == 0) return null;
        float minDistance = Vector3.Distance(_enemies[0].Position, _view.transform.position);
        PawnController closestEnemyPawn = _enemies[0];
        if (minDistance > _pawnSettings.EnemiesDetectionRadius) closestEnemyPawn = null;
        foreach (var enemy in _enemies)
        {
            float distance = Vector3.Distance(enemy.Position, _view.transform.position);
            if (distance > _pawnSettings.EnemiesDetectionRadius) continue;
            if (distance < minDistance)
            {
                minDistance = distance;
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
            if (_view.Debug) Debug.Log("CheckedFor closest target: " + closestPawn);
            if (closestPawn != null)
                FoundClosestEnemyEvent?.Invoke(closestPawn);
        }
    }
}
public class IdleState : PawnState
{
    public override PawnStateType Type => PawnStateType.Idle;

    private readonly PawnFacade _facade;
    private readonly List<PawnController> _enemies;
    private readonly PawnController.Settings _settings;
    private readonly PawnView _view;
    private float _checkEnemiesAroundCd;

    public IdleState(PawnFacade facade)
    {
        _facade = facade;
        _enemies = facade.Enemies;
        _settings = facade.Settings;
        _view = facade.View;
        _facade.EnemiesLocator.FoundClosestEnemyEvent += OnFoundClosestEnemy;
    }

    private void OnFoundClosestEnemy(PawnController closestEnemy)
    {
        _facade.TargetEnemyPawn = closestEnemy;
        CalledForStateChangeEvent?.Invoke(PawnStateType.MovingAttack);
    }

    public override void Start()
    {
        _view.DebugField = "Idle";
        _view.Agent.destination = _view.transform.position;
    }

    public override void Stop()
    {

    }

    public override void Update()
    {
        _facade.Update();
    }


}

public class MovingAttackState : PawnState
{
    private readonly PawnFacade _facade;
    private readonly PawnView _view;
    private readonly PawnController.Settings _settings;

    public override PawnStateType Type => PawnStateType.MovingAttack;

    public MovingAttackState(PawnFacade facade)
    {
        _facade = facade;
        _view = _facade.View;
        _facade.EnemiesLocator.FoundClosestEnemyEvent += OnFoundClosestEnemy;
        _settings = facade.Settings;
    }

    private void OnFoundClosestEnemy(PawnController closestEnemy)
    {
        _facade.TargetEnemyPawn = closestEnemy;
    }

    public override void Start()
    {
        _view.DebugField = "MovingAttack";
    }

    public override void Stop()
    {

    }

    public override void Update()
    {
        _facade.Update();

        if (_facade.TargetEnemyPawn.IsDead)
        {
            CalledForStateChangeEvent(PawnStateType.Idle);
            return;
        }

        _view.Agent.destination = _facade.TargetEnemyPawn.Position;

        // UpdateClosestEnemy
        float distanceToEnemy = Vector3.Distance(_view.transform.position, _facade.TargetEnemyPawn.Position);
        if (distanceToEnemy < _settings.AttackRadius)
        {
            CalledForStateChangeEvent(PawnStateType.Attacking);
        }
    }
}

public class AttackingState : PawnState
{
    private readonly PawnFacade _facade;
    private readonly PawnController.Settings _settings;
    private readonly PawnView _view;

    public override PawnStateType Type => PawnStateType.Attacking;

    private float _attackCD;

    public AttackingState(PawnFacade facade)
    {
        _facade = facade;
        _settings = facade.Settings;
        _view = facade.View;
    }

    public override void Start()
    {
        _view.DebugField = "Attack";
    }

    public override void Stop()
    {

    }

    public override void Update()
    {
        if (_facade.TargetEnemyPawn.IsDead)
        {
            CalledForStateChangeEvent(PawnStateType.Idle);
            return;
        }

        _attackCD = Mathf.MoveTowards(_attackCD, 0f, Time.deltaTime);
        if (_attackCD == 0f)
        {
            AttackClosestEnemy();
            _attackCD = _settings.AttackCD;
        }

    }

    private void AttackClosestEnemy()
    {
        _facade.TargetEnemyPawn.RecieveDamage(_settings.AttackDamage);
    }
}

public class MovingState : PawnState
{
    private readonly PawnView _view;
    private readonly PawnFacade _facade;

    public override PawnStateType Type => PawnStateType.Moving;

    public MovingState(PawnFacade facade)
    {
        _view = facade.View;
        _facade = facade;
    }

    public override void Start()
    {
        _view.Agent.destination = _facade.TargetPosition;
        _view.DebugField = "Moving";
    }

    public override void Stop()
    {

    }

    public override void Update()
    {
        float distanceToDestinationPoint = Vector3.Distance(_view.transform.position, _facade.TargetPosition);
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
