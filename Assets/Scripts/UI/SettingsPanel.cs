using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
	[SerializeField] private Config m_Config;

	[Header("UI")]
	[SerializeField] private Slider m_MasterVolumeSlider;
	[SerializeField] private Slider m_MusicVolumeSlider;
	[SerializeField] private Slider m_SFXVolumeSlider;
	[SerializeField] private Toggle m_ToggleAutoStart;

	private void Start()
	{
		// Set min/max values
		m_SFXVolumeSlider.minValue = 0.0001f;
		m_SFXVolumeSlider.maxValue = 1.0f;

		m_MusicVolumeSlider.minValue = 0.0001f;
		m_MusicVolumeSlider.maxValue = 1.0f;

		m_MasterVolumeSlider.minValue = 0.0001f;
		m_MasterVolumeSlider.maxValue = 1.0f;

		// Load config & update slider user interface
		m_Config.Load();
		UpdateUI();

		// Subscribe to value change events
		m_ToggleAutoStart.onValueChanged.AddListener(OnAutoStartToggle);
		m_SFXVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
		m_MusicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
		m_MasterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
	}

	public void UpdateUI()
	{
		m_SFXVolumeSlider.value = m_Config.SFXVolume;
		m_MusicVolumeSlider.value = m_Config.MusicVolume;
		m_MasterVolumeSlider.value = m_Config.MasterVolume;

		m_ToggleAutoStart.isOn = m_Config.AutoStartRounds;
	}

	private void OnDestroy() => m_Config.Save();

	#region UI Callbacks
	private void OnSFXVolumeChanged(float value)
	{
		m_Config.SFXVolume = value;
		m_Config.UpdateAudioMixer();
	}

	private void OnMusicVolumeChanged(float value)
	{
		m_Config.MusicVolume = value;
		m_Config.UpdateAudioMixer();
	}

	private void OnMasterVolumeChanged(float value)
	{
		m_Config.MasterVolume = value;
		m_Config.UpdateAudioMixer();
	}

	private void OnAutoStartToggle(bool value) => m_Config.AutoStartRounds = value;
	#endregion
}
