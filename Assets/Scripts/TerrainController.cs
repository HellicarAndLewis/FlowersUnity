using UnityEngine;
using System.Collections;

/// <summary>
/// Terrain controller.
/// </summary>
public class TerrainController : MonoBehaviour
{

	// --------------------------------------------------------------------------------------------------------
	//
	public Terrain terrain;
	public float timeScale = 1;
	public float noiseInScale = 0.001f;
	public float noiseOutScale = 2f;

	// --------------------------------------------------------------------------------------------------------
	//
	private TerrainData terrainData;
	private MeshFilter meshFilter;
	private Mesh mesh;
	private Vector3[] baseVertices;
	private Vector3[] baseNormals;


	// --------------------------------------------------------------------------------------------------------
	//
	void Start()
	{
		if(!terrain) {
			Debug.LogError("You need to set terrain in TerrainController");
		}
		terrainData = terrain.terrainData;
		mesh = TerrainToMesh.Generate(terrainData, terrain.GetPosition());
		terrain.enabled = false;
		baseVertices = mesh.vertices;
		baseNormals = mesh.normals;
		meshFilter = GetComponent<MeshFilter>();
		meshFilter.mesh = mesh;
	}
	
	// Update is called once per frame
	void Update()
	{
		Mesh mesh = meshFilter.mesh;
		Vector3[] vertices = mesh.vertices;
		int i = 0;
		while(i < vertices.Length) {
			Vector3 noiseIn = baseVertices[i] * noiseInScale;
			float noise = Mathf.PerlinNoise(noiseIn.x, noiseIn.z) * 10;
			noise = Mathf.PerlinNoise(noise, Time.time * timeScale) - 0.5f;
			vertices[i] = baseVertices[i] + (baseNormals[i] * (noise * noiseOutScale));
			i++;
		}
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
	}
}
