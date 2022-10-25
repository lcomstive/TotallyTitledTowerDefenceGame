using System.Collections.Generic;

public interface IModifierHolder
{
	/// <summary>
	/// Key: Modifier to apply to this entity
	/// Value: Time remaining to apply element
	/// </summary>
	public Dictionary<Elements, float> TimedModifiers { get; }

	/// <summary>
	/// Modifiers that are applied to this entity
	/// </summary>
	public List<Elements> Modifiers { get; }

	/// <summary>
	/// Checks if <paramref name="element"/> is present in <see cref="Modifiers"/> or <see cref="TimedModifiers"/>
	/// </summary>
	public bool HasElement(Elements element);
}