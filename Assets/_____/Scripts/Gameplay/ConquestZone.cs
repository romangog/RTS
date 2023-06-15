using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConquestZone
{
    public Action AllPlayerPawnsInZoneEvent;

    private readonly LevelSettings _levelSettings;
    private readonly ConquerZoneView _view;
    private readonly LevelPawnsData _pawnsData;

    public ConquestZone(
        ConquerZoneView view,
        LevelPawnsData pawnsData,
        GameSettings settings)
    {
        _levelSettings = settings.LevelSettings;
        _view = view;
        _pawnsData = pawnsData;
    }

    public void Update()
    {
        bool allPawnsInZone = true;
        foreach (var playerPawn in _pawnsData.PlayerPawns)
        {
            float distance = Vector3.Distance(playerPawn.Position, _view.transform.position);
            if (distance > _levelSettings.ConquestZoneRadius)
                allPawnsInZone = false;
        }
        if (allPawnsInZone)
            AllPlayerPawnsInZoneEvent?.Invoke();

    }
}
