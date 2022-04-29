using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TraversePath : MonoBehaviour
{
	[SerializeField] private float m_Speed = 10.0f;
	[SerializeField] private float m_RotateSpeed = 10.0f;

	public float Speed
	{
		get => m_Speed;
		set => m_Speed = value;
	}

	public float DistanceFromEndOfPath { get; private set; }
	public float DistanceFromStartOfPath { get; private set; }

    private PathNode m_TargetNode;

	// Moving between nodes
	private Vector3 m_StartPos;

	private void Start()
	{
		if(Path.Instance && Path.Instance.PathNodes.Count > 0)
		{
			m_StartPos = transform.position;
			m_TargetNode = Path.Instance.PathNodes[0];
		}
	}

	private void Update()
	{
		if(m_TargetNode == null)
			return;

		// Move towards target
		float distanceTravelled = Time.deltaTime * m_Speed;

		Vector3 direction = (m_TargetNode.Position - m_StartPos).normalized;
		transform.position += direction * distanceTravelled;
		
		DistanceFromStartOfPath += distanceTravelled;
		DistanceFromEndOfPath -= distanceTravelled;

		// Rotate towards target
		Quaternion lookAtRot =
			Quaternion.LookRotation(m_TargetNode.Position - transform.position, Vector3.up);

		transform.rotation =
			Quaternion.Slerp(transform.rotation, lookAtRot, Time.deltaTime * m_RotateSpeed);

		// Check if overshot
		if((Vector3.Distance(m_StartPos, transform.position) >=
			Vector3.Distance(m_StartPos, m_TargetNode.Position)) ||
			// Check if at target
			Vector3.Distance(transform.position, m_TargetNode.Position) < 0.1f
			)
			CalculateNextNode();
	}

	private void CalculateNextNode()
	{
		List<PathNode> path = Path.Instance.GetPath(m_TargetNode.PathID);
		if(path == null || path.Count == 0)
			return; // Something went wrong

		if(m_TargetNode == Path.Instance.PathNodes[0])
			DistanceFromStartOfPath = 0; // Got to start of path

		List<PathNode> potentialNodes = new List<PathNode>();
		for(int i = 0; i < path.Count; i++)
		{
			if(path[i] != m_TargetNode)
				continue;
			if(i < path.Count - 1)
				potentialNodes.Add(path[i + 1]);
			break;
		}

		if(m_TargetNode.Branches)
			potentialNodes.Add(m_TargetNode.BranchingPath[0]);

		if(m_TargetNode.JoinToParentPath)
			potentialNodes.Add(GetClosestParentNode());

		// Choose randomly from potential paths
		if(potentialNodes.Count > 0)
		{
			m_TargetNode = potentialNodes[Random.Range(0, potentialNodes.Count)];

			/*
			// Traverse path to get distance from goal
			PathNode currentNode = m_TargetNode;
			DistanceFromEndOfPath = 0;
			while(currentNode != null)
			{
				// DistanceFromEndOfPath += currentNode.
			}
			*/
		}
		else
		{
			m_TargetNode = null;
			FinishedPath?.Invoke();
		}

		m_StartPos = transform.position;
	}

	private PathNode GetClosestParentNode()
	{
		List<PathNode> parentPath = Path.Instance.GetParentPath(m_TargetNode.PathID);
		if(parentPath == null)
			return null;

		PathNode closest = null;
		float closestDistance = float.MaxValue;
		foreach(PathNode node in parentPath)
		{
			float distance = Vector3.Distance(m_TargetNode.Position, node.Position);
			if(distance >= closestDistance)
				continue;

			closest = node;
			closestDistance = distance;
		}

		return closest;
	}

	public delegate void OnFinishedPath();
	public event OnFinishedPath FinishedPath;
}
