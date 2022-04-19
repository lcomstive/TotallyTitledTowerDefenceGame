using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public struct WaveEnemy
{
	public float Health;
	public GameObject Prefab;

	[Tooltip("Value representing difficulty of enemy. 0 is not difficult, 99 is extremely difficult")]
	public uint Difficulty;
}

[CreateAssetMenu(fileName = "Wave", menuName = "Custom/Wave")]
public class WaveData : ScriptableObject
{
	public int MaxRounds = 10;
	public int MinEnemiesAtOnce = 5;
	public int MaxEnemiesAtOnce = 30;
	public float EnemySpeedMultiplier = 1.0f;
	public float MinEnemySpawnInterval = 0.1f;
	public AnimationCurve DifficultyCurve;

	[Tooltip("Time, in seconds, between spawning enemies")]
	public float InitialRoundSpawnSpeed = 1.0f;

	public List<WaveEnemy> PotentialEnemies = new List<WaveEnemy>();
}
