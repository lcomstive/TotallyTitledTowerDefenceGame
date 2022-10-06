using UnityEngine;

[CreateAssetMenu(fileName = "Slowing Turret", menuName = "Custom/Buildable/Slowing Turret")]
public class SlowingTurretData : TurretData
{
	[Tooltip("Amount to slow enemies. 1.0 = same speed, 0.5 = half speed, 2.0 = double speed")]
	public float SlowAmount = 0.85f;

	[Tooltip("How much time, in seconds, to apply slow effect")]
	public float SlowTime = 0.2f;
}
