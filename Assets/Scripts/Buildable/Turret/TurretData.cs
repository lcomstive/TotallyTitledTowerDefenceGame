using UnityEngine;

public enum TurretAimSetting
{
	First,
	Closest
}

[CreateAssetMenu(fileName = "Turret", menuName = "Custom/Buildable/Turret")]
public class TurretData : DamageableBuildableData
{
	[Header("Turret Specific")]
	public GameObject BulletPrefab;

	[Tooltip("Shots per seconds")]
	public float FireRate = 2.0f;

	public float BulletVelocity = 10.0f;

	[Tooltip("Speed of rotation to aim at target")]
	public float RotationSpeed = 2.0f;

	[Tooltip("When true, only rotates on the Y axis")]
	public bool RestrictRotation = false;
}