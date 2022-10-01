using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameOverScreen : MonoBehaviour
{
	public UnityEvent OnWin;
	public UnityEvent OnLose;

	public UnityEvent OnStart;

	private void Start() => OnStart?.Invoke();

	public void OnGameIsOver(bool win)
	{
		if (win)
			OnWin?.Invoke();
		else
			OnLose?.Invoke();
	}
}
