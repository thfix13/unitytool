using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
//using System;
public partial class Visibility1 : MonoBehaviour 
{
	private List<NodeShadow> quickShortestPathDetected (NodeShadow headNode)
	{
		
		List<NodeShadow> shortestPathIndices = new List<NodeShadow> ();
		List<NodeShadow> stack = new List<NodeShadow> ();
		List<NodeShadow> tempStack = new List<NodeShadow> ();
		headNode.setMinDistFromHead (0.0f,null);
		stack.Add (headNode);
		while(true)
		{
			foreach(NodeShadow node in stack)
			{
				foreach(NodeShadow child in node.getChildren())
				{
					//Debug.Log(child.getPos());
					float dist = Vector3.Distance(node.getPos(),child.getPos());
					child.setMinDistFromHead (dist,node);
					if(!tempStack.Contains(child))
						tempStack.Add(child);
				}
			}
			if(tempStack.Count==0)
			{
				break;
			}
			stack.Clear();
			stack = tempStack;
			tempStack = new List<NodeShadow> ();
		}
		tempStack = stack;
		float minDist = tempStack [0].getMinDistFromHead ();
		NodeShadow endNodePoint = tempStack [0];
		for(int i=1;i<tempStack.Count;i++)
		{
			if(tempStack [i].getMinDistFromHead ()<minDist)
			{
				minDist = tempStack [i].getMinDistFromHead ();
				endNodePoint = tempStack [0];
			}
		}
		
		while(true)
		{
			shortestPathIndices.Insert (0, endNodePoint);
			endNodePoint = endNodePoint.getParentSelected();
			if(endNodePoint==null)
				break;
		}
		return shortestPathIndices;
	}
	
	
	private List<NodeShadow> findFirstEnemyPathDetected (NodeShadow headNode)
	{
		int lastIndex = pathPoints.Count - 1;
		int maxIndex = -1;
		List<NodeShadow> maxPathIndices = new List<NodeShadow> ();
		List<NodeShadow> currPathIndices = new List<NodeShadow> ();
		List<NodeShadow> stack = new List<NodeShadow> ();
		stack.Add (headNode);
		int topIndex = -1;
		while(stack.Count>0)
		{
			//pop the top
			topIndex = stack.Count-1;
			NodeShadow nodeTop = stack[topIndex];
			stack.RemoveAt(topIndex);
			
			//before adding remove all greater aand equal levels;
			int currNodeLevel = nodeTop.getSafetyLevel();
			if(currPathIndices.Count>0)
			{
				NodeShadow lastNodeInCurrIndices = currPathIndices[currPathIndices.Count-1];
				while(true)
				{
					if(nodeTop.getParent().Contains(lastNodeInCurrIndices))
					{
						break;
					}
					else
					{
						currPathIndices.RemoveAt (currPathIndices.Count-1);
						lastNodeInCurrIndices = currPathIndices[currPathIndices.Count-1];
					}
				}
			}
			currPathIndices.Add(nodeTop);
			
			if(nodeTop.getChildren().Count==0)
			{
				if(currPathIndices.Count>maxPathIndices.Count)
				{
					maxPathIndices.Clear();
					maxPathIndices.AddRange(currPathIndices);
					if(maxPathIndices[maxPathIndices.Count-1].getSafetyLevel()==lastIndex)
					{
						break;
					}
				}
				//currPathIndices.RemoveAt(currPathIndices.Count-1);
			}
			else
			{
				stack.AddRange(nodeTop.getChildren());
			}
			
		}
		return maxPathIndices;
	}
}
