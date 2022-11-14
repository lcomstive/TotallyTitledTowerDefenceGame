using UnityEngine;

public class ColdZoneAoE : AoE
{
	[SerializeField]
	private ParticleSystem m_SnowParticleSystem;

	protected override void Start()
	{
		base.Start();
		SetParticleSystemSize();
	}

	public void SetParticleSystemSize(int upgradeLevel = 0)
	{
		var shape = m_SnowParticleSystem.shape;
		shape.scale = Vector3.one * Data.GetVisionRadius(transform.position.y, upgradeLevel) / 2.0f;
	}
}
