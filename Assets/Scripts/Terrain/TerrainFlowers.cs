using UnityEngine;
using System.Collections;

public class TerrainFlowers : MonoBehaviour
{

    //[Range(0.01f, 0.001f)]
    public float flowerNoisePositionScale = 0.01f;
    //[Range(1, 10)]
    public float flowerNoisePositionMult = 1f;
    [Range(0, 1)]
    public float flowerNoiseTimeScale = 0.1f;
    [Range(0, 1)]
    public float flowerAlpha = 1f;
    [Range(0, 20)]
    public float flowerScale = 1f;
    [Range(0, 20)]
    public float flowerElevation = 0f;
    public bool flowersEnabled = true;
    public int flowersPerTriangle = 1;

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
    private MeshFilter meshFilter;
    private Vector3[] baseVertices;
    private Vector3[] baseNormals;
    private int[] baseTriangles;

    // --------------------------------------------------------------------------------------------------------
    //
    void Start()
    {
        Init();
    }

    public void Init()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (!meshFilter)
        {
            Debug.LogError("TerrainFlowers script requires a mesh filter on the same game object");
            return;
        }
        var mesh = meshFilter.mesh;
        baseVertices = mesh.vertices;
        baseNormals = mesh.normals;
        baseTriangles = mesh.triangles;
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
        numParticles = (baseTriangles.Length / 3) * flowersPerTriangle;
        numParticlesDesired = numParticles;
        numParticles = Mathf.CeilToInt(numParticles / (float)GroupSize) * GroupSize;

        // create the compute buffer with the particle count
        // and the particle data stride which is the size of each element in the buffer
        particleBuffer = new ComputeBuffer(numParticles, ComputeParticleConstants.ParticleDataStride);

        // Create an array of ParticleData
        particles = new ComputeParticleData[numParticles];

        // Set some initial values
        int particleIndex = 0;
        for (var i = 0; i < baseTriangles.Length; i += 3)
        {
            var p1 = baseVertices[baseTriangles[i]];
            var p2 = baseVertices[baseTriangles[i + 1]];
            var p3 = baseVertices[baseTriangles[i + 2]];
            var range = (p2 - p1).magnitude * 0.5;
            for (int j = 0; j < flowersPerTriangle; j++)
            {
                var p1p2 = p2 - p1;
                var p1p32 = p3 - p1;
                var particlePos = p1 + (p1p2 * Random.Range(0, 1f)) + (p1p32 * Random.Range(0, 1f));

                var particle = particles[particleIndex];
                particle.enabled = (particleIndex < numParticlesDesired) ? 1 : 0;
                particle.size = flowerScale;
                particle.seed = 0;

                particle.position = transform.localToWorldMatrix.MultiplyPoint(particlePos);
                particle.position += baseNormals[baseTriangles[i]] * flowerElevation;// Random.Range(0, flowerElevation));

                particle.velocity = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
                particle.colour = Color.white;
                particle.texOffset = new Vector2(0, 0);
                particles[particleIndex] = particle;

                particleIndex++;
            }

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
        if (flowersEnabled) UpdateParticles();

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
        particleComputeShader.SetFloat("time", CaptureTime.Elapsed);
        particleComputeShader.SetFloat("noisePositionScale", flowerNoisePositionScale);
        particleComputeShader.SetFloat("noisePositionMult", flowerNoisePositionMult);
        particleComputeShader.SetFloat("noiseTimeScale", flowerNoiseTimeScale);
        particleComputeShader.SetFloat("alpha", flowerAlpha);

        // dispatch, launch threads on GPUs
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
        if (particleMaterial && flowersEnabled)
        {
            particleMaterial.SetBuffer("particles", particleBuffer);
            particleMaterial.SetBuffer("quadPoints", quadBuffer);
            particleMaterial.SetVector("texBounds", new Vector4(1, 1, 0, 0));
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