using UnityEngine;
using System.Collections;

[System.Serializable]
public class BlendPreset
{
    public float[] blendWeights = new float[5];
    public Material material;

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
public class MeshBlendDeformer : MonoBehaviour
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


    public BlendPreset[] blendPresets = new BlendPreset[5];
    public int blendPresetIndex = 0;
    public int previousBlendPresetIndex = 0;
    public BlendPreset activePreset = new BlendPreset();

    public float blendTime = 0;
    public float blendDuration = 2;


    // --------------------------------------------------------------------------------------------------------
    // Common protected
    protected MeshFilter meshFilter;
    protected Mesh mesh;
    protected Vector3[] baseVertices;
    protected Vector3[] baseNormals;
    protected float texBlendPrevious = 0;
    protected float texBlendTarget = 0;
    protected float texBlend = 0;
    protected fftAnalyzer fft;


    // --------------------------------------------------------------------------------------------------------
    //
    virtual protected void Start()
	{
        if (!baseMesh) {
            baseMesh = gameObject.GetComponentInChildren<MeshFilter>();
            if (!baseMesh)
            {
                Debug.LogError("You need to set a mesh filter!");
                return;
            }
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
        fft = FindObjectOfType<fftAnalyzer>();
    }


    // --------------------------------------------------------------------------------------------------------
    //
    virtual protected void Update()
	{
        if (blendTime < blendDuration) UpdateBlend();
        UpdateDeformation();
    }

    virtual public void Preset(TerrainMode mode, float duration = -1)
    {
        var index = (int)mode;
        if (index == blendPresetIndex) return;

        if (blendPresets[index].material)
        {
            GetComponent<Renderer>().material = blendPresets[index].material;
            if (index > blendPresetIndex)
            {
                // next preset is higher, need to fade the prior material up
                texBlendTarget = 1;
                texBlendPrevious = 0;
                if (index > 0) GetComponent<Renderer>().material = blendPresets[index - 1].material;
            }
            else
            {
                // next preset is lower, fade down to it
                texBlendTarget = 0;
                texBlendPrevious = 1;
            }
            GetComponent<Renderer>().material.SetFloat("_Blend", texBlendPrevious);
            blendTime = 0;
            blendPresetIndex = index;
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
        meshFilter.mesh = mesh;
    }

    virtual protected void UpdateBlend()
    {
        blendTime += CaptureTime.Delta;
        float progress = blendTime / blendDuration;
        texBlend = Mathf.Lerp(texBlendPrevious, texBlendTarget, progress);
        var material = GetComponent<Renderer>().material;
        material.SetFloat("_Blend", texBlend);
    }

    public void SetDeformThresholdY(float deformThresholdY)
    {
        this.deformThresholdY = deformThresholdY;
    }

}
