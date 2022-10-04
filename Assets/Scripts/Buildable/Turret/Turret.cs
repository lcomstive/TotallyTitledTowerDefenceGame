using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

[SelectionBase] // Select the object with this script instead of 
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
	/// Delay to check for enemies in range, milliseconds.
	/// </summary>
	private const int EnemyCheckDelay = 100;

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
		collider.radius = transform.InverseTransformPoint(Vector3.one * m_Data.GetVisionRadius(transform.position.y)).x;
		collider.radius /= 2.0f; // Radius, not diameter

		// Begin checking for enemies in radius
		CheckEnemiesInRangeLoop();
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

		if (bullet.TryGetComponent(out BulletCollisionHandler bulletCollision))
			bulletCollision.Shooter = m_BuildableInfo;

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

		/*
		for(int i = 0; i < Mathf.Min(m_EnemiesInRange.Count, MaxEnemiesToCheck); i++)
		{
			if (m_EnemiesInRange[i] == null)
				continue;

			// Check for line of sight
			Vector3 direction = m_EnemiesInRange[i].position - transform.position;
#if UNITY_EDITOR
			// For debugging. Don't include in release build
			if (Vector3.Distance(m_EnemiesInRange[i].position, transform.position) > m_Data.VisionRadius)
			{
				Debug.DrawRay(transform.position, direction * m_Data.VisionRadius, Color.yellow, EnemyCheckDelay / 1000.0f);
				continue;
			}
#endif
			if (!Physics.Raycast(transform.position, direction, out RaycastHit hit, m_Data.VisionRadius) ||
				hit.transform != m_EnemiesInRange[i]
				)
			{
				Debug.DrawRay(transform.position, direction * m_Data.VisionRadius, Color.red, EnemyCheckDelay / 1000.0f);
				continue;
			}
			Debug.DrawRay(transform.position, direction * m_Data.VisionRadius, Color.green, EnemyCheckDelay / 1000.0f);
			return m_EnemiesInRange[i];
		}
		return null;
		*/
	}

	private async void CheckEnemiesInRangeLoop()
	{
		m_EnemiesInRange.Clear();

		float visionRadius = m_Data.GetVisionRadius(transform.position.y);
		Collider[] colliders = Physics.OverlapSphere(transform.position, visionRadius / 2.0f);
		foreach (Collider collider in colliders)
			if (collider.CompareTag(m_EnemyTag))
				m_EnemiesInRange.Add(collider.transform);

		m_CurrentTarget = ChooseTarget();

		await Task.Delay(EnemyCheckDelay);

		if(Application.isPlaying)
			CheckEnemiesInRangeLoop();
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		Handles.color = new Color(1.0f, 0.66f, 0.35f, 1.0f);
		Handles.DrawSolidArc(m_Barrel.position, m_Barrel.up, m_Barrel.forward, m_MinAngleToTargetBeforeShooting, 1.0f);
	}
#endif
}
