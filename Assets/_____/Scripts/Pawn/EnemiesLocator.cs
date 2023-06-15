using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesLocator
{
    private readonly List<PawnController> _enemies;
    private readonly PawnController.Settings _pawnSettings;
    private readonly PawnView _view;
    private readonly PawnInterStateData _interStateData;
    private float _detectionCD;

    public Action<PawnController> FoundClosestEnemyEvent;

    public EnemiesLocator(
        List<PawnController> enemies,
        PawnController.Settings pawnSettings,
        PawnView view,
        PawnInterStateData interStateData)
    {
        _enemies = enemies;
        _pawnSettings = pawnSettings;
        _view = view;
        _interStateData = interStateData;
    }

    public PawnController GetClosestEnemyPawn()
    {
        if (_enemies.Count == 0) return null;
        float maxPrefertence = 0f;
        PawnController closestEnemyPawn = null;
        for (int i = 0; i < _enemies.Count; i++)
        {
            var enemy = _enemies[i];
            float distance = Vector3.Distance(enemy.Position, _view.transform.position);
            if (distance >= _pawnSettings.EnemiesDetectionRadius) continue;
            float approachingEnemiesDivider =
                (_interStateData.TargetEnemyPawn == enemy)
                ? (enemy.InterStateData.ApproachingEnemies)
                : (enemy.InterStateData.ApproachingEnemies + 1);

            float preference = (1f - distance / _pawnSettings.EnemiesDetectionRadius) / approachingEnemiesDivider;

            if (preference > maxPrefertence)
            {
                maxPrefertence = preference;
                closestEnemyPawn = enemy;
            }
        }
        return closestEnemyPawn;
    }

    internal void Update()
    {
        _detectionCD = Mathf.MoveTowards(_detectionCD, 0f, Time.deltaTime);
        if (_detectionCD == 0f)
        {
            _detectionCD = _pawnSettings.CheckEnemiesAroundCD;
            PawnController closestPawn = GetClosestEnemyPawn();
            if (closestPawn != null)
                FoundClosestEnemyEvent?.Invoke(closestPawn);
        }
    }
}
