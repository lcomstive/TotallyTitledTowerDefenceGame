using UnityEngine;
using System.Collections.Generic;

public class BuildableList : MonoBehaviour
{
	[SerializeField] private BuildableData[] m_Buildables;
	[SerializeField] private GameObject m_UIPrefab;

	private List<BuildableButton> m_Buttons = new List<BuildableButton>();

	private void Start()
	{
		foreach(BuildableData data in m_Buildables)
			AddToList(data);

		if(BuildableManager.PlayerData)
			BuildableManager.PlayerData.Currency.ValueChanged += PlayerCurrencyChanged;

		PlayerCurrencyChanged(BuildableManager.PlayerData.Currency);
	}

	private void OnDestroy()
	{
		if(BuildableManager.PlayerData)
			BuildableManager.PlayerData.Currency.ValueChanged -= PlayerCurrencyChanged;
	}

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

		btn.SetData(data);
		m_Buttons.Add(btn);
	}
}
