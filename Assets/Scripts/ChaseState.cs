using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : IState
{
    private StateMachine _sm;
    private Cazador _hunter;

    public ChaseState(Cazador c, StateMachine sm)
    {
        _sm = sm;
        _hunter = c;
    }

    public void ManualUpdate()
    {
        if (_hunter.Target == null)
        {
            _sm.ChangeState("PatrolState");
            return;
        }

        Advance();
        CheckStamina();
    }

    public void Advance()
    {
        Vector3 boidDistance = _hunter.Target.transform.position - _hunter.transform.position;

        if (boidDistance.magnitude <= _hunter.ViewRadius)
            _hunter.AddForce(Pursuit());
        else
            _hunter.Target = null;

        _hunter.transform.position += _hunter.Velocity * Time.deltaTime;
        _hunter.transform.forward = _hunter.Velocity;
    }

    private Vector3 Pursuit()
    {
        Vector3 futurePos = _hunter.Target.transform.position + _hunter.Target.GetVelocity();

        Vector3 desired = futurePos - _hunter.transform.position;

        Debug.DrawLine(_hunter.transform.position, futurePos, Color.white);
        desired.Normalize();
        desired *= _hunter.MaxSpeed;

        //Steering
        Vector3 steering = desired - _hunter.Velocity;
        steering = Vector3.ClampMagnitude(steering, _hunter.MaxForce);

        return steering;
    }

    public void Rest()
    {
        _hunter.IsResting = true;
        _hunter.CurrentStamina =_hunter.Stamina;
        _sm.ChangeState("IdleState");
    }

    public void CheckStamina()
    {
        _hunter.CurrentStamina = _hunter.CurrentStamina - Time.deltaTime;

        if (_hunter.CurrentStamina <= 0 && !_hunter.IsResting)
            Rest();
    }
}
