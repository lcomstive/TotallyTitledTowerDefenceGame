using System;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[SelectionBase] // Select the object with this script instead of children
[RequireComponent(typeof(BuildableInfo))]
[RequireComponent(typeof(SphereCollider))]
public class Turret : MonoBehaviour, IUpgradeable
{
	[SerializeField] private Transform m_Barrel;
	[SerializeField] private string m_EnemyTag = "Enemy";

	[SerializeField, Tooltip("Minimum angle, in degrees, between aim directiona and target direction before shooting begins")]
	private float m_MinAngleToTargetBeforeShooting = 10;

	[SerializeField, Tooltip("Where to spawn bullets")]
	private Transform m_BarrelTip;

	[Space()]
	[SerializeField]
	private UnityEvent m_OnFired;

	[Header("Upgrades")]
	[SerializeField]
	private PlayerData m_PlayerData;

	public float FireRate => ValueForCurrentUpgrade(UpgradeType.FireRate);

	private BuildableInfo m_BuildableInfo;
	private Transform m_CurrentTarget = null;

	/// <summary>
	/// Delay to check for enemies in range, in seconds.
	/// </summary>
	private const float EnemyCheckDelay = 0.1f;

	private TurretData m_Data;
	private float m_ShootCooldown = 0.0f;
	private Coroutine m_EnemyCheckRoutine = null;
	private List<Transform> m_EnemiesInRange = new List<Transform>();
	private Dictionary<UpgradeType, int> m_Upgrades = new Dictionary<UpgradeType, int>();

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

		// Add all upgrade types to m_Upgrades
		foreach(var upgradeType in Enum.GetValues(typeof(UpgradeType)))
			m_Upgrades.Add((UpgradeType)upgradeType, 0);

		UpdateVisionRadius();

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
		m_ShootCooldown = Mathf.Clamp(m_ShootCooldown - Time.deltaTime, 0, 1.0f / FireRate);

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

		m_ShootCooldown = 1.0f / Mathf.Max(FireRate, 0.1f);

		m_OnFired?.Invoke();
	}

	private void UpdateVisionRadius()
	{
		int upgradeLevel = m_BuildableInfo.VisionRadiusUpgradeLevel;

		// Setup trigger collider
		SphereCollider collider = GetComponent<SphereCollider>();
		collider.isTrigger = true;
		collider.radius = transform.InverseTransformPoint(Vector3.one * m_Data.GetVisionRadius(transform.position.y, upgradeLevel)).x;
		collider.radius /= 2.0f; // Radius, not diameter

		// Update visuals
		if(m_BuildableInfo.IsRadiusShowing)
			m_BuildableInfo.ShowRadius(true);
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

	#region Upgrades
	public bool HasUpgrade(UpgradeType type) => m_Upgrades[type] > 0;
	public bool IsUpgradeMax(UpgradeType type) => m_Upgrades[type] >= GetPath(type).MaxUpgrades;

	public Currency CostForNextUpgrade(UpgradeType type) => Mathf.RoundToInt(GetPath(type).Costs.Evaluate(m_Upgrades[type] + 1));

	public bool CanAffordUpgrade(UpgradeType type) => CostForNextUpgrade(type) <= (m_PlayerData?.Currency ?? int.MaxValue);

	public float ValueForCurrentUpgrade(UpgradeType type) => GetPath(type).Values.Evaluate(m_Upgrades[type]);
	public float ValueForNextUpgrade(UpgradeType type) => GetPath(type).Values.Evaluate(m_Upgrades[type] + 1);

	public void TryUpgrade(UpgradeType type)
	{
		if (!m_Upgrades.ContainsKey(type) || !CanAffordUpgrade(type) || IsUpgradeMax(type))
			return;

		m_PlayerData.Currency -= CostForNextUpgrade(type);
		m_Upgrades[type]++;

		if (type == UpgradeType.VisionRadius)
			UpdateVisionRadius();
	}

	public int GetCurrentUpgradeLevel(UpgradeType type) => m_Upgrades.ContainsKey(type) ? m_Upgrades[type] : 0;

	private UpgradePath GetPath(UpgradeType type)
	{
		switch(type)
		{
			case UpgradeType.FireRate: return m_Data.FireRate;
			case UpgradeType.DamageMultiplier: return m_Data.DamageMultiplier;
			case UpgradeType.VisionRadius: return m_Data.VisionRadius;
			default:
				return new UpgradePath();
		}
	}
	#endregion

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		Handles.color = new Color(1.0f, 0.66f, 0.35f, 1.0f);
		Handles.DrawSolidArc(m_Barrel.position, m_Barrel.up, m_Barrel.forward, m_MinAngleToTargetBeforeShooting, 1.0f);
	}
#endif
}
