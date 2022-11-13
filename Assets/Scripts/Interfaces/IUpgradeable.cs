using System;
using UnityEngine;

public enum UpgradeType
{
	DamageMultiplier,
	FireRate,
	VisionRadius
}

[Serializable]
public struct UpgradePath
{
	public AnimationCurve Values;
	public AnimationCurve Costs;

	public int MaxUpgrades => (int)Costs.keys[^1].time;

	public Currency CostForUpgrade(int upgrade) => Mathf.RoundToInt(Costs.Evaluate(upgrade));
	public float ValueForUpgrade(int upgrade) => Values.Evaluate(upgrade);
}

public interface IUpgradeable
{
	/// <summary>
	/// Has this upgrade been purchased at least once?
	/// </summary>
	public bool HasUpgrade(UpgradeType type);

	/// <summary>
	/// Is upgrade at it's maximum value?
	/// </summary>
	public bool IsUpgradeMax(UpgradeType type);

	/// <summary>
	/// Tries to update an upgrade
	/// </summary>
	public void TryUpgrade(UpgradeType type);

	/// <summary>
	/// Cost to acquire next upgrade
	/// </summary>
	public Currency CostForNextUpgrade(UpgradeType type);

	/// <summary>
	/// Value associated with upgrade, if it was the next level up
	/// </summary>
	public float ValueForNextUpgrade(UpgradeType type);

	/// <summary>
	/// Current value associated with upgrade
	/// </summary>
	public float ValueForCurrentUpgrade(UpgradeType type);

	/// <summary>
	/// Gets the current upgrade index of an upgrade, or 0 if not found
	/// </summary>
	public int GetCurrentUpgradeLevel(UpgradeType type);

	/// <summary>
	/// Checks if the upgrade can be bought
	/// </summary>
	public bool CanAffordUpgrade(UpgradeType type);
}
