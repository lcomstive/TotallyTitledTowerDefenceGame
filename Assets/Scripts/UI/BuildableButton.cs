using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildableButton : MonoBehaviour
{
	[SerializeField] private TMP_Text m_Title;
	[SerializeField] private TMP_Text m_Cost;
	[SerializeField] private TMP_Text m_Description;

	public BuildableData Data { get; private set; }

	private bool m_CanBuy = false;
	public bool CanBuy
	{
		get => m_CanBuy;
		set
		{
			if(m_CanBuy == value)
				return;

			m_CanBuy = value;
			UpdateValues();
		}
	}

	public void SetData(BuildableData data)
	{
		Data = data;

		UpdateValues();
		ShowDescriptionText(false);
	}

	public void OnClick()
	{
		if(!m_CanBuy)
			return;

		BuildableManager.DeselectBuilding();
		BuildableManager.StartBuilding(Data);
	}

	public void ShowDescriptionText(bool show) => m_Description.alpha = show ? 1 : 0;

	private void UpdateValues()
	{
		m_Title.text = Data.DisplayName;
		m_Description.text = Data.Description;
		m_Cost.text = "$" + Data.Cost.DisplayValue();

		m_Cost.alpha = m_CanBuy ? 1.0f : 0.4f;
	}
}
