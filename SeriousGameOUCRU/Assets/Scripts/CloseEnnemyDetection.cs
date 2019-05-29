using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseEnnemyDetection : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    public int maxDetectedCount = 1;


    /*** PRIVATE VARIABLES ***/

    // Dictionnary containing all closest ennemies with their distance to the player
    private List<GameObject> closestEnnemiesList;
    private SortedDictionary<float, GameObject> detectedEnnemiesDictionnary;

    private float defaultDetectionRadius;
    private float detectionRadius;


    /*** INSTANCE ***/

    private static CloseEnnemyDetection _instance;
    public static CloseEnnemyDetection Instance { get { return _instance; } }

private bool canTouch = true;

    /***** MONOBEHAVIOUR FUNCTIONS *****/

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }

        closestEnnemiesList = new List<GameObject>();
        detectedEnnemiesDictionnary = new SortedDictionary<float, GameObject>();

        defaultDetectionRadius = 50f;
        detectionRadius = defaultDetectionRadius;
    }

    void Start()
    {
        
    }

    void Update()
    {
        // if (Input.touchCount >= 2 && canTouch)
        // {
        //     StartCoroutine(TouchRecall()); 
        //     TryToDetectClosestEnnemy(); 
        // }
        TryToDetectClosestEnnemy(); 
    }

    private IEnumerator TouchRecall()
    {
        canTouch = false;
        yield return new WaitForSeconds(1f);
        canTouch = true;
    }


    /***** DETECTION FUNCTIONS *****/

    private void TryToDetectClosestEnnemy()
    {
        // Clear both list every try
        closestEnnemiesList.Clear();
        detectedEnnemiesDictionnary.Clear();

        // Return an array of collided object within detectionRange
        Collider[] hitColliders = DetectColliders();
        int ennemiesCount = BacteriaCell.bacteriaCellList.Count + Virus.virusList.Count;

        // While we hit lass than maxDectedCount object or we detected all ennemies
        while((hitColliders.Length < maxDetectedCount || hitColliders.Length == ennemiesCount) && detectionRadius < 500f)
        {
            // Increase radius each try
            detectionRadius += 10f;
            hitColliders = DetectColliders();
        }

        // Update list of close targets
        RefeshClosestTargets(hitColliders);
    }

    private Collider[] DetectColliders()
    {
        return Physics.OverlapSphere(transform.position, detectionRadius, 1 << LayerMask.NameToLayer("Ennemy"), QueryTriggerInteraction.Ignore);
    }

    // Refresh the list of closest ennemies
    private void RefeshClosestTargets(Collider[] hitColliders)
    {
        // Go through all object to find the closest one
        foreach (Collider c in hitColliders)
        {
//Debug.Log(c.gameObject);
            // Convert collided object world position to screen to know if it's visible, then convert it to a boolean
            Vector3 screenPoint = CameraController.Instance.GetCamera().WorldToViewportPoint(c.ClosestPointOnBounds(transform.position));
            bool onScreen = screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

            // If the object touched is targetable we compute his distance to the player
            if (c.CompareTag("Targetable") )
            {
                // If at least one detected object is visible we don't need any indicator so no need to go further
                if (onScreen)
                {
                    // At least one bacteria is close so we reset the radius to cover the smallest area needed
                    detectionRadius = defaultDetectionRadius;
                    detectedEnnemiesDictionnary.Clear();
// Debug.Log("CANCEL");
// Debug.DrawLine(transform.position, c.gameObject.transform.position, Color.green, 1.5f);
                    break;
                }else
                {
                    // Compute distance from player
                    Vector3 distance = c.ClosestPointOnBounds(transform.position) - transform.position;
                    float sqrDistance = distance.sqrMagnitude;

                    // Add the object to the dictonnary
                    detectedEnnemiesDictionnary.Add(sqrDistance, c.gameObject);
                }

            }
        }

//Debug.Log(detectedEnnemiesDictionnary.Count);
        if (detectedEnnemiesDictionnary.Count > 0)
        {
            // After adding all the element we sort the tab if not empty
            SortDetectedEnnemiesDictionnary();
        }
    }

    // Sort and strip closest ennemy dictionnary to get final result
    private void SortDetectedEnnemiesDictionnary()
    {
        // Convert dictionnary to list and sort it in ascending order
        List<float> distanceList = detectedEnnemiesDictionnary.Keys.ToList();
        distanceList.Sort();

// StartCoroutine(DebugDraw(distanceList));
// List<float> tempDistanceList = new List<float>();

        // Add gameobject to the final list if still some to add and closestEnnemiesList not full
        for (int i = 0; i < maxDetectedCount; i++)
        {
            if (distanceList.Count > i)
            {
                closestEnnemiesList.Add(detectedEnnemiesDictionnary[distanceList[i]]);
//Debug.Log(closestEnnemiesList[i]);
// tempDistanceList.Add(distanceList[i]);
            }else
            {
                break;
            }
        }
//StartCoroutine(DebugDraw(distanceList));
    }

    // private IEnumerator DebugDraw(List<float> list)
    // {
    //     foreach(float distance in list)
    //     {
    //         GameObject g = detectedEnnemiesDictionnary[distance];
    //         Debug.DrawLine(transform.position, g.transform.position, Color.blue, 1.5f);
    //         yield return new WaitForSeconds(1.5f);
    //     }
    // }

    public List<GameObject> GetClosestEnnemiesList()
    {
        return closestEnnemiesList;
    }
}
