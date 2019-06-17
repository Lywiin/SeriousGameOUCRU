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

    private CloseEnnemyDetection closeEnnemyDetection;
    private PlayerController playerController;
    private CameraController cameraController;


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

    private void Start()
    {
        closeEnnemyDetection = CloseEnnemyDetection.Instance;
        playerController = PlayerController.Instance;
        cameraController = CameraController.Instance;
    }

    private void Update()
    {
        if (closeEnnemyDetection)
        {
            UpdateRootPosition();

            UpdateIndicatorsRotation();
        }
    }


    /***** UI FUNCTIONS *****/

    // Make the root follow player position
    private void UpdateRootPosition()
    {
        if (playerController)
            closeEnnemyIndicatorRoot.transform.position = cameraController.GetCamera().WorldToScreenPoint(playerController.transform.position);
    }

    // Change the rotation of all indicator according to their target
    private void UpdateIndicatorsRotation()
    {
        for (int i = 0; i < closeEnnemyDetection.maxDetectedCount; i++)
        {
            // Get indicator at position i
            // GameObject indicator = closeEnnemyIndicatorRoot.transform.GetChild(i).gameObject;

            // If a target exist at this position
            if (closeEnnemyDetection.GetClosestEnnemiesList().Count > i)
            {
                // We fade in the indicator
                closeEnnemyIndicatorAnimatorList[i].enabled = true;
                closeEnnemyIndicatorAnimatorList[i].SetBool("FadeIn", true);

                // We get the corresponding target
                GameObject ennemy = closeEnnemyDetection.GetClosestEnnemiesList()[i];

                UpdateWindowSprite(i, ennemy);
                
                // Compute new rotation from ennemy and player position
                Quaternion newRot = Quaternion.LookRotation(ennemy.transform.position - playerController.transform.position);
                newRot.z = newRot.y * -1; newRot.y = 0f; // Trick to apply the correct UI rotation

                // Apply rotation
                closeEnnemyIndicatorList[i].transform.rotation = newRot;
            }else
            {
                // If no target available we fade out the indicator
                // StartCoroutine(HideIndicator(i));
                closeEnnemyIndicatorAnimatorList[i].SetBool("FadeIn", false);
            }
        }
    }

    // private IEnumerator HideIndicator(int index)
    // {
    //     closeEnnemyIndicatorAnimatorList[index].SetBool("FadeIn", false);
    //     yield return new WaitForSeconds(0.75f);
    //     closeEnnemyIndicatorAnimatorList[index].enabled = false;

    // }

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
