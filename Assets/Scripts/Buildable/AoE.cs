using System.Collections.Generic;
using UnityEngine;

[SelectionBase] // Select the object with this script instead of children
[RequireComponent(typeof(BuildableInfo))]
[RequireComponent(typeof(SphereCollider))]
public class AoE : MonoBehaviour
{
	[SerializeField, Tooltip("Element to apply to an IModifierHolder if hit")]
	private Elements m_Element = Elements.Water;

	[SerializeField, Tooltip("How long to apply Element, in seconds. <= 0 element remains on holder until leaving trigger")]
	private float m_ElementTime = 1.0f;

	private BuildableData m_Data;

	private List<IModifierHolder> m_ModifiedUnits = new List<IModifierHolder>();

	private void Start()
	{
		m_Data = GetComponent<BuildableInfo>().Data;

		SphereCollider trigger = GetComponent<SphereCollider>();
		trigger.isTrigger = true;
		trigger.radius = m_Data.GetVisionRadius(transform.position.y);
		trigger.radius /= 2.0f; // Radius, not diameter
	}

	private void OnDestroy()
	{
		foreach (IModifierHolder holder in m_ModifiedUnits)
			holder.Modifiers.Remove(m_Element);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!other.TryGetComponent(out IModifierHolder modifierHolder))
			return;
		m_ModifiedUnits.Add(modifierHolder);

		if (m_ElementTime > 0)
			modifierHolder.TimedModifiers[m_Element] += m_ElementTime;
		else
			modifierHolder.Modifiers.Add(m_Element);
	}

	private void OnTriggerExit(Collider other)
	{
		if (m_ElementTime <= 0 && other.TryGetComponent(out IModifierHolder modifierHolder))
		{
			modifierHolder.Modifiers.Remove(m_Element);
			m_ModifiedUnits.Remove(modifierHolder);
		}
	}
}
