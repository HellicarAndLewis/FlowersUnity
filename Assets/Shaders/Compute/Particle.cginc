

struct ParticleData
{
	// NOTE:  Ensure this is synced with ComputeParticleData in Particle.cs
	float3 position; 
	float3 velocity;
	float size;
	float enabled;
};