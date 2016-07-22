using UnityEngine;
using System.Collections;

public class TerrainController : MonoBehaviour {


	public Terrain terrain;
	public float timeScale = 1;
	public float noiseInScale = 0.001f;
	public float noiseOutScale = 2f;

	private TerrainData terrainData;
	private MeshFilter meshFilter;
	private Mesh mesh;
	private Mesh deformedMesh;
	private float noiseIncrementer = 0;

	Vector3[] baseVertices;
	Vector3[] baseNormals;


	// Use this for initialization
	void Start () {
		terrainData = terrain.terrainData;
		mesh = TerrainToMesh.Generate (terrainData, terrain.GetPosition());
		terrain.enabled = false;
		baseVertices = mesh.vertices;
		baseNormals = mesh.normals;
		meshFilter = GetComponent<MeshFilter> ();
		meshFilter.mesh = mesh;
	}
	
	// Update is called once per frame
	void Update () {
		Mesh mesh = meshFilter.mesh;
		Vector3[] vertices = mesh.vertices;
		Vector3[] normals = mesh.normals;
		//noiseIncrementer = (Mathf.Sin (Time.time * timeScale) * 1) * noiseInScale;

		int i = 0;
		while (i < vertices.Length) {
			Vector3 noiseIn = baseVertices[i] * noiseInScale;
			float noise = Mathf.PerlinNoise (noiseIn.x, noiseIn.z) * 10;
			noise = Mathf.PerlinNoise (noise, Time.time * timeScale) - 0.5f;
			vertices[i] = baseVertices[i] + (baseNormals[i] * (noise * noiseOutScale));
			i++;
		}
		mesh.vertices = vertices;
		mesh.RecalculateNormals ();
	}
}
