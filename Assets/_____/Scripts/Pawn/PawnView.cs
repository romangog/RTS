using UnityEngine;
using UnityEngine.AI;

public class PawnView : MonoBehaviour
{
    public GameObject SelectionMarker => _selectionMarker;
    public GameObject PreSelectionMarker => _preSelectionMarker;
    public ParticleSystem HealParticles => _healParticles;
    public ParticleSystem HeavyAttackParticles => _heavyAttackParticles;
    public ParticleSystem AttackParticles => _attackParticles;
    public NavMeshAgent Agent => _agent;
    public NavMeshObstacle Obstacle=> _obstacle;
    public PlayerDisplayInfo DisplayBillboard => _displayBillboard;

    [SerializeField] private GameObject _selectionMarker;
    [SerializeField] private GameObject _preSelectionMarker;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private NavMeshObstacle _obstacle;
    [SerializeField] private PlayerDisplayInfo _displayBillboard;
    [SerializeField] private ParticleSystem _healParticles;
    [SerializeField] private ParticleSystem _heavyAttackParticles;
    [SerializeField] private ParticleSystem _attackParticles;
}
