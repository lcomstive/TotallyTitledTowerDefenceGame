/*
 * 
 * Based on tutorial from samyam on YouTube
 * 
 *	https://www.youtube.com/watch?v=Y3WNwl1ObC8
 * 
 */

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.LowLevel;

public class VirtualCursor : MonoBehaviour
{
	[SerializeField] private float m_CursorSpeed = 1000.0f;
	[SerializeField] private PlayerInput m_PlayerInput;
	[SerializeField] private RectTransform m_CursorTransform;
	[SerializeField] private Vector2 m_ClampPadding = new Vector2(10, 10);
	[SerializeField] private InputActionReference m_ClickInput;
	[SerializeField] private InputActionReference m_MoveCursorInput;
	[SerializeField] private List<string> m_EnabledSchemes = new List<string>() { "Gamepad" };
	[SerializeField] private bool m_OverrideDefaultMouse = true;

	private Mouse m_Mouse;
	private bool m_UseVirtual;
	private bool m_PreviousMouseState;

	// Retrieved from GetComponentInParent
	private Canvas m_CursorCanvas;
	private RectTransform m_CursorCanvasTransform;

	private void OnEnable()
	{
		// Initialize mouse
		if(m_Mouse == null)
			m_Mouse = (Mouse)InputSystem.AddDevice("VirtualMouse");
		else if(!m_Mouse.added)
			InputSystem.AddDevice(m_Mouse);

		InputUser.PerformPairingWithDevice(m_Mouse, m_PlayerInput.user);

		if(m_CursorTransform)
		{
			// Set virtual mouse position to UI cursor position
			Vector2 cursorPosition = m_CursorTransform.anchoredPosition;
			InputState.Change(m_Mouse.position, cursorPosition);

			// Get canvas from parent
			m_CursorCanvas = m_CursorTransform.GetComponentInParent<Canvas>();
			m_CursorCanvasTransform = m_CursorCanvas?.GetComponent<RectTransform>();
		}

		InputSystem.onAfterUpdate += OnAfterInputUpdate;

		m_PlayerInput.onControlsChanged += PlayerControlsChanged;

		PlayerControlsChanged(m_PlayerInput);

		if(m_OverrideDefaultMouse)
			Cursor.visible = false;
	}

	private void OnDisable()
	{
		if(m_Mouse != null && m_Mouse.added)
			InputSystem.RemoveDevice(m_Mouse);

		InputSystem.onAfterUpdate -= OnAfterInputUpdate;
		m_PlayerInput.onControlsChanged -= PlayerControlsChanged;

		if(m_OverrideDefaultMouse)
			Cursor.visible = true;
	}

	private void PlayerControlsChanged(PlayerInput obj)
	{
		if(m_Mouse == null || !m_Mouse.added)
			return;

		m_UseVirtual = m_EnabledSchemes.Contains(obj.currentControlScheme);
		m_CursorTransform.gameObject.SetActive(m_UseVirtual || m_OverrideDefaultMouse);
		Cursor.visible = !m_UseVirtual && !m_OverrideDefaultMouse;

		if(m_UseVirtual)
		{
			// Update virtual mouse to mouse position
			Vector2 mousePos = Mouse.current.position.ReadValue();
			InputState.Change(m_Mouse.position, mousePos);
			AnchorCursor(mousePos);
		}
		else // Update mouse to virtual mouse position
			Mouse.current.WarpCursorPosition(m_Mouse.position.ReadValue());
	}

	private void OnAfterInputUpdate()
	{
		if(m_OverrideDefaultMouse && !m_UseVirtual)
			AnchorCursor(Mouse.current.position.ReadValue());

		if(m_Mouse == null || !m_UseVirtual)
			return;

		// Move based on gamepad input
		Vector2 input = m_MoveCursorInput.action.ReadValue<Vector2>() * m_CursorSpeed * Time.unscaledDeltaTime;
		Vector2 currentPosition = m_Mouse.position.ReadValue();
		Vector2 desiredPosition = currentPosition + input;

		desiredPosition.x = Mathf.Clamp(desiredPosition.x, m_ClampPadding.x, Screen.width  - m_ClampPadding.x);
		desiredPosition.y = Mathf.Clamp(desiredPosition.y, m_ClampPadding.y, Screen.height - m_ClampPadding.y);

		InputState.Change(m_Mouse.position, desiredPosition);
		InputState.Change(m_Mouse.delta, input);

		// Handle click input
		bool clickPressed = m_ClickInput.action.IsPressed();
		if(m_PreviousMouseState != clickPressed)
		{
			m_PreviousMouseState = clickPressed;
			m_Mouse.CopyState(out MouseState mouseState);
			mouseState.WithButton(MouseButton.Left, clickPressed);
			InputState.Change(m_Mouse, mouseState);
		}

		AnchorCursor(desiredPosition);
	}

	private void AnchorCursor(Vector2 position)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			m_CursorCanvasTransform,
			position,
			m_CursorCanvas.worldCamera,
			out Vector2 anchorPosition
			);
		m_CursorTransform.anchoredPosition = anchorPosition;
	}
}
