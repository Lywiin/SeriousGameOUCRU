using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseEnnemyUI : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    public GameObject closeEnnemyIndicatorRoot;
    public GameObject closeEnnemyIndicatorPrefab;

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

    private void Start()
    {
        InitIndicators();
    }

    private void Update()
    {
        //Debug.Log(CameraController.Instance.GetCamera().WorldToScreenPoint(PlayerController.Instance.transform.position));
        UpdateRootPosition();

        UpdateIndicatorsRotation();
    }

    private void InitIndicators()
    {
        for (int i = 0; i < CloseEnnemyDetection.Instance.maxDetectedCount; i++)
        {
            GameObject indicator = Instantiate(closeEnnemyIndicatorPrefab, closeEnnemyIndicatorRoot.transform.position, Quaternion.identity);
            indicator.transform.SetParent(closeEnnemyIndicatorRoot.transform);
        }
    }

    private void UpdateRootPosition()
    {
        if (PlayerController.Instance)
            closeEnnemyIndicatorRoot.transform.position = CameraController.Instance.GetCamera().WorldToScreenPoint(PlayerController.Instance.transform.position);
    }

    private void UpdateIndicatorsRotation()
    {
        for (int i = 0; i < CloseEnnemyDetection.Instance.maxDetectedCount; i++)
        {
            if (CloseEnnemyDetection.Instance.GetClosestEnnemiesList().Count > i)
            {
                closeEnnemyIndicatorRoot.transform.GetChild(i).gameObject.SetActive(true);

                GameObject ennemy = CloseEnnemyDetection.Instance.GetClosestEnnemiesList()[i];
                
                // Compute new rotation
                Quaternion newRot = Quaternion.LookRotation(ennemy.transform.position - PlayerController.Instance.transform.position);
                newRot.z = newRot.y * -1; newRot.y = 0f;

                // Apply rotation
                closeEnnemyIndicatorRoot.transform.GetChild(i).transform.rotation = newRot;
            }else
            {
                closeEnnemyIndicatorRoot.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
