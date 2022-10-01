using System;
using UnityEngine;

public enum PlayState
{
	/// <summary>
	/// Wave is not currently spawning,
	/// player can buy, sell & place buildings
	/// </summary>
	Building,

	/// <summary>
	/// Waves are currently spawning.
	/// Player still has functionality of PlayState.Building,
	///		but also has the pressure of enemy waves spawning
	/// </summary>
	Play,

	/// <summary>
	/// Same as PlayState.Play, but sped up
	/// </summary>
	Play2x
}

[CreateAssetMenu(fileName = "Player Data", menuName = "Custom/Player Data")]
public class PlayerData : ScriptableObject
{
	private PlayState m_PlayState = PlayState.Building;

	public int Lives;
	public Currency Currency;
	public PlayState GameState
	{
		get => m_PlayState;
		set => OnStateChanged?.Invoke(m_PlayState, m_PlayState = value);
	}

	public delegate void OnStateChangedDelegate(PlayState previous, PlayState newValue);
	public event OnStateChangedDelegate OnStateChanged;
}
