using System;

public abstract class LevelState
{
    public Action<LevelStateType> CalledForStateChangeEvent;
    public abstract LevelStateType Type { get; }
    public abstract void Start();
    public abstract void Update();
    public abstract void FixedUpdate();
    public abstract void Stop();
}
