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

    Vector2 lookAtDirection;


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

        lookAtDirection = Vector2.zero;
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
                lookAtDirection = ennemy.transform.position - playerController.transform.position;
                float angle = Mathf.Atan2(lookAtDirection.y, lookAtDirection.x) * Mathf.Rad2Deg;

                // Apply rotation
                closeEnnemyIndicatorList[i].transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
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
