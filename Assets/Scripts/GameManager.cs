using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private float boundHeight = 10;
    [SerializeField] private float boundWidth = 18;


    public Cazador hunter;

    public float BoundHeight { get => boundHeight; set => boundHeight = value; }
    public float BoundWidth { get => boundWidth; set => boundWidth = value; }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    public Vector3 SetObjectBoundPosition(Vector3 pos)
    {
        float z = boundHeight / 2;
        float x = boundWidth / 2;
        if (pos.z > z) pos.z = -z;
        if (pos.z < -z) pos.z = z;
        if (pos.x < -x) pos.x = x;
        if (pos.x > x) pos.x = -x;

        return pos;
    }
    private void OnDrawGizmos()
    {
        float x = boundWidth / 2;
        float z = boundHeight / 2;
        Vector3 topLeft = new Vector3(-x, 0, z);
        Vector3 topRight = new Vector3(x, 0, z);
        Vector3 botRight = new Vector3(x, 0, -z);
        Vector3 botLeft = new Vector3(-x, 0, -z);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, botRight);
        Gizmos.DrawLine(botRight, botLeft);
        Gizmos.DrawLine(botLeft, topLeft);
    }
}
