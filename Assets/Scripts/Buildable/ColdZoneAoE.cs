using UnityEngine;

public class ColdZoneAoE : AoE
{
	[SerializeField]
	private ParticleSystem m_SnowParticleSystem;

	protected override void Start()
	{
		base.Start();

		var shape = m_SnowParticleSystem.shape;
		shape.scale = Vector3.one * m_Data.GetVisionRadius(transform.position.y) / 2.0f;
	}
}
