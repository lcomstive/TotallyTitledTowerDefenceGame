using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MousePicker : MonoBehaviour
{
	[SerializeField] private LayerMask m_RayMask;
	[SerializeField] private InputActionReference m_SelectInput;
	[SerializeField] private InputActionReference m_CursorPositionInput;

	/// <summary>
	/// Currently selected <see cref="ISelectable"/>
	/// </summary>
	public ISelectable Selected { get; private set; } = null;

	/// <summary>
	/// Raycast result, originating from main camera
	/// </summary>
	public RaycastHit RayHit => m_RayHit;

	/// <summary>
	/// State of <see cref="Selected"/> validity
	/// </summary>
	public bool IsObjectSelected => Selected != null;

	public bool CanSelect { get; set; } = true;

	private RaycastHit m_RayHit;
	private Camera m_Camera; // From Camera.main
	private EventSystem m_EventSystem; // From EventSystem.current

	private const float MaxRayDistance = 1000.0f;

	private void Awake()
	{
		m_Camera = Camera.main;
		m_EventSystem = EventSystem.current;
	}

	private void Update()
	{
		bool selectPressed = m_SelectInput.action.IsPressed();
		Ray ray = m_Camera.ScreenPointToRay(m_CursorPositionInput.action.ReadValue<Vector2>());

		if (!Physics.Raycast(ray, out m_RayHit, MaxRayDistance, m_RayMask, QueryTriggerInteraction.Ignore))
		{
			if (selectPressed && !m_EventSystem.IsPointerOverGameObject())
				Deselect(); // No objects hit, deselect anything selected
			return;
		}

		if (!CanSelect)
			return; // State set to not select object, exit

		ISelectable selectable = RayHit.collider.GetComponentInParent<ISelectable>();
		if (selectable != null && selectPressed)
			Select(selectable);
		else if (selectPressed && !m_EventSystem.IsPointerOverGameObject())
			Deselect();
	}

	public void Select(ISelectable selectable)
	{
		if (Selected == selectable || selectable == null)
			return; // Already set or invalid, so ignore

		Deselect();

		Selected = selectable;
		Selected.Select();
	}

	public void Deselect()
	{
		if (Selected == null)
			return; // Nothing set, ignore

		Selected.Deselect();
		Selected = null;
	}
}

