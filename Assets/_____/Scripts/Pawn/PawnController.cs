using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PawnController
{
    public Action<PawnController> DiedEvent;
    public Vector3 Position => _view.transform.position;
    public bool IsPlayer => _isPlayer;
    public bool IsDead => _IsDead;
    public bool IsSelected => _IsSelected;
    public bool IsPreSelected => _IsPreSelected;

    private readonly PawnView _view;
    private readonly bool _isPlayer;

    private PawnStateMachine _stateMachine;

    private PawnFacade _facade;
    private bool _IsSelected;
    private bool _IsPreSelected;

    // Health
    private float _health;
    private float _maxHealth;
    private bool _IsDead;

    public PawnController(PawnView view, bool IsPlayer, List<PawnController> enemies, Settings settings, MainCamera mainCamera)
    {
        _view = view;
        _isPlayer = IsPlayer;

        _health = _maxHealth = settings.MaxHealth;

        _facade = new PawnFacade(
            enemies,
            settings,
            view);

        _stateMachine = new PawnStateMachine(_facade);

        _stateMachine.ChangeState(PawnStateType.Idle);

        view.DisplayBillboard.SetCameraTranform(mainCamera.transform);
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
    }

    public void CommandMove(Vector3 destination)
    {
        _facade.TargetPosition = destination;
        _stateMachine.ChangeState(PawnStateType.Moving);
    }

    public void Tick()
    {
        _stateMachine.Tick();
    }

    internal void RecieveDamage(float attackDamage)
    {
        _health = Mathf.MoveTowards(_health, 0f, attackDamage);
        _view.HealthBar.fillAmount = _health / _maxHealth;
        _view.HealthbarRoot.SetActive(_health < _maxHealth);
        if(_health == 0)
        {
            DiedEvent?.Invoke(this);
            Die();
        }
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

    public class Factory : PlaceholderFactory<Vector3, bool,List<PawnController>, PawnController>
    {
        [Inject] private Prefabs _prefabs;
        [Inject] private GameSettings _settings;
        [Inject] private MainCamera _mainCamera;
        public override PawnController Create(Vector3 position, bool IsPlayer, List<PawnController> enemies)
        {
            PawnView view;
            if (IsPlayer)
                view = GameObject.Instantiate(_prefabs.AllyPawn, position, Quaternion.identity);
            else
                view = GameObject.Instantiate(_prefabs.EnemyPawn, position, Quaternion.identity);

            return new PawnController(view, IsPlayer, enemies, _settings.PawnSettings, _mainCamera);
        }
    }

    internal void CommandAttack(PawnController enemy)
    {
        _facade.TargetEnemyPawn = enemy;
        _stateMachine.ChangeState(PawnStateType.MovingAttack);
    }
}
