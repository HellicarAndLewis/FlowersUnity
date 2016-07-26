using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Compute shader based particle system controller
/// </summary>
public class ComputeParticleController : MonoBehaviour
{
    // Public
    public ComputeShader particleComputeShader;
    public int numParticles = 1000;
    public Vector3 bounds = new Vector3(80f, 30f, 80f);
    public Vector3 speed = new Vector3(0.1f, -0.2f, 0.1f);
    public float flowerNoisePositionScale = 0.01f;
    public float flowerNoisePositionMult = 1f;
    public float flowerNoiseTimeScale = 0.1f;
    public float flowerAlpha = 1f;

    // Render
    public Material particleMaterial;
    private ComputeBuffer quadBuffer;
    private const int QuadStride = 12;
    
    // Compute
    [HideInInspector]
    public ComputeParticleData[] particles;
    private ComputeBuffer particleBuffer;
    private int particleUpdateKernel;
    private int numParticlesDesired = 0;
    private const int GroupSize = 128;
    
    
    // ----------------------------------------------------------------------------------
    public void Start()
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
            particle.size = Random.Range(1, 3);
            particle.position = new Vector3(Random.Range(bounds.x * -0.5f, bounds.x * 0.5f),
                                         Random.Range(bounds.y * -0.5f, bounds.y * 0.5f),
                                         Random.Range(bounds.z * -0.5f, bounds.z * 0.5f));
            particle.velocity = Vector3.zero;
            particle.colour = Color.white;
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
    
    // ----------------------------------------------------------------------------------
    void Update()
    {
        if (!SystemInfo.supportsComputeShaders || particles == null)
            return;
        
        // set the particles compute buffer
        particleComputeShader.SetBuffer(particleUpdateKernel, "particles", particleBuffer);

        // set params
        Vector3 boundsMin = gameObject.transform.position + (bounds * -0.5f);
        Vector3 boundsMax = gameObject.transform.position + (bounds * 0.5f);
        particleComputeShader.SetVector("boundsMin", boundsMin);
        particleComputeShader.SetVector("boundsMax", boundsMax);
        particleComputeShader.SetVector("speed", speed);
        particleComputeShader.SetFloat("time", CaptureTime.Elapsed);
        particleComputeShader.SetFloat("noisePositionScale", flowerNoisePositionScale);
        particleComputeShader.SetFloat("noisePositionMult", flowerNoisePositionMult);
        particleComputeShader.SetFloat("noiseTimeScale", flowerNoiseTimeScale);
        particleComputeShader.SetFloat("alpha", flowerAlpha);

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

    // ----------------------------------------------------------------------------------
    //
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(gameObject.transform.position, bounds);
        if (particles != null)
        {
            foreach (var particle in particles)
            {
                //if (particle.enabled > 0)
                    //DrawArrow.ForGizmo(particle.position, particle.velocity.normalized, Color.yellow, 0.5f);
            }
        }
        
    }

    // ----------------------------------------------------------------------------------
    //
    public void OnGUI()
    {
        if (particles != null)
        {
            GUI.Label(new Rect(10, 10, 100, 50), string.Format("{0} people\n{1} particles\n{2} fps", numParticlesDesired, particles.Length, 1 / Time.deltaTime));
        }
    }

}
