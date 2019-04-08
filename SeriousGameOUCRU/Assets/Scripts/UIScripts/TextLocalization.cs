using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum Languages
{
    English,
    Vietnamese,
    French
}

public class TextLocalization : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    // Text for each languages
    public string textEnglish;
    public string textVietnamese;
    public string textFrench;

    // Current language
    public static Languages selectedLanguage = Languages.English;

    // List of all instances of localized text
    public static List<TextLocalization> localizationInstancesList = new List<TextLocalization>();


    /*** PRIVATE VARIABLES ***/
    GameObject myEventSystem;

    /***** MONOBEHAVIOUR FUNCTIONS *****/

    private void Awake()
    {
        // Add instance and update text for initialization
        localizationInstancesList.Add(this);
        UpdateText();
    }

    private void Start()
    {
        // Initialize event system
        myEventSystem = GameObject.Find("EventSystem");
    }


    /***** LANGUAGE FUNCTIONS *****/

    // Change the text of the current object according to current language
    public void UpdateText()
    {
        switch (selectedLanguage)
        {
            case Languages.English:
                transform.GetComponent<TextMeshProUGUI>().text = textEnglish;
                break;
            case Languages.Vietnamese:
                transform.GetComponent<TextMeshProUGUI>().text = textVietnamese;
                break;
            case Languages.French:
                transform.GetComponent<TextMeshProUGUI>().text = textFrench;
                break;
        }
    }

    public void SwitchLanguageToEnglish()
    {
        TextLocalization.ChangeLanguage(Languages.English);
        UnselectButton();
    }
    public void SwitchLanguageToVietnamese()
    {
        TextLocalization.ChangeLanguage(Languages.Vietnamese);
        UnselectButton();
    }
    public void SwitchLanguageToFrench()
    {
        TextLocalization.ChangeLanguage(Languages.French);
        UnselectButton();
    }

    // Change the language
    public static void ChangeLanguage(Languages l)
    {
        selectedLanguage = l;
        TextLocalization.UpdateAllText();
    }

    // Unselect all ui object
    private void UnselectButton()
    {
        myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
    }

    // Update text of all objects, used after changing language
    public static void UpdateAllText()
    {
        foreach (TextLocalization t in localizationInstancesList)
        {
            t.UpdateText();
        }
    }
}
