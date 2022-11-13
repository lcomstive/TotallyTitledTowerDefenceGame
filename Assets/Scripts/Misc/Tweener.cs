using UnityEngine;
using System.Collections;

public abstract class Tweener : MonoBehaviour
{
	[Tooltip("When enabled, ignores changes in the game's timescale")]
	public bool UseUnscaledTime = false;

	[Tooltip("Value to evaluate over time. Horizontal axis represents time, vertical axis is value passed to TweenFrame")]
	public AnimationCurve Curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

	[Tooltip("When enabled, starts & stops tweening to match attached GameObject's active in hierarchy status")]
	public bool ActivateWithObject = false;

	private Coroutine m_Instance = null;

	protected virtual void OnEnable()
	{
		if (ActivateWithObject)
			StartTween();
	}

	protected virtual void OnDisable()
	{
		if (ActivateWithObject)
			StopTween();
	}

	public void StartTween()
	{
		if (m_Instance != null)
			StopTween();
		m_Instance = StartCoroutine(DoTween());
	}

	public void StopTween()
	{
		if (m_Instance == null)
			return;
		StopCoroutine(m_Instance);
		m_Instance = null;
	}

	private IEnumerator DoTween()
	{
		// When loading into scene or entering playmode, Time.unscaledDeltaTime is a larger value than expected.
		// To get a better value, we wait an additional frame for the scene to begin
		if (UseUnscaledTime)
			yield return new WaitForEndOfFrame();

		OnTweenBegin();

		float time = 0;
		float maxTime = Curve.keys[^1].time; // Last key in curve
		while (time < maxTime)
		{
			// Call tween update
			TweenFrame(Curve.Evaluate(time));

			// Wait for frame to render
			yield return new WaitForEndOfFrame();

			// Append time
			time += UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
		}

		// Make sure final value is set correctly
		TweenFrame(Curve.Evaluate(maxTime));

		// Clear coroutine instance, as coroutine has finished
		m_Instance = null;

		OnTweenEnd();
	}

	/// <summary>
	/// When the tween is about to start
	/// </summary>
	protected virtual void OnTweenBegin() { }

	/// <summary>
	/// After the tween has finished
	/// </summary>
	protected virtual void OnTweenEnd() { }

	protected abstract void TweenFrame(float value);
}
