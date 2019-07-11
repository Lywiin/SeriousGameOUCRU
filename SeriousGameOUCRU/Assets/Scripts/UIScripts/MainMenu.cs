using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    public Animator animator;

    [Header("Option")]
    public Toggle qualityToggle;

    public Shader[] backgroundShaderArray;
    public Material[] backgroundMaterialArray;

    public Shader[] organismShaderArray;
    public Material[] organismMaterialArray;

    public Shader[] plantShaderArray;
    public Material[] plantMaterialArray;

    public Shader[] shieldShaderArray;
    public Material[] shieldMaterialArray;

    public Shader[] wallBorderShaderArray;
    public Material[] wallBorderMaterialArray;

    public Shader[] wallShaderArray;
    public Material[] wallMaterialArray;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    private void Awake()
    {
        if (!PlayerPrefs.HasKey("Quality"))
        {
            PlayerPrefs.SetInt("Quality", 0);
        }

        // 1 is low quality
        bool lowQuality = PlayerPrefs.GetInt("Quality") == 1;
        ChangeGraphismQuality(lowQuality);
        qualityToggle.isOn = lowQuality;
    }


    /***** SIZE FUNCTIONS *****/

    public void OnPlayClick()
    {
        animator.SetTrigger("FadeToLevelScreen");
    }

    public void CanFadeTrue()
    {
        animator.SetBool("CanFade", true);
    }

    public void CanFadeFalse()
    {
        animator.SetBool("CanFade", false);
    }

    public void ResetFadeTriggers()
    {
        animator.ResetTrigger("FadeToHomeScreen");
        animator.ResetTrigger("FadeToLevelScreen");
        animator.ResetTrigger("FadeToOptionScreen");
    }


    /***** QUALITY FUNCTIONS *****/

    public void OnGraphismQualityChange()
    {
        ChangeGraphismQuality(qualityToggle.isOn);
    }

    private void ChangeGraphismQuality(bool lowQuality)
    {
        int shaderIndex = lowQuality ? 1 : 0;
        PlayerPrefs.SetInt("Quality", shaderIndex);
        
        for (int i = 0; i < backgroundMaterialArray.Length; i++)
            backgroundMaterialArray[i].shader = backgroundShaderArray[shaderIndex];

        for (int i = 0; i < organismMaterialArray.Length; i++)
            organismMaterialArray[i].shader = organismShaderArray[shaderIndex];

        for (int i = 0; i < plantMaterialArray.Length; i++)
            plantMaterialArray[i].shader = plantShaderArray[shaderIndex];

        for (int i = 0; i < shieldMaterialArray.Length; i++)
            shieldMaterialArray[i].shader = shieldShaderArray[shaderIndex];

        for (int i = 0; i < wallBorderMaterialArray.Length; i++)
            wallBorderMaterialArray[i].shader = wallBorderShaderArray[shaderIndex];

        for (int i = 0; i < wallMaterialArray.Length; i++)
            wallMaterialArray[i].shader = wallShaderArray[shaderIndex];
    }
}
