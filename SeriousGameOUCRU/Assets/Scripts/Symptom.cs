using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Symptom : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    [Header("Health")]
    public float symptomHealth = 50f;


    /*** PRIVATE VARIABLES ***/



    /***** MONOBEHAVIOUR FUNCTIONS *****/




    /***** HEALTH FUNCTIONS *****/

    public void DamageSymptom(float dmg)
    {
        symptomHealth -= dmg;

        if (symptomHealth <= 0)
        {
            KillSymptom();
        }
    }

    private void KillSymptom()
    {
        // Inform parent that child died
        GetComponentInParent<Virus>().NotifySymptomDeath();
        
        Destroy(gameObject);
    }
}
