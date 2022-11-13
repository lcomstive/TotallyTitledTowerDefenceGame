using System.Collections;
using UnityEngine;

public class ScaleTween : Tweener
{
	public Vector3 From = Vector3.zero;
	public Vector3 Target = Vector3.one;

	[SerializeField, Tooltip("When enabled, sets this object's scale to From in Start()")]
	private bool ApplyFromAtStart = false;

	private bool m_Reversing = false;

	private void Start()
	{
		if (ApplyFromAtStart)
			transform.localScale = From;
	}

	public void Play(bool reverse = false)
	{
		m_Reversing = reverse;
		StartTween();
	}

	protected override void TweenFrame(float value) => transform.localScale = Vector3.Lerp(m_Reversing ? Target : From, m_Reversing ? From : Target, value);
}
