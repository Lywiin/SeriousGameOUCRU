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

    void Update()
    {
        TryToDetectClosestEnnemy(); 
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
            // Convert collided object world position to screen to know if it's visible, then convert it to a boolean
            Vector3 screenPoint = CameraController.Instance.GetCamera().WorldToViewportPoint(c.gameObject.transform.position);
            bool onScreen = screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

            // If an ennemy is visible we don't want to display anything 
            if (onScreen)
            {
                // If one ennemy is on screen we can reset the search radius
                detectionRadius = defaultDetectionRadius;
                detectedEnnemiesDictionnary.Clear();
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

        // Add gameobject to the final list if still some to add and closestEnnemiesList not full
        for (int i = 0; i < maxDetectedCount; i++)
        {
            if (distanceList.Count > i)
            {
                closestEnnemiesList.Add(detectedEnnemiesDictionnary[distanceList[i]]);
            }else
            {
                break;
            }
        }
    }

    public List<GameObject> GetClosestEnnemiesList()
    {
        return closestEnnemiesList;
    }
}
