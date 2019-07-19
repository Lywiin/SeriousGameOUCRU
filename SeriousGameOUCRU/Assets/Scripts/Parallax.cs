using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    public GameObject cam;


    /*** PRIVATE VARIABLES ***/

    private Vector3 startPos;
    private Vector2 length;

    private float parallaxEffect;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    void Start()
    {
        startPos = transform.position;
        length = GetComponent<SpriteRenderer>().bounds.size;

        parallaxEffect = GetComponent<SpriteRenderer>().sharedMaterial.GetFloat("_ParallaxEffect");
    }

    void Update()
    {
        float tempX = cam.transform.position.x;
        float tempZ = cam.transform.position.y;

        // Adjust X start position
        if (tempX > startPos.x + length.x / 2)
        {
            startPos.x += length.x; 
            Replace();
        }else if (tempX < startPos.x - length.x / 2) 
        {
            startPos.x -= length.x; 
            Replace();
        }

        // Adjust Y start position
        if (tempZ > startPos.y + length.y / 2) 
        {
            startPos.y += length.y;
            Replace();
            
        }
        else if (tempZ < startPos.y - length.y / 2) 
        {
            startPos.y -= length.y;
            Replace();
        }
    }

    void Replace()
    {
        transform.position = startPos;
    }
}
