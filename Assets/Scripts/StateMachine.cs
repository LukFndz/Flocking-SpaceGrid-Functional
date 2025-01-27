﻿using IA2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class StateMachine : MonoBehaviour //IA2-P3
{
    public enum HunterStates { Idle, Patrol, Chase }
    private EventFSM<HunterStates> _myFsm;

    public Cazador _hunter;

    GridEntity _myGridEntity;

    private void Start()
    {
        _myGridEntity = GetComponent<GridEntity>();
        GameManager.Instance.spatialGrid.AddEntityToGrid(_myGridEntity);

        var idle = new State<HunterStates>("IdleState");
        var patrol = new State<HunterStates>("PatrolState");
        var chase = new State<HunterStates>("ChaseState");

        StateConfigurer.Create(idle)
           .SetTransition(HunterStates.Patrol, patrol)
           .SetTransition(HunterStates.Chase, chase)
           .Done(); 

        StateConfigurer.Create(patrol)
           .SetTransition(HunterStates.Chase, chase)
           .SetTransition(HunterStates.Idle, idle)
           .Done();

        StateConfigurer.Create(chase)
           .SetTransition(HunterStates.Idle, idle)
           .SetTransition(HunterStates.Patrol, patrol)
           .Done();

        float timer = 0;

        idle.OnUpdate += () =>
        {
            if (_hunter.Velocity != new Vector3(0, 0, 0))
                _hunter.SetVelocity(new Vector3(0, 0, 0));

            timer += Time.deltaTime;

            if (timer >= _hunter.IdleTime && _hunter.IsResting)
            {
                _hunter.IsResting = false;
                timer = 0;
                SendInputToFSM(HunterStates.Patrol);
            }
        };

        idle.GetTransition(HunterStates.Patrol).OnTransition += x =>
        {
            _hunter.IsResting = false;
            timer = 0;
        };

        idle.GetTransition(HunterStates.Chase).OnTransition += x =>
        {
            _hunter.IsResting = false;
        };


        patrol.OnUpdate += () =>
        {
            IEnumerable<Boid> nearbyBoids =  GetNeighbors(_myGridEntity.GetNearbyEntities(_hunter.ViewRadius));
            Boid nearestBoid = GetClosestBoid(nearbyBoids);

            if (nearestBoid != null)
            {
                _hunter.Target = nearestBoid; //aca elegir el mas cercano
                SendInputToFSM(HunterStates.Chase);
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
        };

        patrol.GetTransition(HunterStates.Idle).OnTransition += x =>
        {
            _hunter.IsResting = true;
        };

        patrol.GetTransition(HunterStates.Chase).OnTransition += x =>
        {
            _hunter.IsResting = false;
        };

        chase.OnUpdate += () =>
        {
            if (_hunter.Target == null)
            {
                SendInputToFSM(HunterStates.Patrol);
                return;
            }

            Advance(); //perseguir boids
            CheckStamina();
        };

        chase.GetTransition(HunterStates.Idle).OnTransition += x =>
        {
            _hunter.IsResting = true;
            _hunter.CurrentStamina = _hunter.Stamina;
        };

        chase.GetTransition(HunterStates.Patrol).OnTransition += x =>
        {
            _hunter.IsResting = false;
        };


        _myFsm = new EventFSM<HunterStates>(patrol);
    }

    private void SendInputToFSM(HunterStates inp)
    {
        _myFsm.SendInput(inp);
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
    private Vector3 PursuitBoids()
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
    public void Advance()
    {
        Vector3 boidDistance = _hunter.Target.transform.position - _hunter.transform.position;

        if (boidDistance.magnitude <= _hunter.ViewRadius)
            _hunter.AddForce(PursuitBoids());
        else
            _hunter.Target = null;

        _hunter.transform.position += _hunter.Velocity * Time.deltaTime;
        _hunter.transform.forward = _hunter.Velocity;
    }
    public void Rest()
    {
        SendInputToFSM(HunterStates.Idle);
    }
    public void CheckStamina()
    {
        _hunter.CurrentStamina = _hunter.CurrentStamina - Time.deltaTime;

        if (_hunter.CurrentStamina <= 0 && !_hunter.IsResting)
            Rest();
    }
    private void Update()
    {
        _myFsm.Update();
    }
    public IEnumerable<Boid> GetNeighbors(IEnumerable<GridEntity> ents)
    {
        return ents
             .Where(x => x.GetComponent<Boid>() != null)
            .Select(x => x.GetComponent<Boid>());
    } //IA2-LINQ

    public Boid GetClosestBoid(IEnumerable<Boid> boids)
    {
        return boids
             .OrderBy(x => Vector3.Distance(x.transform.position, transform.position))
             .DefaultIfEmpty(null)
             .First();
    }//IA2-LINQ

    private void OnDestroy()
    {
        GameManager.Instance.spatialGrid.RemoveEntityFromGrid(_myGridEntity);
    }
}
