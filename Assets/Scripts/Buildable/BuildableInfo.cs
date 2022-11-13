using UnityEngine;
using UnityEngine.Rendering.Universal; // DecalProjector

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class BuildableInfo : MonoBehaviour
{
	public BuildableData Data;
	[SerializeField] private DecalProjector m_RadiusPreviewProjector;
	[SerializeField] private bool m_ShowRadius = false;

	public int KillCount = 0;

	public bool IsRadiusShowing => m_RadiusPreviewProjector.gameObject.activeInHierarchy;

	private IUpgradeable m_Upgradeable;

	public int VisionRadiusUpgradeLevel => m_Upgradeable?.GetCurrentUpgradeLevel(UpgradeType.VisionRadius) ?? 0;

	private void Awake()
	{
		m_Upgradeable = GetComponent<IUpgradeable>();
		ShowRadius(m_ShowRadius);
	}

	public void ShowRadius(bool show)
	{
		if (!m_RadiusPreviewProjector)
			return;
		float visionRadius = Data.GetVisionRadius(transform.position.y, VisionRadiusUpgradeLevel);

		m_RadiusPreviewProjector.size = new Vector3(visionRadius, visionRadius, m_RadiusPreviewProjector.size.z);
		m_RadiusPreviewProjector.gameObject.SetActive(show);
	}
}
