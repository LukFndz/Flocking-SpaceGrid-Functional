using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    private Dictionary<string, IState> _stateDictionary = new Dictionary<string, IState>();

    private IState _currentState;

    public void ManualUpdate()
    {
        _currentState.ManualUpdate();
    }

    public void ChangeState(string id)
    {
        _currentState = _stateDictionary[id];
    }

    public void AddState(string id, IState state)
    {
        _stateDictionary.Add(id, state);
    }
}
