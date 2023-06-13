using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : IState
{
    private StateMachine _sm;
    private Cazador _hunter;
    private float _timer;

    public IdleState(Cazador c, StateMachine sm)
    {
        _timer = 0;
        _sm = sm;
        _hunter = c;
    }

    public void ManualUpdate()
    {
        Advance();
        Idle();
    }

    public void Advance()
    {
        if (_hunter.Velocity != new Vector3(0, 0, 0))
            _hunter.SetVelocity(new Vector3(0, 0, 0));
    }

    private void Idle()
    {
        _timer += Time.deltaTime;

        if (_timer >= _hunter.IdleTime && _hunter.IsResting)
        {
            _hunter.IsResting = false;
            _timer = 0;
            _sm.ChangeState("PatrolState");
        }
    }
}
