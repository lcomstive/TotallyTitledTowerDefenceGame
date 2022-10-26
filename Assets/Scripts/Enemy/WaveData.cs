using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public struct WaveEnemy
{
	public float Health;
	public GameObject Prefab;

	[Tooltip("Multiplier to input damage when acid modifier is present")]
	public float AcidMultiplier;

	[Tooltip("Value representing difficulty of enemy. 0 is not difficult, 99 is extremely difficult")]
	public uint Difficulty;

	public Currency Reward;
}

[CreateAssetMenu(fileName = "Wave", menuName = "Custom/Wave")]
public class WaveData : ScriptableObject
{
	public int PlayerLives = 50;
	public Currency PlayerStartingCurrency = new Currency(100);

	public int MaxRounds = 10;
	public int MinEnemiesAtOnce = 5;
	public int MaxEnemiesAtOnce = 30;
	public float MaxEnemySpeedMultiplier = 10.0f;
	public float InitialEnemySpeedMultiplier = 1.0f;
	public float MaxEnemyAdditionalHealthMultiplier = 2.5f;
	public float MinEnemySpawnInterval = 0.1f;
	public AnimationCurve DifficultyCurve;

	[Tooltip("Time, in seconds, between spawning enemies")]
	public float InitialRoundSpawnSpeed = 1.0f;

	public List<WaveEnemy> PotentialEnemies = new List<WaveEnemy>();
}
