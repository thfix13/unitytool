using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Priority_Queue;
using KDTreeDLL;
using System.Diagnostics;
namespace Medial{

	public class RRT {
	
		
		List <Vector3> vertices;

		int vindex =0;
		ArenasGenerator arena;
		int startNearestNode; 
		List<int> endNodesList;
		Graph graphObj;
		MedialMetrics metrics1;
		
		int N;

		public RRT(ArenasGenerator arena, int N, Vector3 start, Vector3 end, float angleConstraint, MedialMetrics m1){

			this.vertices = new List<Vector3>(N);
			this.arena=arena;
			this.N= N;
			this.metrics1= m1;

			this.endNodesList=new List<int>();
//			RRT1(start,end,angleConstraint);
//			saveResult(@"/Users/dhsingh/Documents/Thesis/testdata/RRT_"+N+".txt");
			loadResult(@"/Users/dhsingh/Documents/Thesis/testdata/RRT_"+N+".txt",angleConstraint);
		}
		
		void RRT1(Vector3 start, Vector3 end, float angleConstraint){

			var watch= Stopwatch.StartNew();


			KDTree tre= new KDTree(3);;
			double maxrange=Mathf.Max(arena.getMaxX()-arena.getMinX(),arena.getMaxZ()-arena.getMinZ());


			//insert start to KDtree
			tre.insert(new double[]{start.x,start.y,start.z},(int)0);
			//add start to 0th index
			vertices.Add(start);
			vindex++;

			//take multiple end vertices, within a gap of 0.1f 
			for(float f= arena.getMinY2(); f<arena.getMaxY2(); f+=0.1f){
				var e= new Vector3(end.x,f,end.z);
				vertices.Add(e);
				endNodesList.Add(vindex);

				tre.insert(new double[]{e.x,e.y,e.z},(int)vindex);
//				makepoint2(e);
				vindex++;
			}
			
			//add all N points as Zero
			vertices.AddRange(Enumerable.Repeat(Vector3.zero,N).ToList());

			vertices.TrimExcess();
			//create graph now
			graphObj= new Graph(this.vertices,angleConstraint);
		

		
			Vector3 p,q;

			RaycastHit[] obstacleHits;
			int hitcount;
			float x, y, z;


			// get N random new points
			for(int pi=0; pi<N; pi++){
				x=UnityEngine.Random.Range(arena.getMinX(),arena.getMaxX());
				y=UnityEngine.Random.Range(arena.getMinY2(),arena.getMaxY2());
				z=UnityEngine.Random.Range(arena.getMinZ(),arena.getMaxZ());
				p= new Vector3(x,y,z);

				q= new Vector3(x+(float)maxrange,y,z+(float)maxrange);
				obstacleHits= Physics.RaycastAll(p,q-p,Mathf.Infinity);
				hitcount=0;
				foreach(var oH in obstacleHits){
					if( oH.transform.name.Contains("Box"))
						hitcount++;
				}
				//outside?
				if(hitcount%2==0)
				{
					pi--; continue;
				}

				vertices[vindex]=p;
				if(!findClosestNeighbour(p, vindex, tre))
				{ 
					pi--;continue;
				}
//				makepoint(p);
				tre.insert(new double[]{p.x,p.y,p.z},(int)vindex);
				vindex++;

			}
			watch.Stop();
			metrics1.RRT_running_time= watch.ElapsedMilliseconds;
			metrics1.v_in_graph= graphObj.nvertices;
			metrics1.e_in_graph=  graphObj.ndirectededges;
			udl ("finished");
		}

		bool findClosestNeighbour(Vector3 p, int vertex_count_at_the_moment,KDTree tre){
			int r,v;
			object[] foundNodes;
			int oldNodesindex=0;
			bool hit, hitbox;
			
			RaycastHit obstacleHit;
			r=0;
			r=vertex_count_at_the_moment;//(int)Mathf.Sqrt(vertex_count_at_the_moment);

//			while(r<=N){

				foundNodes= tre.nearest(new double[]{p.x,p.y,p.z},r);

				for(int i=oldNodesindex; i< foundNodes.Count(); i++){
					v=(int)foundNodes[i];
					hit= Physics.Raycast(p,this.vertices[v]-p,out obstacleHit,Mathf.Infinity);
					hitbox= hit?obstacleHit.transform.name.Contains("Box") :false;
					if(!hitbox && graphObj.addEdge(vertex_count_at_the_moment,v)){
//						graphObj.addEdge(vertex_count_at_the_moment,v);
//						Debug.DrawLine(p,this.vertices[v],Color.red,10000);
						return true;
					}
				}
				oldNodesindex=foundNodes.Count();
//				if(r>= vertex_count_at_the_moment)
//					return false;
//				r=vertex_count_at_the_moment;

//			}
			return false;
		}

		#region fake RRT
		void RRT2(Vector3 start, Vector3 end, float angleConstraint){
			
			double maxrange=Mathf.Max(arena.getMaxX()-arena.getMinX(),arena.getMaxZ()-arena.getMinZ());
			IntervalKDTree<int> tree;
			tree = new IntervalKDTree<int>(maxrange/2, 10);

			//add start to 0th index
			vertices.Add(start);
			vindex++;

			//take multiple end vertices, within a gap of 0.1f 
			for(float f= arena.getMinY2(); f<arena.getMaxY2(); f+=0.1f){
				var e= new Vector3(end.x,f,end.z);
				vertices.Add(e);
				vindex++;
			}

			
			//add all N points as Zero
			vertices.AddRange(Enumerable.Repeat(Vector3.zero,N).ToList());

			vertices.TrimExcess();
			//create graph now
			graphObj= new Graph(this.vertices,angleConstraint);
			
			
			Vector3 p,q;

			RaycastHit[] obstacleHits;
			int hitcount;
			float x, y, z;

			
			int loop=0;
			// get N random new points
			for(int pi=0; pi<N; pi++){
				x=UnityEngine.Random.Range(arena.getMinX(),arena.getMaxX());
				y=UnityEngine.Random.Range(arena.getMinY2(),arena.getMaxY2());
				z=UnityEngine.Random.Range(arena.getMinZ(),arena.getMaxZ());
				p= new Vector3(x,y,z);

				q= new Vector3(x+(float)maxrange,y,z+(float)maxrange);
				obstacleHits= Physics.RaycastAll(p,q-p,Mathf.Infinity);
				hitcount=0;
				foreach(var oH in obstacleHits){
					if( oH.transform.name.Contains("Box"))
						hitcount++;
				}
				//outside?
				if(hitcount%2==0)
				{
					pi--; continue;
				}

				vertices[vindex]=p;
			}
			createKDTreeDictionary(tree);
		}

		private void createKDTreeDictionary(IntervalKDTree<int> tree){
			
			for(int i=0; i<vertices.Count;i++){
				tree.Put( Mathf.FloorToInt(vertices[i].x), Mathf.FloorToInt(vertices[i].y), Mathf.FloorToInt(vertices[i].z),
				         Mathf.CeilToInt(vertices[i].x),Mathf.CeilToInt(vertices[i].y),Mathf.CeilToInt(vertices[i].z),
				         i);
			}
		}

		#endregion	

		void makepoint(Vector3 v){
			var go= GameObject.CreatePrimitive(PrimitiveType.Sphere);
			go.transform.localScale= new Vector3(0.38f,0.38f,0.38f);
			go.transform.position=v;
			go.gameObject.GetComponent<Renderer>().material.color =Color.magenta;
		}
		void makepoint2(Vector3 v){
			var go= GameObject.CreatePrimitive(PrimitiveType.Cube);
			go.transform.localScale= new Vector3(0.38f,0.38f,0.38f);
			go.transform.position=v;
			go.gameObject.GetComponent<Renderer>().material.color =Color.yellow;
		}
		public static void udl(object s){
			UnityEngine.Debug.Log(s);
		}

		void saveResult(string filename){
			//write number of vertices
			string writeline=graphObj.nvertices+"\n";
			//write vertex coordinates
			foreach(var v in vertices){
				writeline+=v.x+","+v.y+","+v.z+"\n";
			}

			//write startindex
			writeline+= startNearestNode+"\n";

			//write endindices (count , {indices})
			writeline+=endNodesList.Count+",";
			for(int i=0; i<endNodesList.Count-1;i++){
				writeline+= endNodesList[i]+",";
			}
			writeline+=endNodesList[endNodesList.Count-1]+"\n";

			///write adjacencies of a vertex
			/// vertexid		adjacentIds
			for(int i=0; i<vertices.Count;i++){
				writeline+=i+"\t";
				if(graphObj.directedEdges[i]==null)
					continue;
				foreach(var adj in graphObj.directedEdges[i]){
					writeline+=adj.nodeId+"\t";
			
				}
				writeline+="\n";
			}
			System.IO.File.WriteAllText(filename, writeline);
		}
		void loadResult(string InputFile, float angleConstraint){
			char[] delimiterChars = { ' ', '\t',',' };
			string []objectFile;
			int filepoint=0; 
			string[]parsed;
			float a,b, c;
			objectFile = System.IO.File.ReadAllLines(InputFile);
			int nvertices = Convert.ToInt32(objectFile [filepoint++]);


			for( int i=0; i <nvertices;filepoint++, i++){
				parsed= objectFile[filepoint].Split(delimiterChars);
				a=float.Parse(parsed[0], System.Globalization.CultureInfo.InvariantCulture);
				b=float.Parse(parsed[1], System.Globalization.CultureInfo.InvariantCulture);
				c=float.Parse(parsed[2], System.Globalization.CultureInfo.InvariantCulture);
				this.vertices.Add(new Vector3(a,b,c));
			}
			startNearestNode = Convert.ToInt32(objectFile [filepoint++]);
			parsed= objectFile[filepoint++].Split(delimiterChars);
			endNodesList= new List<int>(Convert.ToInt32(parsed[0]));
			int endcount=Convert.ToInt32(parsed[0]);
			for(int i=0; i<endcount; i++){
				endNodesList.Add(Convert.ToInt32(parsed[i+1]));
			}
			graphObj = new Graph(vertices, angleConstraint);
			int s,d;
			for( ; filepoint< objectFile.Count(); filepoint++){
				parsed= objectFile[filepoint].Split(delimiterChars);
				s= Convert.ToInt32(parsed[0]);
				for(int i=1; i<parsed.Count()-1;i++){
					d=Convert.ToInt32(parsed[i]);
					graphObj.addEdge(s,d);
				}
			}
			metrics1.RRT_running_time =0;
			metrics1.v_in_graph= graphObj.nvertices;
			metrics1.e_in_graph= graphObj.ndirectededges;
		}
	}
}
