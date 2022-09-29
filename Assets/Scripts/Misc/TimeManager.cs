using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class TimeManager : MonoBehaviour
{
	[SerializeField] private PlayerData m_PlayerData;

	[SerializeField]
	private AnimationCurve m_TimeChangeSpeed = new AnimationCurve(new Keyframe[]
	{
		new Keyframe(0, 0),
		new Keyframe(0.1f, 1.0f)
	});

	[Header("Time Values")]

	[SerializeField] private float PauseTimescale = 0.0f;
	[SerializeField] private float Play2xTimescale = 2.0f;

	public bool IsPaused { get; private set; }

	private Coroutine m_CurrentRoutine = null;

	private void Start()
	{
		Time.timeScale = 1.0f;

		m_PlayerData.GameState = PlayState.Building;
		m_PlayerData.OnStateChanged += OnPlayerStateChanged;
	}

	public void Pause(bool shouldPause = true)
	{
		IsPaused = shouldPause;

		SetTime(shouldPause ?
				PauseTimescale :
				(m_PlayerData.GameState == PlayState.Play2x ? Play2xTimescale : 1.0f));
	}

	private void OnPlayerStateChanged(PlayState value)
	{
		if (IsPaused)
			return;

		SetTime(value == PlayState.Play2x ? Play2xTimescale : 1.0f);
	}

	private void SetTime(float to)
	{
		if (m_CurrentRoutine != null)
			StopCoroutine(m_CurrentRoutine);
		m_CurrentRoutine = StartCoroutine(SetTime(Time.timeScale, to));
	}

	private IEnumerator SetTime(float from, float to)
	{
		if (Mathf.Abs(from - to) < 0.1f)
		{
			// Values are too close to justify lerping
			Time.timeScale = to;
			m_CurrentRoutine = null;
			yield return null;
		}

		float time = 0;
		float maxTime = m_TimeChangeSpeed.keys[^1].time;
		while (time < maxTime)
		{
			Time.timeScale = Mathf.Lerp(from, to, m_TimeChangeSpeed.Evaluate(time));
			yield return new WaitForEndOfFrame();
			time = Mathf.Clamp(time + Time.unscaledDeltaTime, 0, maxTime);
		}
		Time.timeScale = to;

		m_CurrentRoutine = null;
	}
}
