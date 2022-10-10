public enum Elements
{
	/// <summary>
	/// It's wet.
	/// Increases effect of <see cref="Ice"/> & <see cref="Electricity"/>,
	/// but removes <see cref="Fire"/>
	/// </summary>
	Water,

	/// <summary>
	/// Reduces speed of a unit
	/// </summary>
	Ice,

	/// <summary>
	/// Inflicts damage to a unit over time
	/// </summary>
	Fire,

	/// <summary>
	/// Burns through armour, increasing damage from other units
	/// </summary>
	Acid,

	/// <summary>
	/// Halts unit for short period of time.
	/// Amplified by <see cref="Water"/> element.
	/// </summary>
	Electricity
}