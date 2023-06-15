using DG.Tweening;
using System.Collections;
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
            GameSettings gameSettings,
            PawnController.Factory pawnFactory,
            PawnSelector pawnSelector,
            PawnMoveController pawnMoveController,
            SpawnPointAlly[] spawnPointsAlly,
            SpawnPointEnemy[] spawnPointsEnemy,
            PawnTacticalControl pawnTacticalControl,
            PawnTacticalControlFacade.InterStateData pawnTacticalControlData,
            EnemyAI enemyAi,
            ConquestZone conquestZone,
            UI ui,
            SceneLoaderWrapper sceneLoader,
            LevelView levelView)
        {
            LevelPawnsData = levelPawnsData;
            GameSettings = gameSettings;
            PawnFactory = pawnFactory;
            PawnSelector = pawnSelector;
            PawnMoveController = pawnMoveController;
            SpawnPointsAlly = spawnPointsAlly;
            SpawnPointsEnemy = spawnPointsEnemy;
            PawnTacticalControl = pawnTacticalControl;
            PawnTacticalControlData = pawnTacticalControlData;
            EnemyAi = enemyAi;
            ConquestZone = conquestZone;
            Ui = ui;
            SceneLoader = sceneLoader;
            LevelView = levelView;
        }

        public LevelPawnsData LevelPawnsData { get; }
        public GameSettings GameSettings { get; }
        public PawnController.Factory PawnFactory { get; }
        public PawnSelector PawnSelector { get; }
        public PawnMoveController PawnMoveController { get; }
        public SpawnPointAlly[] SpawnPointsAlly { get; }
        public SpawnPointEnemy[] SpawnPointsEnemy { get; }
        public PawnTacticalControl PawnTacticalControl { get; }
        public PawnTacticalControlFacade.InterStateData PawnTacticalControlData { get; }
        public EnemyAI EnemyAi { get; }
        public ConquestZone ConquestZone { get; }
        public UI Ui { get; }
        public SceneLoaderWrapper SceneLoader { get; }
        public LevelView LevelView { get; }
    }

    private LevelState _currentState;

    private LevelState[] _states;

    public LevelStateMachine(
        GameSettings gameSettings,
        SceneLoaderWrapper sceneLoader,
        PawnController.Factory pawnFactory,
        UI ui,
        PawnTargetPositionsMarkersPool pawnPositionsMarkerPool,
        SpawnPointAlly[] spawnPointsAlly,
        SpawnPointEnemy[] spawnPointsEnemy,
        MainCamera mainCamera,
        ConquerZoneView conquestZoneView,
        LevelView levelView
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

        EnemyAI enemyAi = new EnemyAI(levelPawnsData, gameSettings);

        ConquestZone conquestZone = new ConquestZone(
            conquestZoneView,
            levelPawnsData,
            gameSettings);

        ServicesPack pack = new ServicesPack(
            levelPawnsData,
            gameSettings,
            pawnFactory,
            pawnSelector,
            pawnMoveController,
            spawnPointsAlly,
            spawnPointsEnemy, 
            pawnTacticalControl,
            pawnTacticalControlData,
            enemyAi,
            conquestZone,
            ui,
            sceneLoader,
            levelView);

        StartLevelState startLevelState = new StartLevelState(pack);
        TacticalLevelState tacticalLevelState = new TacticalLevelState(pack);
        FinishLevelState finishLevelState = new FinishLevelState(pack);

        _states = new LevelState[]
        {
            startLevelState,
            tacticalLevelState,
            finishLevelState
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
