using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TacticalControlStateType
{
    Idle,
    Selection,
    Placement
}

public class PawnTacticalControl
{
    private PawnTacticalControlState _currentState;

    private PawnTacticalControlState[] _states;

    public PawnTacticalControl(PawnTacticalControlFacade facade)
    {
        IdleTacticalControlState idleState = new IdleTacticalControlState(facade);
        SelectionTacticalControlState selectionState = new SelectionTacticalControlState(facade);
        PlacementTacticalControlState placementState = new PlacementTacticalControlState(facade);

        _states = new PawnTacticalControlState[]
        {
            idleState,
            selectionState,
            placementState
        };

        foreach (var state in _states)
        {
            state.CalledForStateChangeEvent += ChangeState;
        }
    }
    public void ChangeState(TacticalControlStateType stateType)
    {
        if (_currentState != null && _currentState.Type != stateType)
        {
            _currentState.Stop();
        }
        _currentState = _states[(int)stateType];

        _currentState.Start();
    }

    public void Update()
    {
        _currentState?.Update();
    }

    public class EventBus
    {
        public static Action<Vector3[]> MovePositionsPlacedEvent;
        public static Action<Vector3> MovePositionsRelativeEvent;
        public static Action<PawnController> AttackEvent;
        public static Action SelectedPawnsSizeChangedEvent;
        public static Action SelectedPawnsEvent;
    }
}

public class PawnTacticalControlFacade
{
    public PawnTacticalControlFacade(
        PawnSelector pawnSelector,
        PawnMoveController pawnMoveController,
        SimpleGraphic levelInteractionGraphic,
        InterStateData data)
    {
        PawnSelector = pawnSelector;
        PawnMover = pawnMoveController;
        LevelInteractionGraphic = levelInteractionGraphic;
        Data = data;
    }

    public PawnSelector PawnSelector { get; }
    public PawnMoveController PawnMover { get; }
    public SimpleGraphic LevelInteractionGraphic { get; }
    public InterStateData Data { get; }

    public class InterStateData
    {
        public List<PawnController> SelectedPawns { get; set; } = new List<PawnController>();
    }
}



public abstract class PawnTacticalControlState
{
    public Action<TacticalControlStateType> CalledForStateChangeEvent;
    public abstract TacticalControlStateType Type { get; }
    public abstract void Start();
    public abstract void Update();
    public abstract void Stop();
}

public class IdleTacticalControlState : PawnTacticalControlState
{
    private readonly SimpleGraphic _levelInteractionGraphic;
    private readonly PawnSelector _pawnSelector;
    private readonly PawnMoveController _pawnMover;
    private readonly PawnTacticalControlFacade.InterStateData _data;

    public override TacticalControlStateType Type => TacticalControlStateType.Idle;

    public IdleTacticalControlState(PawnTacticalControlFacade facade)
    {
        _levelInteractionGraphic = facade.LevelInteractionGraphic;
        _pawnSelector = facade.PawnSelector;
        _pawnMover = facade.PawnMover;
        _data = facade.Data;
    }

    public override void Start()
    {
        _levelInteractionGraphic.PointerDownLeftEvent += OnLeftMouseButtonDown;
        _levelInteractionGraphic.PointerUpLeftEvent += OnLeftMouseButtonUp;

        _levelInteractionGraphic.PointerDownRightEvent += OnRightMouseButtonDown;
        _levelInteractionGraphic.PointerUpRightEvent += OnRightMouseButtonUp;

        foreach (var pawn in _data.SelectedPawns)
        {
            pawn.DiedEvent += OnSelectedPawnDied;
        }
    }

    private void OnSelectedPawnDied(PawnController pawn)
    {
        pawn.DiedEvent -= OnSelectedPawnDied;
        _data.SelectedPawns.Remove(pawn);
        PawnTacticalControl.EventBus.SelectedPawnsSizeChangedEvent?.Invoke();
    }

    private void OnLeftMouseButtonDown()
    {
        DropSelection();
        _pawnSelector.StartSelection();
        CalledForStateChangeEvent(TacticalControlStateType.Selection);
    }

    private void DropSelection()
    {
        foreach (var pawn in _data.SelectedPawns)
        {
            pawn.SetSelected(false);
        }

        foreach (var pawn in _data.SelectedPawns)
        {
            pawn.DiedEvent -= OnSelectedPawnDied;
        }

        _data.SelectedPawns.Clear();
    }

    private void OnLeftMouseButtonUp()
    {

    }

    private void OnRightMouseButtonDown()
    {
        if (_data.SelectedPawns.Count == 0) return;
        _pawnMover.StartPlacement();
        CalledForStateChangeEvent(TacticalControlStateType.Placement);
    }

    private void OnRightMouseButtonUp()
    {
        
    }

    public override void Update()
    {
        _pawnSelector.Update();
        _pawnMover.Update();
    }

    public override void Stop()
    {
        _levelInteractionGraphic.PointerDownLeftEvent -= OnLeftMouseButtonDown;
        _levelInteractionGraphic.PointerUpLeftEvent -= OnLeftMouseButtonUp;

        _levelInteractionGraphic.PointerDownRightEvent -= OnRightMouseButtonDown;
        _levelInteractionGraphic.PointerUpRightEvent -= OnRightMouseButtonUp;

        foreach (var pawn in _data.SelectedPawns)
        {
            pawn.DiedEvent -= OnSelectedPawnDied;
        }
    }
}

public class SelectionTacticalControlState : PawnTacticalControlState
{
    private readonly SimpleGraphic _levelInteractionGraphic;
    private readonly PawnSelector _pawnSelector;
    private readonly PawnTacticalControlFacade.InterStateData _data;

    public SelectionTacticalControlState(PawnTacticalControlFacade facade)
    {
        _levelInteractionGraphic = facade.LevelInteractionGraphic;
        _pawnSelector = facade.PawnSelector;

        _data = facade.Data;
    }

    public override TacticalControlStateType Type => TacticalControlStateType.Selection;

    public override void Start()
    {
        _levelInteractionGraphic.PointerDownLeftEvent += OnLeftMouseButtonDown;
        _levelInteractionGraphic.PointerUpLeftEvent += OnLeftMouseButtonUp;

        _levelInteractionGraphic.PointerDownRightEvent += OnRightMouseButtonDown;
        _levelInteractionGraphic.PointerUpRightEvent += OnRightMouseButtonUp;

        foreach (var pawn in _data.SelectedPawns)
        {
            pawn.DiedEvent += OnSelectedPawnDied;
        }
    }

    private void OnLeftMouseButtonDown()
    {

    }

    private void OnLeftMouseButtonUp()
    {
        _pawnSelector.StopSelection();
        CalledForStateChangeEvent(TacticalControlStateType.Idle);
    }

    private void OnRightMouseButtonDown()
    {
        _pawnSelector.DropSelection();
        CalledForStateChangeEvent(TacticalControlStateType.Idle);
    }

    private void OnRightMouseButtonUp()
    {
        
    }


    private void OnSelectedPawnDied(PawnController pawn)
    {
        pawn.DiedEvent -= OnSelectedPawnDied;
        _data.SelectedPawns.Remove(pawn);
        PawnTacticalControl.EventBus.SelectedPawnsSizeChangedEvent?.Invoke();
    }

    public override void Update()
    {
        _pawnSelector.Update();
    }



    public override void Stop()
    {
        _levelInteractionGraphic.PointerDownLeftEvent -= OnLeftMouseButtonDown;
        _levelInteractionGraphic.PointerUpLeftEvent -= OnLeftMouseButtonUp;

        _levelInteractionGraphic.PointerDownRightEvent -= OnRightMouseButtonDown;
        _levelInteractionGraphic.PointerUpRightEvent -= OnRightMouseButtonUp;

        foreach (var pawn in _data.SelectedPawns)
        {
            pawn.DiedEvent -= OnSelectedPawnDied;
        }
    }


}

public class PlacementTacticalControlState : PawnTacticalControlState
{
    private readonly SimpleGraphic _levelInteractionGraphic;
    private readonly PawnMoveController _pawnMover;
    private readonly PawnTacticalControlFacade.InterStateData _data;

    public PlacementTacticalControlState(PawnTacticalControlFacade facade)
    {
        _levelInteractionGraphic = facade.LevelInteractionGraphic;
        _pawnMover = facade.PawnMover;
        _data = facade.Data;
    }

    public override TacticalControlStateType Type => TacticalControlStateType.Placement;

    public override void Start()
    {

        _levelInteractionGraphic.PointerDownLeftEvent += OnLeftMouseButtonDown;
        _levelInteractionGraphic.PointerUpLeftEvent += OnLeftMouseButtonUp;

        _levelInteractionGraphic.PointerDownRightEvent += OnRightMouseButtonDown;
        _levelInteractionGraphic.PointerUpRightEvent += OnRightMouseButtonUp;

        PawnTacticalControl.EventBus.SelectedPawnsSizeChangedEvent += OnSelectedPawnSizeChanged;

        _pawnMover.UpdateSelectionSize();

        foreach (var pawn in _data.SelectedPawns)
        {
            pawn.DiedEvent += OnSelectedPawnDied;
        }
    }

    private void OnSelectedPawnSizeChanged()
    {
        _pawnMover.UpdateSelectionSize();
    }

    private void OnSelectedPawnDied(PawnController pawn)
    {
        pawn.DiedEvent -= OnSelectedPawnDied;
        _data.SelectedPawns.Remove(pawn);
        PawnTacticalControl.EventBus.SelectedPawnsSizeChangedEvent?.Invoke();
    }

    private void OnLeftMouseButtonDown()
    {
        _pawnMover.DropPlacement();
        CalledForStateChangeEvent(TacticalControlStateType.Idle);
    }

    private void OnLeftMouseButtonUp()
    {
        
    }

    private void OnRightMouseButtonDown()
    {

    }

    private void OnRightMouseButtonUp()
    {
        _pawnMover.StopPlacement();
        
        CalledForStateChangeEvent(TacticalControlStateType.Idle);
    }

    public override void Update()
    {
        _pawnMover.Update();
    }

    public override void Stop()
    {
        _levelInteractionGraphic.PointerDownLeftEvent -= OnLeftMouseButtonDown;
        _levelInteractionGraphic.PointerUpLeftEvent -= OnLeftMouseButtonUp;

        _levelInteractionGraphic.PointerDownRightEvent -= OnRightMouseButtonDown;
        _levelInteractionGraphic.PointerUpRightEvent -= OnRightMouseButtonUp;

        PawnTacticalControl.EventBus.SelectedPawnsSizeChangedEvent -= OnSelectedPawnSizeChanged;

        foreach (var pawn in _data.SelectedPawns)
        {
            pawn.DiedEvent -= OnSelectedPawnDied;
        }
    }
}
