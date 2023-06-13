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
    public LevelSettings LevelSettings;
}

[Serializable]
public class LevelSettings
{
}


