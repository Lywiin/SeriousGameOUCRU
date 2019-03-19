using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    [Header("Health")]
    public int oneShieldHealth = 20;

    // Private variables
    private int shieldHealth = 0;

    private GameController gameController;

    // Start is called before the first frame update
    void Start()
    {
        // CHANGE THIS LATER
        gameController = Camera.main.GetComponent<GameController>();

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
}
