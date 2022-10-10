using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Threading.Tasks;
using TMPro;

public enum WaveState
{
	None,
	Spawning,

	/// <summary>
	/// Waiting for all enemies to die so next round can begin
	/// </summary>
	Waiting,

	/// <summary>
	/// Delay between rounds when autostart is enabled
	/// </summary>
	AutoStartDelay,

	/// <summary>
	/// Game end state has triggered
	/// </summary>
	GameEnded
}

public class WaveSpawner : MonoBehaviour
{
	[SerializeField] private WaveData m_WaveData;
	[SerializeField] private PlayerData m_PlayerData;
	[SerializeField] private Config m_Config;
	[SerializeField] private TMP_Text m_RoundsText;

	[SerializeField, Tooltip("Time between starting wave and spawning enemies, in seconds")]
	private float m_StartDelay = 2.0f;

	[SerializeField, Tooltip("Time between clearing enemies and starting next wave, in seconds")]
	private float m_AutoStartDelay = 2.0f;

	[SerializeField, Tooltip("When the wave spawning begins")]
	private UnityEvent m_WaveStarted;

	[SerializeField, Tooltip("Wave has finished spawning and all enemies no longer exist in scene")]
	private UnityEvent m_WaveFinished;
	
	[SerializeField, Tooltip("All enemies have been spawned")]
	private UnityEvent m_SpawningFinished;

	[Header("Game State Changes")]
	[SerializeField]
	private UnityEvent m_AllLivesLost;

	[SerializeField]
	private UnityEvent m_AllWavesFinished;

#if UNITY_EDITOR
	[Header("Debug")]
	[SerializeField] private int m_StartingRound = 0;
#endif

	public int Round { get; private set; } = 0;
	public int MaxRounds => m_WaveData?.MaxRounds ?? -1;
	public WaveData WaveData => m_WaveData;
	public bool IsWaveFinished => m_State == WaveState.None;

	public static WaveSpawner Instance { get; private set; }

	private int m_SpawnedEnemies = 0;
	private PlayState m_PreviousPlayState;
	private WaveState m_State = WaveState.None;

	private void Start()
	{
		if (Instance)
			Destroy(gameObject);
		else
			Instance = this;

		m_PlayerData.Lives = m_WaveData.PlayerLives;
		m_PlayerData.Currency.Copy(m_WaveData.PlayerStartingCurrency);

		m_PlayerData.GameState = PlayState.Building;

#if UNITY_EDITOR
		Round = m_StartingRound;
#endif

		m_PlayerData.OnStateChanged += OnGameStateChanged;

		if (m_RoundsText)
			m_RoundsText.text = $"{Round + 1} / {MaxRounds}";

		StartCoroutine(CheckRoundFinished());
	}

	private void OnGameStateChanged(PlayState oldState, PlayState newState)
	{
		if (oldState == PlayState.Building &&
			newState != PlayState.Building)
			NextWave();
	}

	private void OnDestroy()
	{
		if (Instance == this)
			Instance = null;
	}

	private async void NextWave()
	{
		if (!IsWaveFinished || !Instance)
			return;

		Debug.Log($"Starting round {Round + 1}/{MaxRounds + 1}...");
		m_WaveStarted?.Invoke();

		if (m_StartDelay > 0)
			await Task.Delay((int)(m_StartDelay * 1000));

		// Update gamestate
		if(m_PlayerData.GameState == PlayState.Building)
			m_PlayerData.GameState = m_PreviousPlayState;

		StartCoroutine(SpawnWave());
	}

	private IEnumerator SpawnWave()
	{
		if (!Path.Instance || Path.Instance.PathNodes == null || Path.Instance.PathNodes.Count == 0)
			yield break; // No path to spawn on

		m_State = WaveState.Spawning;

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

			enemyData.Destroyed += (damageable) =>
			{
				if(damageable != null)
					damageable.KillCount++;

				if(m_PlayerData)
					m_PlayerData.Currency += enemyData.Data.Reward;

				if(m_SpawnedEnemies > 0)
					m_SpawnedEnemies--;
			};

			TraversePath traversePath = go.GetComponentInChildren<TraversePath>();
			if (traversePath)
			{
				traversePath.Speed *= (1.0f + difficulty) * m_WaveData.InitialEnemySpeedMultiplier;
				traversePath.Speed = Mathf.Clamp(traversePath.Speed, 0.01f, m_WaveData.MaxEnemySpeedMultiplier);

				traversePath.FinishedPath += () =>
				{
					enemyData.ApplyDamage(float.MaxValue);
					m_PlayerData.Lives--;

					if (m_PlayerData.Lives <= 0 &&
						m_State != WaveState.GameEnded)
					{
						m_State = WaveState.GameEnded;
						m_AllLivesLost?.Invoke();
					}
				};
			}

			m_SpawnedEnemies++;

			// Time to wait between spawning enemies
			float spawnInterval = m_WaveData.InitialRoundSpawnSpeed * (1.0f - (Round / (float)MaxRounds));
			spawnInterval = Mathf.Max(spawnInterval, m_WaveData.MinEnemySpawnInterval);

			if(i < spawnCount - 1)
				yield return new WaitForSeconds(spawnInterval);
		}

		m_SpawningFinished?.Invoke();
		m_State = WaveState.Waiting;
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

	private IEnumerator RunWaveFinished()
	{
		m_SpawnedEnemies = 0;

		// C# Event
		WaveFinished?.Invoke();

		// Unity Event
		m_WaveFinished?.Invoke();

		m_PreviousPlayState = m_PlayerData.GameState;
		// m_PlayerData.GameState = PlayState.Building;

		Round++;
		if (m_RoundsText)
			m_RoundsText.text = $"{Round + 1} / {MaxRounds}";

		if (m_Config && m_Config.AutoStartRounds)
		{
			m_State = WaveState.AutoStartDelay;

			yield return new WaitForSeconds(m_AutoStartDelay);

			// Wave is finished after delay, so player cannot start
			//	next round during delay. This will cause two+ rounds at once.
			m_State = WaveState.None;

			NextWave();
		}
		else
		{
			m_PlayerData.GameState = PlayState.Building;
			m_State = WaveState.None;
		}
	}

	private IEnumerator CheckRoundFinished()
	{
		if (m_SpawnedEnemies <= 0 &&
			m_State == WaveState.Waiting)
		{
			if(Round < MaxRounds - 1)
				StartCoroutine(RunWaveFinished());
			else
			{
				m_AllWavesFinished?.Invoke();
				m_State = WaveState.GameEnded;
			}
		}

		if (m_State == WaveState.GameEnded)
			yield break;

		yield return new WaitForSeconds(1.0f);

		StartCoroutine(CheckRoundFinished());
	}

	public delegate void OnWaveFinished();
	public static event OnWaveFinished WaveFinished;
}
