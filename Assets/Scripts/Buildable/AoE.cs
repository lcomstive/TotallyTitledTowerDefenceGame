using System.Collections.Generic;
using UnityEngine;

[SelectionBase] // Select the object with this script instead of children
[RequireComponent(typeof(BuildableInfo))]
[RequireComponent(typeof(SphereCollider))]
public class AoE : MonoBehaviour
{
	[SerializeField, Tooltip("Element to apply to an IModifierHolder if hit")]
	protected Elements m_Element = Elements.Water;

	[SerializeField, Tooltip("How long to apply Element, in seconds. <= 0 element remains on holder until leaving trigger")]
	private float m_ElementTime = 1.0f;

	public BuildableData Data { get; protected set; }

	private List<IModifierHolder> m_ModifiedUnits = new List<IModifierHolder>();

	protected virtual void Start()
	{
		Data = GetComponent<BuildableInfo>().Data;

		SphereCollider trigger = GetComponent<SphereCollider>();
		trigger.isTrigger = true;
		trigger.radius = Data.GetVisionRadius(transform.position.y);
		trigger.radius /= 2.0f; // Radius, not diameter
	}

	protected virtual void OnDestroy()
	{
		foreach (IModifierHolder holder in m_ModifiedUnits)
			holder.Modifiers.Remove(m_Element);
	}

	protected virtual void OnTriggerEnter(Collider other)
	{
		if (!other.TryGetComponent(out IModifierHolder modifierHolder))
			return;
		m_ModifiedUnits.Add(modifierHolder);

		if (m_ElementTime > 0)
			modifierHolder.TimedModifiers[m_Element] += m_ElementTime;
		else
			modifierHolder.Modifiers.Add(m_Element);
	}

	protected virtual void OnTriggerExit(Collider other)
	{
		if (m_ElementTime <= 0 && other.TryGetComponent(out IModifierHolder modifierHolder))
		{
			modifierHolder.Modifiers.Remove(m_Element);
			m_ModifiedUnits.Remove(modifierHolder);
		}
	}
}
