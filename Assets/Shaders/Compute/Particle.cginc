

struct ParticleData
{
	// NOTE:  Ensure this is synced with ComputeParticleData in Particle.cs
	float4 colour;

	float3 position; 
	float3 velocity;
	float3 triIndex;

	float2 texOffset;

	float enabled;
	float size;
	float mass;
	float seed;
	float baseAngle;
	float angle;
};