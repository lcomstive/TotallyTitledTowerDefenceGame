using System.Collections.Generic;

public interface IModifierHolder
{
	/// <summary>
	/// Key: Element
	/// Value: Time remaining until element is removed
	/// </summary>
	public Dictionary<Elements, float> Modifiers { get; }
}