using UnityEngine;

/// <summary>
/// </summary>
/// <remarks>
/// Ensure this is synced with Particle.cginc
/// </remarks>
public struct ComputeParticleData
{
    public Vector3 position;
    public Vector3 velocity;
    public float size;
    public float enabled;
};

/// <summary>
/// This must be updated if ComputeParticleData changes!
/// </summary>
internal static class ComputeParticleConstants
{
    public const int SizeofFloat = 4;
    public const int SizeofVector3 = 3 * SizeofFloat;
    public const int SizeofVector4 = 4 * SizeofFloat;
    public const int ParticleDataStride = (2 * SizeofVector3) + (0 * SizeofVector4) + (2 * SizeofFloat);
}