using UnityEngine;
using System.Collections;

/// <summary>
/// Manages terrain mesh and animation
/// </summary>
public class TerrainController : MonoBehaviour
{

	// --------------------------------------------------------------------------------------------------------
	//
	public Terrain terrain;
    public MeshFilter baseMesh;
	public float timeScale = 1;
	public float noiseInScale = 0.001f;
	public float noiseOutScale = 2f;

    // Compute particles
    public ComputeShader particleComputeShader;
    private ComputeBuffer particleBuffer;
    private ComputeParticleData[] particles;
    private int numParticles = 1000;
    private int numParticlesDesired = 0;
    private int particleUpdateKernel;
    private const int GroupSize = 128;
    // Render particles
    public Material particleMaterial;
    private ComputeBuffer quadBuffer;
    private const int QuadStride = 12;


    // --------------------------------------------------------------------------------------------------------
    //
    private TerrainData terrainData;
	private MeshFilter meshFilter;
	private Mesh mesh;
	private Vector3[] baseVertices;
	private Vector3[] baseNormals;
	private float[] noiseSeeds;


	// --------------------------------------------------------------------------------------------------------
	//
	void Start()
	{
        if (!terrain && !baseMesh) {
			Debug.LogError("You need to set a terrain or mesh filter");
		}
        if (terrain)
        {
            terrainData = terrain.terrainData;
            mesh = TerrainToMesh.Generate(terrainData, terrain.GetPosition());
            terrain.enabled = false;
        }
        else if (baseMesh)
        {
            mesh = baseMesh.mesh;
            baseMesh.gameObject.SetActive(false);
        }
		
		baseVertices = mesh.vertices;
		baseNormals = mesh.normals;
		meshFilter = GetComponent<MeshFilter>();
		meshFilter.mesh = mesh;
        
        /*
		noiseSeeds = new float[baseVertices.Length];
		for(int i = 0; i < baseVertices.Length; i++) {
			Vector3 noiseIn = baseVertices[i] * noiseInScale;
			float noise = Mathf.PerlinNoise(noiseIn.x, noiseIn.z) * 10;
			noiseSeeds[i] = noise;
		}
        */
        Debug.Log(baseVertices.Length);
        
        InitParticles();

    }

    
    void InitParticles()
    {
        if (!SystemInfo.supportsComputeShaders)
        {
            Debug.LogError("System does not support compute shaders!");
            return;
        }

        // find and set the particle compute shader kernal
        particleUpdateKernel = particleComputeShader.FindKernel("CSMain");
        if (particleUpdateKernel == -1)
        {
            Debug.LogError("Failed to find CSMain kernel in ParticleController.Start()");
            Application.Quit();
        }

        // Number of particles needs to be divisible by GroupSize
        // Store the original desired number of particles
        numParticles = baseVertices.Length;
        numParticlesDesired = numParticles;
        numParticles = Mathf.CeilToInt(numParticles / (float)GroupSize) * GroupSize;

        // create the compute buffer with the particle count
        // and the particle data stride which is the size of each element in the buffer
        particleBuffer = new ComputeBuffer(numParticles, ComputeParticleConstants.ParticleDataStride);

        // Create an array of ParticleData
        particles = new ComputeParticleData[numParticles];

        // Set some initial values
        for (var i = 0; i < numParticles; i++)
        {
            var particle = particles[i];
            particle.enabled = (i < numParticlesDesired) ? 1 : 0;
            particle.size = 4;
            if (i < baseVertices.Length)
                particle.position = baseVertices[i];
            else
                particle.position = new Vector3(0, 0, 0);
            particle.velocity = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            particles[i] = particle;
        }

        // set the initial values in the buffer
        particleBuffer.SetData(particles);

        // Initialise the quad compute buffer: 6 positions for rendering a quad made of two triangles
        quadBuffer = new ComputeBuffer(6, QuadStride);
        quadBuffer.SetData(new[]
        {
            new Vector3(-0.5f, 0.5f),
            new Vector3(0.5f, 0.5f),
            new Vector3(0.5f, -0.5f),
            new Vector3(0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f),
        });

    }
    
	
	// --------------------------------------------------------------------------------------------------------
	//
	void Update()
	{
        //UpdateParticles();
        Mesh mesh = meshFilter.mesh;
		Vector3[] vertices = mesh.vertices;
		int i = 0;
		float scaledTime = CaptureTime.Elapsed * timeScale;
		while(i < vertices.Length) {
            //vertices[i] = particles[i].position;
            Vector3 noiseIn = baseVertices[i] * noiseInScale;
			float noise = Mathf.PerlinNoise(noiseIn.x, noiseIn.z) * 10;
			noise = Mathf.PerlinNoise(noise, scaledTime) - 0.5f;
			vertices[i] = baseVertices[i] + (baseNormals[i] * (noise * noiseOutScale));
            
			i++;
		}
		mesh.vertices = vertices;
		mesh.RecalculateNormals();

    }

    // ----------------------------------------------------------------------------------
    //
    void UpdateParticles()
    {
        if (!SystemInfo.supportsComputeShaders || particles == null)
            return;

        // set the particles compute buffer
        particleComputeShader.SetBuffer(particleUpdateKernel, "particles", particleBuffer);

        // set params
        particleComputeShader.SetFloat("speed", 1.0f);
        particleComputeShader.SetFloat("time", Time.fixedTime);

        // dispatch, launch threads on GPU
        // numParticles need to be divisible by group size, which corresponds to [numthreads] in the shader
        var numberOfGroups = Mathf.CeilToInt((float)numParticles / GroupSize);
        particleComputeShader.Dispatch(particleUpdateKernel, numberOfGroups, 1, 1);

        // read data from the buffer back into particles so that they can be rendered?
        if (!particleMaterial)
        {
            particleBuffer.GetData(particles);
        }

    }

    // ----------------------------------------------------------------------------------
    public void OnRenderObject()
    {
        if (!SystemInfo.supportsComputeShaders)
        {
            return;
        }
        if (particleMaterial)
        {
            particleMaterial.SetBuffer("particles", particleBuffer);
            particleMaterial.SetBuffer("quadPoints", quadBuffer);
            particleMaterial.SetPass(0);
            Graphics.DrawProcedural(MeshTopology.Triangles, 6, numParticles);
        }

    }

    // ----------------------------------------------------------------------------------
    //
    private void OnDestroy()
    {
        if (!SystemInfo.supportsComputeShaders)
            return;
        // must deallocate here
        particleBuffer.Release();
        quadBuffer.Release();
    }
}
