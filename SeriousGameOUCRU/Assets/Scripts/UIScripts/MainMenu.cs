using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    public Animator animator;

    [Header("Option")]
    public Toggle qualityToggle;

    public GameObject backgroundHolderPrefab;

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

    [Header("Level Selection")]
    public Button[] levelButtonArray;
    public TextMeshProUGUI[] levelTextArray;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    private void Awake()
    {
        if (!PlayerPrefs.HasKey("Quality"))
        {
            PlayerPrefs.SetInt("Quality", 0);
        }

        if (!PlayerPrefs.HasKey("CurrentLevel"))
        {
            PlayerPrefs.SetInt("CurrentLevel", 0);
        }

        // 1 is low quality
        bool lowQuality = PlayerPrefs.GetInt("Quality") == 1;
        qualityToggle.isOn = lowQuality;

        DesactivateUncompletedLevels();
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

        if (BackgroundHolder.Instance) BackgroundHolder.Instance.ToggleBackgroundQuality(shaderIndex);

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


    /***** LEVEL FUNCTIONS *****/

    private void DesactivateUncompletedLevels()
    {
        int currentLevel = PlayerPrefs.GetInt("CurrentLevel");

        for (int i = 0; i < levelButtonArray.Length; i++)
        {
            if (currentLevel < i)
            {
                levelButtonArray[i].interactable = false;
                levelTextArray[i].color = new Color(1f, 1f, 1f, 0.5f);
            }
        }
    }


    /***** SOUNDS FUNCTIONS *****/

    public void OnSlide()
    {
        if (AudioManager.Instance) AudioManager.Instance.Play("Slide");
    }

    public void OnSelect1()
    {
        if (AudioManager.Instance) AudioManager.Instance.Play("Select1");
    }

    public void OnSelect2()
    {
        if (AudioManager.Instance) AudioManager.Instance.Play("Select2");
    }


    /***** LANGUAGE FUNCTIONS *****/

    public void SwitchLanguage(string newLanguage)
    {
        TextLocalization.ChangeLanguage(newLanguage);
    }
}
