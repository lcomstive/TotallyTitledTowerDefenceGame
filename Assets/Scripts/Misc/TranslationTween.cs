using UnityEngine;

public class TranslationTween : Tweener
{
	[Tooltip("Movement, in global space, from current position")]
	public Vector3 Movement = new Vector3(0, 1, 0);

	private Vector3 m_Origin;

	protected override void OnTweenBegin() => m_Origin = transform.position;

	protected override void TweenFrame(float value) => transform.position = Vector3.LerpUnclamped(m_Origin, m_Origin + Movement, value);
}
