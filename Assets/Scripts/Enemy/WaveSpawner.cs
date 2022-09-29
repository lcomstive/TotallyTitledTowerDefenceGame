using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Threading.Tasks;

public class WaveSpawner : MonoBehaviour
{
	[SerializeField] private WaveData m_WaveData;
	[SerializeField] private PlayerData m_PlayerData;

	[SerializeField, Tooltip("Time between starting wave and spawning enemies")]
	private float m_StartDelay = 2.0f;

	[SerializeField] private UnityEvent m_WaveStarted, m_WaveFinished;

#if UNITY_EDITOR
	[Header("Debug")]
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

		BuildableManager.PlayerData.Lives = m_WaveData.PlayerLives;
		BuildableManager.PlayerData.Currency.Copy(m_WaveData.PlayerStartingCurrency);

		m_PlayerData.GameState = PlayState.Building;

#if UNITY_EDITOR
		Round = m_StartingRound;
#endif

		m_PlayerData.OnStateChanged += OnGameStateChanged;
	}

	private void OnGameStateChanged(PlayState state)
	{
		if (state != PlayState.Building &&
			Instance && IsWaveFinished)
			NextWave();
	}

	private void OnDestroy()
	{
		if (Instance == this)
			Instance = null;
	}

	public async void NextWave()
	{
		if (!IsWaveFinished || !Instance)
			return;
		Debug.Log($"Starting round {Round + 1}/{MaxRounds + 1}...");
		m_WaveStarted?.Invoke();

		if (m_StartDelay > 0)
			await Task.Delay((int)(m_StartDelay * 1000));

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
			// TODO: Base this on difficulties of each enemy and difficulty curve
			// WaveEnemy selectedEnemy = m_WaveData.PotentialEnemies[0];
			WaveEnemy selectedEnemy = m_WaveData.PotentialEnemies[Random.Range(0, m_WaveData.PotentialEnemies.Count)];

			GameObject prefab = selectedEnemy.Prefab;
			GameObject go = Instantiate(prefab, spawnLocation, prefab.transform.rotation);

			EnemyData enemyData = go.GetComponent<EnemyData>();
			if (!enemyData)
				enemyData = go.AddComponent<EnemyData>();
			enemyData.SetData(selectedEnemy);

			enemyData.Destroyed += (info) =>
			{
				if(info)
					info.KillCount++;

				if(BuildableManager.PlayerData)
					BuildableManager.PlayerData.Currency += enemyData.Data.Reward;

				m_SpawnedEnemies--;
				if (m_SpawnedEnemies <= 0)
					RunWaveFinished();
			};

			TraversePath traversePath = go.GetComponentInChildren<TraversePath>();
			if (traversePath)
			{
				traversePath.Speed *= (1.0f + difficulty) * m_WaveData.InitialEnemySpeedMultiplier;
				traversePath.Speed = Mathf.Clamp(traversePath.Speed, 0.01f, m_WaveData.MaxEnemySpeedMultiplier);

				traversePath.FinishedPath += () =>
				{
					enemyData.ApplyDamage(float.MaxValue);
					BuildableManager.PlayerData.Lives--;
				};
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

	private int CalculateEnemiesToSpawn(float difficulty)
	{
		int enemies = m_WaveData ?
			Mathf.RoundToInt(
				difficulty * (m_WaveData.MaxEnemiesAtOnce - m_WaveData.MinEnemiesAtOnce) +
				m_WaveData.MinEnemiesAtOnce
				)
		: 0;
		return Mathf.Clamp(enemies, 0, m_WaveData.MaxEnemiesAtOnce);
	}

	private void RunWaveFinished()
	{
		m_SpawnedEnemies = 0;

		// C# Event
		WaveFinished?.Invoke();

		// Unity Event
		m_WaveFinished?.Invoke();

		m_PlayerData.GameState = PlayState.Building;

		Round++;
	}

	public delegate void OnWaveFinished();
	public static event OnWaveFinished WaveFinished;
}
