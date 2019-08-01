using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundHolder : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    [Header("Shaders")]
    public Shader[] backgroundShaderArray;

    [Header("Background 1")]
    public Material background1Material;
    public GameObject background1Root;
    public Sprite[] background1Sprites;


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
        // Change background 1 quality
        background1Material.shader = backgroundShaderArray[quality];
        ChangeBackground1Texture(quality);

        // Disable other background in low quality settings
        for (int i = 1; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(quality == 1 ? false : true);
    }

    // Change texture of all background 1 objects based on quality settings
    private void ChangeBackground1Texture(int quality)
    {
        background1Root.GetComponent<SpriteRenderer>().sprite = background1Sprites[quality];

        for (int i = 0; i < background1Root.transform.childCount; i++)
            background1Root.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = background1Sprites[quality];
    }
}
