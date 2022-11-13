using UnityEngine;
using System.Collections;

public class StickingProjectile : Projectile
{
	[SerializeField] private AnimationCurve m_ScaleCurve;

	protected override void Hit(GameObject other)
	{
		// Apply damage
		TurretData turretData = Shooter.Data as TurretData;
		if (turretData && other.TryGetComponent(out IDamageable damageable))
			damageable.ApplyDamage(turretData, Shooter.GetComponent<IUpgradeable>());

		StartCoroutine(SlowEnemy(other.GetComponent<IModifierHolder>(), other.transform));
	}

	private IEnumerator SlowEnemy(IModifierHolder modifierHolder, Transform otherTransform)
	{
		// Apply element
		if (modifierHolder != null)
			modifierHolder.TimedModifiers[Element] += ElementTime;

		transform.parent = otherTransform; // Stick... like goo...

		// Remove rigidbody, as this object is stuck to the parent and won't move
		if (TryGetComponent(out Rigidbody rb))
			Destroy(rb);

		// Destroy any collider, so other goos or bullets don't get deflected
		if (TryGetComponent(out Collider collider))
			Destroy(collider);

		yield return new WaitForSeconds(ElementTime);

		// Scale down this object before destroying
		float time = 0.0f;
		float maxTime = m_ScaleCurve.keys[^1].time;
		Vector3 originalScale = transform.localScale;
		while(time < maxTime)
		{
			transform.localScale = originalScale * m_ScaleCurve.Evaluate(time);
			yield return new WaitForEndOfFrame();
			time += Time.deltaTime;
		}
		Destroy(gameObject);
	}
}
