using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics;
using Priority_Queue;
namespace Medial{
	public class PathFinding {
		List <Vector3> vertices;
		Graph graphObj;
		List<List<int>> paths_xyz= null, paths_xz=null;
		int startNodeId; 
		List<int> endNearestNodesIndices;

		IntervalKDTree<int> tree_forMA;

		float arenaMinY2, arenaMaxY2;
		MedialMetrics metrics1;
		
		public PathFinding(MedialMesh m){//List <Vector3> v, List <int> triangles,IntervalKDTree<int> t, MedialMetrics m1,ArenasGenerator arena, Graph g){
			this.vertices=m.vertices;
			this.tree_forMA=m.tree;
			this.metrics1=m.metrics1;
			this.arenaMinY2=m.arena.getMinY2();
			this.arenaMaxY2=m.arena.getMaxY2();
			this.graphObj=m.graphObj;


		}
		/// Constructor for RRT
		public PathFinding(List <Vector3> vertices,
		                   Graph graphObj,float arenaMinY2, float arenaMaxY2,
		                   MedialMetrics metrics1, int endNodesCount)
		{
			this.vertices= vertices;
			this.graphObj=graphObj;
			this.metrics1=metrics1;
			this.startNodeId= 0;
			this.endNearestNodesIndices= Enumerable.Range(1,endNodesCount).ToList();

		}

		#region Path Finding

		public void findPathsMA(Vector3 start,Vector3 end){
			findNearestsMA(start,end);

			Stopwatch watch= null;
			if(metrics1!=null)
				watch.Start();
			findShortestPath_xyz();

			if(metrics1!=null){
				watch.Stop();
				metrics1.time_for_shortest_path_MA_xyz= watch.ElapsedMilliseconds;
				metrics1.length_of_shortest_path_xyz_MA= pathLengthxz( paths_xyz[0]);

			}

			if(metrics1!=null)
				watch.Start();
			findShortestPath_xz();
			if(metrics1!=null)
			{	
				watch.Start();
				metrics1.time_for_shortest_path_MA_xz= watch.ElapsedMilliseconds;
				metrics1.length_of_shortest_path_xz_MA= pathLengthxz(paths_xz[0]);
			}

		}

		public void findPathsRRT(){

			
			Stopwatch watch= null;
			if(metrics1!=null)
				watch.Start();
			findShortestPath_xyz();
			
			if(metrics1!=null){
				watch.Stop();
				metrics1.time_for_shortest_path_RRT_xyz= watch.ElapsedMilliseconds;
				metrics1.length_of_shortest_path_xyz_RRT= pathLengthxz( paths_xyz[0]);
				
			}
			
			if(metrics1!=null)
				watch.Start();
			findShortestPath_xz();
			if(metrics1!=null)
			{	
				watch.Start();
				metrics1.time_for_shortest_path_RRT_xz= watch.ElapsedMilliseconds;
				metrics1.length_of_shortest_path_xz_RRT= pathLengthxz(paths_xz[0]);
			}
			
		}

		private void findNearestsMA(Vector3 start,Vector3 end){

			float min=-1,diff;
			int minvertex=-1;
			
			HashSet<int> foundNodes= new HashSet<int>();
			var v= start;
			float r;
			for(r=1f;foundNodes.Count==0;){
				foundNodes= tree_forMA.GetValues(v.x-r,v.y,v.z-r,v.x+r,v.y+r,v.z+r,new HashSet<int>());
				r++;
			}
			
			foreach(var i in foundNodes){
				diff=Vector3.Distance(vertices[i],start);
				if(min==-1 || diff<min){
					min=diff;
					minvertex=i;
				}
			}
			
			startNodeId=minvertex;
			v=end;
			endNearestNodesIndices= new List<int>();
			for(float y=arenaMinY2; y< arenaMaxY2-1; y++){
				foundNodes= new HashSet<int>();
				for(r=1f ;foundNodes.Count==0;){
					foundNodes= tree_forMA.GetValues(v.x-r, y, v.z-r,
					                           v.x+r, y+1 ,v.z+r,new HashSet<int>());
					r++;
				}
				endNearestNodesIndices.AddRange(foundNodes);
			}
			var go2= new GameObject();
			go2.name=  "endnearest";
			foreach(var i in endNearestNodesIndices){
				var go= GameObject.CreatePrimitive(PrimitiveType.Sphere);
				go.transform.localScale= new Vector3(0.3f,0.3f,0.3f);
				go.transform.position=vertices[i];
				go.name= "end";
				go.transform.parent=go2.transform;
			}
		}

		#region Finding all paths
		List<Int64> numberofPaths;
		HashSet<int> NodesinPath;
		int loop=0;
		public void findTotalPaths(){
			numberofPaths = Enumerable.Repeat((Int64)(-1),vertices.Count).ToList();
			NodesinPath= new HashSet<int>();
			foreach(var end in endNearestNodesIndices)
				numberofPaths[end]=1;
			NodesinPath.Add(startNodeId);
			DFS (startNodeId);
			udl (numberofPaths[startNodeId]);
		}

		private void DFS( int node){
			if(loop++>600000)
				return;
			//processed node
			if(numberofPaths[node]!=-1)
				return;
			numberofPaths[node]=0;
			var adjs= graphObj.directedEdges[node];
			if(adjs==null)
				return;
			foreach(var adj in adjs){
				if(NodesinPath.Contains(adj.nodeId))
					continue;
				NodesinPath.Add(adj.nodeId);
				DFS (adj.nodeId);
				NodesinPath.Remove(adj.nodeId);
				if(numberofPaths[node]+numberofPaths[adj.nodeId]<0)
					udl ("umm");
				numberofPaths[node]+=numberofPaths[adj.nodeId];
			}

		}

		int[][]adjacency;
		void Nlenghtwalks(){
//			adjacency= new int[vertices.Count][]();
		}

		#endregion

		private void findShortestPath_xyz(){
			
			//holds the visited node
			HashSet<int> visitedNodes= new HashSet<int>();
			Hashtable pqSet = new Hashtable();
			HeapPriorityQueue<element> pq= new HeapPriorityQueue<element>(graphObj.nvertices);
			Hashtable backtrack = new Hashtable();
			
			for(int i=0;i< graphObj.nvertices;i++)
				backtrack.Add(i,-1);
			element currentv, tempElement;
			tempElement= new element(startNodeId);
			pq.Enqueue(tempElement,0);
			pqSet.Add(startNodeId, new hashnode(0,tempElement));
			
			double currentdist, old_dist, new_dist;
			hashnode temp;
			
			while(pq.Count>0){
				
				///remove the topmost nodes from the priority queue
				currentv= pq.Dequeue();
				
				try{
					currentdist=((hashnode)pqSet[currentv.nodeId]).priority;
				}
				catch{
					currentdist=0;
				}
				pqSet.Remove(currentv.nodeId);
				visitedNodes.Add(currentv.nodeId);
				
				if(graphObj.directedEdges[currentv.nodeId]==null)
					continue;
				
				///access all adjacent nodes
				foreach(var adjv in graphObj.directedEdges[currentv.nodeId]){
					
					if(visitedNodes.Contains (adjv.nodeId))
						continue;
					new_dist= currentdist + (double)adjv.weight;
					
					//put the adjacent nodes in pq or update their priority
					if(pqSet.Contains(adjv.nodeId))
					{
						temp=(hashnode)pqSet[adjv.nodeId];
						old_dist=temp.priority;
						if(old_dist> new_dist){
							pq.UpdatePriority(temp.e,new_dist);
							pqSet[adjv.nodeId]=new hashnode(new_dist,temp.e);
							backtrack[adjv.nodeId]=currentv.nodeId;
						}
					}
					else{
						tempElement= new element(adjv.nodeId);
						pq.Enqueue(tempElement,new_dist);
						pqSet.Add(adjv.nodeId,new hashnode(new_dist,tempElement));
						backtrack[adjv.nodeId]=currentv.nodeId;
					}
				}
			}
			
			paths_xyz= new List<List<int>>(endNearestNodesIndices.Count);
			List<int> path;
			
			///do this for every endNearestNode
			foreach(var endNearestNode in endNearestNodesIndices){
				
				//create reverse path
				path = new List<int>();
				path.Insert(0,endNearestNode);
				try{
					for(int i=endNearestNode; i!=startNodeId;i=(int)backtrack[i]){
						path.Insert(0,(int)backtrack[i]);
					}
				}
				catch(NullReferenceException){
//					udl ("Path not found for this endNode: "+vertices[endNearestNode]);
					continue;
				}
				paths_xyz.Add(path);
			}
//			udl ("#of endnodes"+endNearestNodesIndices.Count);
//			udl ("#of paths"+paths.Count);
		}

		private void findShortestPath_xz(){
			
			//holds the visited node
			HashSet<int> visitedNodes= new HashSet<int>();
			Hashtable pqSet = new Hashtable();
			HeapPriorityQueue<element> pq= new HeapPriorityQueue<element>(graphObj.nvertices);
			Hashtable backtrack = new Hashtable();
			
			for(int i=0;i< graphObj.nvertices;i++)
				backtrack.Add(i,-1);
			element currentv, tempElement;
			tempElement= new element(startNodeId);
			pq.Enqueue(tempElement,0);
			pqSet.Add(startNodeId, new hashnode(0,tempElement));
			
			double currentdist, old_dist, new_dist;
			hashnode temp;
			
			while(pq.Count>0){
				
				///remove the topmost nodes from the priority queue
				currentv= pq.Dequeue();
				
				try{
					currentdist=((hashnode)pqSet[currentv.nodeId]).priority;
				}
				catch{
					currentdist=0;
				}
				pqSet.Remove(currentv.nodeId);
				visitedNodes.Add(currentv.nodeId);
				
				if(graphObj.directedEdges[currentv.nodeId]==null)
					continue;
				
				///access all adjacent nodes
				foreach(var adjv in graphObj.directedEdges[currentv.nodeId]){
					
					if(visitedNodes.Contains (adjv.nodeId))
						continue;
					new_dist= currentdist + (double)adjv.weight_xz;
					
					//put the adjacent nodes in pq or update their priority
					if(pqSet.Contains(adjv.nodeId))
					{
						temp=(hashnode)pqSet[adjv.nodeId];
						old_dist=temp.priority;
						if(old_dist> new_dist){
							pq.UpdatePriority(temp.e,new_dist);
							pqSet[adjv.nodeId]=new hashnode(new_dist,temp.e);
							backtrack[adjv.nodeId]=currentv.nodeId;
						}
					}
					else{
						tempElement= new element(adjv.nodeId);
						pq.Enqueue(tempElement,new_dist);
						pqSet.Add(adjv.nodeId,new hashnode(new_dist,tempElement));
						backtrack[adjv.nodeId]=currentv.nodeId;
					}
				}
			}
			
			paths_xz= new List<List<int>>(endNearestNodesIndices.Count);
			List<int> path;
			
			///do this for every endNearestNode
			foreach(var endNearestNode in endNearestNodesIndices){
				
				//create reverse path
				path = new List<int>();
				path.Insert(0,endNearestNode);
				try{
					for(int i=endNearestNode; i!=startNodeId;i=(int)backtrack[i]){
						path.Insert(0,(int)backtrack[i]);
					}
				}
				catch(NullReferenceException){
//					udl ("Path not found for this endNode: "+vertices[endNearestNode]);
					continue;
				}
				paths_xz.Add(path);
			}
//			udl ("#of endnodes"+endNearestNodesIndices.Count);
//			udl ("#of paths"+paths_xz.Count);
		}

		private float pathTime(List<int> path){
			return vertices[path.Count-1].y-vertices[0].y;
		}
		private float pathLengthxz(List<int>path){
			float sum=0;
			for(int i=0;i<path.Count-1; i++)
				sum+=Vector3.Distance(vertices[path[i]],vertices[path[i+1]]);
			return sum;
		}
			
		public void showPath(){
			
			if(paths_xz==null){
				udl ("no paths");
				return;
			}
			foreach(var path in paths_xz){
				Color triColor= Color.red;
				for(int i=0;i<path.Count-1; i++)
					UnityEngine.Debug.DrawLine(vertices[path[i]],vertices[path[i+1]],triColor,2000,false);
				//				udl ("nodes in path= "+path.Count);
			}
		}
		
		
		//to be invoked from update
		private int currentpath_i=0;
		
		public Vector3 movePlayer(float t){
			if(t<vertices[paths_xyz[0][0]].y)
				return Vector3.zero;
			
			while(currentpath_i<paths_xyz[0].Count
			      && t>vertices[paths_xyz[0][currentpath_i]].y)
				currentpath_i++;
			
			if(currentpath_i>=paths_xyz[0].Count)
				return -Vector3.one;
			float frac1=t- vertices[paths_xyz[0][currentpath_i-1]].y, frac2= vertices[paths_xyz[0][currentpath_i]].y -t;
			var playerpos= (vertices[paths_xyz[0][currentpath_i-1]]*frac2 + vertices[paths_xyz[0][currentpath_i]] *frac1)/(frac1+frac2);
			return playerpos;
		}
		
		private static void udl(object s){
			UnityEngine.Debug.Log(s);
		}
		
		#endregion
		
	}
	

}

