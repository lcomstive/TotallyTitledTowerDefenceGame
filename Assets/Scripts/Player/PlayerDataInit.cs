using UnityEngine;

public class PlayerDataInit : MonoBehaviour
{
	[SerializeField] private PlayerData m_PlayerData;

	[Space()]

	[SerializeField] private int m_InitialPlayerLives = 99;
	[SerializeField] private Currency m_InitialCurrency = new Currency(100);

	private void Awake()
	{
		m_PlayerData.Lives = m_InitialPlayerLives;
		m_PlayerData.Currency = m_InitialCurrency;
	}
}
