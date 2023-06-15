using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PawnController
{
    public Action<PawnController> DiedEvent;
    public Action<PawnController> SpottedEnemyEvent;

    public Action<float> HealthChangedEvent;
    public Vector3 Position => _view.transform.position;
    public PawnView View => _view;
    public bool IsPlayer => _isPlayer;
    public bool IsDead => _IsDead;
    public bool IsSelected => _IsSelected;
    public bool IsPreSelected => _IsPreSelected;
    public PawnInterStateData InterStateData => _interStateData;

    // Health
    public float Health => _health;
    public float MaxHealth => _maxHealth;

    private readonly PawnView _view;
    private readonly bool _isPlayer;

    private PawnStateMachine _stateMachine;
    private PawnInterStateData _interStateData;



    private bool _IsSelected;
    private bool _IsPreSelected;
    private Dictionary<Type, Ability> _abilities;


    // Health
    private float _health;
    private float _maxHealth;
    private bool _IsDead;

    public PawnController(PawnView view, bool IsPlayer, List<PawnController> enemies, Settings settings, MainCamera mainCamera, Dictionary<Type, Ability> abilities)
    {
        _view = view;
        _isPlayer = IsPlayer;

        _health = _maxHealth = settings.MaxHealth;

        _interStateData = new PawnInterStateData();

        _interStateData.SpottedEnemyEvent += OnSpottedEnemy;

        PawnFacade facade = new PawnFacade(
            enemies,
            settings,
            view,
            _interStateData);

        _stateMachine = new PawnStateMachine(facade);

        _stateMachine.ChangeState(PawnStateType.Idle);

        view.DisplayBillboard.SetCameraTranform(mainCamera.transform);

        _abilities = abilities;
    }

    private void OnSpottedEnemy(PawnController enemy)
    {
        SpottedEnemyEvent?.Invoke(enemy);
    }

    internal void TryCastAbility<T>() where T : Ability
    {
        var ability = _abilities[typeof(T)];
        if (ability.IsAvilable)
        {
            if (_view.Debug)
                Debug.Log("Cast " + ability);
            ability.Cast();
        }
    }
    internal bool ShouldCast<T>() where T : Ability
    {
        var ability = _abilities[typeof(T)];
        return (ability.ShouldCast(this));
    }

    public void SetLocalScale(Vector3 scale)
    {
        _view.transform.localScale = scale;
    }
    internal void SetPreSelected(bool v)
    {
        _IsPreSelected = v;
        _view.PreSelectionMarker.SetActive(v);
    }
    internal void SetSelected(bool v)
    {
        _IsSelected = v;
        _view.SelectionMarker.SetActive(v);
        _view.DisplayBillboard.SetinfoVisible(v);
    }

    public void CommandMove(Vector3 destination)
    {
        _interStateData.TargetPosition = destination;
        _stateMachine.ChangeState(PawnStateType.Moving);
    }

    public void Tick()
    {
        _stateMachine.Tick();
        foreach (var ability in _abilities)
        {
            ability.Value.Update();
        }

        _view.DisplayBillboard.SetHealAbilityCdPercent(_abilities[typeof(HealAbility)].CooldownPercent);
        _view.DisplayBillboard.SetHeavyAttackAbilityCdPercent(_abilities[typeof(HeavyAttackAbility)].CooldownPercent);
        _view.ApproachingEnemies = _interStateData.ApproachingEnemies;
    }

    internal void RecieveDamage(float attackDamage)
    {
        SetHealth(Mathf.MoveTowards(_health, 0f, attackDamage));


    }

    public void SetHealth(float value)
    {
        _health = value;
        _health = Mathf.Clamp(_health, 0f, _maxHealth);
        _view.DisplayBillboard.SetHealthbarPercent(_health / _maxHealth);
        

        if (_health == 0)
        {
            DiedEvent?.Invoke(this);
            Die();
        }
        HealthChangedEvent?.Invoke(_health);
    }
    private void Die()
    {
        _IsDead = true;
        _view.gameObject.SetActive(false);
    }

    [Serializable]
    public class Settings
    {
        public float CheckEnemiesAroundCD;
        public float EnemiesDetectionRadius;
        public float AttackRadius;
        public float AttackCD;
        public float AttackDamage;
        public float MaxHealth;
    }

    public class Factory : PlaceholderFactory<Vector3, bool, Transform, List<PawnController>, PawnController>
    {
        [Inject] private Prefabs _prefabs;
        [Inject] private GameSettings _settings;
        [Inject] private MainCamera _mainCamera;
        public override PawnController Create(Vector3 position, bool IsPlayer, Transform parent, List<PawnController> enemies)
        {
            PawnView view;
            if (IsPlayer)
                view = GameObject.Instantiate(_prefabs.AllyPawn, position, Quaternion.identity, parent);
            else
                view = GameObject.Instantiate(_prefabs.EnemyPawn, position, Quaternion.identity, parent);

            Dictionary<Type, Ability> abilities = new Dictionary<Type, Ability>();

            PawnController pawnController = new PawnController(view, IsPlayer, enemies, _settings.PawnSettings, _mainCamera, abilities);

            abilities.Add(typeof(HealAbility), new HealAbility(_settings, pawnController));
            abilities.Add(typeof(HeavyAttackAbility), new HeavyAttackAbility(_settings, pawnController));
            return pawnController;
        }
    }

    internal void CommandAttack(PawnController enemy)
    {
        if (_interStateData.TargetEnemyPawn != null)
        {
            _interStateData.TargetEnemyPawn.InterStateData.ApproachingEnemies--;
        }
        enemy.InterStateData.ApproachingEnemies++;
        _interStateData.TargetEnemyPawn = enemy;
        _stateMachine.ChangeState(PawnStateType.MovingAttack);
    }
}
