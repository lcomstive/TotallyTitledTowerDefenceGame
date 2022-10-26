using UnityEngine;
using System.Collections.Generic;

public class BuildableList : MonoBehaviour
{
	[SerializeField] private BuildableData[] m_Buildables;
	[SerializeField] private GameObject m_UIPrefab;
	[SerializeField] private PlayerData m_PlayerData;

	private List<BuildableButton> m_Buttons = new List<BuildableButton>();

	private void Start()
	{
		if(!m_PlayerData)
		{
			Debug.LogError($"Player data is not set in '{name}' BuildableList");
			enabled = false;
			return;
		}

		foreach(BuildableData data in m_Buildables)
			AddToList(data);

		m_PlayerData.Currency.ValueChanged += PlayerCurrencyChanged;
		PlayerCurrencyChanged(m_PlayerData.Currency);
	}

	private void OnDestroy() => m_PlayerData.Currency.ValueChanged -= PlayerCurrencyChanged;

	private void PlayerCurrencyChanged(Currency available)
	{
		foreach(BuildableButton button in m_Buttons)
			button.CanBuy = button.Data.Cost <= available;
	}

	public void AddToList(BuildableData data)
	{
		BuildableButton btn = Instantiate(m_UIPrefab, transform).GetComponentInChildren<BuildableButton>();
		if(!btn)
			return;

		data.ResetData();

		btn.SetData(data);
		m_Buttons.Add(btn);
	}
}
