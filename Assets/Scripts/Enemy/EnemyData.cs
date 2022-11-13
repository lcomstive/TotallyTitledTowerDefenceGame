using TMPro;
using UnityEngine;

public class EnemyData : MonoBehaviour, IDamageable
{
	[SerializeField] WaveEnemy m_Data;
	[SerializeField] private float m_Health = 0;

	public WaveEnemy Data => m_Data;

	public float Health => m_Health;

	[field: SerializeField]
	public float MaxHealth { get; private set; } = 10.0f;

	[SerializeField]
	private GameObject m_DamageTextPrefab;

	private IModifierHolder m_ModifierHolder;

	private void Start() => m_ModifierHolder = GetComponent<IModifierHolder>();

	private void OnDestroy()
	{
		if(m_Health > 0) // Most likely deleted in editor
			Destroyed?.Invoke(null);
	}

	public void SetData(WaveEnemy data, float healthMultiplier = 1.0f)
	{
		m_Data = data;
		m_Health = (MaxHealth = m_Data.Health * healthMultiplier);
	}

	public void ApplyDamage(float damage)
	{
		damage = CalculateDamage(damage, null);
		m_Health -= damage;
		Damaged?.Invoke(damage, null);

		if (m_Health <= 0)
		{
			Destroyed?.Invoke(null);
			Destroy(gameObject);
		}
	}
	
	public void ApplyDamage(IDamageDealer dealer, IUpgradeable upgradeable)
	{
		float damage = CalculateDamage(dealer.Damage, upgradeable);
		m_Health -= damage;
		Damaged?.Invoke(damage, dealer);

		if(m_Health <= 0)
		{
			Destroyed?.Invoke(dealer);
			Destroy(gameObject);
		}
	}

	private float CalculateDamage(float initialValue, IUpgradeable upgradeable)
	{
		// Apply damage multiplier from upgrades
		float damageMultiplier = upgradeable?.ValueForCurrentUpgrade(UpgradeType.DamageMultiplier) ?? 1.0f;
		initialValue *= damageMultiplier;

		// Apply acid modifier, if present
		if (m_ModifierHolder != null && m_ModifierHolder.HasElement(Elements.Acid))
			initialValue *= Data.AcidMultiplier;

		return initialValue;
	}

	public event IDamageable.OnDamaged Damaged;
	public event IDamageable.OnDestroyed Destroyed;
}
