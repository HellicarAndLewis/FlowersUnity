using UnityEngine;
using System.Collections;

[System.Serializable]
public class BlendPreset
{
    public float[] blendWeights = new float[4];
    public BlendPreset()
    {
        for (int i = 0; i < blendWeights.Length; i++)
        {
            blendWeights[i] = 0;
        }
    }
    public bool Lerp(int i, float target, float t)
    {
        blendWeights[i] = Mathf.Lerp(blendWeights[i], target, t);
        return (blendWeights[i] >= target - 0.1 && blendWeights[i] <= target + 0.1);
    }
}


/// <summary>
/// Takes the TerrainDeformer and adds the ability to blend between blend shapes in a skinned mesh
/// </summary>
public class TerrainBlendDeformer : TerrainDeformer
{

	// --------------------------------------------------------------------------------------------------------
	//
    public SkinnedMeshRenderer baseSkinnedMesh;
    public BlendPreset[] blendPresets = new BlendPreset[4];
    public int blendPresetIndex = 0;
    public BlendPreset activePreset = new BlendPreset();

    // --------------------------------------------------------------------------------------------------------
    // Blend specific
    public enum State
    {
        Deform=0, PreBlend, Blend, PostBlend 
    }
    public State state = State.Deform;
    private float noiseOutScaleTransition = 1;
    private TerrainFlowers flowers;


    // --------------------------------------------------------------------------------------------------------
    //
    override protected void Awake()
	{
        if (!baseSkinnedMesh) {
			Debug.LogError("You need to set a SkinnedMeshRenderer");
            return;
		}
        mesh = new Mesh();
        baseSkinnedMesh.BakeMesh(mesh);
        baseSkinnedMesh.gameObject.SetActive(false);
        
		baseVertices = mesh.vertices;
		baseNormals = mesh.normals;
        meshFilter = GetComponent<MeshFilter>();

        if (!meshFilter) meshFilter = gameObject.AddComponent<MeshFilter>();
        if (!GetComponent<MeshRenderer>()) gameObject.AddComponent<MeshRenderer>();
        meshFilter.mesh = mesh;
        Debug.Log("TerrainBlendDeformer, verts: " + baseVertices.Length);

        for (int i = 0; i < blendPresets.Length; i++)
        {
            if (blendPresets[i] == null)
            {
                blendPresets[i] = new BlendPreset();
                blendPresets[i].blendWeights[i] = 100;
            }
        }

        flowers = gameObject.GetComponent<TerrainFlowers>();

    }

    public void Preset(TerrainMode mode, float duration = -1)
    {
        var index = (int)mode;
        state = State.PreBlend;
        blendPresetIndex = index;
    }

    // --------------------------------------------------------------------------------------------------------
    //
    override protected void Update()
	{
        if (Input.GetKeyDown("b"))
        {
            Preset(TerrainMode.Dawn);
        }

        switch (state)
        {
            case State.Deform:
                UpdateDeformation(noiseOutScaleTransition);
                break;
            case State.PreBlend:
                noiseOutScaleTransition -= 0.01f;
                flowers.flowerAlpha = noiseOutScaleTransition;
                if (noiseOutScaleTransition <= 0)
                {
                    noiseOutScaleTransition = 0;
                    state = State.Blend;
                }
                UpdateDeformation(noiseOutScaleTransition);
                break;
            case State.Blend:
                UpdateBlend();
                break;
            case State.PostBlend:
                noiseOutScaleTransition += 0.01f;
                flowers.flowerAlpha = noiseOutScaleTransition;
                if (noiseOutScaleTransition >= 1)
                {
                    noiseOutScaleTransition = 1;
                    state = State.Deform;
                }
                UpdateDeformation(noiseOutScaleTransition);
                break;
            default:
                break;
        }
        
    }

    // --------------------------------------------------------------------------------------------------------
    //
    void UpdateBlend()
    {
        bool isComplete = true;
        var targetPreset = blendPresets[blendPresetIndex];
        for (int i = 0; i < 3; i++)
        {
            if (!activePreset.Lerp(i, targetPreset.blendWeights[i], 0.05f))
                isComplete = false;
            baseSkinnedMesh.SetBlendShapeWeight(i, activePreset.blendWeights[i]);
        }

        baseSkinnedMesh.BakeMesh(mesh);
        meshFilter.mesh = mesh;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        if (isComplete)
        {
            baseVertices = mesh.vertices;
            baseNormals = mesh.normals;
            noiseOutScaleTransition = 0;
            state = State.PostBlend;
            if (flowers)
                flowers.Init();
        }

    }
    
}
