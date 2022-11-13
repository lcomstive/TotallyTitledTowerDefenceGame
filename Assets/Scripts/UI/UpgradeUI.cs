using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(IUpgradeable))]
public class UpgradeUI : MonoBehaviour
{
	[Serializable]
	private struct UpgradeButton
	{
		public UpgradeType Upgrade;
		[Space()]
		public Button Button;
		public TMP_Text CostText;
		public TMP_Text ValueText;
	}

	[SerializeField]
	private UpgradeButton[] m_Buttons;

	private IUpgradeable m_Upgradeable;

	private void Start()
	{
		m_Upgradeable = GetComponent<IUpgradeable>();

		foreach (UpgradeButton button in m_Buttons)
			button.Button.onClick.AddListener(() =>
			{
				m_Upgradeable.TryUpgrade(button.Upgrade);
				RefreshUI();
			});

		RefreshUI();
	}

	public void RefreshUI()
	{
		for(int i = 0; i < m_Buttons.Length; i++)
		{
			UpgradeType type = m_Buttons[i].Upgrade;
			m_Buttons[i].CostText.text = $"${m_Upgradeable.CostForNextUpgrade(type)}";
			m_Buttons[i].ValueText.text = $"{m_Upgradeable.ValueForCurrentUpgrade(type)} -> {m_Upgradeable.ValueForNextUpgrade(type)}";

			if (m_Upgradeable.IsUpgradeMax(type))
			{
				m_Buttons[i].CostText.text = "MAX";
				m_Buttons[i].Button.interactable = false;
				m_Buttons[i].ValueText.text = string.Empty;
			}
		}
	}
}
