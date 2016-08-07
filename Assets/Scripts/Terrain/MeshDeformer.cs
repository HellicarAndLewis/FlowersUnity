using UnityEngine;
using System.Collections;

/// <summary>
/// Deforms a terrain mesh by manupulating its vertices
/// </summary>
public class MeshDeformer : MonoBehaviour
{

    public MeshFilter baseMesh;

    // --------------------------------------------------------------------------------------------------------
    // Common public
    [Range(0, 10)]
    public float terrainScale = 1;
    [Range(0, 2)]
    public float timeScale = 1;
    [Range(0, 1f)]
	public float posNoiseInScale = 0.001f;
    [Range(0, 100f)]
    public float posNoiseOutScale = 10f;
    public float deformThresholdY = -999;
    public Vector3 noiseOutScale = Vector3.one;
    public Camera cam;
    
    public Vector4 growFrom;
    public Vector4 growTo;
    public Bounds growBounds;

    // --------------------------------------------------------------------------------------------------------
    // Common protected
    protected MeshFilter meshFilter;
    protected Mesh mesh;
    protected Renderer meshRenderer;
    protected Vector3[] baseVertices;
    protected Vector3[] baseNormals;
    protected fftAnalyzer fft;
    protected Plane[] planes;
    protected BoxCollider meshCollider;


    // Compute particles
    public ComputeShader particleComputeShader;
    private ComputeBuffer particleBuffer;
    private ComputeParticleData[] particles;
    private int numParticles = 1000;
    private int numParticlesDesired = 0;
    private int particleUpdateKernel = -1;
    private const int GroupSize = 128;


    // --------------------------------------------------------------------------------------------------------
    //
    void Awake()
	{
        if (!baseMesh) {
            baseMesh = gameObject.GetComponent<MeshFilter>();
            if (!baseMesh)
            {
                Debug.LogError("You need to set a mesh filter!");
                return;
            }
		}
        var meshGo = new GameObject("Base Mesh", typeof(MeshFilter));
        meshGo.transform.parent = transform;
        var meshFilter = meshGo.GetComponent<MeshFilter>();
        meshFilter.mesh = baseMesh.mesh;
        meshGo.SetActive(false);

        mesh = baseMesh.mesh;
		baseVertices = mesh.vertices;
		baseNormals = mesh.normals;
        meshFilter = GetComponent<MeshFilter>();
        if (!meshFilter) meshFilter = gameObject.AddComponent<MeshFilter>();
        if (!GetComponent<MeshRenderer>()) gameObject.AddComponent<MeshRenderer>();
        meshFilter.mesh = mesh;

        meshRenderer = GetComponent<Renderer>();
        fft = FindObjectOfType<fftAnalyzer>();
        planes = GeometryUtility.CalculateFrustumPlanes(cam);
        meshCollider = GetComponent<BoxCollider>();

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
        numParticles = baseVertices.Length;
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
        for (var i = 0; i < baseVertices.Length; i += 3)
        {
            var particle = particles[particleIndex];
            particle.enabled = (particleIndex < numParticlesDesired) ? 1 : 0;
            particle.size = 1;
            particle.seed = Random.value;
            particle.triIndex = Vector3.zero;
            particle.baseAngle = 0;
            particle.position = transform.localToWorldMatrix.MultiplyPoint(baseVertices[i]);
            particle.velocity = transform.localToWorldMatrix.MultiplyPoint(baseVertices[i]);
            particle.colour = Color.white;
            particle.texOffset = new Vector2(0, 0);
            particles[particleIndex] = particle;

            particleIndex++;
        }

        // set the initial values in the buffer
        particleBuffer.SetData(particles);
        
    }

    // --------------------------------------------------------------------------------------------------------
    //
    void Update()
    {
        if (meshCollider && GeometryUtility.TestPlanesAABB(planes, meshCollider.bounds))
        {
            meshRenderer.enabled = true;
            //GrowTerrain();
        }
        else
        {
            meshRenderer.enabled = false;
        }
    }

    // --------------------------------------------------------------------------------------------------------
    //
    protected void UpdateDeformation(float scale = 1.0f)
    {
        if (timeScale > 0)
        {

            Vector3[] vertices = mesh.vertices;
            int i = 0;
            float scaledTime = CaptureTime.Elapsed * timeScale;
            while (i < vertices.Length)
            {
                if (vertices[i].y >= deformThresholdY)
                {
                    Vector3 noiseIn = baseVertices[i] * posNoiseInScale;
                    float noise = Mathf.PerlinNoise(noiseIn.x, noiseIn.y) * posNoiseOutScale;
                    if (fft)
                    {
                        int sampleI = (int)MathUtils.Map(noise, 0, posNoiseOutScale, 0, fft.spectrum.Length - 1, true);
                        noise *= fft.spectrum[sampleI];

                    }
                    else
                    {
                        noise = Mathf.PerlinNoise(noise, scaledTime);
                    }
                    var scaledNormal = baseNormals[i];
                    scaledNormal.Scale(noiseOutScale);
                    vertices[i] = baseVertices[i] + (scaledNormal * noise * scale);
                    vertices[i].y *= terrainScale;
                }
                else
                {
                    vertices[i] = baseVertices[i];
                }
                i++;
            }
            mesh.vertices = vertices;
        }

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        //meshFilter.mesh = mesh;
    }

    protected void GrowTerrainCompute()
    {
        particleBuffer.GetData(particles);
        growFrom = growBounds.min;
        growTo = growBounds.max;

        Vector3[] vertices = mesh.vertices;
        
        for (int i = 0; i < particles.Length; i++)
        {
            vertices[i] = particles[i].position;
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        //meshFilter.mesh = mesh;
    }

    protected void GrowTerrain()
    {

        growFrom = growBounds.min;
        growTo = growBounds.max;

        Vector3[] vertices = mesh.vertices;
        int i = 0;
        float scaledTime = CaptureTime.Elapsed * timeScale;
        while (i < vertices.Length)
        {
            var growthPos = transform.localToWorldMatrix.MultiplyPoint(vertices[i]);
            var scale = 0.0f;
            bool isGrowing = (
                            (growthPos.x > growFrom.x && growthPos.x < growTo.x) &&
                            (growthPos.y > growFrom.y && growthPos.y < growTo.y) &&
                            (growthPos.z > growFrom.z && growthPos.z < growTo.z)
                            );

            scale = MathUtils.Map(growthPos.z, growFrom.z, growTo.z, 0, 1, true);
            //scale *= MathUtils.Map(growthPos.y, growFrom.y, growTo.y, 0, 1, true);

            vertices[i] = baseVertices[i];
            var low = baseVertices[i].normalized * 40;
            vertices[i].x = MathUtils.Map(scale, 0, 1, low.x, baseVertices[i].x);
            vertices[i].y = MathUtils.Map(scale, 0, 1, low.y, baseVertices[i].y);
            vertices[i].z = MathUtils.Map(scale, 0, 1, low.z, baseVertices[i].z);
            i++;
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        //meshFilter.mesh = mesh;
    }

    public void SetDeformThresholdY(float deformThresholdY)
    {
        this.deformThresholdY = deformThresholdY;
    }

    void OnDrawGizmosSelected()
    {
        //Vector3 size = new Vector3(growTo.x-growFrom.x, growTo.y - growFrom.y, growTo.z - growFrom.z);
        //Vector3 center = new Vector3(growFrom.x, growFrom.y, growFrom.z) + size;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(growBounds.center, growBounds.size);
    }

}
