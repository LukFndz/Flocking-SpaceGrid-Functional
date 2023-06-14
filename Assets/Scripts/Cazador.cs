using IA2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cazador : MonoBehaviour
{

    [Header("Properties")]
    [SerializeField] private float _maxSpeed;
    [Range(0.01f, 1f)]
    [SerializeField] private float _maxForce;
    [SerializeField] private float _stamina = 20f;
    [SerializeField] private float _idleTime = 5f;
    [SerializeField] private float _viewRadius = 6f;
    [SerializeField] private Boid _target;

    [Header("Movement")]
    public Transform[] wayPoints;
    public float stoppingDistance;

    private int _currentWayPoint = 0;

    private bool _isResting = false;

    private Vector3 _velocity;
    private float _currentStamina;

    public float IdleTime { get => _idleTime; set => _idleTime = value; }
    public float MaxForce { get => _maxForce; set => _maxForce = value; }
    public Boid Target { get => _target; set => _target = value; }
    public float ViewRadius { get => _viewRadius; set => _viewRadius = value; }
    public float MaxSpeed { get => _maxSpeed; set => _maxSpeed = value; }
    public float Stamina { get => _stamina; set => _stamina = value; }
    public int CurrentWayPoint { get => _currentWayPoint; set => _currentWayPoint = value; }
    public bool IsResting { get => _isResting; set => _isResting = value; }
    public float CurrentStamina { get => _currentStamina; set => _currentStamina = value; }
    public Vector3 Velocity { get => _velocity; set => _velocity = value; }

    private void Awake()
    {
        //_sm.AddState("PatrolState", new PatrolState(this, _sm));
        //_sm.AddState("IdleState", new IdleState(this, _sm));
        //_sm.AddState("ChaseState", new ChaseState(this, _sm));

        //_sm.ChangeState("PatrolState");

        _currentStamina = _stamina;
    }

    private void Update()
    {
        CheckBounds();
        //_sm.ManualUpdate();
    }

    public void SetVelocity(Vector3 newVelocity)
    {
        _velocity = newVelocity;
    }

    private void CheckBounds()
    {
        transform.position = GameManager.Instance.SetObjectBoundPosition(transform.position);
    }

    public void AddForce(Vector3 force)
    {
        _velocity += force;
        _velocity = Vector3.ClampMagnitude(_velocity, _maxSpeed);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _viewRadius);
    }
}
