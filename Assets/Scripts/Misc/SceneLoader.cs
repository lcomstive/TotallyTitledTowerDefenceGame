using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
	[Tooltip("Called when this scene has been loaded")]
	public UnityEvent SceneLoaded;

	public void LoadScene(int index) => SceneManager.LoadScene(index);

	public void Exit() => Application.Quit();

	private void Start() => SceneManager.sceneLoaded += OnSceneLoaded;

	private void OnSceneLoaded(Scene _, LoadSceneMode __) => SceneLoaded?.Invoke();
}
