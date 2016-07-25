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
            particle.size = 4;
            var bounds = 160;
            particle.position = new Vector3(Random.Range(bounds * -0.5f, bounds * 0.5f), 0, 100 + Random.Range(bounds * -0.5f, bounds * 0.5f));
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
    
    // ----------------------------------------------------------------------------------
    void Update()
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
            /*
            particleMaterial.SetBuffer("particles", particleBuffer);
            particleMaterial.SetBuffer("quadPoints", quadBuffer);
            particleMaterial.SetPass(0);
            Graphics.DrawProcedural(MeshTopology.Triangles, 6, numParticles);
            */
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
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (particles != null)
        {
            foreach (var particle in particles)
            {
                if (particle.enabled > 0)
                    DrawArrow.ForGizmo(particle.position, particle.velocity.normalized, Color.yellow, 0.5f);
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
