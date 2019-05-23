using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoodBacteriaGroup : MonoBehaviour
{
    void Update()
    {
        // Destroy himself if doesn't have anymore child
        if (transform.childCount == 0)
        {
            Destroy(gameObject);
        }
    }
}
