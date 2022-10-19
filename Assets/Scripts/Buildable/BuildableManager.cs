using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GameAnalyticsSDK;

[RequireComponent(typeof(MousePicker))]
public class BuildableManager : MonoBehaviour
{
	[SerializeField] private PlayerData m_PlayerData;
	[SerializeField] private InputActionReference m_SelectInput;
	[SerializeField] private bool m_PlacementSnapping = true;

	public bool IsBuilding { get; private set; }
	
	/// <summary>
	/// Time, in seconds, to ignore select/click input after placing a building.
	/// This is to prevent immediately showing building info,
	///		unless the player holds down the input
	/// </summary>
	[Tooltip("Time, in milliseconds, to ignore select/click input after placing a building")]
	[SerializeField] private int m_IgnoreSelectTimeAfterPlaced = 150;

	/// <summary>
	/// Instantiated preview of selected building
	/// </summary>
	private GameObject m_NewBuildingPreview = null;

	// Cached variables
	public MousePicker Picker { get; private set; } = null;
	private BuildableData m_Selected = null;

	private void Start() => Picker = GetComponent<MousePicker>();

	private void LateUpdate()
	{
		bool selectPressed = m_SelectInput.action.IsPressed();

		if (!IsBuilding)
			return;

		if (Vector3.Dot(Picker.RayHit.normal, Vector3.up) > 0.75f &&
			m_Selected.ValidPlacementTags.Contains(Picker.RayHit.collider.tag)
			) // Update build preview. Also handles spawning buildable from preview
			PlaceBuilding(Picker.RayHit.point);

		if (Picker.RayHit.collider == null && selectPressed)
			Deselect();
	}

	public void StartBuilding(BuildableData data)
	{
		Picker.Deselect();
		Picker.CanSelect = false;

		m_Selected = data;
		IsBuilding = true;
	}

	public void Deselect()
	{
		if (!IsBuilding || !m_Selected)
			return; // Invalid state

		IsBuilding = false;

		if(m_NewBuildingPreview)
		{
			Destroy(m_NewBuildingPreview);
			m_NewBuildingPreview = null;
		}
	}

	public void RefundBuilding(BuildableData data)
	{
		m_PlayerData.Currency += data.SellValue;

		// Update analytics
		GameAnalytics.NewDesignEvent($"buildable:sell:{m_Selected.name}");
		GameAnalytics.NewResourceEvent(GAResourceFlowType.Source, "Currency", (int)data.SellValue, "buildable", data.name);
	}

	private async void PlaceBuilding(Vector3 point)
	{
		// Spawn preview object if it doesn't exist
		if (!m_NewBuildingPreview)
		{
			m_NewBuildingPreview = Instantiate(
				m_Selected.PreviewPrefab,
				point + m_Selected.SpawnOffset,
				m_Selected.PreviewPrefab.transform.rotation
			);
			BuildableInfo newInfo = m_NewBuildingPreview.GetComponent<BuildableInfo>();
			if(!newInfo)
				newInfo = m_NewBuildingPreview.AddComponent<BuildableInfo>();
			newInfo.Data = m_Selected;
			newInfo.ShowRadius(true);
		}

		// Set preview position
		Vector3 desiredPosition = point;
		if(m_PlacementSnapping)
		{
			desiredPosition.x = Mathf.Round(desiredPosition.x);
			desiredPosition.z = Mathf.Round(desiredPosition.z);
		}
		desiredPosition += m_Selected.SpawnOffset;
		m_NewBuildingPreview.transform.position = desiredPosition;

		if (m_SelectInput.action.IsPressed())
		{
			// User has clicked, wanting to place buildable
			// Destroy preview
			Destroy(m_NewBuildingPreview);
			m_NewBuildingPreview = null;

			// Spawn buildable
			GameObject go = Instantiate(
				m_Selected.Prefab,
				desiredPosition,
				m_Selected.Prefab.transform.rotation
			);
			BuildableInfo buildInfo = go.GetComponent<BuildableInfo>();
			if(!buildInfo)
				buildInfo = go.AddComponent<BuildableInfo>();
			buildInfo.Data = m_Selected;

			// Subtract from player currency
			m_PlayerData.Currency -= m_Selected.Cost;

			// Reset state
			Deselect();

			// Update analytics
			GameAnalytics.NewDesignEvent($"buildable:buy:{m_Selected.name}");
			GameAnalytics.NewResourceEvent(GAResourceFlowType.Sink, "Currency", (int)m_Selected.Cost, "buildable", m_Selected.name);
		}

		await Task.Delay(m_IgnoreSelectTimeAfterPlaced);
		Picker.CanSelect = true;
	}
}
