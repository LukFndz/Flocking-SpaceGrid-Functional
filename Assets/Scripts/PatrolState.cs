using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : IState
{
    private StateMachine _sm;
    private Cazador _hunter;

    public PatrolState(Cazador c, StateMachine sm)
    {
        _sm = sm;
        _hunter = c;
    }

    public void ManualUpdate()
    {
        Advance();
    }

    public void Advance()
    {
        Collider[] colliders = Physics.OverlapSphere(_hunter.transform.position, _hunter.ViewRadius, 1 << 6);

        if (colliders.Length > 0)
        {
            _hunter.Target = colliders[0].transform.parent.root.gameObject.GetComponent<Boid>();
            _sm.ChangeState("ChaseState");
        }
        else
        {
            Vector3 distance = _hunter.wayPoints[_hunter.CurrentWayPoint].transform.position - _hunter.transform.position;

            if (distance.magnitude < _hunter.stoppingDistance)
            {
                _hunter.CurrentWayPoint++;
                if (_hunter.CurrentWayPoint > _hunter.wayPoints.Length - 1)
                    _hunter.CurrentWayPoint = 0;
            }

            _hunter.AddForce(Pursuit());
        }

        _hunter.transform.position += _hunter.Velocity * Time.deltaTime;
        _hunter.transform.forward = _hunter.Velocity;
    }

    private Vector3 Pursuit()
    {
        Vector3 futurePos = _hunter.wayPoints[_hunter.CurrentWayPoint].transform.position;

        Vector3 desired = futurePos - _hunter.transform.position;

        Debug.DrawLine(_hunter.transform.position, futurePos, Color.white);
        desired.Normalize();
        desired *= _hunter.MaxSpeed;

        //Steering
        Vector3 steering = desired - _hunter.Velocity;
        steering = Vector3.ClampMagnitude(steering, _hunter.MaxForce);

        return steering;
    }
}
