using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveSpawner : MonoBehaviour
{
	[SerializeField] private WaveData m_WaveData;

#if UNITY_EDITOR
	[SerializeField] private int m_StartingRound = 0;
#endif

	public int Round { get; private set; } = 0;
	public int MaxRounds => m_WaveData?.MaxRounds ?? -1;
	public WaveData WaveData => m_WaveData;
	public bool IsWaveFinished => m_SpawnedEnemies <= 0;

	public static WaveSpawner Instance { get; private set; }

	private int m_SpawnedEnemies = 0;

	private void Start()
	{
		if (Instance)
			Destroy(gameObject);
		else
			Instance = this;

#if UNITY_EDITOR
		Round = m_StartingRound;
#endif
		NextWave();
	}

	private void OnDestroy()
	{
		if (Instance == this)
			Instance = null;
	}

	public void NextWave()
	{
		if (!IsWaveFinished || !Instance)
			return;
		Debug.Log($"Starting round {Round + 1}/{MaxRounds + 1}...");
		StartCoroutine(SpawnWave());
	}

	private IEnumerator SpawnWave()
	{
		if (!Path.Instance || Path.Instance.PathNodes == null || Path.Instance.PathNodes.Count == 0)
			yield break; // No path to spawn on

		m_SpawnedEnemies = 0;
		float roundProgress = Round / (float)MaxRounds;
		float difficulty = m_WaveData.DifficultyCurve.Evaluate(Round == 0 ? 0.0f : roundProgress);
		if(roundProgress > 1.0f)
			difficulty = roundProgress;
		Vector3 spawnLocation = Path.Instance.PathNodes[0].Position;
		int spawnCount = CalculateEnemiesToSpawn(difficulty);
		Debug.Log($"Difficulty: {Mathf.RoundToInt(difficulty * 100)}% | Enemies spawning: {spawnCount}");
		for (int i = 0; i < spawnCount; i++)
		{
			WaveEnemy selectedEnemy = m_WaveData.PotentialEnemies[0]; // TODO: Base this on difficulties of each enemy and difficulty curve

			GameObject prefab = selectedEnemy.Prefab;
			GameObject go = Instantiate(prefab, spawnLocation, prefab.transform.rotation);

			EnemyData enemyData = go.GetComponent<EnemyData>();
			if (!enemyData)
				enemyData = go.AddComponent<EnemyData>();
			enemyData.SetData(selectedEnemy);

			enemyData.Destroyed += (_) =>
			{
				m_SpawnedEnemies--;
				if (m_SpawnedEnemies <= 0)
					RunWaveFinished();
			};

			TraversePath traversePath = go.GetComponentInChildren<TraversePath>();
			if (traversePath)
			{
				traversePath.Speed *= (1.0f + difficulty) * m_WaveData.EnemySpeedMultiplier;
				traversePath.FinishedPath += () => enemyData.ApplyDamage(float.MaxValue, null);
			}

			m_SpawnedEnemies++;

			// Time to wait between spawning enemies
			float spawnInterval = m_WaveData.InitialRoundSpawnSpeed * (1.0f - (Round / (float)MaxRounds));
			spawnInterval = Mathf.Max(spawnInterval, m_WaveData.MinEnemySpawnInterval);
			yield return new WaitForSeconds(spawnInterval);
		}

		if(m_SpawnedEnemies <= 0)
			RunWaveFinished(); // No enemies spawned?
	}

	private int CalculateEnemiesToSpawn(float difficulty) =>
		m_WaveData ?
			Mathf.RoundToInt(
				difficulty * (m_WaveData.MaxEnemiesAtOnce - m_WaveData.MinEnemiesAtOnce) +
				m_WaveData.MinEnemiesAtOnce
				)
		: 0;

	private void RunWaveFinished()
	{
		m_SpawnedEnemies = 0;
		WaveFinished?.Invoke();
		Round++;

		// TESTING
		NextWave();
	}

	public delegate void OnWaveFinished();
	public static event OnWaveFinished WaveFinished;
}
