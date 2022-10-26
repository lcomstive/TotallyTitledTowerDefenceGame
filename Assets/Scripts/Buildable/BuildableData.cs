using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Buildable", menuName = "Custom/Buildable/Generic")]
public class BuildableData : ScriptableObject
{
	public string DisplayName = "Buildable";
	public GameObject Prefab;
	public GameObject PreviewPrefab;
	public Currency Cost = new Currency();
	public Currency SellValue = new Currency();
	public string Description = "This is a buildable item. Try placing me!";

	[Tooltip("Audio to play when tower is placed down")]
	public AudioClip PlacedDownAudio;

	/// <summary>
	/// Additional information appended to text of <see cref="Description"/>.
	/// Useful for showing additional stats, such as damage
	/// </summary>
	public virtual string DescriptionAdditional => string.Empty;

	[SerializeField]
	private float VisionRadius = 1.0f;

	[Tooltip("Valid tags of objects in the level that this object can be placed on")]
	public List<string> ValidPlacementTags = new List<string>() { "Terrain" };

	public Vector3 SpawnOffset = Vector3.zero;

	public BuildableData() => Cost.Validate();

	/// <summary>
	/// Gets the total vision radius this buildable has
	/// </summary>
	/// <param name="transformY">Y value of the buildable transform</param>
	public float GetVisionRadius(float transformY) => VisionRadius * Mathf.Max(1.0f, 0.5f + transformY / 2.0f);

	/// <summary>
	/// Called at the start of each level.
	/// Used to reset anything that's specific to this playthrough (e.g. unit kill count)
	/// </summary>
	public virtual void ResetData() { }
}
