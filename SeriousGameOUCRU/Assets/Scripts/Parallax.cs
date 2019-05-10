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
    private float lenX = 100f;
    private float lenZ = 50f;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    void Start()
    {
        startPos = transform.position;
    }

    void FixedUpdate()
    {
        float tempX = cam.transform.position.x * (1 - parallaxEffect);
        float tempZ = cam.transform.position.z * (1 - parallaxEffect);

        float distX = cam.transform.position.x * parallaxEffect;
        float distZ = cam.transform.position.z * parallaxEffect;

        // Affect new position
        transform.position = new Vector3(startPos.x + distX, transform.position.y, startPos.z + distZ);

        // Adjust X start position
        if (tempX > startPos.x + lenX / 2) startPos.x += lenX;
        else if (tempX < startPos.x - lenX / 2) startPos.x -= lenX;

        // Adjust Z start position
        if (tempZ > startPos.z + lenZ / 2) startPos.z += lenZ;
        else if (tempZ < startPos.z - lenZ / 2) startPos.z -= lenZ;
    }
}
