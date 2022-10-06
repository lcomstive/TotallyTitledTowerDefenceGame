using UnityEngine;
using System.Collections;

public class GooProjectile : MonoBehaviour, IProjectile
{
	[SerializeField] private string m_EnemyTag;
	[SerializeField] private AnimationCurve m_ScaleCurve;

	private SlowingTurretData m_Data = null;

	public BuildableInfo Shooter { get; set; }

	private void Start() => m_Data = Shooter.Data as SlowingTurretData;

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.TryGetComponent(out TraversePath path))
			StartCoroutine(SlowEnemy(path));
	}

	private IEnumerator SlowEnemy(TraversePath path)
	{
		path.SpeedMultipliers.Add(m_Data.SlowAmount);
		transform.parent = path.transform; // Stick... like goo...

		// Remove rigidbody, as this object is stuck to the parent and won't move
		if (TryGetComponent(out Rigidbody rb))
			Destroy(rb);

		// Destroy any collider, so other goos or bullets don't get deflected
		if (TryGetComponent(out Collider collider))
			Destroy(collider);

		yield return new WaitForSeconds(m_Data.SlowTime);

		// If still exists, undo speed reduction
		if (path)
			path.SpeedMultipliers.Remove(m_Data.SlowAmount);

		float time = 0.0f;
		float maxTime = m_ScaleCurve.keys[^1].time;
		Vector3 originalScale = transform.localScale;
		while(time < maxTime)
		{
			transform.localScale = originalScale * m_ScaleCurve.Evaluate(time);
			yield return new WaitForEndOfFrame();
			time += Time.deltaTime;
		}
	}
}
