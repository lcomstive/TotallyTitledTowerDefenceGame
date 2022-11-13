using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class BuildableList : MonoBehaviour
{
	[SerializeField] private BuildableData[] m_Buildables;
	[SerializeField] private GameObject m_UIPrefab;
	[SerializeField] private PlayerData m_PlayerData;

	[SerializeField]
	private InputActionReference[] m_SelectInputs;

	private BuildableManager m_Manager;
	private List<BuildableButton> m_Buttons = new List<BuildableButton>();

	private void Start()
	{
		m_Manager = FindObjectOfType<BuildableManager>();

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

	private void OnEnable()
	{
		for (int i = 0; i < m_SelectInputs.Length; i++)
		{
			int buildingIndex = i;
			m_SelectInputs[i].action.started += (_) => SelectBuilding(buildingIndex);
		}
	}

	private void OnDisable()
	{
		for (int i = 0; i < m_SelectInputs.Length; i++)
		{
			int buildingIndex = i;
			m_SelectInputs[i].action.started -= (_) => SelectBuilding(buildingIndex);
		}
	}

	private void SelectBuilding(int index)
	{
		m_Manager.Deselect();
		m_Buttons[Mathf.Clamp(index, 0, m_Buttons.Count - 1)].OnClick();
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

		data.ResetData();

		btn.SetData(data, m_Manager);
		m_Buttons.Add(btn);
	}
}
