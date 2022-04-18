using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PathNode
{
	/// <summary>
	/// Runtime-generated ID of path this node is a child of
	/// </summary>
	[HideInInspector]
	public ushort PathID;

    public Vector3 Position;

	/// <summary>
	/// True if splits into two paths
	/// </summary>
	public bool Branches => BranchingPath != null && BranchingPath.Count > 0;

	public List<PathNode> BranchingPath;

	[Tooltip("Finds the closest node in parent path and attaches to it")]
	public bool JoinToParentPath;
}

public class Path : MonoBehaviour
{
	public List<PathNode> PathNodes = new List<PathNode>();

	public static Path Instance { get; private set; }

	private Dictionary<ushort, List<PathNode>> m_PathIDs = new Dictionary<ushort, List<PathNode>>();
	private Dictionary<ushort, ushort> m_PathParentIDs = new Dictionary<ushort, ushort>(); // Maps PathID -> PathID of parent

	public List<PathNode> GetPath(ushort ID) => m_PathIDs.ContainsKey(ID) ? m_PathIDs[ID] : null;
	public List<PathNode> GetParentPath(ushort pathID) =>
		m_PathParentIDs.ContainsKey(pathID) ? GetPath(m_PathParentIDs[pathID]) : null;

	private void Awake()
	{
		if(Instance)
			Destroy(gameObject);
		else
			Instance = this;

		AssignIDs(PathNodes);
	}

	private void OnDestroy()
	{
		if(Instance == this)
			Instance = null;
		m_PathIDs.Clear();
	}

	private ushort AssignIDs(List<PathNode> nodes)
	{
		ushort ID = 0;
		while(m_PathIDs.ContainsKey(ID))
			ID++;
		m_PathIDs.Add(ID, nodes);
		foreach(PathNode node in nodes)
		{
			node.PathID = ID;

			if(node.Branches)
				m_PathParentIDs.Add(AssignIDs(node.BranchingPath), ID);
		}
		return ID;
	}
}
