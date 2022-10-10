using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ModifierHolder : MonoBehaviour, IModifierHolder
{
	/// <summary>
	/// Modifiers attached to this GameObject
	/// </summary>
	public Dictionary<Elements, float> Modifiers { get; } = new Dictionary<Elements, float>();

	/// <summary>
	/// Amount of damage to apply every tick
	/// </summary>
	public const float FireTickDamage = 1;

	/// <summary>
	/// Duration of one tick
	/// </summary>
	public const float TickTime = 0.1f;

	/// <summary>
	/// Local health-holder
	/// </summary>
	private IDamageable m_Damageable;

	void Start()
	{
		// Get damageable in this object
		m_Damageable = GetComponent<IDamageable>();

		// Begin tick loop
		if(m_Damageable != null)
			StartCoroutine(TickLoop());
	}

	private void Update()
	{
		// Reduce each modifier by delta time
		foreach (var pair in Modifiers)
			Modifiers[pair.Key] = Mathf.Clamp(pair.Value - Time.deltaTime, 0, 999);
	}

	private IEnumerator TickLoop()
	{
		// If fire element present, apply tick damage
		if (Modifiers[Elements.Fire] > 0)
			m_Damageable.ApplyDamage(FireTickDamage);

		yield return new WaitForSeconds(TickTime);

		if (Application.isPlaying && m_Damageable != null)
			StartCoroutine(TickLoop());
	}

	public float this[Elements element]
	{
		get => Modifiers[element];
		set => Modifiers[element] = value;
	}
}
