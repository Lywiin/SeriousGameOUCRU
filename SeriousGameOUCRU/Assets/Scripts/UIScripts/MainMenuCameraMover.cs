using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCameraMover : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    public Vector2 moveZone;
    public float speed = 3f;


    /*** PRIVATE VARIABLES ***/

    private Vector3 desiredPos;
    private Vector3 smoothedPosition;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    private void Start()
    {
        // Initialize a desired position
        GetRandomDesiredPos();
        smoothedPosition = Vector3.zero;
    }

    private void Update()
    {
        // Get a new position if gets close to desiredPosition
        if(transform.position == desiredPos)
            GetRandomDesiredPos();

        // Smooth that position to add delay in camera movement
        smoothedPosition = Vector3.MoveTowards(transform.position, desiredPos, speed * Time.deltaTime);

        // Apply new position
        transform.position = smoothedPosition;
    }


    /***** POSITION FUNCTIONS *****/

    private void GetRandomDesiredPos()
    {
        desiredPos = new Vector3(Random.Range(-moveZone.x, moveZone.x), Random.Range(-moveZone.y, moveZone.y), -10f);
    }
}
