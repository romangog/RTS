using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI
{
    private readonly Settings _settings;
    private readonly LevelPawnsData _levelPawnsData;

    public EnemyAI(LevelPawnsData levelPawnsData, GameSettings settings)
    {
        _settings = settings.EnemyAiSettings;
        _levelPawnsData = levelPawnsData;


    }

    public void Initialize()
    {
        foreach (var enemy in _levelPawnsData.EnemyPawns)
        {
            enemy.SpottedEnemyEvent += (target) => OnSpottedTarget(target, enemy);
        }
    }

    private void OnSpottedTarget(PawnController target, PawnController spotter)
    {
        foreach (var enemy in _levelPawnsData.EnemyPawns)
        {
            if (enemy != spotter
                && enemy.InterStateData.PawnStateType == PawnStateType.Idle
                && Vector3.Distance(spotter.Position, enemy.Position) <= _settings.AlertRange)
            {
                enemy.CommandAttack(target);
            }
        }
    }

    public void Update()
    {
        foreach (var enemy in _levelPawnsData.EnemyPawns)
        {
            ControlEnemyHealthAbility(enemy);
            ControlEnemyHeavyAttackAbility(enemy);
        }
    }

    private void ControlEnemyHealthAbility(PawnController enemy)
    {
        if (enemy.Health < enemy.MaxHealth
            && enemy.ShouldCast<HealAbility>())
        {
            enemy.TryCastAbility<HealAbility>();
        }
    }

    private void ControlEnemyHeavyAttackAbility(PawnController enemy)
    {
        if (enemy.ShouldCast<HeavyAttackAbility>())
        {
            enemy.TryCastAbility<HeavyAttackAbility>();
        }
    }

    [Serializable]
    public class Settings
    {
        public float AlertRange;
    }
}

public abstract class Ability
{
    public Ability(PawnController pawn)
    {
        Pawn = pawn;
    }

    public Action<bool> AvailableStatusChangedEvent;

    private bool _isAvailable;
    public bool IsAvilable
    {
        get
        {
            return _isAvailable;
        }
        set
        {
            if (_isAvailable != value)
            {
                _isAvailable = value;
                AvailableStatusChangedEvent?.Invoke(_isAvailable);
            }
        }
    }
    private bool _isPossible;
    private bool _IsCdReady;

    public bool IsPossble
    {
        get
        {
            return _isPossible;
        }
        protected set
        {
            _isPossible = value;
            IsAvilable = _isPossible && _IsCdReady;
        }
    }

    public bool IsCdReady
    {
        get
        {
            return _IsCdReady;
        }
        protected set
        {
            _IsCdReady = value;
            IsAvilable = _isPossible && _IsCdReady;
        }
    }
    public float Cooldown { get; protected set; }
    public float CooldownPercent { get; protected set; }
    public PawnController Pawn { get; }

    public abstract void Cast();

    public abstract void Update();

    internal abstract bool ShouldCast(PawnController pawnController);
}

public class HealAbility : Ability
{
    private readonly AbilitiesSettings.HealthAbilitySettings _healthAbilitySettings;

    public HealAbility(GameSettings settings, PawnController pawn) : base(pawn)
    {
        _healthAbilitySettings = settings.AbilitiesSettings.HealthAbility;
        pawn.HealthChangedEvent += OnHealthChanged;
        IsPossble = true;
        IsCdReady = true;
    }

    public override void Cast()
    {
        Pawn.SetHealth(Mathf.MoveTowards(Pawn.Health, Pawn.MaxHealth, _healthAbilitySettings.HealValue));
        Cooldown = _healthAbilitySettings.CoolDown;
        IsCdReady = false;
        Pawn.View.HealParticles.Play();
    }

    public override void Update()
    {
        if (!IsCdReady)
        {
            Cooldown = Mathf.MoveTowards(Cooldown, 0f, Time.deltaTime);
            if (Cooldown == 0f)
            {
                IsCdReady = true;
            }
        }
        CooldownPercent = 1f - Cooldown / _healthAbilitySettings.CoolDown;
    }
    private void OnHealthChanged(float health)
    {
        IsPossble = health < Pawn.MaxHealth;
    }

    internal override bool ShouldCast(PawnController pawnController)
    {
        return (pawnController.Health < pawnController.MaxHealth - _healthAbilitySettings.HealValue);
    }

    public override string ToString()
    {
        return "Heave attack";
    }
}

public class HeavyAttackAbility : Ability
{
    private readonly AbilitiesSettings.HeavyAttackAbilitySettings _heavyAttackAbiltySettings;

    public HeavyAttackAbility(GameSettings settings, PawnController pawn) : base(pawn)
    {
        _heavyAttackAbiltySettings = settings.AbilitiesSettings.HeavyAttackAbility;
        IsPossble = true;
        IsCdReady = true;
    }

    public override void Cast()
    {
        Pawn.InterStateData.TargetEnemyPawn.RecieveDamage(_heavyAttackAbiltySettings.DamageValue);
        Cooldown = _heavyAttackAbiltySettings.CoolDown;
        IsCdReady = false;
        Pawn.View.HeavyAttackParticles.Play();
    }
    public override void Update()
    {
        IsPossble = Pawn.InterStateData.TargetEnemyPawn != null && Pawn.InterStateData.PawnStateType == PawnStateType.Attacking;
        if (!IsCdReady)
        {
            Cooldown = Mathf.MoveTowards(Cooldown, 0f, Time.deltaTime);
            if (Cooldown == 0f)
            {
                IsCdReady = true;
            }
        }
        CooldownPercent = 1f - Cooldown / _heavyAttackAbiltySettings.CoolDown;
    }

    internal override bool ShouldCast(PawnController pawnController)
    {
        return true;
    }

    public override string ToString()
    {
        return "Heave attack";
    }
}
