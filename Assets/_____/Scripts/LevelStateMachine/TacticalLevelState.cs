using UnityEngine;

public class TacticalLevelState : LevelState
{
    private readonly PawnTacticalControl _pawnControl;
    private readonly PawnTacticalControlFacade.InterStateData _pawnControlData;
    private readonly LevelPawnsData _levelData;
    private readonly PawnTacticalControl _pawnTacticalControl;
    private readonly EnemyAI _enemyAi;
    private readonly ConquestZone _conquestZone;

    public override LevelStateType Type => LevelStateType.Tactical;

    public TacticalLevelState(LevelStateMachine.ServicesPack pack)
    {
        _pawnControl = pack.PawnTacticalControl;
        _pawnControlData = pack.PawnTacticalControlData;
        _levelData = pack.LevelPawnsData;
        _pawnTacticalControl = pack.PawnTacticalControl;
        _enemyAi = pack.EnemyAi;
        _conquestZone = pack.ConquestZone;
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

        _conquestZone.AllPlayerPawnsInZoneEvent += OnAllPlayersEnteredZone;
    }

    private void OnAllPlayersEnteredZone()
    {
        CalledForStateChangeEvent(LevelStateType.Finish);
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

        _conquestZone.AllPlayerPawnsInZoneEvent -= OnAllPlayersEnteredZone;

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

        if(Input.GetKeyDown(KeyCode.Q))
        {
            foreach (var selectedPawn in _pawnControlData.SelectedPawns)
            {
                selectedPawn.TryCastAbility<HealAbility>();
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            foreach (var selectedPawn in _pawnControlData.SelectedPawns)
            {
                selectedPawn.TryCastAbility<HeavyAttackAbility>();
            }
        }

        _enemyAi.Update();

        _conquestZone.Update();
    }

    public override void FixedUpdate()
    {

    }
}
