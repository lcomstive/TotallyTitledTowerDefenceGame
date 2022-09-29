using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class BuildableInfoPanel : MonoBehaviour
{
	[SerializeField] private TMP_Text m_Title;
	[SerializeField] private TMP_Text m_SellCostText;
	[SerializeField] private Button m_SellButton;
	[SerializeField] private Button m_CloseButton;

	private Camera m_Camera;
	private RectTransform m_RectTransform;
	private BuildableInfo m_CurrentInfo = null;

	private void Awake()
	{
		m_Camera = Camera.main;
		m_RectTransform = GetComponent<RectTransform>();

		BuildableManager.SelectedBuildingChanged += OnSelectedBuildingChanged;
		OnSelectedBuildingChanged(null);
	}

	private void OnDestroy() => BuildableManager.SelectedBuildingChanged -= OnSelectedBuildingChanged;

	private void OnSelectedBuildingChanged(BuildableInfo info)
	{
		if (info != m_CurrentInfo && m_CurrentInfo != null)
			m_CurrentInfo.ShowRadius(false);
		m_CurrentInfo = info;

		gameObject.SetActive(m_CurrentInfo != null);

		if (m_CurrentInfo == null)
			return;

		m_RectTransform.position = m_Camera.WorldToScreenPoint(m_CurrentInfo.transform.position);

		m_Title.text = m_CurrentInfo.Data.DisplayName;
		m_SellCostText.text = $"${m_CurrentInfo.Data.SellValue.DisplayValue()}";
		m_SellButton .onClick.AddListener(() => BuildableManager.RefundSelected());
		m_CloseButton.onClick.AddListener(() => BuildableManager.DeselectBuilding());

		m_CurrentInfo.ShowRadius(true);
	}
}
