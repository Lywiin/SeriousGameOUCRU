using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float slowDownFactor;
    public float accelFactor;
    public float maxVelocity;

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        float moveHor = Input.GetAxis("Horizontal");
        float moveVer = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHor, 0.0f, moveVer);

        rb.AddForce(movement * ((maxVelocity - rb.velocity.magnitude) * accelFactor) * speed);
        rb.velocity = Vector3.ClampMagnitude(rb.velocity * slowDownFactor, maxVelocity);

        Debug.Log((maxVelocity - rb.velocity.magnitude));
    }
}
