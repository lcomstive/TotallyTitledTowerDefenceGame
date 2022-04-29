using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCollisionHandler : MonoBehaviour
{
	public BuildableInfo Shooter;
	[SerializeField] private GameObject m_HitEnemyPrefab;
	[SerializeField] private GameObject m_HitOtherPrefab;

	[SerializeField] private string m_EnemyTag;

	private void OnCollisionEnter(Collision collision)
	{
		Destroy(gameObject);

		GameObject prefab = null;
		if(collision.transform.CompareTag(m_EnemyTag))
		{
			prefab = m_HitEnemyPrefab;

			EnemyData enemyData = collision.gameObject.GetComponent<EnemyData>();
			if(enemyData)
				enemyData.ApplyDamage(Shooter);
		}
		else
			prefab = m_HitOtherPrefab;

		Instantiate(prefab, transform.position, prefab.transform.rotation);
	}
}
