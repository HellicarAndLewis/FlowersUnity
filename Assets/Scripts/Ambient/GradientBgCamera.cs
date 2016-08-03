using UnityEngine;
using System.Collections;

public class GradientBgCamera : MonoBehaviour
{
    public Camera mainCamera;
    public Color topColor = Color.blue;
    public Color bottomColor = Color.white;
    public int gradientLayer = 7;

    // --------------------------------------------------------------------------------------------------------
    //
    private Mesh mesh;

    // --------------------------------------------------------------------------------------------------------
    //
    void Awake()
    {
        if (!enabled) return;
        gradientLayer = Mathf.Clamp(gradientLayer, 0, 31);
        if (!mainCamera)
        {
            Debug.LogError("Must attach GradientBackground script to the camera");
            return;
        }

        mainCamera.clearFlags = CameraClearFlags.Depth;
        mainCamera.cullingMask = mainCamera.cullingMask & ~(1 << gradientLayer);

        var gradientGO = new GameObject("Gradient Cam", typeof(Camera));
        Camera gradientCam = gradientGO.GetComponent<Camera>();
        gradientCam.depth = mainCamera.depth - 1;
        gradientCam.cullingMask = 1 << gradientLayer;

        mesh = new Mesh();
        mesh.vertices = new Vector3[4]
                        {new Vector3(-100f, .577f, 1f), new Vector3(100f, .577f, 1f), new Vector3(-100f, -.577f, 1f), new Vector3(100f, -.577f, 1f)};
        mesh.colors = new Color[4] { topColor, topColor, bottomColor, bottomColor };
        mesh.triangles = new int[6] { 0, 1, 2, 1, 3, 2 };

        Shader shader = Shader.Find("Vertex Colour Only");
        Material mat = new Material(shader);
        GameObject gradientPlane = new GameObject("Gradient Plane", typeof(MeshFilter), typeof(MeshRenderer));

        gradientPlane.GetComponent<MeshFilter>().mesh = mesh;
        gradientPlane.GetComponent<Renderer>().material = mat;
        gradientPlane.layer = gradientLayer;
    }

    // --------------------------------------------------------------------------------------------------------
    //
    void Update()
    {
        mesh.vertices = new Vector3[4]
                        {new Vector3(-100f, .577f, 1f), new Vector3(100f, .577f, 1f), new Vector3(-100f, -.577f, 1f), new Vector3(100f, -.577f, 1f)};
        mesh.colors = new Color[4] { topColor, topColor, bottomColor, bottomColor };
        mesh.triangles = new int[6] { 0, 1, 2, 1, 3, 2 };
        mesh.colors = new Color[4] { topColor, topColor, bottomColor, bottomColor };
    }
}