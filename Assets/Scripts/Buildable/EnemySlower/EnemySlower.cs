using UnityEngine;

[SelectionBase] // Select the object with this script instead of children
[RequireComponent(typeof(BuildableInfo))]
[RequireComponent(typeof(SphereCollider))]
public class EnemySlower : MonoBehaviour
{
	private EnemySlowerData m_Data;

	private void Start()
	{
		m_Data = GetComponent<BuildableInfo>().Data as EnemySlowerData;

		SphereCollider trigger = GetComponent<SphereCollider>();
		trigger.isTrigger = true;
		trigger.radius = m_Data.GetVisionRadius(transform.position.y);
		trigger.radius /= 2.0f; // Radius, not diameter
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent(out TraversePath pathTraversal))
			pathTraversal.SpeedMultipliers.Add(m_Data.SlowMultiplier);
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.TryGetComponent(out TraversePath pathTraversal))
			pathTraversal.SpeedMultipliers.Remove(m_Data.SlowMultiplier);
	}
}
