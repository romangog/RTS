using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PawnView : MonoBehaviour
{
    public GameObject SelectionMarker => _selectionMarker;
    public GameObject PreSelectionMarker => _preSelectionMarker;
    public NavMeshAgent Agent => _agent;
    public GameObject HealthbarRoot => _healthbarRoot;
    public Billboard DisplayBillboard => _displayBillboard;
    public Image HealthBar => _healthBar;

    [SerializeField] private GameObject _selectionMarker;
    [SerializeField] private GameObject _preSelectionMarker;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Image _healthBar;
    [SerializeField] private GameObject _healthbarRoot;
    [SerializeField] private Billboard _displayBillboard;
    public string DebugField;
    public bool Debug;
}
