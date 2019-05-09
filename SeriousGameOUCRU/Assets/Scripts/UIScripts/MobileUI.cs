using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobileUI : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    public Image weaponTempImage;


    /*** PRIVATE VARIABLES ***/


    /*** INSTANCE ***/

    private static MobileUI _instance;
    public static MobileUI Instance { get { return _instance; } }


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    private void Start()
    {
        weaponTempImage.color = Color.green;
    }

    
    /***** COLOR CHANGE FUNCTIONS *****/

    public void ToggleImageColor(bool displayHeavyWeaponColor)
    {
        if (displayHeavyWeaponColor)
        {
            weaponTempImage.color = Color.magenta;
        }else
        {
            weaponTempImage.color = Color.green;
        }
    }
}
