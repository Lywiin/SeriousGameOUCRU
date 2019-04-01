using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    [Header("Light Shake")]
    public float lightShakeDuration = 0.1f;
    public float lightShakeSpeed = 1f;
    public float lightShakeMagnitude = 0.4f;
    
    [Header("Heavy Shake")]
    public float heavyShakeDuration = 0.1f;
    public float heavyShakeSpeed = 1f;
    public float heavyShakeMagnitude = 0.4f;


    /*** INSTANCE ***/

    private static CameraShake _instance;
    public static CameraShake Instance { get { return _instance; } }


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


    /***** SHAKE FUNCTIONS *****/

    // Function use to shake the camera
    public static IEnumerator Shake(float duration, float speed, float magnitude)
    {
        // Keep track of the original position
        Vector3 originalPos = Instance.transform.localPosition;

        // Define where we start from for the perlin noise
        float randomStart = Random.Range(-1000f, 1000f);

        // Time elapsed from the start of the shaking to the end
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Update the percentage of completion of the screen shake
            float percentComplete = elapsed / duration;

            // Allows to smooth the screen shake while it gets closer to the end
            float damper = 1.0f - Mathf.Clamp(2.0f * percentComplete - 1.0f, 0.0f, 1.0f);

            // Compute the noise parameter
            float alpha = randomStart + speed * percentComplete;

            // Use that parameter to get the perlin noise at that point, then map the noise between -1 and 1
            float x = Mathf.PerlinNoise(alpha, 0.0f) * 2.0f - 1.0f;
            float z = Mathf.PerlinNoise(0.0f, alpha) * 2.0f - 1.0f;

            // Apply the magnitude and the damper
            x *= magnitude * damper;
            z *= magnitude * damper;

            // We use z on y coordinate because cameraHolder is rotated 90 degrees on x
            Instance.transform.localPosition = new Vector3(x, z, originalPos.z);

            yield return null;
        }

        Instance.transform.localPosition = originalPos;
    }

    // Do a light screenShake
    public void LightScreenShake()
    {
        StartCoroutine(Shake(lightShakeDuration, lightShakeSpeed, lightShakeMagnitude));
    }

    // Do a heavy screenShake
    public void HeavyScreenShake()
    {
        StartCoroutine(Shake(heavyShakeDuration, heavyShakeSpeed, heavyShakeMagnitude));
    }
}
