using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Priority_Queue;

namespace Medial
{
	class Graph{
		public HashSet<edgenode> [] directedEdges;
		public HashSet<edgenode> [] unDirectedEdges;
//		public HashSet<int>[]undirectedUnweightedEdges;
		
		//		int [] degree;
		public int nvertices;
		
		public Graph(int vertices){
			nvertices=vertices;
			//TODO:: need to change it to array of hashmaps, so that .contains works better
			directedEdges= new HashSet<edgenode>[vertices];
			unDirectedEdges= new HashSet<edgenode>[vertices];
//			undirectedUnweightedEdges= new HashSet<int>[vertices];
			
			//			UnityEngine.Debug.Log(nvertices);
			//			degree= new int[nvertices];
			//			for(int i=0;i<nvertices;i++)
			//			{
			//				degree[i]=0;
			//			}
		}
		
		/// <summary>
		/// make sure that the rule for 'from' and 'to' is properly followed
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		/// <param name="w">w.</param>
		public void addEdge(int from , int to, float w){
			addDirectededge(from, to , w);
			addUnDirectededge(from,to,w);
			
		}
		
		public void addDirectededge(int from, int to, float w){
			if(directedEdges[from]==null)
				directedEdges[from]=new HashSet<edgenode>();
			if(directedEdges[from].Contains(new edgenode(to,w))){
				return;
			}
			directedEdges[from].Add(new edgenode(to,w));
		}

		private void addUnDirectededge(int from, int to, float w){
			if(unDirectedEdges[from]==null)
				unDirectedEdges[from]=new HashSet<edgenode>();
			if(unDirectedEdges[to]==null)
				unDirectedEdges[to]=new HashSet<edgenode>();

			if(!unDirectedEdges[from].Contains(new edgenode(to,w))){
				unDirectedEdges[from].Add(new edgenode(to,w));
				
			}
			if(!unDirectedEdges[to].Contains(new edgenode(from,w))){
				unDirectedEdges[to].Add(new edgenode(from,w));
				
			}
		}
		/// <summary>
		/// Warning: Very unsafe, as you don't know other triangles connecting v1-v2 still exist or not
		/// </summary>
		/// <param name="v1">V1.</param>
		/// <param name="v2">V2.</param>
		public void removeEdge(int v1 , int v2 ){
			if(unDirectedEdges[v1]!=null && unDirectedEdges[v1].Contains(new edgenode(v2,0))){
				unDirectedEdges[v1].Remove (new edgenode(v2,0));
//				undirectedUnweightedEdges[v1].Remove(v2);
				
			}
			if(unDirectedEdges[v2]!=null && unDirectedEdges[v2].Contains(new edgenode(v1,0))){
				unDirectedEdges[v2].Remove (new edgenode(v1,0));
//				undirectedUnweightedEdges[v2].Remove(v1);
			}
			if(directedEdges[v1]!=null && directedEdges[v1].Contains(new edgenode(v2,0))){
				directedEdges[v1].Remove (new edgenode(v2,0));
				
			}
			if(directedEdges[v2]!=null && directedEdges[v2].Contains(new edgenode(v1,0))){
				directedEdges[v2].Remove (new edgenode(v1,0));
			}
			
		}
		public void removeNode(int v, ref HashSet<string> bllines,  ref HashSet<string> rlines){
			foreach (var n in unDirectedEdges[v]) 
			{
				removeEdge(v,n.y);
				bllines.Remove(v+"+"+n.y);
				bllines.Remove(n.y+"+"+v);
				rlines.Remove(v+"+"+n.y);
				rlines.Remove(n.y+"+"+v);
				
			}
			unDirectedEdges[v].Clear();
		}
	}
}

