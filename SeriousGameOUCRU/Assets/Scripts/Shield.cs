using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    [Header("Health")]
    public int oneShieldHealth = 20;
    public float shieldGrowthSpeed = 2f;

    [Header("Conjugaison")]
    public float conjugaisonProba = 0.3f;
    public float recallTime = 3f;

    // Private variables
    private int shieldHealth = 0;
    private bool canCollide = false;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(collidingRecall());
        UpdateShieldSize();
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
        //Debug.Log(shieldHealth);
        // Compute new scale
        Vector3 newScale = new Vector3(0.8f, 0.1f, 0.8f);
        if (shieldHealth > 0)
        {
            newScale.x = newScale.z = 1.0f + (float)shieldHealth / 100;  
        }
        
        // Animate the scale change
        StartCoroutine(RepeatLerp(transform.localScale, newScale, Mathf.Abs(transform.localScale.magnitude - newScale.magnitude) / shieldGrowthSpeed));
    }

    // Do a complete lerp between two vectors
    private IEnumerator RepeatLerp(Vector3 a, Vector3 b, float time)
    {
        float i = 0.0f;
        float rate = (1.0f / time);
        while (i < 1.0f)
        {
            i += Time.deltaTime * rate;
            transform.localScale = Vector3.Lerp(a, b, i);
            yield return null;
        }
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
        UpdateShieldSize();
    }

    // Resistance transmited by conjugation
    private void OnCollisionEnter(Collision collision)
    {
        if (canCollide)
        {
            // Start coroutine to prevent multiColliding
            StartCoroutine(collidingRecall());
            
            // If conjugaison chance is triggered
            if (Random.Range(0, 1) < conjugaisonProba)
            {
                if (collision.gameObject.CompareTag("BadBacteria") || collision.gameObject.CompareTag("GoodBacteria"))
                {
                    // If we collide a bacteria we activate the resistance
                    transform.parent.GetComponent<Bacteria>().ActivateResistance(collision.gameObject.GetComponent<Bacteria>());
                    //collision.gameObject.GetComponent<Bacteria>().ActivateResistance(shieldHealth);
                }else if (collision.gameObject.CompareTag("Shield"))
                {
                    // If we collide with a shield we change the shield health
                    collision.gameObject.GetComponent<Shield>().SetShieldHealth(Mathf.Max(shieldHealth, collision.gameObject.GetComponent<Shield>().GetShieldHealth()));
                }

                // Move away from collided object
                //transform.parent.GetComponent<Bacteria>().MoveAway(collision.gameObject.transform.position);
            }
        }
    }

    private IEnumerator collidingRecall()
    {
        canCollide = false;
        yield return new WaitForSeconds(recallTime); // Time to wait before it can collide again
        canCollide = true;
    }
}
