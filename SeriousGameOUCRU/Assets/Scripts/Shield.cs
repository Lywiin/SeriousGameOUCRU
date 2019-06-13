using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    [Header("Health")]
    public int oneShieldHealth = 20;
    public float shieldGrowthSpeed = 2f;

    public GameObject shield;


    /*** PRIVATE VARIABLES ***/

    // Components
    private BacteriaCell cellScript;
    private SphereCollider bodyCollider;

    // Health
    private int shieldHealth = 0;
    private int shieldMaxHealth = 0;

    // Disolve
    protected Renderer render;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    private void Awake()
    {
        // Initialize components
        render = shield.GetComponent<Renderer>();
        cellScript = transform.GetComponent<BacteriaCell>();
        bodyCollider = transform.GetChild(1).GetComponent<SphereCollider>();
    }

    /***** SHIELD SIZE FUNCTIONS *****/

    // Use to visualize shield health
    public void UpdateShieldSize()
    {
        // Compute new scale
        Vector3 newScale = new Vector3(0.999f, 0.999f, 0.999f);
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
            bodyCollider.radius = 3f * shield.transform.localScale.x;
            Debug.Log(render.bounds.extents.x);
            yield return null;
        }
        cellScript.UpdateCellSize();
    }


    /***** HEALTH FUNCTIONS *****/

    // Called by cell when it mutate stronger
    public void DuplicateShield()
    {
        shieldHealth += oneShieldHealth;
        UpdateShieldMaxHealth();
        UpdateShieldSize();
    }

    public void DamageShield(int dmg)
    {
        shieldHealth -= dmg;

        if (shieldHealth < 0)
        {
            // Keep track of damage left
            int dmgLeft = shieldHealth;
            //If shield health is below 0 we set is back to 0
            shieldHealth = 0;

            // Apply remaining damages to cell
            cellScript.DamageOrganism(Mathf.Abs(dmgLeft));
        }

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
        UpdateShieldMaxHealth();
        UpdateShieldSize(); // Update size when changing health
    }

    public int GetShieldMaxHealth()
    {
        return shieldMaxHealth;
    }

    // Keep in memory the largest health shield had
    public void UpdateShieldMaxHealth()
    {
        if (shieldHealth > shieldMaxHealth)
        {
            shieldMaxHealth = shieldHealth;
        }
    }


    /***** KILL FUNCTIONS *****/

    public void DesactivateShield()
    {
        shield.SetActive(false);
    }

    public void ActivateShield()
    {
        shield.SetActive(true);
        shield.transform.localScale = new Vector3(0.999f, 0.999f, 0.999f);
        bodyCollider.radius = 3f;
        shieldHealth = 0;
        shieldMaxHealth = 0;
    }


    /***** GETTERS FUNCTIONS *****/

    public Renderer GetRenderer()
    {
        return render;
    }

    // Return size of the shield
    public Vector3 GetShieldSize()
    {
        return render.bounds.size;
    }
}
