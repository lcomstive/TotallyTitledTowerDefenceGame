using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class BuildableManager : MonoBehaviour
{
	public enum BuildState { None, SelectExisting, BuildNew }

	public static BuildState State { get; private set; } = BuildState.None;
	public static BuildableData SelectedBuildable { get; private set; }

	[SerializeField] private LayerMask m_RayMask;
	[SerializeField] private PlayerData m_PlayerData;
	[SerializeField] private InputActionReference m_SelectInput;
	[SerializeField] private string m_BuildableTag = "Buildable";
	[SerializeField] private InputActionReference m_CursorPositionInput;
	[SerializeField] private bool m_PlacementSnapping = true;

	public BuildState m_State;

	// Cached variables
	private EventSystem m_EventSystem;
	private Camera m_Camera; // Uses Camera.main
	private static BuildableInfo m_SelectedBuilding = null;

	public static BuildableManager Instance { get; private set; } = null;

	public static PlayerData PlayerData => Instance?.m_PlayerData;

	private void Awake()
	{
		if (!Instance)
			Instance = this;
		else
			Destroy(gameObject);

		m_Camera = Camera.main;
		m_EventSystem = EventSystem.current;
	}

	private void OnDestroy()
	{
		if (Instance == this)
			Instance = null;
	}

	private void Update()
	{
		m_State = State;
		bool selectPressed = m_SelectInput.action.IsPressed();

		Ray ray = m_Camera.ScreenPointToRay(m_CursorPositionInput.action.ReadValue<Vector2>());

		if(m_EventSystem.IsPointerOverGameObject()) // Pointer is over UI, don't update
			return;
		if (!Physics.Raycast(ray, out RaycastHit hit, 1000.0f)) // Check if hit any world object
		{
			if(selectPressed)
				DeselectBuilding();
			return;
		}

		Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.magenta, 0.1f);

		switch(State)
		{
			case BuildState.BuildNew:
				{
					if (Vector3.Dot(hit.normal, Vector3.up) > 0.75f &&
						SelectedBuildable.ValidPlacementTags.Contains(hit.collider.tag)
						) // Update build preview. Also handles spawning buildable from preview
						PlaceBuilding(hit.point);
					break;
				}
			default:
			case BuildState.None:
				{
					if(!selectPressed)
						break; // No mouse input, don't do anything

					// Select building to view info
					bool buildableTag = hit.collider.CompareTag(m_BuildableTag);
					if(buildableTag)
					{
						BuildableInfo info = hit.collider.GetComponentInParent<BuildableInfo>();
						if(info != m_SelectedBuilding)
							SelectBuilding(info);
					}
					// Deselect object
					else
						DeselectBuilding();
					break;
				}
		}
	}

	private void SelectBuilding(BuildableInfo info)
	{
		if (!info)
			return;
		m_SelectedBuilding = info;
		SelectedBuildable = info.Data;
		State = BuildState.SelectExisting;
		SelectedBuildingChanged?.Invoke(info);

		Debug.Log($"Selected {info.gameObject.name}");
	}

	public static void StartBuilding(BuildableData data)
	{
		DeselectBuilding();

		SelectedBuildable = data;
		State = BuildState.BuildNew;
	}

	public static void DeselectBuilding()
	{
		if(m_SelectedBuilding)
			m_SelectedBuilding.ShowRadius(false);

		State = BuildState.None;
		SelectedBuildable = null;
		m_SelectedBuilding = null;

		SelectedBuildingChanged?.Invoke(null);
	}

	public static void RefundSelected()
	{
		if(!SelectedBuildable)
			return;
		Instance.m_PlayerData.Currency += SelectedBuildable.SellValue;
		Destroy(m_SelectedBuilding.gameObject);
		DeselectBuilding();

		// TODO: Play poof effect :3
	}

	private GameObject m_NewBuildingPreview = null;
	private void PlaceBuilding(Vector3 point)
	{
		// Spawn preview object if it doesn't exist
		if (!m_NewBuildingPreview)
		{
			m_NewBuildingPreview = Instantiate(
				SelectedBuildable.PreviewPrefab,
				point + SelectedBuildable.SpawnOffset,
				SelectedBuildable.PreviewPrefab.transform.rotation
			);
			BuildableInfo newInfo = m_NewBuildingPreview.GetComponent<BuildableInfo>();
			if(!newInfo)
				newInfo = m_NewBuildingPreview.AddComponent<BuildableInfo>();
			newInfo.Data = SelectedBuildable;
			newInfo.ShowRadius(true);
		}

		// Set preview position
		Vector3 desiredPosition = point;
		if(m_PlacementSnapping)
		{
			desiredPosition.x = Mathf.Round(desiredPosition.x);
			desiredPosition.z = Mathf.Round(desiredPosition.z);
		}
		desiredPosition += SelectedBuildable.SpawnOffset;
		m_NewBuildingPreview.transform.position = desiredPosition;

		if (m_SelectInput.action.IsPressed())
		{
			// User has clicked, wanting to place buildable
			// Destroy preview
			Destroy(m_NewBuildingPreview);
			m_NewBuildingPreview = null;

			// Spawn buildable
			GameObject go = Instantiate(
				SelectedBuildable.Prefab,
				desiredPosition,
				SelectedBuildable.Prefab.transform.rotation
			);
			BuildableInfo buildInfo = go.GetComponent<BuildableInfo>();
			if(!buildInfo)
				buildInfo = go.AddComponent<BuildableInfo>();
			buildInfo.Data = SelectedBuildable;

			// Subtract from player currency
			PlayerData.Currency -= SelectedBuildable.Cost;

			// Reset state
			DeselectBuilding();
		}
	}

	public delegate void OnSelectedBuildingChanged(BuildableInfo info);
	public static event OnSelectedBuildingChanged SelectedBuildingChanged;
}
