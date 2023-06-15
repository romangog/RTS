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


public abstract class PawnState
{
    public Action<PawnStateType> CalledForStateChangeEvent;
    public abstract PawnStateType Type { get; }
    public abstract void Start();
    public abstract void Update();
    public abstract void Stop();
}
