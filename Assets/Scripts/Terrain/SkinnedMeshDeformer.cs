using UnityEngine;
using System.Collections;

/// <summary>
/// Takes the TerrainDeformer and adds the ability to blend between blend shapes in a skinned mesh
/// </summary>
public class SkinnedMeshDeformer : MeshBlendDeformer
{

    // --------------------------------------------------------------------------------------------------------
    //
    public SkinnedMeshRenderer baseSkinnedMesh;

    // --------------------------------------------------------------------------------------------------------
    // Blend specific
    public enum State
    {
        Deform = 0, PreBlend, Blend, PostBlend
    }
    public State state = State.Deform;

    private float noiseOutScaleTransition = 1;
    private TerrainFlowers flowers;


    // --------------------------------------------------------------------------------------------------------
    //
    override protected void Start()
    {
        if (!baseSkinnedMesh)
        {
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

        flowers = gameObject.GetComponent<TerrainFlowers>();
        fft = FindObjectOfType<fftAnalyzer>();
    }

    public override void Preset(TerrainMode mode, float duration = -1)
    {
        var index = (int)mode;
        if (index == blendPresetIndex) return;

        base.Preset(mode, duration);
        state = State.PreBlend;
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
                noiseOutScaleTransition -= 0.05f;
                //flowers.flowerTerrainScale = noiseOutScaleTransition;
                if (noiseOutScaleTransition <= 0)
                {
                    noiseOutScaleTransition = 0;
                    blendTime = 0;
                    state = State.Blend;
                }
                UpdateDeformation(noiseOutScaleTransition);
                break;
            case State.Blend:
                UpdateBlend();
                flowers.Reposition();
                break;
            case State.PostBlend:
                noiseOutScaleTransition += 0.05f;
                //flowers.flowerTerrainScale = noiseOutScaleTransition;
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
    override protected void UpdateBlend()
    {
        base.UpdateBlend();

        var previousPreset = blendPresets[previousBlendPresetIndex];
        var targetPreset = blendPresets[blendPresetIndex];
        bool isComplete = (blendTime > blendDuration);

        float progress = blendTime / blendDuration;
        for (int i = 0; i < 4; i++)
        {
            activePreset.Lerp(i, previousPreset.blendWeights[i], targetPreset.blendWeights[i], progress);
            baseSkinnedMesh.SetBlendShapeWeight(i, activePreset.blendWeights[i]);
        }

        baseSkinnedMesh.BakeMesh(mesh);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

        if (isComplete)
        {
            previousBlendPresetIndex = blendPresetIndex;
            baseVertices = mesh.vertices;
            baseNormals = mesh.normals;
            noiseOutScaleTransition = 0;
            state = State.PostBlend;

            if (flowers)
                flowers.Init();
        }

    }

    public void OnDrawGizmosSelected()
    {
        /*
        for (int i = 0; i < mesh.vertexCount/10; i++)
        {
            var p1 = transform.localToWorldMatrix.MultiplyPoint(mesh.vertices[i]);
            Gizmos.DrawLine(p1, p1 + (mesh.normals[i] * 0.3f));
        }
        */
    }

    public void OnPosOutScale(float _val)
    {
        posNoiseOutScale = _val * 100.0f;
    }

    public void OnPosInScale(float _val)
    {
        posNoiseInScale = _val;
    }

    public void OnThreshold(float _val)
    {
        deformThresholdY = _val;
    }

    public void OnScaleX(float _val)
    {
        noiseOutScale.x = _val;
    }

    public void OnScaleY(float _val)
    {
        noiseOutScale.y = _val;
    }

    public void OnScaleZ(float _val)
    {
        noiseOutScale.z = _val;
    }
}
