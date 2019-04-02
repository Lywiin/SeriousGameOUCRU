using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    public GameObject cam;
    public float parallaxEffect;


    /*** PRIVATE VARIABLES ***/

    private Vector3 startPos;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    void Start()
    {
        startPos = transform.position;
    }

    void FixedUpdate()
    {
        float newX = cam.transform.position.x * parallaxEffect;
        float newZ = cam.transform.position.z * parallaxEffect;

        transform.position = new Vector3(startPos.x + newX, transform.position.y, startPos.z + newZ);
    }
}
