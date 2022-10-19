using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.tvOS;
using UnityEngine.Networking.Types;


public class ModifierHolder : MonoBehaviour, IModifierHolder
{
	/// <summary>
	/// Modifiers attached to this GameObject
	/// </summary>
	public Dictionary<Elements, float> TimedModifiers { get; } = new Dictionary<Elements, float>();

	/// <summary>
	/// Modifiers that are applied to this entity
	/// </summary>
	[field: SerializeField]
	public List<Elements> Modifiers { get; private set; } = new List<Elements>();

	#region Events
	[System.Serializable]
	public struct ElementEvent
	{
		public Elements Element;

		[Tooltip("Event that passes new state (active/inactive) of element's presence")]
		public UnityEvent<bool> OnStatusChanged;

		public UnityEvent OnAdded;
		public UnityEvent OnRemoved;
	}
	[SerializeField]
	private List<ElementEvent> m_ElementEvents = new List<ElementEvent>();
	#endregion

	#region Element Constants
	/// <summary>
	/// Duration of one tick
	/// </summary>
	public const float TickTime = 0.5f;

	/// <summary>
	/// Multiplier applied to traversal speed, compounding.
	/// </summary>
	public const float IceSlowAmount = 0.65f;

	/// <summary>
	/// Ice effect multiplier when water element is present.
	/// </summary>
	public const float IceAmplifierWater = 0.85f;

	/// <summary>
	/// When ice is converted to water (from fire),
	/// how long should water element be present
	/// </summary>
	public const float IceToWaterTime = 1.0f;

	/// <summary>
	/// Amount of damage to apply every tick
	/// </summary>
	public const float FireTickDamage = 3.5f;

	/// <summary>
	/// Multiplier for electricity time reduced when water present.
	/// </summary>
	public const float ElectricityTimeAmplifierWater = 0.85f;
	#endregion

	#region Local Components
	/// <summary>
	/// Local health-holder
	/// </summary>
	private IDamageable m_Damageable;

	/// <summary>
	/// Local component to traverse path. Used when speed modifiers are applied (<see cref="Elements.Ice"/>)
	/// </summary>
	private TraversePath m_TraversePath;
	#endregion

	/// <summary>
	/// Tracks whether modifier is applied or not to this object.
	/// Updated every frame, used to fire events for adding/removing <see cref="Elements"/>
	/// </summary>
	private Dictionary<Elements, bool> m_ModifierPresent = new Dictionary<Elements, bool>();

	void Start()
	{
		// Add all elements to timed modifier & present dictionaries.
		// This is done to prevent future safety checks to see if the element is in the dictionary.
		foreach (object enumItem in System.Enum.GetValues(typeof(Elements)))
		{
			m_ModifierPresent.Add((Elements)enumItem, false);
			TimedModifiers.Add((Elements)enumItem, 0.0f);
		}

		// Trigger all element events as modifier not being present
		foreach (ElementEvent events in m_ElementEvents)
			TriggerEvents(events, false);

		// Get damageable in this object
		m_Damageable = GetComponent<IDamageable>();

		// Get traversable path component
		m_TraversePath = GetComponent<TraversePath>();
		
		// Begin tick loop
		if (m_Damageable != null)
			StartCoroutine(TickLoop());
	}

	public bool HasElement(Elements element) => m_ModifierPresent[element];

	public void RemoveElement(Elements element)
	{
		if (!HasElement(element))
			return;

		TimedModifiers[element] = 0.0f;
		Modifiers.RemoveAll(x => x == element);

		// Invoke event(s) for element being removed
		TriggerEvents(element, false);
	}

	private void Update()
	{
		List<Elements> keys = TimedModifiers.Keys.ToList();
		foreach (Elements key in keys)
		{
			float timeReduceMultiplier = 1.0f;
			if (key == Elements.Electricity)
				timeReduceMultiplier = ElectricityTimeAmplifierWater;

			// Reduce each modifier by delta time
			TimedModifiers[key] = Mathf.Clamp(TimedModifiers[key] - Time.deltaTime * timeReduceMultiplier, 0, 999);

			// Check if element presence has changed
			bool elementPresent = TimedModifiers[key] > 0 || Modifiers.Contains(key);
			if (elementPresent != m_ModifierPresent[key])
				TriggerEvents(key, elementPresent);
		}

		// Apply elements
		ApplyWaterElement();
		ApplyIceElement();
		ApplyElectricityElement();
	}

	private IEnumerator TickLoop()
	{
		// If ice element present, remove one water element
		if(HasElement(Elements.Ice) && HasElement(Elements.Water))
		{
			// Remove one water element modifier
			if (Modifiers.Contains(Elements.Water))
				Modifiers.Remove(Elements.Water);

			// Decrease timed modifier by 2xTickTime seconds
			else if (TimedModifiers[Elements.Water] > 0)
				TimedModifiers[Elements.Water] -= TickTime * 2.0f;
		}

		// If fire element present, apply tick damage.
		// When ice is also present, convert it in to water and remove fire
		if (HasElement(Elements.Fire))
		{
			if(HasElement(Elements.Ice))
			{
				// Count all ice
				float iceTime = TimedModifiers[Elements.Ice];
				float iceModifiers = Modifiers.Count(x => x == Elements.Ice);

				// Remove all fire & ice
				RemoveElement(Elements.Ice);
				RemoveElement(Elements.Fire);

				// Replace all ice with water element
				TimedModifiers[Elements.Water] += iceTime + iceModifiers * IceToWaterTime;
			}
			else
				m_Damageable.ApplyDamage(FireTickDamage);
		}

		yield return new WaitForSeconds(TickTime);

		if (Application.isPlaying && m_Damageable != null)
			StartCoroutine(TickLoop());
	}

	/// <summary>
	/// <see cref="Elements.Water"/>
	///		- Is primarily used to amplify other elements (e.g. ice, electricity)
	///		- Removes fire element
	/// </summary>
	private void ApplyWaterElement()
	{
		if (HasElement(Elements.Water))
			RemoveElement(Elements.Fire);
	}

	/// <summary>
	/// <see cref="Ice"/>
	///		- Slows affected unit, this effect compounds
	///		- Consumes one water element per tick
	/// </summary>
	private void ApplyIceElement()
	{
		if (!m_TraversePath)
			return; // Nothing to apply ice effect to

		// Count amount of modifiers to apply		
		int iceModifiers = Modifiers.Count(x => x == Elements.Ice);

		// +1 if timed modifier exists
		if (TimedModifiers[Elements.Ice] > 0)
			iceModifiers++;

		float speedMultiplier = iceModifiers > 0 ? Mathf.Pow(IceSlowAmount, iceModifiers) : 1.0f;
		if (iceModifiers > 0 && HasElement(Elements.Water))
			speedMultiplier *= IceAmplifierWater;

		// Apply speed modifier
		m_TraversePath.SpeedMultiplier = speedMultiplier;
	}

	/// <summary>
	/// <see cref="Electricity"/>
	///		- Halts unit for short period of time
	///		- Time increased when water is present
	///		- Cannot be applied to units with ice present
	/// </summary>
	private void ApplyElectricityElement()
	{
		if (HasElement(Elements.Electricity) && m_TraversePath)
			m_TraversePath.SpeedMultiplier = 0.0f;
	}

	/// <summary>
	/// Updates <see cref="m_ModifierPresent"/> and invokes event(s) for status change of an element
	/// </summary>
	/// <param name="element"></param>
	/// <param name="status"></param>
	private void TriggerEvents(Elements element, bool status) => TriggerEvents(m_ElementEvents.Find(x => x.Element == element), status);

	private void TriggerEvents(ElementEvent events, bool status)
	{
		// Update dictionary
		m_ModifierPresent[events.Element] = status;

		// Fire event(s)
		events.OnStatusChanged?.Invoke(status);

		if (status)
			events.OnAdded?.Invoke();
		else
			events.OnRemoved?.Invoke();
	}
}
