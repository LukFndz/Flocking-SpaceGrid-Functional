using System;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class GridEntity : MonoBehaviour
{
	public event Action<GridEntity> OnMove = delegate {};
	public Vector3 velocity = new Vector3(0, 0, 0);
    public bool onGrid;
    Renderer _rend;

    SpatialGrid targetGrid;

    public bool canCheck = true;

    private void Start()
    {
        _rend = GetComponent<Renderer>();
        targetGrid = GameManager.Instance.spatialGrid;
        //targetGrid = GetComponentInParent<SpatialGrid>(); //solo porque soy hijo de grid
    }

    void Update() {
        //if (onGrid)
        //    _rend.material.color = Color.red;
        //else
        //    _rend.material.color = Color.gray;
        //Optimization: Hacer esto solo cuando realmente se mueve y no en el update
        transform.position += velocity * Time.deltaTime;
	    OnMove(this);

        //if (canCheck && Input.GetKeyDown(KeyCode.Space))
        //{
        //    foreach (var item in GetNearby())
        //    {
        //        print("pinto verde" + item);
        //        item._rend.material.color = Color.red;
        //    }
        //}
    }

    
    public IEnumerable<GridEntity> GetNearby(float radius)
    {
        return targetGrid.Query(
                transform.position + new Vector3(-radius, 0, -radius),
                transform.position + new Vector3(radius, 0, radius),
                x => {
                    var position2d = x - transform.position;
                    position2d.y = 0;
                    return position2d.sqrMagnitude < radius * radius;
                });
    }

    private void OnDrawGizmos()
    {
        //if (!canCheck)
        //{
        //    return;
        //}
        //else
        //{
        //    Gizmos.color = Color.red;
        //    Gizmos.DrawWireSphere(transform.position, radius);
        //}
    }
}
