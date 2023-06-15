using System;
using System.Collections.Generic;

public class LevelPawnsData
{
    public List<PawnController> PlayerPawns;
    public List<PawnController> EnemyPawns;
    public Action PawnsListChangedEvent;
}
