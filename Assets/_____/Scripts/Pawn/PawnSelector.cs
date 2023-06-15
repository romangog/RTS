using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PawnSelector
{
    private readonly LevelPawnsData _levelPawnsData;
    private readonly MainCamera _mainCamera;
    private readonly PawnTacticalControlFacade.InterStateData _pawnTacticalControlData;
    private readonly SelectionRectView _selectionRectView;

    private Vector3 _firstPointOnScreen;
    private Vector3 _secondPointOnScreen;
    private int _selectedPawnsCount;

    private bool _IsHoldingSelection;

    private bool _IsClick;
    private float _rectDiagonal;
    private PawnController _preSelectedPawn;
    private Plane _levelPlane;
    public PawnSelector(
        SelectionRectView selectionRectView,
        LevelPawnsData levelPawnsData,
        MainCamera mainCamera,
        PawnTacticalControlFacade.InterStateData pawnTacticalControlData)
    {
        _levelPawnsData = levelPawnsData;
        _mainCamera = mainCamera;
        _pawnTacticalControlData = pawnTacticalControlData;
        _selectionRectView = selectionRectView;
        _levelPlane = new Plane(Vector3.up, 0f);
    }

    public void Update()
    {
        if (_IsHoldingSelection)
        {
            SwitchSelectMode();
            _secondPointOnScreen = Input.mousePosition;
            if (_IsClick)
            {

            }
            else
            {
                SelectPawnsInRect();
            }
        }
        else
        {
            PreSelectPawn();
        }
    }

    private void PreSelectPawn()
    {
        if (_levelPawnsData.PlayerPawns.Count == 0) return;
        Ray ray = _mainCamera.Camera.ScreenPointToRay(Input.mousePosition);
        _levelPlane.Raycast(ray, out float distance);
        Vector3 mousePointOnLevel = ray.GetPoint(distance);
        float minDist = 0f;
        PawnController closestPawn = null;
        for (int i = 0; i < _levelPawnsData.PlayerPawns.Count; i++)
        {
            float dist = Vector3.Distance(_levelPawnsData.PlayerPawns[i].Position, mousePointOnLevel);
            if (i == 0)
                minDist = dist;

            if (dist <= minDist && dist < 1f)
            {
                minDist = dist;
                closestPawn = _levelPawnsData.PlayerPawns[i];
            }
        }

        if (_preSelectedPawn != closestPawn)
        {
            _preSelectedPawn?.SetPreSelected(false);
            if (closestPawn != null)
                closestPawn.SetPreSelected(true);
            _preSelectedPawn = closestPawn;
        }

    }

    private void SwitchSelectMode()
    {
        _rectDiagonal = Vector3.Distance(_firstPointOnScreen, _secondPointOnScreen);
        bool IsClickNow = _rectDiagonal < 10;
        _IsClick = IsClickNow;
    }

    private void SelectPawnsInRect()
    {
        _selectedPawnsCount = 0;
        foreach (var pawn in _levelPawnsData.PlayerPawns)
        {
            if (_mainCamera.Camera.WorldToScreenPoint(pawn.Position).IsBetweenPoints(_firstPointOnScreen, _secondPointOnScreen))
            {
                pawn.SetPreSelected(true);
                _selectedPawnsCount++;
            }
            else
            {
                pawn.SetPreSelected(false);
            }
        }
        _selectionRectView.SelectionRect.anchoredPosition = (_secondPointOnScreen + _firstPointOnScreen) / 2f;
        _selectionRectView.SelectionRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Abs(_secondPointOnScreen.y - _firstPointOnScreen.y));
        _selectionRectView.SelectionRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Abs(_secondPointOnScreen.x - _firstPointOnScreen.x));

    }

    public void StartSelection()
    {
        _firstPointOnScreen = Input.mousePosition;
        _IsHoldingSelection = true;
        _selectionRectView.SelectionRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0f);
        _selectionRectView.SelectionRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);
        _selectionRectView.SelectionRect.gameObject.SetActive(true);

        foreach (var pawn in _levelPawnsData.PlayerPawns)
        {
            pawn.SetPreSelected(false);
        }
    }

    public void StopSelection()
    {

        if (_IsClick)
        {
            if (_preSelectedPawn != null)
            {
                _selectedPawnsCount = 1;
                _preSelectedPawn.SetPreSelected(true);
            }
            else
            {
                _selectedPawnsCount = 0;
            }
        }

        _pawnTacticalControlData.SelectedPawns = new List<PawnController>(_selectedPawnsCount);
        foreach (var pawn in _levelPawnsData.PlayerPawns)
        {
            if (!pawn.IsPreSelected) continue;
            _pawnTacticalControlData.SelectedPawns.Add(pawn);
            pawn.SetSelected(true);
        }

        DropSelection();

        PawnTacticalControl.EventBus.SelectedPawnsSizeChangedEvent?.Invoke();

        PawnTacticalControl.EventBus.SelectedPawnsEvent?.Invoke();
    }

    internal void DropSelection()
    {
        _IsHoldingSelection = false;
        _selectionRectView.SelectionRect.gameObject.SetActive(false);
        foreach (var pawn in _levelPawnsData.PlayerPawns)
        {
            if (!pawn.IsPreSelected) continue;
            pawn.SetPreSelected(false);
        }
    }
}

public class LevelPawnsData
{
    public List<PawnController> PlayerPawns;
    public List<PawnController> EnemyPawns;
    public Action PawnsListChangedEvent;
}
