using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
	[Header("Input")]
	[SerializeField] private InputActionReference m_MoveCameraInput;

	[SerializeField, Tooltip("The speed to move the camera")]
	private float m_MoveSpeed = 10.0f;

	[SerializeField, Tooltip("The lerp speed to move the camera")]
	private float m_MoveResponsiveness = 10.0f;

	[Space()]

	[SerializeField] private InputActionReference m_ZoomCameraInput;

	[SerializeField, Tooltip("The size of the orthographic camera")]
	private Vector2 m_ZoomLimits = new Vector2(2.5f, 10.0f);

	[SerializeField, Tooltip("The speed to zoom the camera")]
	private float m_ZoomSpeed = 10.0f;

	[SerializeField, Tooltip("The lerp speed to zoom the camera")]
	private float m_ZoomReponsiveness = 10.0f;

	[Space()]

	[SerializeField] private InputActionReference m_RotateCameraInput;

	[SerializeField, Tooltip("The speed to rotate the camera")]
	private float m_RotationSpeed = 10.0f;

	[SerializeField, Tooltip("The lerp speed to rotate the camera")]
	private float m_RotationReponsiveness = 10.0f;

	[SerializeField] private bool m_CanRotate = true;

	[Header("Startup")]
	[SerializeField, Tooltip("On startup the camera lerps to this position")]
	private Transform m_GameplayInitialPosition;

	[SerializeField, Tooltip("Curve with horizontal axis as seconds, vertical as value between initial and \"initial gameplay position\"")]
	private AnimationCurve m_GameplayPositionLerp;

	[SerializeField]
	private UnityEvent m_BeginStartup;
	[SerializeField]
	private UnityEvent m_FinishedStartup;

	private Camera m_Camera;

	// Values to lerp to
	private float m_DesiredZoom = 0;
	private Vector3 m_DesiredMove = Vector3.zero;

	/// <summary>
	/// Euler angles
	/// </summary>
	private Vector3 m_DesiredRotation = Vector3.zero;

	[SerializeField]
	private bool m_EnablePlayerInput = true;

	public bool EnablePlayerInput { get => m_EnablePlayerInput; set => m_EnablePlayerInput = value; }

	private void Start()
	{
		m_Camera = GetComponent<Camera>();

		m_Camera.orthographic = true;

		m_DesiredMove = transform.position;
		m_DesiredZoom = m_Camera.orthographicSize;
		m_DesiredRotation = transform.eulerAngles;

		StartCoroutine(LerpToInitialGameplayPosition());
	}

	private IEnumerator LerpToInitialGameplayPosition()
	{
		if (!m_GameplayInitialPosition || m_GameplayPositionLerp == null)
			yield break;

		m_EnablePlayerInput = false;
		m_BeginStartup?.Invoke();

		float time = 0.0f;
		float maxTime = m_GameplayPositionLerp.keys[^1].time; // Get time at last keyframe
		Vector3 endPos = transform.position;
		transform.position = m_GameplayInitialPosition.position;

		while (time < maxTime)
		{
			yield return new WaitForEndOfFrame();
			transform.position = Vector3.Lerp(m_GameplayInitialPosition.position, endPos, m_GameplayPositionLerp.Evaluate(time / maxTime));
			time += Time.deltaTime;
		}

		transform.position = endPos;
		m_EnablePlayerInput = true;

		m_FinishedStartup?.Invoke();
	}

	private void HandleInputs()
	{
		// Move
		Vector2 moveInput = m_MoveCameraInput.action.ReadValue<Vector2>();
		m_DesiredMove += transform.TransformDirection(new Vector3(moveInput.x, moveInput.y, 0)) * m_MoveSpeed * Time.unscaledDeltaTime;

		// Zoom
		m_DesiredZoom = Mathf.Clamp(
			m_DesiredZoom + m_ZoomCameraInput.action.ReadValue<float>() * m_ZoomSpeed * Time.unscaledDeltaTime,
			m_ZoomLimits.x,
			m_ZoomLimits.y
		);

		// Rotate
		m_DesiredRotation.y += m_RotateCameraInput.action.ReadValue<float>() * m_RotationSpeed * Time.unscaledDeltaTime;

		if (m_DesiredRotation.y > 180.0f)
			m_DesiredRotation.y -= 360.0f;
		else if (m_DesiredRotation.y < -180.0f)
			m_DesiredRotation.y += 360.0f;
	}

	private void Update()
	{
		if(!m_EnablePlayerInput)
		{
			m_DesiredMove = transform.position;
			m_DesiredZoom = m_Camera.orthographicSize;
			m_DesiredRotation = transform.eulerAngles;
			return;
		}

		HandleInputs();

		// Move
		transform.position = Vector3.Lerp(transform.position, m_DesiredMove, Time.unscaledDeltaTime * m_MoveResponsiveness);

		// Zoom
		m_Camera.orthographicSize = Mathf.Lerp(m_Camera.orthographicSize, m_DesiredZoom, Time.unscaledDeltaTime * m_ZoomReponsiveness);

		// Rotate
		if(m_CanRotate)
			transform.rotation = Quaternion.Slerp(
				transform.rotation,
				Quaternion.Euler(m_DesiredRotation),
				Time.unscaledDeltaTime * m_RotationReponsiveness
			);
	}
}
