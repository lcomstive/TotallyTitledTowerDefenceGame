using UnityEngine;

[CreateAssetMenu(fileName = "EnemySlower", menuName = "Custom/Buildable/Enemy Slower")]
public class EnemySlowerData : BuildableData
{
	[Tooltip("Amount to slow enemies. 1.0 = same speed, 0.5 = half speed, 2.0 = double speed")]
	public float SlowMultiplier = 0.85f;
}
