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

	private void Awake() => ShowRadius(m_ShowRadius);

#if UNITY_EDITOR
	private void Update()
	{
		float visionRadius = Data.GetVisionRadius(transform.position.y);
		if(m_RadiusPreviewProjector)
			m_RadiusPreviewProjector.size = new Vector3(visionRadius, visionRadius, m_RadiusPreviewProjector.size.z);
	}
#endif

	public void ShowRadius(bool show)
	{
		if (!m_RadiusPreviewProjector)
			return;
		float visionRadius = Data.GetVisionRadius(transform.position.y);

		m_RadiusPreviewProjector.size = new Vector3(visionRadius, visionRadius, m_RadiusPreviewProjector.size.z);
		m_RadiusPreviewProjector.gameObject.SetActive(show);
	}
}
