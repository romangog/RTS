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
