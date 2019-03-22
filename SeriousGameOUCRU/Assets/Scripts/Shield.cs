using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    [Header("Health")]
    public int oneShieldHealth = 20;
    public float shieldGrowthSpeed = 2f;

    [Header("Shield")]
    public GameObject shield;

    // Private variables
    private int shieldHealth = 0;
    private bool canCollide = false;

    private BadBacteria bacteriaScript;

    // Start is called before the first frame update
    void Start()
    {
        shield = transform.GetChild(0).gameObject;
        bacteriaScript = transform.GetComponent<BadBacteria>();

        // Prevent duplication on spawn
        //StartCoroutine(collidingRecall());

        // Update shield to init size
        //UpdateShieldSize();
    }

    // Called by bacteria when it mutate stronger
    public void DuplicateShield()
    {
        shieldHealth += oneShieldHealth;
        UpdateShieldSize();
    }

    // Use to visualize shield health
    public void UpdateShieldSize()
    {
        // Compute new scale
        Vector3 newScale = new Vector3(0.8f, 0.1f, 0.8f);
        if (shieldHealth > 0)
        {
            newScale.x = newScale.z = 1.0f + (float)shieldHealth / 100;  
        }
        
        // Animate the scale change
        StartCoroutine(RepeatLerp(shield.transform.localScale, newScale, Mathf.Abs(shield.transform.localScale.magnitude - newScale.magnitude) / shieldGrowthSpeed));
    }

    // Do a complete lerp between two vectors
    private IEnumerator RepeatLerp(Vector3 a, Vector3 b, float time)
    {
        float i = 0.0f;
        float rate = (1.0f / time);
        while (i < 1.0f)
        {
            i += Time.deltaTime * rate;
            shield.transform.localScale = Vector3.Lerp(a, b, i);
            yield return null;
        }
        bacteriaScript.UpdateBacteriaSize();
    }

    public void DamageShield(int dmg)
    {
        //Apply damage to shield's health
        shieldHealth -= dmg;

        //If shield health is below 0 we set is back to 0
        shieldHealth = Mathf.Max(0, shieldHealth);

        //Change shield size according to health
        UpdateShieldSize();
    }

    public int GetShieldHealth()
    {
        return shieldHealth;
    }

    public void SetShieldHealth(int h)
    {
        shieldHealth = h;
        UpdateShieldSize(); // Update size when changing health
    }
}
