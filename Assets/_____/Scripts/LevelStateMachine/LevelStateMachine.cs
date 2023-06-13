using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public enum LevelStateType
{
    Start,
    Tactical,
    Finish
}

public class LevelStateMachine : ITickable, IInitializable, IFixedTickable
{
    public class ServicesPack
    {
        public ServicesPack(
            LevelPawnsData levelPawnsData,
            CamerasController camerasController,
            GameSettings gameSettings,
            PawnController.Factory pawnFactory,
            PawnSelector pawnSelector,
            PawnMoveController pawnMoveController,
            SpawnPointAlly[] spawnPointsAlly,
            SpawnPointEnemy[] spawnPointsEnemy,
            PawnTacticalControl pawnTacticalControl,
            PawnTacticalControlFacade.InterStateData pawnTacticalControlData)
        {
            LevelPawnsData = levelPawnsData;
            CamerasController = camerasController;
            GameSettings = gameSettings;
            PawnFactory = pawnFactory;
            PawnSelector = pawnSelector;
            PawnMoveController = pawnMoveController;
            SpawnPointsAlly = spawnPointsAlly;
            SpawnPointsEnemy = spawnPointsEnemy;
            PawnTacticalControl = pawnTacticalControl;
            PawnTacticalControlData = pawnTacticalControlData;
        }

        public LevelPawnsData LevelPawnsData { get; }
        public CamerasController CamerasController { get; }
        public GameSettings GameSettings { get; }
        public PawnController.Factory PawnFactory { get; }
        public PawnSelector PawnSelector { get; }
        public PawnMoveController PawnMoveController { get; }
        public SpawnPointAlly[] SpawnPointsAlly { get; }
        public SpawnPointEnemy[] SpawnPointsEnemy { get; }
        public PawnTacticalControl PawnTacticalControl { get; }
        public PawnTacticalControlFacade.InterStateData PawnTacticalControlData { get; }
    }

    private LevelState _currentState;

    private LevelState[] _states;

    // Можно было бы биндить интерфейсы по принципу DI, но так проще
    public LevelStateMachine(
        EventBus eventBus,
        CamerasController camerasController,
        GameSettings gameSettings,
        SimpleTouchInput simpleTouchInput,
        BlackScreen blackScreen,
        SLS.Snapshot snapshot,
        Prefabs prefabs,
        SceneLoaderWrapper sceneLoader,
        Money money,
        PawnController.Factory pawnFactory,
        UI ui,
        PawnTargetPositionsMarkersPool pawnPositionsMarkerPool,
        SpawnPointAlly[] spawnPointsAlly,
        SpawnPointEnemy[] spawnPointsEnemy,
        MainCamera mainCamera
        )
    {
        LevelPawnsData levelPawnsData = new LevelPawnsData();

        PawnTacticalControlFacade.InterStateData pawnTacticalControlData = new PawnTacticalControlFacade.InterStateData();

        PawnSelector pawnSelector = new PawnSelector(
            ui.SelectionRectView,
            levelPawnsData,
            mainCamera,
            pawnTacticalControlData);

        PawnMoveController pawnMoveController = new PawnMoveController(
            pawnPositionsMarkerPool,
            gameSettings.PawnMoveControllerSettings,
            levelPawnsData,
            pawnTacticalControlData,
            mainCamera);


        PawnTacticalControlFacade pawnTacticalControlFacade = new PawnTacticalControlFacade(
            pawnSelector,
            pawnMoveController, 
            ui.LevelInteractionGraphic,
            pawnTacticalControlData);

        PawnTacticalControl pawnTacticalControl = new PawnTacticalControl(pawnTacticalControlFacade);


        ServicesPack pack = new ServicesPack(
            levelPawnsData,
            camerasController,
            gameSettings,
            pawnFactory,
            pawnSelector,
            pawnMoveController,
            spawnPointsAlly,
            spawnPointsEnemy, 
            pawnTacticalControl,
            pawnTacticalControlData);

        StartLevelState startLevelState = new StartLevelState(pack);
        TacticalLevelState tacticalLevelState = new TacticalLevelState(pack);

        _states = new LevelState[]
        {
            startLevelState,
            tacticalLevelState,
        };

        foreach (var state in _states)
        {
            state.CalledForStateChangeEvent += ChangeState;
        }
    }

    public void ChangeState(LevelStateType stateType)
    {
        if (_currentState != null && _currentState.Type != stateType)
        {
            _currentState.Stop();
        }
        _currentState = _states[(int)stateType];

        _currentState.Start();
    }

    public void FixedTick()
    {
        _currentState?.FixedUpdate();
    }

    public void Initialize()
    {
        ChangeState(LevelStateType.Start);
    }

    public void Tick()
    {
        _currentState?.Update();
    }
}



public abstract class LevelState
{
    public Action<LevelStateType> CalledForStateChangeEvent;
    public abstract LevelStateType Type { get; }
    public abstract void Start();
    public abstract void Update();
    public abstract void FixedUpdate();
    public abstract void Stop();
}

public class StartLevelState : LevelState
{
    private readonly LevelPawnsData _levelPawnsData;
    private readonly LevelSettings _levelSettings;
    private readonly PawnController.Factory _pawnFactory;
    private readonly SpawnPointAlly[] _allySpawnPoints;
    private readonly SpawnPointEnemy[] _enemySpawnPoints;

    public override LevelStateType Type => LevelStateType.Start;

    public StartLevelState(LevelStateMachine.ServicesPack pack)
    {
        _levelPawnsData = pack.LevelPawnsData;
        _levelSettings = pack.GameSettings.LevelSettings;
        _pawnFactory = pack.PawnFactory;
        _allySpawnPoints = pack.SpawnPointsAlly;
        _enemySpawnPoints = pack.SpawnPointsEnemy;
    }

    public override void Start()
    {
        _levelPawnsData.PlayerPawns = new List<PawnController>(_allySpawnPoints.Length);
        _levelPawnsData.EnemyPawns = new List<PawnController>(_enemySpawnPoints.Length);
        // Spawn player units
        for (int i = 0; i < _allySpawnPoints.Length; i++)
        {
            _levelPawnsData.PlayerPawns.Add(_pawnFactory.Create(_allySpawnPoints[i].transform.position, true, _levelPawnsData.EnemyPawns));
        }

        // Spawn enemyUnits
        for (int i = 0; i < _enemySpawnPoints.Length; i++)
        {
            _levelPawnsData.EnemyPawns.Add(_pawnFactory.Create(_enemySpawnPoints[i].transform.position, false, _levelPawnsData.PlayerPawns));
        }
        CalledForStateChangeEvent?.Invoke(LevelStateType.Tactical);
    }

    public override void Stop()
    {
    }

    public override void Update()
    {

    }

    public override void FixedUpdate()
    {

    }
}


public class TacticalLevelState : LevelState
{
    private readonly PawnTacticalControl _pawnControl;
    private readonly PawnTacticalControlFacade.InterStateData _pawnControlData;
    private readonly LevelPawnsData _levelData;
    private readonly PawnTacticalControl _pawnTacticalControl;

    public override LevelStateType Type => LevelStateType.Tactical;

    public TacticalLevelState(LevelStateMachine.ServicesPack pack)
    {
        _pawnControl = pack.PawnTacticalControl;
        _pawnControlData = pack.PawnTacticalControlData;
        _levelData = pack.LevelPawnsData;
        _pawnTacticalControl = pack.PawnTacticalControl;


    }
    public override void Start()
    {
        _pawnTacticalControl.ChangeState(TacticalControlStateType.Idle);
        foreach (var playerPawn in _levelData.PlayerPawns)
        {
            playerPawn.DiedEvent += OnPlayerPawnDied;
        }

        foreach (var enemyPawn in _levelData.EnemyPawns)
        {
            enemyPawn.DiedEvent += OnEnemyPawnDied;
        }

        PawnTacticalControl.EventBus.MovePositionsPlacedEvent += OnPawnsMoveSet;
        PawnTacticalControl.EventBus.MovePositionsRelativeEvent += OnPawnsMoveRelative;
        PawnTacticalControl.EventBus.AttackEvent += OnPawnsAttack;
    }

    private void OnPawnsAttack(PawnController enemy)
    {
        for (int i = 0; i < _pawnControlData.SelectedPawns.Count; i++)
        {
            _pawnControlData.SelectedPawns[i].CommandAttack(enemy);
        }
    }

    private void OnPawnsMoveRelative(Vector3 relativePosition)
    {
        Vector3 middlePoint = Vector3.zero;
        for (int i = 0; i < _pawnControlData.SelectedPawns.Count; i++)
        {
            middlePoint += _pawnControlData.SelectedPawns[i].Position;
        }
        middlePoint /= (float)_pawnControlData.SelectedPawns.Count;

        for (int i = 0; i < _pawnControlData.SelectedPawns.Count; i++)
        {
            Vector3 distanceFromMiddlePoint = _pawnControlData.SelectedPawns[i].Position - middlePoint;
            _pawnControlData.SelectedPawns[i].CommandMove(relativePosition + distanceFromMiddlePoint);
        }
    }

    private void OnPawnsMoveSet(Vector3[] pawnMovePositions)
    {
        for (int i = 0; i < pawnMovePositions.Length; i++)
        {
            _pawnControlData.SelectedPawns[i].CommandMove(pawnMovePositions[i]);
        }
    }

    private void OnPlayerPawnDied(PawnController pawn)
    {
        _levelData.PlayerPawns.Remove(pawn);
    }

    private void OnEnemyPawnDied(PawnController pawn)
    {
        _levelData.EnemyPawns.Remove(pawn);
    }

    public override void Stop()
    {
        PawnTacticalControl.EventBus.MovePositionsPlacedEvent -= OnPawnsMoveSet;
        PawnTacticalControl.EventBus.MovePositionsRelativeEvent -= OnPawnsMoveRelative;
        PawnTacticalControl.EventBus.AttackEvent -= OnPawnsAttack;

        foreach (var playerPawn in _levelData.PlayerPawns)
        {
            playerPawn.DiedEvent -= OnPlayerPawnDied;
        }
        foreach (var enemyPawn in _levelData.EnemyPawns)
        {
            enemyPawn.DiedEvent -= OnEnemyPawnDied;
        }
    }

    public override void Update()
    {
        _pawnTacticalControl.Update();
        foreach (var playerPawn in _levelData.PlayerPawns)
        {
            playerPawn.Tick();
        }
        foreach (var enemyPawn in _levelData.EnemyPawns)
        {
            enemyPawn.Tick();
        }

        if(Input.GetMouseButton(2))
        {

        }
    }

    public override void FixedUpdate()
    {

    }
}