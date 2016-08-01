using UnityEngine;
using System.Collections;

[System.Serializable]
public class BlendPreset
{
    public float[] blendWeights = new float[5];
    public float texBlend = 0;

    public BlendPreset()
    {
        for (int i = 0; i < blendWeights.Length; i++)
        {
            blendWeights[i] = 0;
        }
    }
    public void Lerp(int i, float from, float to, float t)
    {
        blendWeights[i] = Mathf.Lerp(from, to, t);
    }
}


/// <summary>
/// Deforms a terrain mesh by manupulating its vertices
/// </summary>
public class TerrainDeformer : MonoBehaviour
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

    public Vector3 noiseOutScale = Vector3.one;


    // --------------------------------------------------------------------------------------------------------
    // Common protected
    protected MeshFilter meshFilter;
    protected Mesh mesh;
    protected Vector3[] baseVertices;
    protected Vector3[] baseNormals;

    private float texBlendTarget = 0;
    private float texBlend = 0;


    // --------------------------------------------------------------------------------------------------------
    //
    virtual protected void Awake()
	{
        if (!baseMesh) {
			Debug.LogError("You need to set a mesh filter");
            return;
		}
        mesh = baseMesh.mesh;
        baseMesh.gameObject.SetActive(false);
		baseVertices = mesh.vertices;
		baseNormals = mesh.normals;
        meshFilter = GetComponent<MeshFilter>();
        if (!meshFilter) meshFilter = gameObject.AddComponent<MeshFilter>();
        if (!GetComponent<MeshRenderer>()) gameObject.AddComponent<MeshRenderer>();
        meshFilter.mesh = mesh;

        Debug.Log(baseVertices.Length);
        
    }


    // --------------------------------------------------------------------------------------------------------
    //
    virtual protected void Update()
	{
        texBlend = Mathf.Lerp(texBlend, texBlendTarget, 0.05f);
        var material = GetComponent<Renderer>().material;
        material.SetFloat("_Blend", texBlend);
        UpdateDeformation();
    }

    virtual public void Preset(TerrainMode mode, float duration = -1)
    {
        if (mode==TerrainMode.Daytime || mode==TerrainMode.Dusk)
        {
            texBlendTarget = 1;
        }
        else
        {
            texBlendTarget = 0;
        }
    }

    protected void UpdateDeformation(float scale = 1.0f)
    {
        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;
        int i = 0;
        float scaledTime = CaptureTime.Elapsed * timeScale;
        while (i < vertices.Length)
        {
            Vector3 noiseIn = baseVertices[i] * posNoiseInScale;
            float noise = Mathf.PerlinNoise(noiseIn.x, noiseIn.z) * posNoiseOutScale;
            noise = Mathf.PerlinNoise(noise, scaledTime) - 0.5f;
            var scaledNormal = baseNormals[i];
            scaledNormal.Scale(noiseOutScale);
            vertices[i] = baseVertices[i] + (scaledNormal * noise * scale);
            vertices[i].y *= terrainScale;
            i++;
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
    
}
