using UnityEngine;

public class BulletProjectile : MonoBehaviour, IProjectile
{
	[SerializeField] private GameObject m_HitEnemyPrefab;
	[SerializeField] private GameObject m_HitOtherPrefab;

	[SerializeField] private string m_EnemyTag;

	public BuildableInfo Shooter { get; set; }

	private void OnCollisionEnter(Collision collision)
	{
		Destroy(gameObject);
		Hit(collision.gameObject, collision.transform.CompareTag(m_EnemyTag));
	}

	protected virtual void Hit(GameObject other, bool isEnemy)
	{
		GameObject prefab = null;
		if (isEnemy)
		{
			prefab = m_HitEnemyPrefab;
			IDamageable damageable = Shooter.Data as IDamageable;
			if (damageable != null && other.TryGetComponent(out EnemyData enemyData))
				enemyData.ApplyDamage(damageable);
		}
		else
			prefab = m_HitOtherPrefab;

		Instantiate(prefab, transform.position, prefab.transform.rotation);
	}
}