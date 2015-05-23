using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Priority_Queue;
using KDTreeDLL;
namespace Medial{

	public class RRT {
	
		KDTree tre;
		List <Vector3> vertices;

		int vindex =0, endindex;
		ArenasGenerator arena;
		int startNearestNode; 
		List<int> endNearestNodes;
		Graph graphObj;
		public RRT(ArenasGenerator arena, int N, Vector3 start, Vector3 end){

			this.vertices = new List<Vector3>(N);
			this.arena=arena;


			double maxrange=Mathf.Max(arena.getMaxX()-arena.getMinX(),arena.getMaxZ()-arena.getMinZ());

			tre= new KDTree(3);

			//insert start to KDtree
			tre.insert(new double[]{start.x,start.y,start.z},(int)0);
			//add start to 0th index
			vertices.Add(start);
			vindex++;

			//take multiple end vertices, within a gap of 0.1f 
			for(float f= arena.getMinY2(); f<arena.getMaxY2(); f+=0.1f){
				var e= new Vector3(end.x,f,end.z);
				vertices.Add(e);
				//				partialAdjacency.Add(-1);

				tre.insert(new double[]{e.x,e.y,e.z},(int)vindex);
//				makepoint2(e);
				vindex++;
			}
			endindex=vindex;
		
			Vector3 p,q;

			RaycastHit[] obstacleHits;
			int hitcount;
			float x, y, z;

			udl (vertices.Count() +" "+vindex);
			//add all N points as Zero
			vertices.AddRange(Enumerable.Repeat(Vector3.zero,N).ToList());
			udl (vertices.Count());

			//create graph now
			graphObj= new Graph(this.vertices,10f);
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
				if(!findClosestNeighbour(p, vindex))
				{ 
					pi--;continue;
				}
//				makepoint(p);
				tre.insert(new double[]{p.x,p.y,p.z},(int)vindex);
				vindex++;

				if(loop++>N*N)
				{
					udl ("nooo");
					return;
				}
			}

		}

		bool findClosestNeighbour(Vector3 p, int vertex_count_at_the_moment){
			int r,v;
			object[] foundNodes;
			int oldNodesindex=0;
			bool hit, hitbox;
			
			RaycastHit obstacleHit;

			r=0;
			while(true){

				r+=10;
				if(r>= vertex_count_at_the_moment)
					r=vertex_count_at_the_moment;
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
				if(r>= vertex_count_at_the_moment)
					return false;

			}

		}

		
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

	}
}
