using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Path))]
public class PathEditor : Editor
{
	public override void OnInspectorGUI()
	{
		GUILayout.Label("Blue = Start");
		GUILayout.Label("Green = End");

		GUILayout.Space(5.0f);

		base.OnInspectorGUI();
	}

	private void OnSceneGUI()
	{
		HandlePath(((Path)target).PathNodes);
	}

	private void HandlePath(List<PathNode> path, List<PathNode> parentPath = null)
	{
		for(int i = 0; i < path.Count; i++)
		{
			path[i].Position = Handles.PositionHandle(path[i].Position, Quaternion.identity);

			Handles.color = Color.white;
			if(i > 0)
				Handles.DrawLine(path[i - 1].Position, path[i].Position, 5.0f);

			if(i == 0)
				Handles.color = Color.cyan;
			else if(i == path.Count - 1)
				Handles.color = Color.red;
			else
				Handles.color = Color.white;
			Handles.DrawWireCube(path[i].Position, new Vector3(0.3f, 0.3f, 0.3f));
			Handles.Label(path[i].Position, i.ToString());

			if(path[i].Branches && path[i].BranchingPath.Count > 0)
			{
				Handles.color = Color.cyan;
				Handles.DrawLine(path[i].Position, path[i].BranchingPath[0].Position, 5.0f);
				HandlePath(path[i].BranchingPath, path);
			}

			// Handle path joining
			if(path[i].JoinToParentPath)
			{
				Handles.color = Color.red;

				if(parentPath != null && parentPath.Count > 0)
				{
					PathNode closestNode = GetClosestNode(path[i], parentPath);
					Handles.DrawLine(
						path[i].Position,
						closestNode.Position,
						5.0f
					);
				}
				else
					path[i].JoinToParentPath = false;
			}
		}
	}

	private PathNode GetClosestNode(PathNode currentNode, List<PathNode> path)
	{
		PathNode closest = null;
		float closestDistance = float.MaxValue;
		foreach(PathNode node in path)
		{
			float distance = Vector3.Distance(currentNode.Position, node.Position);
			if(distance >= closestDistance)
				continue;

			closestDistance = distance;
			closest = node;
		}

		return closest;
	}
}