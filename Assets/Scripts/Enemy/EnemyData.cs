using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyData : MonoBehaviour
{
	[SerializeField] WaveEnemy m_Data;
	[SerializeField] private float m_Health = 0;

	public WaveEnemy Data => m_Data;

	public float Health => m_Health;

	private void OnDestroy()
	{
		if(m_Health >= 0) // Most likely deleted in editor
			Destroyed(null);
	}

	public void SetData(WaveEnemy data)
	{
		m_Data = data;
		m_Health = m_Data.Health;
	}

	public void ApplyDamage(float damage, BuildableInfo info)
	{
		m_Health -= damage;

		if(m_Health <= 0)
		{
			Destroyed(info);
			Destroy(gameObject);
		}
	}

	public delegate void OnDestroyed(BuildableInfo buildable);
	public event OnDestroyed Destroyed;
}
