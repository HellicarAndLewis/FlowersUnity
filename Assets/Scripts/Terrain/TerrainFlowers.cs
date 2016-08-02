﻿using UnityEngine;
using System.Collections;
using System.Linq;

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
    [Range(0, 1)]
    public float flowerTerrainScale = 1f;
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
    private int particleUpdateKernel = -1;
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
        if (particleUpdateKernel == -1) particleUpdateKernel = particleComputeShader.FindKernel("CSMain");
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
        if (particleBuffer != null)
            particleBuffer.Release();
        particleBuffer = new ComputeBuffer(numParticles, ComputeParticleConstants.ParticleDataStride);

        // Create an array of ParticleData
        particles = new ComputeParticleData[numParticles];

        // Set some initial values
        int particleIndex = 0;
        for (var i = 0; i < baseTriangles.Length; i += 3)
        {
            // points for this triangle
            var p1 = baseVertices[baseTriangles[i]];
            var p2 = baseVertices[baseTriangles[i + 1]];
            var p3 = baseVertices[baseTriangles[i + 2]];
            // draw multiple flowers per triangle for denser coverage
            for (int j = 0; j < flowersPerTriangle; j++)
            {
                // random point on the surface of the triangle
                var p1p2 = p2 - p1;
                var p1p32 = p3 - p1;
                var particlePos = p1 + (p1p2 * Random.Range(0, 1f)) + (p1p32 * Random.Range(0, 1f));

                var particle = particles[particleIndex];
                particle.enabled = (particleIndex < numParticlesDesired) ? 1 : 0;
                particle.size = flowerScale;
                particle.seed = Random.Range(-0.06f, 0.06f);
                // transform the position to take into account the mesh position and rotation
                particle.position = transform.localToWorldMatrix.MultiplyPoint(particlePos);
                // push the position out along the normal
                particle.position += baseNormals[baseTriangles[i]] * flowerElevation;
                particle.velocity = Vector3.zero;
                particle.colour = Color.white;
                particle.texOffset = new Vector2(0, 0);
                particles[particleIndex] = particle;

                particleIndex++;
            }

        }

        // set the initial values in the buffer
        particles.OrderBy(particle => particle.position.z);
        particleBuffer.SetData(particles);

        // Initialise the quad compute buffer: 6 positions for rendering a quad made of two triangles
        if (quadBuffer != null)
            quadBuffer.Release();
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
            particleMaterial.SetInt("revealType", 3);
            particleMaterial.SetFloat("scale", flowerTerrainScale);
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