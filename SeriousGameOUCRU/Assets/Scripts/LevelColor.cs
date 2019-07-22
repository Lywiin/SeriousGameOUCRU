using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelColor : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    [Header("Materials")]
    private Camera cam;
    public Material plantMaterial;
    public Material wallBorderMaterial;
    public Material wallMaterial;
    public Material[] backgroundMaterials;

    [Header("Colors")]
    public Color cameraColor;
    public Color plantColor;
    public Color wallCoreColor;
    public Color wallSideColor;
    public Color[] backgroundColors;

    [Header("Textures")]
    public Texture wallTexture;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    // Start is called before the first frame update
    void Start()
    {
        if (CameraController.Instance)
            cam = CameraController.Instance.GetCamera();
        else
            cam = Camera.main;

        UpdateLevelColor();
    }


    /***** COLOR FUNCTIONS *****/

    private void UpdateLevelColor()
    {
        Debug.Log("UPDATE COLOR");

        cam.backgroundColor = cameraColor;
        plantMaterial.SetColor("_PlantColor", plantColor);
        wallBorderMaterial.SetColor("_BorderColor", wallSideColor);

        wallMaterial.SetColor("_CoreColor", wallCoreColor);
        wallMaterial.SetColor("_SideColor", wallSideColor);
        wallMaterial.SetTexture("_WallSprite", wallTexture);

        for (int i = 0; i < backgroundMaterials.Length; i++)
            backgroundMaterials[i].SetColor("_Tint", backgroundColors[i]);
    }
}
