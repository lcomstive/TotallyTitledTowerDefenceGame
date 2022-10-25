using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

[SelectionBase] // Select the object with this script instead of children
[RequireComponent(typeof(BuildableInfo))]
[RequireComponent(typeof(SphereCollider))]
public class Turret : MonoBehaviour
{
	[SerializeField] private Transform m_Barrel;
	[SerializeField] private string m_EnemyTag = "Enemy";

	[SerializeField, Tooltip("Minimum angle, in degrees, between aim directiona and target direction before shooting begins")]
	private float m_MinAngleToTargetBeforeShooting = 10;

	[SerializeField, Tooltip("Where to spawn bullets")]
	private Transform m_BarrelTip;
	private BuildableInfo m_BuildableInfo;

	[Space()]
	[SerializeField]
	private UnityEvent m_OnFired;

	private Transform m_CurrentTarget = null;

	/// <summary>
	/// Delay to check for enemies in range, in seconds.
	/// </summary>
	private const float EnemyCheckDelay = 0.1f;

	private TurretData m_Data;
	private float m_ShootCooldown = 0.0f;
	private Coroutine m_EnemyCheckRoutine = null;
	private List<Transform> m_EnemiesInRange = new List<Transform>();

	private void Start()
	{
		m_BuildableInfo = GetComponent<BuildableInfo>();
		// Get data
		m_Data = m_BuildableInfo.Data as TurretData;
		if (!m_Data)
		{
			Debug.LogError("Buildable info does not contain turret data");
			enabled = false;
			return;
		}

		// Setup collider
		SphereCollider collider = GetComponent<SphereCollider>();
		collider.isTrigger = true;
		collider.radius = transform.InverseTransformPoint(Vector3.one * m_Data.GetVisionRadius(transform.position.y)).x;
		collider.radius /= 2.0f; // Radius, not diameter

		// Begin checking for enemies in radius
		m_EnemyCheckRoutine = StartCoroutine(CheckEnemiesInRangeLoop());
	}

	private void OnDestroy()
	{
		if(m_EnemyCheckRoutine != null)
			StopCoroutine(m_EnemyCheckRoutine);
	}

	private void Update()
	{
		m_ShootCooldown = Mathf.Clamp(m_ShootCooldown - Time.deltaTime, 0, 1.0f / m_Data.FireRate);

		if (m_CurrentTarget == null)
			return;

		Vector3 delta = m_CurrentTarget.position - transform.position;
		Quaternion lookAtRotation = delta.magnitude == 0 ? Quaternion.identity :
			Quaternion.LookRotation(m_CurrentTarget.position - transform.position, Vector3.up);

		if (m_Data.RestrictRotation) // Restrict rotation to y axis
			lookAtRotation = Quaternion.Euler(m_Barrel.eulerAngles.x, lookAtRotation.eulerAngles.y, m_Barrel.eulerAngles.z);

		m_Barrel.rotation = Quaternion.Slerp(m_Barrel.rotation, lookAtRotation, Time.deltaTime * m_Data.RotationSpeed);

		float visionRadius = m_Data.GetVisionRadius(transform.position.y);
		if (m_ShootCooldown <= 0.0f && Quaternion.Angle(m_Barrel.rotation, lookAtRotation) < m_MinAngleToTargetBeforeShooting &&
			Physics.Raycast(m_BarrelTip.position, -delta.normalized, visionRadius))
			Shoot();
	}

	private void Shoot()
	{
		GameObject bullet = Instantiate(m_Data.BulletPrefab, m_BarrelTip.position, m_BarrelTip.rotation);

		if (bullet.TryGetComponent(out Rigidbody rigidbody))
			rigidbody.AddForce(m_BarrelTip.forward * m_Data.BulletVelocity, ForceMode.Impulse);

		if (bullet.TryGetComponent(out IProjectile projectile))
		{
			projectile.Shooter = m_BuildableInfo;
			projectile.Element = m_Data.Element;
			projectile.ElementTime = m_Data.ElementTime;
		}

		m_ShootCooldown = 1.0f / Mathf.Max(m_Data.FireRate, 0.1f);

		m_OnFired?.Invoke();
	}

	private Transform ChooseTarget()
	{
		if (m_EnemiesInRange.Count == 0)
			return null;

		// Sort by distance
		m_EnemiesInRange.OrderBy(x => Vector3.Distance(transform.position, x.position)).ToList();
		return m_EnemiesInRange[0];
	}

	private IEnumerator CheckEnemiesInRangeLoop()
	{
		m_EnemiesInRange.Clear();

		float visionRadius = m_Data.GetVisionRadius(transform.position.y);
		Collider[] colliders = Physics.OverlapSphere(transform.position, visionRadius / 2.0f);
		foreach (Collider collider in colliders)
			if (collider.CompareTag(m_EnemyTag))
				m_EnemiesInRange.Add(collider.transform);

		m_CurrentTarget = ChooseTarget();

		yield return new WaitForSeconds(EnemyCheckDelay);

		m_EnemyCheckRoutine = StartCoroutine(CheckEnemiesInRangeLoop());
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		Handles.color = new Color(1.0f, 0.66f, 0.35f, 1.0f);
		Handles.DrawSolidArc(m_Barrel.position, m_Barrel.up, m_Barrel.forward, m_MinAngleToTargetBeforeShooting, 1.0f);
	}
#endif
}
