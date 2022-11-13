using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

[RequireComponent(typeof(AoE))]
public class AoEUpgrades : MonoBehaviour, IUpgradeable
{
	[SerializeField]
	private PlayerData m_PlayerData;

	private AoE m_Target;
	private int m_UpgradeLevel = 0;
	private BuildableInfo m_BuildableInfo;

	private void Start()
	{
		m_Target = GetComponent<AoE>();
		m_BuildableInfo= GetComponent<BuildableInfo>();
	}

	public Currency CostForNextUpgrade(UpgradeType type) => type == UpgradeType.VisionRadius ? m_Target.Data.VisionRadius.CostForUpgrade(m_UpgradeLevel + 1) : int.MaxValue;

	public int GetCurrentUpgradeLevel(UpgradeType type) => type == UpgradeType.VisionRadius ? m_UpgradeLevel : -1;

	public bool HasUpgrade(UpgradeType type) => type == UpgradeType.VisionRadius ? m_UpgradeLevel > 0 : false;

	public bool IsUpgradeMax(UpgradeType type) => type == UpgradeType.VisionRadius ? m_UpgradeLevel >= m_Target.Data.VisionRadius.MaxUpgrades : true;

	public bool CanAffordUpgrade(UpgradeType type) => CostForNextUpgrade(type) <= (m_PlayerData?.Currency ?? int.MaxValue);

	public void TryUpgrade(UpgradeType type)
	{
		if (type != UpgradeType.VisionRadius || !CanAffordUpgrade(type) || IsUpgradeMax(type))
			return;

		m_PlayerData.Currency -= CostForNextUpgrade(type);
		m_UpgradeLevel++;
		UpdateVisionRadius();
	}

	public float ValueForCurrentUpgrade(UpgradeType type) => type == UpgradeType.VisionRadius ? m_Target.Data.VisionRadius.ValueForUpgrade(m_UpgradeLevel) : 0.0f;

	public float ValueForNextUpgrade(UpgradeType type) => type == UpgradeType.VisionRadius ? m_Target.Data.VisionRadius.ValueForUpgrade(m_UpgradeLevel + 1) : 0.0f;

	private void UpdateVisionRadius()
	{
		// Setup trigger collider
		SphereCollider collider = GetComponent<SphereCollider>();
		collider.isTrigger = true;
		collider.radius = transform.InverseTransformPoint(Vector3.one * m_Target.Data.GetVisionRadius(transform.position.y, m_UpgradeLevel)).x;
		collider.radius /= 2.0f; // Radius, not diameter

		// Update visuals
		if (m_BuildableInfo && m_BuildableInfo.IsRadiusShowing)
			m_BuildableInfo.ShowRadius(true);
	}

}
