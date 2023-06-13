using System;
using System.Collections.Generic;
using UnityEngine;

public class PawnMoveController
{
    private readonly PawnTargetPositionsMarkersPool _pawnMarkersPool;
    private readonly Settings _settings;
    private readonly LevelPawnsData _levelPawnsData;
    private readonly PawnTacticalControlFacade.InterStateData _pawnTacticalControlData;
    private readonly MainCamera _mainCamera;
    private Vector3 _firstPoint;
    private Vector3 _secondPoint;
    private Plane _levelPlane;
    private bool _IsHolding;
    private Vector3[] _pawnMovePositions;

    private GameObject[] _usedMarkers;
    private float _positionsDifference;
    private bool _IsRelative = true;
    private PawnController _preSelectedEnemyPawn;
    public PawnMoveController(
        PawnTargetPositionsMarkersPool pawnMarkersPool,
        Settings settings,
        LevelPawnsData levelPawnsData,
        PawnTacticalControlFacade.InterStateData pawnTacticalControlData,
        MainCamera mainCamera)
    {
        _levelPlane = new Plane(Vector3.up, 0f);
        _pawnMarkersPool = pawnMarkersPool;
        _settings = settings;
        _levelPawnsData = levelPawnsData;
        _pawnTacticalControlData = pawnTacticalControlData;
        _mainCamera = mainCamera;
    }

    public void UpdateSelectionSize()
    {
        _pawnMovePositions = new Vector3[_pawnTacticalControlData.SelectedPawns.Count];
        if (_IsHolding && !_IsRelative)
        {
            _usedMarkers = new GameObject[_pawnTacticalControlData.SelectedPawns.Count];
            _pawnMarkersPool.ReturnAll();
            for (int i = 0; i < _usedMarkers.Length; i++)
            {
                _usedMarkers[i] = _pawnMarkersPool.Get();
            }
        }
    }

    public void StartPlacement()
    {
        _firstPoint = GetLevelPointFromScreenPos(Input.mousePosition);
        _IsRelative = true;
        _IsHolding = true;
    }

    public void StopPlacement()
    {
        DropPlacement();

        if (_IsRelative)
        {
            Vector3 middlePosition = GetLevelPointFromScreenPos(Input.mousePosition);

            // IsAttackingCommand
            PawnController closestEnemy = null;
            if (_levelPawnsData.EnemyPawns.Count != 0)
            {
                float minDist = 0f;
                for (int i = 0; i < _levelPawnsData.EnemyPawns.Count; i++)
                {
                    float dist = Vector3.Distance(middlePosition, _levelPawnsData.EnemyPawns[i].Position);
                    if (i == 0)
                        minDist = dist;

                    if (dist < minDist && dist < 1f)
                    {
                        minDist = dist;
                        closestEnemy = _levelPawnsData.EnemyPawns[i];
                    }
                }
            }

            if (closestEnemy == null)
            {
                PawnTacticalControl.EventBus.MovePositionsRelativeEvent?.Invoke(middlePosition);
            }
            else
            {
                PawnTacticalControl.EventBus.AttackEvent?.Invoke(closestEnemy);
            }

        }
        else
        {
            PawnTacticalControl.EventBus.MovePositionsPlacedEvent?.Invoke(_pawnMovePositions);
        }
    }

    private Vector3 GetLevelPointFromScreenPos(Vector3 screenPos)
    {
        Ray screenWorldRay = Camera.main.ScreenPointToRay(screenPos);
        _levelPlane.Raycast(screenWorldRay, out float distance);

        return screenWorldRay.GetPoint(distance);
    }

    public void Update()
    {
        if (_IsHolding)
        {
            TickHolding();
        }
        else
        {
            PreSelectEnemyPawn();
        }
    }

    private void PreSelectEnemyPawn()
    {
        if (_levelPawnsData.EnemyPawns.Count == 0) return;
        Ray ray = _mainCamera.Camera.ScreenPointToRay(Input.mousePosition);
        _levelPlane.Raycast(ray, out float distance);
        Vector3 mousePointOnLevel = ray.GetPoint(distance);
        float minDist = 0f;
        PawnController closestEnemyPawn = null;
        for (int i = 0; i < _levelPawnsData.EnemyPawns.Count; i++)
        {
            float dist = Vector3.Distance(_levelPawnsData.EnemyPawns[i].Position, mousePointOnLevel);
            if (i == 0)
                minDist = dist;

            if (dist < minDist && dist < 1f)
            {
                minDist = dist;
                closestEnemyPawn = _levelPawnsData.EnemyPawns[i];
            }
        }

        if (_preSelectedEnemyPawn != closestEnemyPawn)
        {
            _preSelectedEnemyPawn?.SetPreSelected(false);
            if (closestEnemyPawn != null)
                closestEnemyPawn.SetPreSelected(true);
            _preSelectedEnemyPawn = closestEnemyPawn;
        }
    }

    private void TickHolding()
    {
        _secondPoint = GetLevelPointFromScreenPos(Input.mousePosition);
        _positionsDifference = (_secondPoint - _firstPoint).magnitude;
        SwitchRelativeMode();
        if (_IsRelative) return;
        Vector3 rowLine = _secondPoint - _firstPoint;
        Vector3 columnLine = Vector3.Cross(rowLine, Vector3.down);
        float rowMagnitude = rowLine.magnitude;
        float columnMagnitude = columnLine.magnitude;
        int columnsCount = Mathf.CeilToInt(rowMagnitude / _settings.PawnPositioningSize);
        columnsCount = Mathf.Clamp(columnsCount, 1, _pawnTacticalControlData.SelectedPawns.Count);
        int rowsCount = _pawnTacticalControlData.SelectedPawns.Count / columnsCount;

        if (_pawnTacticalControlData.SelectedPawns.Count % columnsCount > 0)
            rowsCount += 1;

        Vector3 rowLineDirection = new Vector3(
            rowLine.x / rowMagnitude,
            0f,
            rowLine.z / rowMagnitude);

        Vector3 columnDirection = new Vector3(
    columnLine.x / columnMagnitude,
    0f,
    columnLine.z / columnMagnitude);

        Vector3 nextPawnPosRowOffset = Vector3.zero;
        Vector3 nextPawnPosColumnOffset = Vector3.zero;

        int pawnPositionindex = 0;
        for (int i = 0; i < rowsCount; i++)
        {
            for (int j = 0; j < columnsCount; j++)
            {
                _pawnMovePositions[pawnPositionindex] = _firstPoint + nextPawnPosRowOffset + nextPawnPosColumnOffset;
                _usedMarkers[pawnPositionindex].transform.position = _pawnMovePositions[pawnPositionindex];
                pawnPositionindex++;
                nextPawnPosRowOffset += rowLineDirection * _settings.PawnPositioningSize;
                if (pawnPositionindex == _pawnTacticalControlData.SelectedPawns.Count)
                {
                    break;
                }
            }
            nextPawnPosColumnOffset += columnDirection * _settings.PawnPositioningSize;
            if (i == rowsCount - 2)
            {
                int pawnsLeft = _pawnTacticalControlData.SelectedPawns.Count - pawnPositionindex;
                nextPawnPosRowOffset = rowLineDirection * (columnsCount - pawnsLeft);
            }
            else
            {
                nextPawnPosRowOffset = Vector3.zero;
            }
        }
        _pawnMarkersPool.ForwardArrow.transform.position = _firstPoint + rowLine / 2f - columnDirection * 1.5f;
        _pawnMarkersPool.ForwardArrow.transform.forward = -columnDirection;

    }

    private void SwitchRelativeMode()
    {
        bool IsRelativeNow = _positionsDifference < 1f;
        if (!_IsRelative && IsRelativeNow)
        {
            _pawnMarkersPool.ReturnAll();
            _pawnMarkersPool.ForwardArrow.SetActive(false);
        }
        if (_IsRelative && !IsRelativeNow)
        {
            _usedMarkers = new GameObject[_pawnTacticalControlData.SelectedPawns.Count];
            for (int i = 0; i < _usedMarkers.Length; i++)
            {
                _usedMarkers[i] = _pawnMarkersPool.Get();
            }
            _pawnMarkersPool.ForwardArrow.SetActive(true);
        }
        _IsRelative = IsRelativeNow;
    }

    public void Enable()
    {
    }

    public void Disable()
    {

    }

    [Serializable]
    public class Settings
    {
        public float PawnPositioningSize;
    }

    internal void DropPlacement()
    {
        if (!_IsRelative)
        {
            _pawnMarkersPool.ForwardArrow.SetActive(false);
            foreach (var marker in _usedMarkers)
            {
                _pawnMarkersPool.Return(marker);
            }
        }
        _IsHolding = false;
    }
}
