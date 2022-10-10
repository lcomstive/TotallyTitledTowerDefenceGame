using UnityEngine;

public class EnemyData : MonoBehaviour, IDamageable
{
	[SerializeField] WaveEnemy m_Data;
	[SerializeField] private float m_Health = 0;

	public WaveEnemy Data => m_Data;

	public float Health => m_Health;

	[field: SerializeField]
	public float MaxHealth { get; private set; } = 10.0f;

	private void OnDestroy()
	{
		if(m_Health > 0) // Most likely deleted in editor
			Destroyed?.Invoke(null);
	}

	public void SetData(WaveEnemy data)
	{
		m_Data = data;
		m_Health = m_Data.Health;
	}

	public void ApplyDamage(float damage)
	{
		m_Health -= damage;

		if(m_Health <= 0)
		{
			Destroyed?.Invoke(null);
			Destroy(gameObject);
		}
	}
	
	public void ApplyDamage(IDamageDealer dealer)
	{
		m_Health -= dealer.Damage;

		if(m_Health <= 0)
		{
			Destroyed?.Invoke(dealer);
			Destroy(gameObject);
		}
	}

	public event IDamageable.OnDestroyed Destroyed;
}
