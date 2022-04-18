using UnityEngine;

[CreateAssetMenu(fileName = "Player Data", menuName = "Custom/Player Data")]
public class PlayerData : ScriptableObject
{
	public int Lives;
	public Currency Currency;
}
