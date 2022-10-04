using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Buildable", menuName = "Custom/Buildable/Generic")]
public class BuildableData : ScriptableObject
{
	public string DisplayName = "Buildable";
	public GameObject Prefab;
	public GameObject PreviewPrefab;
	public Currency Cost = new Currency();
	public Currency SellValue = new Currency();
	public string Description = "This is a buildable item. Try placing me!";

	[SerializeField]
	private float VisionRadius = 1.0f;

	[Tooltip("Valid tags of objects in the level that this object can be placed on")]
	public List<string> ValidPlacementTags = new List<string>() { "Terrain" };

	public Vector3 SpawnOffset = Vector3.zero;

	public BuildableData()
	{
		Cost.Validate();
	}

	/// <summary>
	/// Gets the total vision radius this buildable has
	/// </summary>
	/// <param name="transformY">Y value of the buildable transform</param>
	public float GetVisionRadius(float transformY) => VisionRadius * (0.5f + transformY / 2.0f);
}

public abstract class DamageableBuildableData : BuildableData
{
	public float Damage = 1.0f;
}
