using UnityEngine;
using System.Collections;

public class QuickController : MonoBehaviour
{
    //Flower Controls
    [Range(0,1)]
    public float flowerScale = 0;
    TerrainFlowers[] flowers;

    //Terrain Camera Controls
    [Range(35, 41)]
    public float TerrainCamHeight;
    [Range(35, 100)]
    public float TerrainCamFieldOfView;
    public Camera TerrainCam;
    public Camera SecondaryTerrainCam;

    //World Rotation
    public GameObject Terrain;
    private Vector3 TerrainRotationSaved;
    public Vector3 TerrainRotation;

    //Skybox1 Rotation
    public GameObject Skybox1;
    private Vector3 Skybox1RotationSaved;
    public Vector3 Skybox1Rotation;

    //Skybox2 Rotation
    public GameObject Skybox2;
    private Vector3 Skybox2RotationSaved;
    public Vector3 Skybox2Rotation;

    //

    // Use this for initialization
    void Start()
    {
        //Flowers
        flowers = FindObjectsOfType<TerrainFlowers>();

        //Terrain
        TerrainRotationSaved = Terrain.transform.eulerAngles;

        //Skybox1
        Skybox1RotationSaved = Skybox1.transform.eulerAngles;

        //Skybox2
        Skybox2RotationSaved = Skybox2.transform.eulerAngles;

    }

    // Update is called once per frame
    void Update()
    {
        //Flower Controls
        foreach (var flower in flowers)
        {
            flower.flowerAlpha = flowerScale;
        }

        //Terrain cam Controls
        Vector3 pos = TerrainCam.transform.position;
        TerrainCam.transform.position = new Vector3(pos.x, TerrainCamHeight, pos.z);
        TerrainCam.fieldOfView = TerrainCamFieldOfView;
        SecondaryTerrainCam.transform.position = TerrainCam.transform.position;
        SecondaryTerrainCam.fieldOfView = TerrainCam.fieldOfView;
        /*
        //Terrain Rotation Controls
        TerrainRotationSaved.x += TerrainRotation.x;
        TerrainRotationSaved.y += TerrainRotation.y;
        TerrainRotationSaved.z += TerrainRotation.z;

        Terrain.transform.eulerAngles = TerrainRotationSaved;

        //Skybox 1 Rotation Controls
        Skybox1RotationSaved.x += Skybox1Rotation.x;
        Skybox1RotationSaved.y += Skybox1Rotation.y;
        Skybox1RotationSaved.z += Skybox1Rotation.z;

        Skybox1.transform.eulerAngles = Skybox1RotationSaved;

        //Skybox 2 Rotation controls
        Skybox2RotationSaved.x += Skybox2Rotation.x;
        Skybox2RotationSaved.y += Skybox2Rotation.y;
        Skybox2RotationSaved.z += Skybox2Rotation.z;

        Skybox2.transform.eulerAngles = Skybox2RotationSaved;
        */
    }

    void OnTerrainCamHeight(float _val)
    {
        float m = 1.0f;
        float b = 0.0f;
        TerrainCamHeight = m  *_val + b;
    }

    void OnTerrainCamFOV(float _val)
    {
        float m = 1.0f;
        float b = 0.0f;
        TerrainCamFieldOfView = m * _val + b;
    }

    void OnTerrainRotationX(float _val)
    {
        float m = 1.0f;
        float b = 0.0f;
        TerrainRotation.x = m * _val + b;
    }

    void OnTerrainRotationY(float _val)
    {
        float m = 1.0f;
        float b = 0.0f;
        TerrainRotation.y = m * _val + b;
    }

    void OnTerrainRotationZ(float _val)
    {
        float m = 1.0f;
        float b = 0.0f;
        TerrainRotation.z = m * _val + b;
    }

    void OnSkybox1RotationX(float _val)
    {
        float m = 1.0f;
        float b = 0.0f;
        Skybox1Rotation.x = m * _val + b;
    }

    void OnSkybox1RotationY(float _val)
    {
        float m = 1.0f;
        float b = 0.0f;
        Skybox1Rotation.y = m * _val + b;
    }

    void OnSkybox1RotationZ(float _val)
    {
        float m = 1.0f;
        float b = 0.0f;
        Skybox1Rotation.z = m * _val + b;
    }

    void OnSkybox2RotationX(float _val)
    {
        float m = 1.0f;
        float b = 0.0f;
        Skybox2Rotation.x = m * _val + b;
    }

    void OnSkybox2RotationY(float _val)
    {
        float m = 1.0f;
        float b = 0.0f;
        Skybox2Rotation.y = m * _val + b;
    }

    void OnSkybox2RotationZ(float _val)
    {
        float m = 1.0f;
        float b = 0.0f;
        Skybox2Rotation.z = m * _val + b;
    }
}
