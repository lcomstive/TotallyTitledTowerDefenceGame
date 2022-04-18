using TMPro;
using UnityEngine;

public class PlayerStatsDisplay : MonoBehaviour
{
	[SerializeField] private PlayerData m_PlayerData;

	[Header("UI")]
	[SerializeField] private TMP_Text m_LivesText;
	[SerializeField] private TMP_Text m_CurrencyText;

	// FixedUpdate because it calls (probably) less than Update
	private void FixedUpdate()
	{
		m_LivesText.text = m_PlayerData.Lives.ToString();
		m_CurrencyText.text = m_PlayerData.Currency.DisplayValue();
	}
}
