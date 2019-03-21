using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    [Header("Health")]
    public int oneShieldHealth = 20;

    // Private variables
    private int shieldHealth = 0;

    // Start is called before the first frame update
    void Start()
    {
        UpdateShieldSize();
    }

    // Called by bacteria when it mutate stronger
    public void DuplicateShield()
    {
        shieldHealth += oneShieldHealth;
        UpdateShieldSize();
    }

    // Use to visualize shield health
    private void UpdateShieldSize()
    {
        if (shieldHealth == 0)
        {
            transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }else
        {
            float newSize = 1.0f + (float)shieldHealth / 100;
            transform.localScale = new Vector3(newSize, 0.1f, newSize);
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
        Shield s = collision.gameObject.GetComponent<Shield>();

        if (collision.gameObject.CompareTag("BadBacteria") || collision.gameObject.CompareTag("GoodBacteria"))
        {
            // If we collide a bacteria we activate the resistance
            collision.gameObject.GetComponent<Bacteria>().ActivateResistance(shieldHealth);
        }else if (collision.gameObject.CompareTag("Shield"))
        {
            // If we collide with a shield we change the shield health
            collision.gameObject.GetComponent<Shield>().SetShieldHealth(Mathf.Max(shieldHealth, s.GetShieldHealth()));
        }
    }
}
