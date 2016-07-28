using UnityEngine;
using System.Collections;

/// <summary>
/// Manages terrain mesh and animation
/// </summary>
public class TerrainBlend : MonoBehaviour
{

	// --------------------------------------------------------------------------------------------------------
	//
    public SkinnedMeshRenderer baseMesh;
    [Range(0, 1)]
    public float terrainScale = 1;
    [Range(0.1f, 2)]
    public float timeScale = 1;
    [Range(0f, 1f)]
	public float noiseInScale = 0.001f;
    [Range(0, 30)]
    public float noiseOutScale = 2f;

    public enum State
    {
        Noise=0, PreBlend, Blend, PostBlend 
    }
    public State state = State.Noise;

    public float blendWeight = 0;
    public int blendDirection = 1;
    public float noiseOutScaleTransition = 0;



    // --------------------------------------------------------------------------------------------------------
    //
    private MeshFilter meshFilter;
	private Mesh mesh;
	private Vector3[] baseVertices;
	private Vector3[] baseNormals;
    private int[] baseTriangles;
    private float[] noiseSeeds;


	// --------------------------------------------------------------------------------------------------------
	//
	void Start()
	{
        if (!baseMesh) {
			Debug.LogError("You need to set a mesh filter");
            return;
		}
        mesh = new Mesh();
        baseMesh.BakeMesh(mesh);
        //baseMesh.gameObject.SetActive(false);
        
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
        if (Input.GetKeyDown("b"))
        {
            state = State.PreBlend;
        }

        switch (state)
        {
            case State.Noise:
                UpdateNoise();
                break;
            case State.PreBlend:
                noiseOutScaleTransition -= 0.01f;
                if (noiseOutScaleTransition <= 0)
                {
                    noiseOutScaleTransition = 0;
                    state = State.Blend;
                }
                UpdateNoise();
                break;
            case State.Blend:
                UpdateBlend();
                break;
            case State.PostBlend:
                noiseOutScaleTransition += 0.01f;
                if (noiseOutScaleTransition >= 1)
                {
                    noiseOutScaleTransition = 1;
                    state = State.Noise;
                }
                UpdateNoise();
                break;
            default:
                break;
        }
        
    }

    void UpdateBlend()
    {
        blendWeight += (1 * blendDirection);
        baseMesh.SetBlendShapeWeight(0, blendWeight);

        baseMesh.BakeMesh(mesh);
        meshFilter.mesh = mesh;

        if (blendWeight > 100 || blendWeight <= 0)
        {
            noiseOutScaleTransition = 0;
            state = State.PostBlend;
            blendDirection *= -1;
            //baseMesh.BakeMesh(mesh);
            //meshFilter.mesh = mesh;
            baseVertices = mesh.vertices;
            baseNormals = mesh.normals;
            baseTriangles = mesh.triangles;
        }

        mesh.RecalculateNormals();
    }

    void UpdateNoise()
    {
        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;
        int i = 0;
        float scaledTime = CaptureTime.Elapsed * timeScale;
        while (i < vertices.Length)
        {
            Vector3 noiseIn = baseVertices[i] * noiseInScale;
            float noise = Mathf.PerlinNoise(noiseIn.x, noiseIn.z) * 10;
            noise = Mathf.PerlinNoise(noise, scaledTime) - 0.5f;
            vertices[i] = baseVertices[i] + (baseNormals[i] * (noise * noiseOutScale) * noiseOutScaleTransition);
            vertices[i].y *= terrainScale;
            i++;
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
    
}
