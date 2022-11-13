using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TextFadeTimed : Tweener
{
	private TMP_Text m_Target;

	// Get target
	private void Awake() => m_Target = GetComponent<TMP_Text>();

	protected override void TweenFrame(float value) => m_Target.alpha = value;
}
