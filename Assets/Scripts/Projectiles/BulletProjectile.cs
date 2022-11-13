using System;
using UnityEngine;

public class BulletProjectile : Projectile
{
	[Serializable]
	private struct PrefabPair
	{
		public string Tag;
		public GameObject HitPrefab;
	}

	[SerializeField]
	private GameObject m_DefaultHitPrefab;

	[SerializeField]
	private PrefabPair[] m_HitPrefabs;

	protected override void Hit(GameObject other)
	{
		// Apply damage
		TurretData turretData = Shooter.Data as TurretData;
		if (turretData && other.TryGetComponent(out IDamageable damageable))
			damageable.ApplyDamage(turretData, Shooter.GetComponent<IUpgradeable>());

		// Apply element
		if (other.TryGetComponent(out IModifierHolder modifierHolder))
			modifierHolder.TimedModifiers[Element] += ElementTime;

		// Spawn hit prefab
		GameObject hitPrefab = ChooseHitPrefab(other.tag);
		if(hitPrefab)
			Instantiate(hitPrefab, transform.position, hitPrefab.transform.rotation);

		// Remove this object from scene
		Destroy(gameObject);
	}

	private GameObject ChooseHitPrefab(string tag)
	{
		foreach(PrefabPair pair in m_HitPrefabs)
		{
			if (pair.Tag.Equals(tag))
				return pair.HitPrefab;
		}
		return m_DefaultHitPrefab;
	}
}
