using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PlayButton : MonoBehaviour
{
	[SerializeField] private PlayerData m_PlayerData;
	[SerializeField] private InputActionReference m_PlayInput;

	[Header("Visuals")]
	[SerializeField] private Image m_PlayImage;
	[SerializeField] private Sprite m_PlaySprite;
	[SerializeField] private Sprite m_Play2xSprite;
	[Space()]
	[SerializeField, Range(0, 1)] private float m_BackgroundOpacityBuilding = 0.7f;
	[SerializeField, Range(0, 1)] private float m_BackgroundOpacityPlaying = 1.0f;
	
	// Button this component is attached to
	private Button m_Button;

	/// <summary>
	/// Set to true when 2x speed is selected.
	/// Used to set back to 2x when playing after a round has finished
	/// and set the state back to Building
	/// </summary>
	private bool m_Playing2x = false;

	private void Start()
	{
		if(!TryGetComponent(out m_Button))
		{
			Debug.LogWarning("Script PlayButton should have a Button component also attached to same game object");
			return;
		}
		if(!m_PlayerData)
		{
			Debug.LogWarning($"PlayButton.PlayerData not set on '{name}'");
			return;
		}

		// Listen to events
		m_Button.onClick.AddListener(OnButtonClick);
		m_PlayerData.OnStateChanged += OnGameStateChanged;

		// Set initial button state
		OnGameStateChanged(m_PlayerData.GameState, m_PlayerData.GameState);

		// Listen to key to toggle button
		if(m_PlayInput)
			m_PlayInput.action.started += OnPlayInput;
	}

	private void OnDestroy()
	{
		m_PlayerData.OnStateChanged -= OnGameStateChanged;

		if (m_PlayInput)
			m_PlayInput.action.started -= OnPlayInput;
	}

	private void OnPlayInput(InputAction.CallbackContext obj) => m_Button.onClick.Invoke();

	private void OnGameStateChanged(PlayState oldValue, PlayState value)
	{
		Color buttonColor = m_Button.image.color;
		buttonColor.a = value == PlayState.Building ? m_BackgroundOpacityBuilding : m_BackgroundOpacityPlaying;
		m_Button.image.color = buttonColor;

		m_PlayImage.sprite = value == PlayState.Play2x ? m_Play2xSprite : m_PlaySprite;
	}

	private void OnButtonClick()
	{
		switch(m_PlayerData.GameState)
		{
			case PlayState.Building:
				// Resume playing in speed previously selected
				m_PlayerData.GameState = m_Playing2x ? PlayState.Play2x : PlayState.Play;
				break;
			case PlayState.Play:
				m_PlayerData.GameState = PlayState.Play2x;
				break;
			case PlayState.Play2x:
				m_PlayerData.GameState = PlayState.Play;
				break;
		}

		m_Playing2x = m_PlayerData.GameState == PlayState.Play2x;
	}
}
