using System.Collections;
using UnityEngine;

public class ScaleTween : MonoBehaviour
{
	public Vector3 From = Vector3.zero;
	public Vector3 Target = Vector3.one;
	public AnimationCurve SpeedCurve = new AnimationCurve(new Keyframe[]
	{
		new Keyframe(0, 0),
		new Keyframe(1, 1)
	});

	public bool UseUnscaledTime = true;

	[SerializeField, Tooltip("When enabled, sets this object's scale to From in Start()")]
	private bool ApplyFromAtStart = false;

	private Coroutine m_CurrentRoutine = null;

	private void Start()
	{
		if (ApplyFromAtStart)
			transform.localScale = From;
	}

	public void Play(bool reverse = false)
	{
		// Cancel previous coroutine
		if (m_CurrentRoutine != null)
			StopCoroutine(m_CurrentRoutine);

		m_CurrentRoutine = StartCoroutine(StartTween(reverse ? Target : From, reverse ? From : Target));
	}

	private IEnumerator StartTween(Vector3 from, Vector3 to)
	{
		float time = 0;
		float maxTime = SpeedCurve.keys[^1].time;
		while(time < maxTime)
		{
			transform.localScale = Vector3.Lerp(from, to, SpeedCurve.Evaluate(time));
			yield return new WaitForEndOfFrame();
			float delta = UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
			time = Mathf.Clamp(time + delta, 0, maxTime);
		}
		transform.localScale = to;

		// Clear routine as it is now finished
		m_CurrentRoutine = null;
	}
}
