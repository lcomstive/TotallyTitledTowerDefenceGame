using TMPro;
using UnityEngine;

public class DamageTextSpawner : MonoBehaviour
{
	[SerializeField]
	private GameObject m_TextPrefab;

	// Cache local component
	private IDamageable m_Damageable;

	private const float MinDamageShown = 1;
	private const float MaxDamageShown = 99999999;
	
	private void Start()
	{
		m_Damageable = GetComponent<IDamageable>();
		m_Damageable.Damaged += OnDamaged;
	}

	private void OnDamaged(float amount, IDamageDealer dealer)
	{
		if (amount < MinDamageShown || amount > MaxDamageShown)
			return; // Don't show values outside of desired range

		GameObject spawned = Instantiate(m_TextPrefab, transform.position, Quaternion.identity);
		TMP_Text text = spawned.GetComponentInChildren<TMP_Text>();
		text.text = string.Format("{0:n0}", amount);
	}
}
