using UnityEngine;
using System.Collections;

public class GradientBg : MonoBehaviour
{
    
    public Color topColor = Color.blue;
    public Color bottomColor = Color.white;
    public int gradientLayer = 7;

    // --------------------------------------------------------------------------------------------------------
    //
    private Mesh mesh;
    private Camera mainCam;

    // --------------------------------------------------------------------------------------------------------
    //
    void Awake()
    {
        if (!enabled) return;

        mainCam = FindObjectOfType<Camera>();

        var frustum = DisplayUtils.GetFrustumSizeAtZ(transform.position.z, mainCam);
        var w = frustum.x;
        var h = frustum.y;

        mesh = new Mesh();
        mesh.vertices = new Vector3[4]
                        {new Vector3(-w, h, 1f), new Vector3(w, h, 1f), new Vector3(-w, -h, 1f), new Vector3(w, -h, 1f)};
        mesh.colors = new Color[4] { bottomColor, bottomColor, topColor, topColor, };
        mesh.triangles = new int[6] { 0, 1, 2, 1, 3, 2 };

        var scale = transform.localScale;
        scale.y = 0.6f;
        transform.localScale = scale;
        var pos = transform.position;
        pos.y = frustum.y * -0.5f;
        transform.position = pos;

        Shader shader = Shader.Find("Vertex Colour Only");
        Material mat = new Material(shader);

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<Renderer>().material = mat;
    }

    // --------------------------------------------------------------------------------------------------------
    //
    void Update()
    {
        mesh.colors = new Color[4] { bottomColor, bottomColor, topColor, topColor, };
    }
}