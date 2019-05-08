using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobileUI : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    public GameObject weaponToggleButton;


    /*** PRIVATE VARIABLES ***/

    private Image buttonSprite;
    private bool displayHeavyWeaponColor = false;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    void Start()
    {
        // Init of the sprite
        buttonSprite = weaponToggleButton.GetComponent<Image>();
        buttonSprite.color = Color.magenta;
    }

    
    /***** COLOR CHANGE FUNCTIONS *****/

    public void ToggleHeavyWeaponSprite()
    {
        displayHeavyWeaponColor = !displayHeavyWeaponColor;

        if (displayHeavyWeaponColor)
        {
            buttonSprite.color = Color.magenta;
        }else
        {
            buttonSprite.color = Color.green;
        }
    }
}
