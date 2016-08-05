using UnityEngine;

/// <summary>
/// </summary>
/// <remarks>
/// Ensure this is synced with Particle.cginc
/// </remarks>
public struct ComputeParticleData
{
    public Vector4 colour;

    public Vector3 position;
    public Vector3 velocity;
    public Vector3 triIndex;

    public Vector2 texOffset;

    public float enabled;
    public float size;
    public float mass;
    public float seed;
    public float baseAngle;
    public float angle;

};

/// <summary>
/// This must be updated if ComputeParticleData changes!
/// </summary>
internal static class ComputeParticleConstants
{
    public const int SizeofFloat = 4;
    public const int SizeofVector2 = 2 * SizeofFloat;
    public const int SizeofVector3 = 3 * SizeofFloat;
    public const int SizeofVector4 = 4 * SizeofFloat;
    public const int ParticleDataStride = (1 * SizeofVector4) + (3 * SizeofVector3) + (1 * SizeofVector2) + (6 * SizeofFloat);
}