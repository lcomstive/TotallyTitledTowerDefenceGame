using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

	private Transform m_CurrentTarget = null;

	/// <summary>
	/// Delay to check for enemies in range, seconds.
	/// </summary>
	private const float EnemyCheckDelay = 0.5f;

	/// <summary>
	/// Maximum size of <see cref="m_EnemiesInRange"/>, limiting max amount of raycasts done each check
	/// </summary>
	private const int MaxEnemiesToCheck = 5;

	private TurretData m_Data;
	[SerializeField] private float m_ShootCooldown = 0.0f;
	[SerializeField] private List<Transform> m_EnemiesInRange = new List<Transform>();

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
		collider.radius = transform.InverseTransformPoint(Vector3.one * m_Data.VisionRadius).x;
		collider.radius /= 2.0f; // Radius, not diameter

		// Begin checking for enemies in radius
		StartCoroutine(CheckEnemiesInRangeLoop());
	}

	private void Update()
	{
		m_ShootCooldown = Mathf.Clamp(m_ShootCooldown - Time.deltaTime, 0, m_Data.FireRate);

		if (m_CurrentTarget == null)
			return;

		Vector3 delta = m_CurrentTarget.position - transform.position;
		Quaternion lookAtRotation = delta.magnitude == 0 ? Quaternion.identity :
			Quaternion.LookRotation(m_CurrentTarget.position - transform.position, Vector3.up);

		if (m_Data.RestrictRotation) // Restrict rotation to y axis
			lookAtRotation = Quaternion.Euler(m_Barrel.eulerAngles.x, lookAtRotation.eulerAngles.y, m_Barrel.eulerAngles.z);

		m_Barrel.rotation = Quaternion.Slerp(m_Barrel.rotation, lookAtRotation, Time.deltaTime * m_Data.RotationSpeed);

		if (m_ShootCooldown <= 0.0f && Quaternion.Angle(m_Barrel.rotation, lookAtRotation) < m_MinAngleToTargetBeforeShooting)
			Shoot();
	}

	private void Shoot()
	{
		GameObject bullet = Instantiate(m_Data.BulletPrefab, m_BarrelTip.position, m_BarrelTip.rotation);

		if (bullet.TryGetComponent(out Rigidbody rigidbody))
			rigidbody.AddForce(m_BarrelTip.forward * m_Data.BulletVelocity, ForceMode.Impulse);

		if (bullet.TryGetComponent(out BulletCollisionHandler bulletCollision))
			bulletCollision.Shooter = m_BuildableInfo;

		m_ShootCooldown = 1.0f / Mathf.Max(m_Data.FireRate, 0.1f);
	}

	private Transform ChooseTarget()
	{
		if (m_EnemiesInRange.Count == 0)
			return null;

		// Sort by distance
		m_EnemiesInRange.OrderBy(x => Vector3.Distance(m_BarrelTip.position, x.position)).ToList();

		for(int i = 0; i < m_EnemiesInRange.Count; i++)
		{
			// Check for line of sight
			Vector3 direction = m_EnemiesInRange[i].position - m_BarrelTip.position;
			if (!Physics.Raycast(m_BarrelTip.position, direction, out RaycastHit hit, m_Data.VisionRadius) ||
				hit.transform != m_EnemiesInRange[i]
				)
				continue;
			Debug.DrawRay(m_BarrelTip.position, direction * m_Data.VisionRadius, Color.green, EnemyCheckDelay);
			return m_EnemiesInRange[i];
		}

		return null;
	}

	private IEnumerator CheckEnemiesInRangeLoop()
	{
		m_EnemiesInRange.Clear();

		Collider[] colliders = Physics.OverlapSphere(transform.position, m_Data.VisionRadius / 2.0f);
		foreach (Collider collider in colliders)
			if (collider.CompareTag(m_EnemyTag))
				m_EnemiesInRange.Add(collider.transform);

		m_CurrentTarget = ChooseTarget();

		yield return new WaitForSeconds(EnemyCheckDelay);

		StartCoroutine(CheckEnemiesInRangeLoop());
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		Handles.color = new Color(1.0f, 0.66f, 0.35f, 1.0f);
		Handles.DrawSolidArc(m_Barrel.position, m_Barrel.up, m_Barrel.forward, m_MinAngleToTargetBeforeShooting, 1.0f);
	}
#endif
}
