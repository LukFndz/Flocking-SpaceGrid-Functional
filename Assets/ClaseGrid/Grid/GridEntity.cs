using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


//[ExecuteInEditMode]
public class GridEntity : MonoBehaviour
{
	public event Action<GridEntity> OnMove = delegate {};
	public Vector3 velocity = new Vector3(0, 0, 0);
    public bool onGrid;

    SpatialGrid targetGrid;

    public bool canCheck = true;

    private void Start()
    {
        targetGrid = GameManager.Instance.spatialGrid;
    }

    void Update() {

        transform.position += velocity * Time.deltaTime;
	    OnMove(this);
    }

    
    public IEnumerable<GridEntity> GetNearbyEntities(float radius)
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
