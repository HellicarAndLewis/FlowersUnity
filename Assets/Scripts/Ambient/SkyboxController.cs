using UnityEngine;
using System.Collections;

public class SkyboxController : MonoBehaviour
{

    public Material skybox;
    public Color SkyTint = new Color(0, 0.2f, 0.5f);
    public Color Ground = new Color(1, 1, 1);
    [Range(0, 8)]
    public float Exposure = 1.75f;
    [Range(0,1)]
    public float SunSize = 0.042f;
    [Range(0, 5)]
    public float AtmosphereThickness = 1.15f;

    void Start()
    {
        UpdateMaterial();
    }

    void Update()
    {
        UpdateMaterial();
    }

    void UpdateMaterial()
    {
        skybox.SetColor("_SkyTint", SkyTint);
        skybox.SetColor("_GroundColor", Ground);
        skybox.SetFloat("_Exposure", Exposure);
        skybox.SetFloat("_SunSize", SunSize);
        skybox.SetFloat("_AtmosphereThickness", AtmosphereThickness);
    }
}
