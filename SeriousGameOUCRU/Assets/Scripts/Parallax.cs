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
    private Vector2 length;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    void Start()
    {
        startPos = transform.position;
        length = GetComponent<SpriteRenderer>().bounds.size;
    }

    void FixedUpdate()
    {
        float tempX = cam.transform.position.x * (1 - parallaxEffect);
        float tempZ = cam.transform.position.y * (1 - parallaxEffect);

        float distX = cam.transform.position.x * parallaxEffect;
        float distZ = cam.transform.position.y * parallaxEffect;

        // Affect new position
        transform.position = new Vector3(startPos.x + distX, startPos.y + distZ, startPos.z);

        // Adjust X start position
        if (tempX > startPos.x + length.x / 2) startPos.x += length.x;
        else if (tempX < startPos.x - length.x / 2) startPos.x -= length.x;

        // Adjust Y start position
        if (tempZ > startPos.y + length.y / 2) startPos.y += length.y;
        else if (tempZ < startPos.y - length.y / 2) startPos.y -= length.y;
    }
}
