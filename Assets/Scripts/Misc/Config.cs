using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "Config", menuName = "Custom/Config")]
public class Config : ScriptableObject
{
	/// <summary>
	/// Scales volume of other all audio.
	/// Ranges from [0.0001f-1.0f].
	/// </summary>
	[Range(0.0001f, 1.0f), Tooltip("Scales volume of all audio")]
	public float MasterVolume = 1.0f;

	/// <summary>
	/// Scales volume of music audio.
	/// Ranges from [0.0001f-1.0f].
	/// </summary>
	[Range(0.0001f, 1.0f), Tooltip("Scales volume of music audio")]
	public float MusicVolume = 1.0f;
	
	/// <summary>
	/// Scales volume of game effects audio.
	/// Ranges from [0.0001f-1.0f].
	/// </summary>
	[Range(0.0001f, 1.0f), Tooltip("Scales volume of game effect audio")]
	public float SFXVolume = 1.0f;

	/// <summary>
	/// When enabled, starts the next round when all enemies of current round is finished.
	/// </summary>
	[Tooltip("When enabled, starts the next round when all enemies of current round is finished")]
	public bool AutoStartRounds = false;

	[Header("Audio Mixer")]
	[SerializeField] private AudioMixer m_AudioMixer;
	[SerializeField] private string m_MasterVolumeProperty = "MasterVol";
	[SerializeField] private string m_MusicVolumeProperty = "MusicVol";
	[SerializeField] private string m_SFXVolumeProperty = "SFXVol";

	public void Save()
	{
		PlayerPrefs.SetFloat("SFXVolume",	 SFXVolume);
		PlayerPrefs.SetFloat("MusicVolume",  MusicVolume);
		PlayerPrefs.SetFloat("MasterVolume", MasterVolume);
		PlayerPrefs.SetInt("AutoStart", AutoStartRounds ? 1 : 0);

		PlayerPrefs.Save();
	}

	public void Load()
	{
		SFXVolume		= PlayerPrefs.GetFloat("SFXVolume");
		MusicVolume		= PlayerPrefs.GetFloat("MusicVolume");
		MasterVolume	= PlayerPrefs.GetFloat("MasterVolume");
		AutoStartRounds = PlayerPrefs.GetInt("AutoStart") == 1;

		UpdateAudioMixer();

		Save();
	}

	public void UpdateAudioMixer()
	{
		m_AudioMixer.SetFloat(m_SFXVolumeProperty,	  Mathf.Log10(SFXVolume)    * 20.0f);
		m_AudioMixer.SetFloat(m_MusicVolumeProperty,  Mathf.Log10(MusicVolume)  * 20.0f);
		m_AudioMixer.SetFloat(m_MasterVolumeProperty, Mathf.Log10(MasterVolume) * 20.0f);
	}
}
