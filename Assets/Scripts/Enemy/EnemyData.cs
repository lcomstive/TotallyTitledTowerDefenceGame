using UnityEngine;

public class EnemyData : MonoBehaviour
{
	[SerializeField] WaveEnemy m_Data;
	[SerializeField] private float m_Health = 0;

	public WaveEnemy Data => m_Data;

	public float Health => m_Health;

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
	
	public void ApplyDamage(IDamageable damageable)
	{
		m_Health -= damageable.Damage;

		if(m_Health <= 0)
		{
			Destroyed?.Invoke(damageable);
			Destroy(gameObject);
		}
	}

	public delegate void OnDestroyed(IDamageable damageable);
	public event OnDestroyed Destroyed;
}
