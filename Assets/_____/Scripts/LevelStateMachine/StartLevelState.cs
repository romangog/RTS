using System.Collections.Generic;

public class StartLevelState : LevelState
{
    private readonly LevelPawnsData _levelPawnsData;
    private readonly PawnController.Factory _pawnFactory;
    private readonly SpawnPointAlly[] _allySpawnPoints;
    private readonly SpawnPointEnemy[] _enemySpawnPoints;
    private readonly EnemyAI _enemyAi;
    private readonly LevelView _levelView;

    public override LevelStateType Type => LevelStateType.Start;

    public StartLevelState(LevelStateMachine.ServicesPack pack)
    {
        _levelPawnsData = pack.LevelPawnsData;
        _pawnFactory = pack.PawnFactory;
        _allySpawnPoints = pack.SpawnPointsAlly;
        _enemySpawnPoints = pack.SpawnPointsEnemy;
        _enemyAi = pack.EnemyAi;
        _levelView = pack.LevelView;
    }

    public override void Start()
    {
        _levelPawnsData.PlayerPawns = new List<PawnController>(_allySpawnPoints.Length);
        _levelPawnsData.EnemyPawns = new List<PawnController>(_enemySpawnPoints.Length);
        // Spawn player units
        for (int i = 0; i < _allySpawnPoints.Length; i++)
        {
            _levelPawnsData.PlayerPawns.Add(_pawnFactory.Create(_allySpawnPoints[i].transform.position, true, _levelView.transform, _levelPawnsData.EnemyPawns));
        }

        // Spawn enemyUnits
        for (int i = 0; i < _enemySpawnPoints.Length; i++)
        {
            _levelPawnsData.EnemyPawns.Add(_pawnFactory.Create(_enemySpawnPoints[i].transform.position, false, _levelView.transform, _levelPawnsData.PlayerPawns));
        }
        _enemyAi.Initialize();

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
