using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Priority_Queue;

namespace Medial
{
	public class Graph{
		public HashSet<edgenode> [] directedEdges;
		public HashSet<edgenode> [] unDirectedEdges;
		private List <Vector3> vertices;
		private float addEdgeAngleConstraint;

		public int nvertices, ndirectededges;
		private Graph(){}
		/// <summary>
		/// Initializes a new instance of the <see cref="Medial.Graph"/> class.
		/// </summary>
		/// <param name="vertices">Vertices.</param>
		/// <param name="addEdgeAngle">Add edge angle. range: [0,90)</param>
		public Graph(List<Vector3> vertices, float addEdgeAngle){
			this.vertices=vertices;
			this.nvertices=vertices.Count;
			this.directedEdges= new HashSet<edgenode>[nvertices];
			this.unDirectedEdges= new HashSet<edgenode>[nvertices];
			this.addEdgeAngleConstraint= addEdgeAngle;
			this.ndirectededges=0;
		}
		
		/// <summary>
		/// make sure that the rule for 'from' and 'to' is properly followed
		/// </summary>
		public bool addEdge(int v1 , int v2){
			float w=Vector3.Distance(vertices[v1],vertices[v2]);
			float w_xz= Mathf.Sqrt(Mathf.Pow(vertices[v1].x-vertices[v2].x,2f)+Mathf.Pow(vertices[v1].z-vertices[v2].z,2f));
			return this.addEdge(v1,v2,w, w_xz);
		}
		public bool addEdge(int v1 , int v2, float w){
			float w_xz= Mathf.Sqrt(Mathf.Pow(vertices[v1].x-vertices[v2].x,2f)+Mathf.Pow(vertices[v1].z-vertices[v2].z,2f));
			return this.addEdge(v1,v2,w, w_xz);
		}
		public bool addEdge(int v1 , int v2, float w, float w_xz){
			float angle= Vector3.Angle(vertices[v1]-vertices[v2], Vector3.ProjectOnPlane(vertices[v1]-vertices[v2],Vector3.up));
			if(angle <=addEdgeAngleConstraint){
//				Debug.DrawLine(vertices[v1],vertices[v2], Color.red,10000);
				return false;
			}
			if(vertices[v1].y >vertices[v2].y){
				addUnDirectededge(v1,v2,w, w_xz);
				return addDirectededge(v2, v1 , w, w_xz);
			}
			else{
				if(vertices[v1].y <vertices[v2].y){
					addUnDirectededge(v1, v2 , w, w_xz);
					return addDirectededge(v1,v2,w, w_xz);
				}
			}
			return false;
		}

		public bool addDirectededge(int from, int to, float w, float w_xz){
			if(directedEdges[from]==null)
				directedEdges[from]=new HashSet<edgenode>();
			if(directedEdges[from].Contains(new edgenode(to,w,w_xz))){
				return false;
			}
			directedEdges[from].Add(new edgenode(to,w,w_xz));
			ndirectededges++;
			return true;
		}

		private void addUnDirectededge(int from, int to, float w, float w_xz){
			if(unDirectedEdges[from]==null)
				unDirectedEdges[from]=new HashSet<edgenode>();
			if(unDirectedEdges[to]==null)
				unDirectedEdges[to]=new HashSet<edgenode>();

			if(!unDirectedEdges[from].Contains(new edgenode(to,w,w_xz))){
				unDirectedEdges[from].Add(new edgenode(to,w,w_xz));
				
			}
			if(!unDirectedEdges[to].Contains(new edgenode(from,w,w_xz))){
				unDirectedEdges[to].Add(new edgenode(from,w,w_xz));
				
			}
		}
		/// <summary>
		/// Warning: Very unsafe, as you don't know other triangles connecting v1-v2 still exist or not
		/// </summary>
		/// <param name="v1">V1.</param>
		/// <param name="v2">V2.</param>
		public void removeEdge(int v1 , int v2 ){
			if(unDirectedEdges[v1]!=null && unDirectedEdges[v1].Contains(new edgenode(v2,0,0))){
				unDirectedEdges[v1].Remove (new edgenode(v2,0,0));
//				undirectedUnweightedEdges[v1].Remove(v2);
				
			}
			if(unDirectedEdges[v2]!=null && unDirectedEdges[v2].Contains(new edgenode(v1,0,0))){
				unDirectedEdges[v2].Remove (new edgenode(v1,0,0));
//				undirectedUnweightedEdges[v2].Remove(v1);
			}
			if(directedEdges[v1]!=null && directedEdges[v1].Contains(new edgenode(v2,0,0))){
				directedEdges[v1].Remove (new edgenode(v2,0,0));
				
			}
			if(directedEdges[v2]!=null && directedEdges[v2].Contains(new edgenode(v1,0,0))){
				directedEdges[v2].Remove (new edgenode(v1,0,0));
			}
			
		}
		public void removeNode(int v, ref HashSet<string> bllines,  ref HashSet<string> rlines){
			foreach (var n in unDirectedEdges[v]) 
			{
				removeEdge(v,n.nodeId);
				bllines.Remove(v+"+"+n.nodeId);
				bllines.Remove(n.nodeId+"+"+v);
				rlines.Remove(v+"+"+n.nodeId);
				rlines.Remove(n.nodeId+"+"+v);
				
			}
			unDirectedEdges[v].Clear();
		}
	}
}

