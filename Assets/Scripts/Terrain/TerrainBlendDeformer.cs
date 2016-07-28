using UnityEngine;
using System.Collections;

/// <summary>
/// Takes the TerrainDeformer and adds the ability to blend between blend shapes in a skinned mesh
/// </summary>
public class TerrainBlendDeformer : TerrainDeformer
{

	// --------------------------------------------------------------------------------------------------------
	//
    public SkinnedMeshRenderer baseSkinnedMesh;

    // --------------------------------------------------------------------------------------------------------
    // Blend specific
    public enum State
    {
        Deform=0, PreBlend, Blend, PostBlend 
    }
    private State state = State.Deform;
    private float blendWeight = 0;
    private int blendDirection = 1;
    private float noiseOutScaleTransition = 0;


	// --------------------------------------------------------------------------------------------------------
	//
	override protected void Awake()
	{
        if (!baseMesh) {
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
        Debug.Log(baseVertices.Length);
    }



    // --------------------------------------------------------------------------------------------------------
    //
    override protected void Update()
	{
        if (Input.GetKeyDown("b"))
        {
            state = State.PreBlend;
        }

        switch (state)
        {
            case State.Deform:
                UpdateDeformation(noiseOutScaleTransition);
                break;
            case State.PreBlend:
                noiseOutScaleTransition -= 0.01f;
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
        blendWeight += (1 * blendDirection);
        baseSkinnedMesh.SetBlendShapeWeight(0, blendWeight);
        baseSkinnedMesh.BakeMesh(mesh);
        meshFilter.mesh = mesh;

        if (blendWeight > 100 || blendWeight <= 0)
        {
            noiseOutScaleTransition = 0;
            state = State.PostBlend;
            blendDirection *= -1;
            baseVertices = mesh.vertices;
            baseNormals = mesh.normals;

            if (gameObject.GetComponent<TerrainFlowers>())
            {
                gameObject.GetComponent<TerrainFlowers>().Init();
            }
        }
        mesh.RecalculateNormals();
    }
    
}
