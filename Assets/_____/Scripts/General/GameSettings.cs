using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSettings
{
    public TacticalCameraController.Settings TacticalCameraSettings;
    public PawnMoveController.Settings PawnMoveControllerSettings;
    public PawnController.Settings PawnSettings;
    public AbilitiesSettings AbilitiesSettings;
    public LevelSettings LevelSettings;
    public EnemyAI.Settings EnemyAiSettings;
}

[Serializable]
public class LevelSettings
{
    public float ConquestZoneRadius;
}


[Serializable]
public class AbilitiesSettings
{
    public HealthAbilitySettings HealthAbility;
    public HeavyAttackAbilitySettings HeavyAttackAbility;
    [Serializable]
    public class HealthAbilitySettings
    {
        public float HealValue;
        public float CoolDown;
    }

    [Serializable]
    public class HeavyAttackAbilitySettings
    {
        public float DamageValue;
        public float CoolDown;
    }

}


