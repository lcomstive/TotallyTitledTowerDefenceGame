using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildableInfoPanel : MonoBehaviour
{
	[SerializeField] private BuildableInfo m_Info;

	[Header("UI")]
	[SerializeField] private TMP_Text m_Title;
	[SerializeField] private TMP_Text m_SellCostText;
	[SerializeField] private Button m_SellButton;
	[SerializeField] private Button m_CloseButton;

	private BuildableManager m_Manager;

	private void Start()
	{
		m_Manager = FindObjectOfType<BuildableManager>();
		UpdateUI();
	}

	private void UpdateUI()
	{
		m_Title.text = m_Info.Data.DisplayName;
		m_SellCostText.text = $"${m_Info.Data.SellValue.DisplayValue()}";
		m_SellButton .onClick.AddListener(Refund);
		m_CloseButton.onClick.AddListener(Close);
	}

	public void Refund()
	{
		m_Manager.RefundBuilding(m_Info.Data);
		Destroy(m_Info.gameObject);
	}

	public void Close() => m_Manager.Picker.Deselect();
}
