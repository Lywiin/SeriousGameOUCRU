using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundHolder : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    public Material background1Material;
    // public Material[] backgroundMaterialArray;
    public Shader[] backgroundShaderArray;


    /*** INSTANCE ***/

    private static BackgroundHolder _instance;
    public static BackgroundHolder Instance { get { return _instance; } }


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    private void Awake()
    {
        // Setup the instance
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }

        if (PlayerPrefs.HasKey("Quality"))
        {
            ToggleBackgroundQuality(PlayerPrefs.GetInt("Quality"));
        }
    }


    /***** QUALITY FUNCTIONS *****/

    public void ToggleBackgroundQuality(int quality)
    {
        background1Material.shader = backgroundShaderArray[quality];

        for (int i = 1; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(quality == 1 ? false : true);

        // for (int i = 0; i < backgroundMaterialArray.Length; i++)
        //     backgroundMaterialArray[i].shader = backgroundShaderArray[quality];
    }
}
