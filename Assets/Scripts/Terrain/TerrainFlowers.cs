using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.Rendering;

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
    [Range(0, 1)]
    public float minBrightness = 0.8f;
    [Range(0, 1)]
    public float minLightBrightness = 1.0f;
    [Range(0, 1)]
    public float minScale = 0.8f;
    public float growAbove = 0.0f;
    public bool isAudioResponsive = true;

    public Vector4 growFrom;
    public Vector4 growTo;
    public Bounds growBounds;

    // --------------------------------------------------------------------------------------------------------
    // Require particle system refresh
    public bool forceRefresh = false;
    [Range(0, 20)]
    public float flowerScale = 1f;
    public float flowersPerTriangle = 1;
    [Range(-1, 1)]
    public float flowerElevation = -0.1f;

    [Range(0, 1)]
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
    private int texRows = 3;
    private int texCols = 3;
    private ShowController showControl;


    // --------------------------------------------------------------------------------------------------------
    //
    void Start()
    {
        showControl = FindObjectOfType<ShowController>();
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
        numParticles = (int)((baseTriangles.Length / 3) * Mathf.Ceil(flowersPerTriangle));
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
            if (flowersPerTriangle >= 1)
            {
                // draw multiple flowers per triangle for denser coverage
                for (int j = 0; j < flowersPerTriangle; j++)
                {
                    InitParticle(particleIndex, baseTriangles[i], baseTriangles[i + 1], baseTriangles[i + 2]);
                    particleIndex++;
                }
            }
            else
            {
                if (Random.value <= flowersPerTriangle)
                {
                    InitParticle(particleIndex, baseTriangles[i], baseTriangles[i + 1], baseTriangles[i + 2]);
                    particleIndex++;
                }
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

    public void Reposition()
    {
        Mesh mesh = meshFilter.mesh;
        baseVertices = mesh.vertices;
        baseNormals = mesh.normals;
        baseTriangles = mesh.triangles;

        for (int i = 0; i < particles.Length; i++)
        {
            var particle = particles[i];

            int vert1 = (int)particle.triIndex.x;
            int vert2 = (int)particle.triIndex.y;
            int vert3 = (int)particle.triIndex.z;

            var p1 = baseVertices[vert1];
            var p2 = baseVertices[vert2];
            var p3 = baseVertices[vert3];
            // random point on the surface of the triangle
            var p1p2 = p2 - p1;
            var p1p32 = p3 - p1;
            var particlePos = p1 + (p1p2 * particle.seed) + (p1p32 * (1 - particle.seed));

            var avgNormal = (baseNormals[vert1] + baseNormals[vert2] + baseNormals[vert3]) / 3;
            var direction = avgNormal;
            particle.position = transform.localToWorldMatrix.MultiplyPoint(particlePos + (direction * flowerElevation));

            particles[i] = particle;
        }
        //particleBuffer.SetData(particles);
    }

    private void InitParticle(int particleIndex, int vert1, int vert2, int vert3)
    {

        // points for this triangle
        var p1 = baseVertices[vert1];
        var p2 = baseVertices[vert2];
        var p3 = baseVertices[vert3];
        var avgNormal = (baseNormals[vert1] + baseNormals[vert2] + baseNormals[vert3]) / 3;

        // random point on the surface of the triangle
        var p1p2 = p2 - p1;
        var p1p32 = p3 - p1;

        var particle = particles[particleIndex];
        particle.enabled = (particleIndex < numParticlesDesired) ? 1 : 0;
        particle.size = flowerScale;
        particle.seed = Random.value;
        particle.triIndex = new Vector3(vert1, vert2, vert3);
        var particlePos = p1 + (p1p2 * particle.seed) + (p1p32 * (1 - particle.seed));
        particle.baseAngle = -Vector3.Cross(Vector3.up, avgNormal).z * 0.1f;
        // transform the position to take into account the mesh position and rotation
        // push the position out along the normal
        var direction = avgNormal;
        particle.position = particlePos + (direction * flowerElevation);// transform.localToWorldMatrix.MultiplyPoint(particlePos + (direction * flowerElevation));
        particle.velocity = Vector3.zero;
        particle.colour = Color.white;
        particle.texOffset = new Vector2(0, 0);

        particle.texOffset = GetTexOffset();

        particles[particleIndex] = particle;

    }

    private Vector2 GetTexOffset()
    {
        Vector2[] offsets = { GetTexOffset(1, 1), GetTexOffset(3, 2), GetTexOffset(2, 2), GetTexOffset(1, 2), GetTexOffset(2, 1) };
        return offsets[Random.Range(0, offsets.Length)];
    }

    private Vector2 GetTexOffset(float col, float row)
    {
        return new Vector2(col / texCols, row / texRows);
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
        particleComputeShader.SetFloat("minBrightness", minBrightness);
        particleComputeShader.SetFloat("growAbove", growAbove);
        particleComputeShader.SetFloat("minScale", minScale);

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
            Matrix4x4 m = transform.localToWorldMatrix;
            Matrix4x4 v = Camera.current.worldToCameraMatrix;
            Matrix4x4 p = Camera.current.projectionMatrix;
            Matrix4x4 MVP = p * v * m;

            growFrom = growBounds.min;
            growTo = growBounds.max;

            var scale = flowerTerrainScale;
            particleMaterial.SetMatrix("ModelViewProjection", m);
            particleMaterial.SetBuffer("particles", particleBuffer);
            particleMaterial.SetBuffer("quadPoints", quadBuffer);
            particleMaterial.SetVector("texBounds", new Vector4(1, 1, 0, 0));
            particleMaterial.SetVector("texBounds", new Vector4(1 / (float)texCols, 1 / (float)texRows, 0, 0));
            particleMaterial.SetInt("revealType", 3);
            particleMaterial.SetFloat("scale", scale);
            particleMaterial.SetFloat("minBright", minLightBrightness);
            particleMaterial.SetInt("fogEnabled", 1);
            particleMaterial.SetVector("growFrom", growBounds.min);
            particleMaterial.SetVector("growTo", growBounds.max);
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
        if (particleBuffer != null)
            particleBuffer.Release();
        if (quadBuffer != null)
            quadBuffer.Release();
    }

    void OnDrawGizmosSelected()
    {
        //Vector3 size = new Vector3(growTo.x-growFrom.x, growTo.y - growFrom.y, growTo.z - growFrom.z);
        //Vector3 center = new Vector3(growFrom.x, growFrom.y, growFrom.z) + size;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(growBounds.center, growBounds.size);
    }

    // ----------------------------------------------------------------------------------
    //
    public void OnNoiseMult(float _val)
    {
        flowerNoisePositionMult = _val;
    }

    // ----------------------------------------------------------------------------------
    //
    public void OnNoiseScale(float _val)
    {
        flowerNoisePositionScale = _val * 40;
    }

    // ----------------------------------------------------------------------------------
    //
    public void OnMaxAngle(float _val)
    {
        maxAngle = _val * Mathf.PI;
    }

    // ----------------------------------------------------------------------------------
    //
    public void OnMinBrightness(float _val)
    {
        minBrightness = _val;
    }

    // ----------------------------------------------------------------------------------
    //
    public void OnMinScale(float _val)
    {
        minScale = _val;
    }

}