using UnityEngine;
using System.Collections;

/// <summary>
/// Manages terrain mesh and animation
/// </summary>
public class TerrainDeformer : MonoBehaviour
{

	// --------------------------------------------------------------------------------------------------------
	//
    public MeshFilter baseMesh;
    [Range(0, 10)]
    public float terrainScale = 1;
    [Range(0, 2)]
    public float timeScale = 1;
    [Range(0, 1f)]
	public float noiseInScale = 0.001f;
    [Range(0, 10)]
    public float noiseOutScale = 2f;


    // --------------------------------------------------------------------------------------------------------
    //
	private MeshFilter meshFilter;
	private Mesh mesh;
	private Vector3[] baseVertices;
	private Vector3[] baseNormals;
    private int[] baseTriangles;


	// --------------------------------------------------------------------------------------------------------
	//
	void Awake()
	{
        if (!baseMesh) {
			Debug.LogError("You need to set a mesh filter");
            return;
		}
        mesh = baseMesh.mesh;
        baseMesh.gameObject.SetActive(false);
		baseVertices = mesh.vertices;
		baseNormals = mesh.normals;
        baseTriangles = mesh.triangles;
        meshFilter = GetComponent<MeshFilter>();
        if (!meshFilter) meshFilter = gameObject.AddComponent<MeshFilter>();
        if (!GetComponent<MeshRenderer>()) gameObject.AddComponent<MeshRenderer>();
        meshFilter.mesh = mesh;

        Debug.Log(baseVertices.Length);
        
    }
    
	
	// --------------------------------------------------------------------------------------------------------
	//
	void Update()
	{
        Mesh mesh = meshFilter.mesh;
		Vector3[] vertices = mesh.vertices;
		int i = 0;
		float scaledTime = CaptureTime.Elapsed * timeScale;
		while(i < vertices.Length) {
            Vector3 noiseIn = baseVertices[i] * noiseInScale;
			float noise = Mathf.PerlinNoise(noiseIn.x, noiseIn.z) * 10;
			noise = Mathf.PerlinNoise(noise, scaledTime) - 0.5f;
			vertices[i] = baseVertices[i] + (baseNormals[i] * (noise * noiseOutScale));
            vertices[i].y *= terrainScale;
            i++;
		}
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
        
    }
    
}
