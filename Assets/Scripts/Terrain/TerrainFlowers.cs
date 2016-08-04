using UnityEngine;
using System.Collections;
using System.Linq;

public class TerrainFlowers : MonoBehaviour
{
    // --------------------------------------------------------------------------------------------------------
    // Realtime
    [Range(0, 1)]
    public float flowerNoisePositionScale = 0.01f;
    [Range(0, 40)]
    public float flowerNoisePositionMult = 1f;
    [Range(0, 1)]
    public float flowerNoiseTimeScale = 0.1f;
    [Range(0, 1)]
    public float flowerAlpha = 1f;
    [Range(0, Mathf.PI)]
    public float maxAngle = 0.2f;
    public bool isAudioResponsive = true;

    // --------------------------------------------------------------------------------------------------------
    // Require particle system refresh
    public bool forceRefresh = false;
    [Range(0, 20)]
    public float flowerScale = 1f;
    public int flowersPerTriangle = 1;
    [Range(0, 20)]
    public float flowerElevation = 0f;
    
    [HideInInspector]
    public float flowerTerrainScale = 1f;

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
    private SkinnedMeshRenderer skinnedMesh;
    private Vector3[] baseVertices;
    private Vector3[] baseNormals;
    private int[] baseTriangles;
    private fftAnalyzer fft;


    // --------------------------------------------------------------------------------------------------------
    //
    void Start()
    {
        fft = FindObjectOfType<fftAnalyzer>();
        Init();
    }

    public void Init()
    {
        Mesh mesh;
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter)
        {
            mesh = meshFilter.mesh;
            baseVertices = mesh.vertices;
            baseNormals = mesh.normals;
            baseTriangles = mesh.triangles;
        }
        else
        {
            if (GetComponent<SkinnedMeshRenderer>())
            {
                mesh = new Mesh();
                GetComponent<SkinnedMeshRenderer>().BakeMesh(mesh);
                baseVertices = mesh.vertices;
                baseNormals = mesh.normals;
                baseTriangles = mesh.triangles;
            }
            else
            {
                Debug.LogError("TerrainFlowers script requires a mesh filter or skinned mesh renderer on the same game object");
                return;
            }
        }
        
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

            var avgNormal = (baseNormals[baseTriangles[i]] + baseNormals[baseTriangles[i + 1]] + baseNormals[baseTriangles[i + 2]]) / 3;

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
                particle.seed = Random.value;
                particle.baseAngle = -Vector3.Cross(Vector3.up, avgNormal).z;
                // transform the position to take into account the mesh position and rotation
                particle.position = transform.localToWorldMatrix.MultiplyPoint(particlePos);
                // push the position out along the normal
                var direction = baseNormals[baseTriangles[i]];
                particle.position += (direction * flowerElevation);
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
        if (forceRefresh)
        {
            forceRefresh = false;
            InitParticles();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            flowerAlpha += 0.05f;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            flowerAlpha -= 0.05f;
        }
        UpdateParticles();
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
        var fftVolume0 = 1.0f;
        var fftVolume1 = 1.0f;
        var fftVolume2 = 1.0f;
        if (fft && isAudioResponsive)
        {
            fftVolume0 = fft.spectrumBinned[0] + 0.5f;
            fftVolume1 = fft.spectrumBinned[1] + 0.5f;
            fftVolume2 = fft.spectrumBinned[2] + 0.5f;
        }
        particleComputeShader.SetFloat("time", CaptureTime.Elapsed);
        particleComputeShader.SetFloat("noisePositionScale", flowerNoisePositionScale);
        particleComputeShader.SetFloat("noisePositionMult", flowerNoisePositionMult);
        particleComputeShader.SetFloat("noiseTimeScale", flowerNoiseTimeScale);
        particleComputeShader.SetFloat("alpha", flowerAlpha);
        particleComputeShader.SetFloat("fftVolume0", fftVolume0);
        particleComputeShader.SetFloat("fftVolume1", fftVolume1);
        particleComputeShader.SetFloat("fftVolume2", fftVolume2);
        particleComputeShader.SetFloat("maxAngle", maxAngle);

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
        if (particleMaterial)
        {
            var scale = flowerTerrainScale;
            particleMaterial.SetBuffer("particles", particleBuffer);
            particleMaterial.SetBuffer("quadPoints", quadBuffer);
            particleMaterial.SetVector("texBounds", new Vector4(1, 1, 0, 0));
            particleMaterial.SetInt("revealType", 3);
            particleMaterial.SetFloat("scale", scale);
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