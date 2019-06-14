using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CloseEnnemyUI : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    [Header("GameObject")]
    public GameObject closeEnnemyIndicatorRoot;
    // public GameObject closeEnnemyIndicatorPrefab;
    public List<GameObject> closeEnnemyIndicatorList;
    public List<Animator> closeEnnemyIndicatorAnimatorList;
    public List<Image> closeEnnemyIndicatorImageList;

    [Header("Sprite")]
    public Sprite indicatorWindowBacteriaSprite;
    public Sprite indicatorWindowVirusSprite;


    /*** PRIVATE VARIABLES ***/



    /*** INSTANCE ***/

    private static CloseEnnemyUI _instance;
    public static CloseEnnemyUI Instance { get { return _instance; } }


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

    // private void Start()
    // {
    //     // InitIndicators();
    // }

    private void Update()
    {
        if (CloseEnnemyDetection.Instance)
        {
            UpdateRootPosition();

            UpdateIndicatorsRotation();
        }
    }


    /***** UI FUNCTIONS *****/

    // Instantiate indicators according to the desired number and parent them to the root
    // private void InitIndicators()
    // {
    //     // for (int i = 0; i < CloseEnnemyDetection.Instance.maxDetectedCount; i++)
    //     // {
    //     //     GameObject indicator = Instantiate(closeEnnemyIndicatorPrefab, closeEnnemyIndicatorRoot.transform.position, Quaternion.identity);
    //     //     indicator.transform.SetParent(closeEnnemyIndicatorRoot.transform);
    //     // }
    // }

    // Make the root follow player position
    private void UpdateRootPosition()
    {
        if (PlayerController.Instance)
            closeEnnemyIndicatorRoot.transform.position = CameraController.Instance.GetCamera().WorldToScreenPoint(PlayerController.Instance.transform.position);
    }

    // Change the rotation of all indicator according to their target
    private void UpdateIndicatorsRotation()
    {
        for (int i = 0; i < CloseEnnemyDetection.Instance.maxDetectedCount; i++)
        {
            // Get indicator at position i
            // GameObject indicator = closeEnnemyIndicatorRoot.transform.GetChild(i).gameObject;

            // If a target exist at this position
            if (CloseEnnemyDetection.Instance.GetClosestEnnemiesList().Count > i)
            {
                // We fade in the indicator
                closeEnnemyIndicatorAnimatorList[i].SetBool("FadeIn", true);

                // We get the corresponding target
                GameObject ennemy = CloseEnnemyDetection.Instance.GetClosestEnnemiesList()[i];

                UpdateWindowSprite(i, ennemy);
                
                // Compute new rotation from ennemy and player position
                Quaternion newRot = Quaternion.LookRotation(ennemy.transform.position - PlayerController.Instance.transform.position);
                newRot.z = newRot.y * -1; newRot.y = 0f; // Trick to apply the correct UI rotation

                // Apply rotation
                closeEnnemyIndicatorList[i].transform.rotation = newRot;
            }else
            {
                // If no target available we fade out the indicator
                closeEnnemyIndicatorAnimatorList[i].SetBool("FadeIn", false);
            }
        }
    }

    public void HideAllIndicators()
    {
        closeEnnemyIndicatorRoot.SetActive(false);
    }

    // Update window sprite according to target type
    private void UpdateWindowSprite(int indicatorIndex, GameObject ennemy)
    {
        if (ennemy.GetComponentInParent<Virus>())
        {
            closeEnnemyIndicatorImageList[indicatorIndex].sprite = indicatorWindowVirusSprite;
        }else
        {
            closeEnnemyIndicatorImageList[indicatorIndex].sprite = indicatorWindowBacteriaSprite;
        }
    }
}
