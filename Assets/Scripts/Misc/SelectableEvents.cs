using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SelectableEvents : MonoBehaviour, ISelectable
{
	[SerializeField, Tooltip("Whether to call OnSelected or OnDeselected during startup")]
	private bool m_InitialValue = false;

	[Space()]

	public UnityEvent OnSelected;
	public UnityEvent OnDeselected;

	void Start()
	{
		if (m_InitialValue)
			Select();
		else
			Deselect();
	}

	public void Select() => OnSelected?.Invoke();
	public void Deselect() => OnDeselected?.Invoke();
}
