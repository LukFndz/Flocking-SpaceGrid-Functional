using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Boid : MonoBehaviour
{
    private Vector3 _velocity;

    [SerializeField]private float _maxSpeed;
    [Range(0.01f, 1f)]
    [SerializeField] private float _maxForce;
    [Range(0f, 4f)]
    [SerializeField] private float _viewRadiusSeparation;
    [Range(4f, 6f)]
    [SerializeField] private float _viewRadius;
    [Range(0f, 2f)]
    [SerializeField] private float _cohesionWeight;
    [Range(0f, 2f)]
    [SerializeField] private float _alignWeight;
    [Range(0f, 2.5f)]
    [SerializeField] private float _separationWeight;

    [SerializeField] private float _collideDistance;

    private Cazador _hunter;

    GridEntity _myGridEntity;
    IEnumerable<GridEntity> _entities;
    IEnumerable<Boid> _myNeighbors;

    public static List<Boid> allBoids = new List<Boid>();
    private void Start()
    {
        _hunter = GameManager.Instance.hunter;
        _myGridEntity = GetComponent<GridEntity>();

        allBoids.Add(this);

        Vector3 random = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f));

        AddForce(random.normalized * _maxSpeed);

        GameManager.Instance.spatialGrid.AddEntityToGrid(_myGridEntity);
    }
    private void Update()
    {
        CheckBounds();
        CheckCollision();
        _entities = _myGridEntity.GetNearbyEntities(_viewRadius);
        _myNeighbors = GetNeighbors(_entities);
        Advance();
    }
    private void CheckBounds()
    {
        transform.position = GameManager.Instance.SetObjectBoundPosition(transform.position);
    }
    public IEnumerable<Boid> GetNeighbors(IEnumerable<GridEntity> ents)
    {
        return ents
             .Where(x => x.GetComponent<Boid>() != null)
            .Select(x => x.GetComponent<Boid>());
    } //IA2-P1
    private IEnumerable<Boid> CheckClosestNeighbors(GridEntity myGridEntity, float radius, int neighborAmount)
    {
        return myGridEntity.GetNearbyEntities(radius)
            .Select(x => x.GetComponent<Boid>())
            .OrderBy(x => Vector3.Distance(x.transform.position, transform.position))
            .Take(neighborAmount);

    } //IA2-P1

    private void Advance()
    {
        Vector3 hunterDistance = _hunter.transform.position - transform.position;

        if (hunterDistance.magnitude <= _viewRadius)
        {
            Evade();
        }
        else
        {
            IEnumerable<Food> nearbyFoods = GetNearbyFoods(_myGridEntity.GetNearbyEntities(_hunter.ViewRadius));
            Food nearestFood = GetClosestFood(nearbyFoods, transform);

            if (nearestFood != null)
                AddForce(Arrive(nearestFood.transform)); //arrive es para la comida
            else
                AddForce(GetSeparation(_myNeighbors, this, _viewRadiusSeparation, CalculateSteering) * _separationWeight +
                    GetAlignment(_myNeighbors, this, _viewRadius, CalculateSteering) * _alignWeight +
                    GetCohesion(_myNeighbors, this, _viewRadius, CalculateSteering) * _cohesionWeight);
        }

        transform.position += _velocity * Time.deltaTime;
        transform.forward = _velocity;
    }

    public IEnumerable<Food> GetNearbyFoods(IEnumerable<GridEntity> ents)
    {
        return ents
             .Where(x => x.GetComponent<Food>() != null)
            .Select(x => x.GetComponent<Food>());
    } //IA2-P1

    public static Food GetClosestFood(IEnumerable<Food> foods, Transform transform)
    {
        return foods
             .OrderBy(x => Vector3.Distance(x.transform.position, transform.position))
             .DefaultIfEmpty(null)
             .First();
    }
    private static Vector3 GetCohesion(IEnumerable<Boid> boids, Boid currentBoid, float viewRadius, Func<Vector3, Vector3> CalculateSteering)
    {
        var desired = Vector3.zero;
        var count = 0;

        desired = boids.Where(x => x != currentBoid)
            //.Where(boid => Vector3.Distance(transform.position, boid.transform.position) <= _viewRadius)
                        .Aggregate(desired, (x, y) =>
                        {
                            if (Vector3.Distance(currentBoid.transform.position, y.transform.position) <= viewRadius)
                            {
                                count++;
                                return x + y.transform.position;
                            }
                            return x;
                        });

        if (count == 0) return desired;

        desired /= count;
        desired -= currentBoid.transform.position;

        return CalculateSteering(desired);
    } //IA2-P1
    private static Vector3 GetSeparation(IEnumerable<Boid> boids, Boid currentBoid, float viewRadiusSeparation, Func<Vector3, Vector3> CalculateSteering)
    {
        Vector3 desired = boids.Where(x => x != currentBoid)
                                    .Select(x => x.transform.position - currentBoid.transform.position)
                                    .Where(x => x.magnitude < viewRadiusSeparation)
                                    .Aggregate(Vector3.zero, (x, y) => x + y);

        if (desired == Vector3.zero)
            return desired;

        desired *= -1;

        return CalculateSteering(desired);
    } //IA2-P1
    private static Vector3 GetAlignment(IEnumerable<Boid> boids, Boid currentBoid, float viewRadius, Func<Vector3,Vector3> CalculateSteering)
    {
        //esto por con un WHERE FOOD COLLIDER IS IN BUCKET(MYBUCKET)

        Vector3 desired = boids
            .Where(x => x != currentBoid && Vector3.Distance(currentBoid.transform.position, x.transform.position) < viewRadius)
            .Select(x => x._velocity)
            .Aggregate(Vector3.zero, (x, y) => x + y);

        int count = boids
            .Count(boid => boid != currentBoid && Vector3.Distance(currentBoid.transform.position, boid.transform.position) < viewRadius);

        return count == 0 ? desired : CalculateSteering(desired / count);
    } //IA2-P1

    private void Evade()
    {
        Vector3 desired;

        Vector3 pos = _hunter.transform.position + _hunter.Velocity * Time.deltaTime;

        desired = pos - transform.position;
        desired.Normalize();
        desired *= _maxSpeed;
        desired *= -1;

        Vector3 steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, _maxForce);

        _velocity = Vector3.ClampMagnitude(_velocity + steering, _maxSpeed);
    }
    private Vector3 Arrive(Transform target)
    {
        Vector3 desired = target.position - transform.position;
        if (desired.magnitude <= _viewRadius)
        {
            float speed = _maxSpeed * (desired.magnitude / _viewRadius);
            desired.Normalize();
            desired *= speed;
        }
        else
        {
            desired.Normalize();
            desired *= _maxSpeed;
        }

        Vector3 distance = target.position - transform.position;

        if (distance.magnitude < _collideDistance)
        {
            Food.allFoods.Remove(target.gameObject.GetComponent<Food>());
            Destroy(target.gameObject);
        }

        Vector3 steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, _maxForce);

        return steering;
    }
    Vector3 CalculateSteering(Vector3 desired)
    {
        return Vector3.ClampMagnitude((desired.normalized * _maxSpeed) - _velocity, _maxForce);
    }
    private void AddForce(Vector3 force)
    {
        _velocity = Vector3.ClampMagnitude(_velocity + force, _maxSpeed);
    }
    public Vector3 GetVelocity()
    {
        return _velocity;
    }
    public void CheckCollision()
    {
        Vector3 distance = _hunter.transform.position - transform.position;

        if (distance.magnitude < _collideDistance)
        {
            allBoids.Remove(this);
            Destroy(gameObject);
        }
    }


    private void OnDestroy()
    {
        GameManager.Instance.spatialGrid.RemoveEntityFromGrid(_myGridEntity);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _viewRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _viewRadiusSeparation);
    }
}
