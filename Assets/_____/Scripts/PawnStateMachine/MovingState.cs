using UnityEngine;

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
