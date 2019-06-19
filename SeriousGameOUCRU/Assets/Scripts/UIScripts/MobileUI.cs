using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobileUI : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    [Header("Current Weapon")]
    public Image currentWeaponImage;
    public Sprite antibodySprite;
    public Sprite antibioticSprite;

    [Header("Weapon Change Slider")]
    public Image weaponChangeSlider;

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


    /***** COLOR CHANGE FUNCTIONS *****/

    public void ToggleCurrentWeaponImage(bool displayHeavyWeapon)
    {
        if (displayHeavyWeapon)
        {
            currentWeaponImage.sprite = antibioticSprite;
        }else
        {
            currentWeaponImage.sprite = antibodySprite;
        }
    }

    /***** WEAPON SLIDER FUNCTIONS *****/

    public void FillWeaponChangeSlider(float amount)
    {
        if (PlayerController.Instance)
        {
            weaponChangeSlider.transform.position = CameraController.Instance.GetCamera().WorldToScreenPoint(PlayerController.Instance.gameObject.transform.position);

            // Activate slider if amount different from 0
            weaponChangeSlider.gameObject.SetActive(amount == 0f? false: true);

            // Update slider amount
            weaponChangeSlider.fillAmount = amount;
        }
    }
}
