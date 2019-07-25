using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextLocalization : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    // Text for each languages
    public string textEnglish;
    public string textVietnamese;
    public string textFrench;

    // List of all instances of localized text
    public static List<TextLocalization> localizationInstancesList = new List<TextLocalization>();


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    private void Awake()
    {
        // Add the default language in the pref the first time
        if (!PlayerPrefs.HasKey("Language"))
        {
            PlayerPrefs.SetString("Language", "English");
        }

        // UpdateText();
    }

    private void Start()
    {
        // Initialize event system
        // myEventSystem = GameObject.Find("EventSystem");
    }

    private void OnEnable()
    {
        localizationInstancesList.Add(this);
        UpdateText();
    }
    private void OnDisable()
    {
        localizationInstancesList.Remove(this);
    }


    /***** LANGUAGE FUNCTIONS *****/

    // Change the text of the current object according to current language
    public void UpdateText()
    {
        switch (PlayerPrefs.GetString("Language"))
        {
            case "English":
                transform.GetComponent<TextMeshProUGUI>().text = textEnglish;
                break;
            case "Vietnamese":
                transform.GetComponent<TextMeshProUGUI>().text = textVietnamese;
                break;
            case "French":
                transform.GetComponent<TextMeshProUGUI>().text = textFrench;
                break;
        }
    }

    // public void SwitchLanguage(string newLanguage)
    // {
    //     // if (string.Compare(PlayerPrefs.GetString("Language"), newLanguage) != 0)
    //     AudioManager.Instance.Play("Select2");

    //     TextLocalization.ChangeLanguage(newLanguage);
    //     UnselectButton();
    // }

    // public void SwitchLanguageToEnglish()
    // {
    //     AudioManager.Instance.Play("Select1");
    //     TextLocalization.ChangeLanguage("English");
    //     UnselectButton();
    // }
    // public void SwitchLanguageToVietnamese()
    // {
    //     AudioManager.Instance.Play("Select1");
    //     TextLocalization.ChangeLanguage("Vietnamese");
    //     UnselectButton();
    // }
    // public void SwitchLanguageToFrench()
    // {
    //     AudioManager.Instance.Play("Select1");
    //     TextLocalization.ChangeLanguage("French");
    //     UnselectButton();
    // }

    // Change the language
    public static void ChangeLanguage(string l)
    {
        AudioManager.Instance.Play("Select2");

        // Change language in the preference
        PlayerPrefs.SetString("Language", l);

        TextLocalization.UpdateAllText();

        UnselectButton();
    }

    // Unselect all ui object
    private static void UnselectButton()
    {
        GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
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
