using UnityEngine;
using System.Collections;

/// <summary>
/// Deforms a terrain mesh by manupulating its vertices
/// </summary>
public class MeshDeformer : MonoBehaviour
{

    public MeshFilter baseMesh;

    // --------------------------------------------------------------------------------------------------------
    // Common public
    [Range(0, 10)]
    public float terrainScale = 1;
    [Range(0, 2)]
    public float timeScale = 1;
    [Range(0, 1f)]
	public float posNoiseInScale = 0.001f;
    [Range(0, 100f)]
    public float posNoiseOutScale = 10f;
    public float deformThresholdY = -999;

    public Vector3 noiseOutScale = Vector3.one;
    
    // --------------------------------------------------------------------------------------------------------
    // Common protected
    protected MeshFilter meshFilter;
    protected Mesh mesh;
    protected Renderer meshRenderer;
    protected Vector3[] baseVertices;
    protected Vector3[] baseNormals;
    protected fftAnalyzer fft;

    private Camera cam;
    private Plane[] planes;
    private BoxCollider collider;


    // --------------------------------------------------------------------------------------------------------
    //
    void Awake()
	{
        if (!baseMesh) {
            baseMesh = gameObject.GetComponent<MeshFilter>();
            if (!baseMesh)
            {
                Debug.LogError("You need to set a mesh filter!");
                return;
            }
		}
        var meshGo = new GameObject("Base Mesh", typeof(MeshFilter));
        meshGo.transform.parent = transform;
        var meshFilter = meshGo.GetComponent<MeshFilter>();
        meshFilter.mesh = baseMesh.mesh;
        mesh = baseMesh.mesh;

        //baseMesh.gameObject.SetActive(false);
		baseVertices = mesh.vertices;
		baseNormals = mesh.normals;
        meshFilter = GetComponent<MeshFilter>();
        if (!meshFilter) meshFilter = gameObject.AddComponent<MeshFilter>();
        if (!GetComponent<MeshRenderer>()) gameObject.AddComponent<MeshRenderer>();
        meshFilter.mesh = mesh;

        meshRenderer = GetComponent<Renderer>();

        //Debug.Log(baseVertices.Length);
        fft = FindObjectOfType<fftAnalyzer>();

        cam = Camera.main;
        planes = GeometryUtility.CalculateFrustumPlanes(cam);
        collider = GetComponent<BoxCollider>();
    }


    // --------------------------------------------------------------------------------------------------------
    //
    void Update()
    {
        if (collider && GeometryUtility.TestPlanesAABB(planes, collider.bounds))
        {
            UpdateDeformation();
        }
    }
    
    protected void UpdateDeformation(float scale = 1.0f)
    {
        //Mesh mesh = meshFilter.mesh;

        if (timeScale > 0)
        {

            Vector3[] vertices = mesh.vertices;
            int i = 0;
            float scaledTime = CaptureTime.Elapsed * timeScale;
            while (i < vertices.Length)
            {
                if (vertices[i].y >= deformThresholdY)
                {
                    Vector3 noiseIn = baseVertices[i] * posNoiseInScale;
                    float noise = Mathf.PerlinNoise(noiseIn.x, noiseIn.y) * posNoiseOutScale;
                    if (fft)
                    {
                        int sampleI = (int)MathUtils.Map(noise, 0, posNoiseOutScale, 0, fft.spectrum.Length - 1, true);
                        noise *= fft.spectrum[sampleI];

                    }
                    else
                    {
                        noise = Mathf.PerlinNoise(noise, scaledTime);
                    }
                    var scaledNormal = baseNormals[i];
                    scaledNormal.Scale(noiseOutScale);
                    vertices[i] = baseVertices[i] + (scaledNormal * noise * scale);
                    vertices[i].y *= terrainScale;
                }
                else
                {
                    vertices[i] = baseVertices[i];
                }
                i++;
            }
            mesh.vertices = vertices;
        }

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        //meshFilter.mesh = mesh;
    }
    
    public void SetDeformThresholdY(float deformThresholdY)
    {
        this.deformThresholdY = deformThresholdY;
    }

}
