using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Priority_Queue;
namespace Medial{
	public class GraphUtil {
		// takes points and traingles from buildobject (reading of file).. remember that the reverse directioned 
		//triangles have also been created... 
		List <Vector3> v;
		List <int> tri;
		Graph g;
		List<int> path= null;
		int startNearestNode, endNearestNode;

		public GraphUtil(List<Vector3> vertices, List<int>triangles){
			g= new Graph(vertices.Count);
			v=vertices;
			tri=triangles;
		}


		public void createGraph(){
			int a,b,c;
			Vector3 av,bv,cv;
			float ab, bc,ac;

			for(int i=0; i<tri.Count; i=i+3){
				a=tri[i];b=tri[i+1];c=tri[i+2];
				av=v[a];bv=v[b];cv=v[c];
				ab=Vector3.Distance(av,bv);
				bc=Vector3.Distance(bv,cv);
				ac=Vector3.Distance(av,cv);
				if(bv.y- av.y >0)
					g.addedge(a,b,ab);
				if(av.y- bv.y >0)
					g.addedge(b,a,ab);
				if(bv.y- cv.y >0)
					g.addedge(c,b,bc);
				if(cv.y- bv.y >0)
					g.addedge(b,c,bc);
				if(cv.y- av.y >0)
					g.addedge(a,c,ac);
				if(av.y- cv.y >0)
					g.addedge(c,a,ac);
	//			if(bv.y- av.y >0)
	//				g.addedge(a,b,1f/ab);
	//			if(av.y- bv.y >0)
	//				g.addedge(b,a,1f/ab);
	//			if(bv.y- cv.y >0)
	//				g.addedge(c,b,1f/bc);
	//			if(cv.y- bv.y >0)
	//				g.addedge(b,c,1f/bc);
	//			if(cv.y- av.y >0)
	//				g.addedge(a,c,1f/ac);
	//			if(av.y- cv.y >0)
	//				g.addedge(c,a,1f/ac);
			}
		}

		//add edges from-to random vertices in medial skeleton
		public void addEdgesThatDontCollideWithArena(){

			int r= UnityEngine.Random.Range(0,g.nvertices-1);
			int s=UnityEngine.Random.Range(0,g.nvertices-1);
			RaycastHit obstacleHit;
			bool hit, rcontainss,scontainsr,hitbox, loopingflag=true;
	//		hit= Physics.Raycast(v[r],v[s]-v[r],out obstacleHit,Mathf.Infinity);//Vector3.Distance(v[r],v[s]));
	//		rcontainss= g.edges[r]!=null? g.edges[r].Contains( new edgenode(s,0)):true;
	//		scontainsr= g.edges[s]!=null? g.edges[s].Contains(new edgenode(r,0)):true ;
	//		hitbox= hit ? !obstacleHit.transform.gameObject.name.Equals("Map"):false;
	//		comboflag= (r==s|| rcontainss||scontainsr|| hitbox );
			int loop=10000;

			while(loop >0){

					
				r= UnityEngine.Random.Range(0,g.nvertices-1);
				s=UnityEngine.Random.Range(0,g.nvertices-1);
				hit= Physics.Raycast(v[r],v[s]-v[r],out obstacleHit,Mathf.Infinity);
				rcontainss= g.edges[r]!=null? g.edges[r].Contains( new edgenode(s,0)):false;
				scontainsr= g.edges[s]!=null? g.edges[s].Contains(new edgenode(r,0)):false ;
				hitbox= hit ? obstacleHit.transform.name.Contains("Box"):false;
				loopingflag= (v[r].y==v[s].y|| rcontainss||scontainsr|| hitbox );
	//			Debug.Log(obstacleHit.transform+ " \ncomboflag="+hit+" "+loopingflag+ " "+hitbox);
				if(!loopingflag)
				{
					if(v[s].y <v[r].y)
						g.addedge(s,r,Vector3.Distance(v[s],v[r]));
					else
						g.addedge(r,s,Vector3.Distance(v[s],v[r]));
	//				Debug.DrawLine(v[r],v[s],Color.green,100000, true);
					loop--;
				}
			}

			//catch(NullReferenceException e){
	//			Debug.DrawLine(v[r],v[s],Color.red,100000);
	//			Debug.Log("yayyyy"+s +" "+r);
	//		}
	//		finally{
	//		if(loop<100000){
	//			Debug.DrawLine(v[r],v[s],Color.cyan,100000);
	//			Debug.Log("yayyyy"+v[s] +" "+v[r]);
	//		}
		}

		public void findNearest(Vector3 start,Vector3 end){
			float min=-1,diff;
			int minvertex=-1;
			//for now very inefficient
			for(int i=0;i<v.Count;i++){
				if(v[i].y<start.y)
					continue;
				diff=Vector3.Distance(v[i],start);
				if(min==-1)
					min=diff;
				if(diff<min){
					min=diff;
					minvertex=i;
				}
			}
			startNearestNode=minvertex;

			min=-1;
			for(int i=0;i<v.Count;i++){
				if(v[i].y<end.y)
					continue;
				diff=Vector3.Distance(v[i],end);
				if(min==-1)
					min=diff;
				if(diff<min){
					min=diff;
					minvertex=i;
				}
			}
			endNearestNode=minvertex;


		}

		public void findPath(){
			HashSet<int> visitedNodes= new HashSet<int>();
			Hashtable pqSet = new Hashtable();
			HeapPriorityQueue<element> pq= new HeapPriorityQueue<element>(g.nvertices);
			Hashtable backtrack = new Hashtable();
			for(int i=0;i< g.nvertices;i++)
				backtrack.Add(i,-1);
			element currentv, tempElement;
			tempElement= new element(startNearestNode);
			pq.Enqueue(tempElement,0);
			pqSet.Add(startNearestNode, new hashnode(0,tempElement));
	//		udl ("Starting node= "+startNearestNode+ "Ending node="+endNearestNode);
			double currentdist, old_dist, new_dist;
			hashnode temp;
			udl(pq.Count);
			while(visitedNodes.Count!=g.nvertices){
				currentv= pq.Dequeue();
	//			if(currentv==null)

	//			udl (currentv.nodeId);
				if(currentv.nodeId==endNearestNode)
					break;
				try{
					currentdist=((hashnode)pqSet[currentv.nodeId]).priority;
				}
				catch{
					currentdist=0;
				}
				pqSet.Remove(currentv.nodeId);
				visitedNodes.Add(currentv.nodeId);

				//access all adjacent nodes
				if(g.edges[currentv.nodeId]==null)
					continue;
				foreach(var adjv in g.edges[currentv.nodeId]){

					if(visitedNodes.Contains (adjv.y))
						continue;
					new_dist= currentdist + (double)adjv.weight;
					if(pqSet.Contains(adjv.y))
					{
	//					udl (adjv.y+" "+adjv.weight+" yo");
						temp=(hashnode)pqSet[adjv.y];
						old_dist=temp.priority;
						if(old_dist> new_dist){
							pq.UpdatePriority(temp.e,new_dist);
							pqSet[adjv.y]=new hashnode(new_dist,temp.e);
							backtrack[adjv.y]=currentv.nodeId;
						}
					}
					else{
	//					udl (adjv.y+" "+adjv.weight+" noo");
						tempElement= new element(adjv.y);
						pq.Enqueue(tempElement,new_dist);
						pqSet.Add(adjv.y,new hashnode(new_dist,tempElement));
						backtrack[adjv.y]=currentv.nodeId;
					}
				}
			}

			//create reverse path
			path = new List<int>();
			path.Insert(0,endNearestNode);
			for(int i=endNearestNode; i!=startNearestNode;i=(int)backtrack[i]){
				path.Insert(0,(int)backtrack[i]);
			}
		}
		public void showPath(){

			if(path==null)
				return;

			for(int i=0;i<path.Count-1; i++)
				UnityEngine.Debug.DrawLine(v[path[i]],v[path[i+1]],Color.cyan,200,false);
			
			udl ("nodes in path= "+path.Count);
		}

	//	was here
		//to be invoked from update
		private int currentpath_i=0;
		public Vector3 movePlayer(float t){
			if(t<v[path[0]].y)
				return Vector3.zero;
			while(currentpath_i<path.Count-1 && t>v[path[currentpath_i]].y)
				currentpath_i++;
			if(currentpath_i>=path.Count)
				return Vector3.zero;
			float frac1=t- v[path[currentpath_i-1]].y, frac2= v[path[currentpath_i]].y -t;
			var playerpos= (v[path[currentpath_i-1]]*frac2 + v[path[currentpath_i]] *frac1)/(frac1+frac2);
			return playerpos;
		}

		public static void udl(object s){
			UnityEngine.Debug.Log(s);
		}

	}
	class edgenode:IEquatable<edgenode> {
		public int y;
		public float weight;
		public edgenode(int y, float weight){
			this.y=y;
			this.weight=weight;
		}
		public override bool Equals(System.Object e){
			return e!=null && this.y==((edgenode)e).y;
		}
		public bool Equals(edgenode e){
			return e!=null && this.y==e.y;
		}
	}
	class Graph{
		public List<edgenode> [] edges;
		int [] degree;
		public int nvertices;
	//	int nedges;
		bool directed;

		public Graph(int vertices){
			nvertices=vertices;
			edges= new List<edgenode>[nvertices];
			degree= new int[nvertices];
			for(int i=0;i<nvertices;i++)
			{
				degree[i]=0;
			}
			directed=true;
		}
		
		public void addedge(int from, int to, float w){
			if(edges[from]==null)
				edges[from]=new List<edgenode>();
			if(edges[from].Contains(new edgenode(to,w))){
				return;
			}
			edges[from].Add(new edgenode(to,w));
		}
		

	}
	class element: PriorityQueueNode{
		public int nodeId{get; private set;}
		public element(int nodeId){
			this.nodeId=nodeId;
		}
		public override bool Equals(System.Object p){
			if(p==null)
				return false;
			element t=p as element;
			if((System.Object)t ==null)
				return false;
			return this.nodeId==t.nodeId;
		}
		public bool Equals(element p){
			return p!=null && this.nodeId==p.nodeId;
		}
	}

	class hashnode{
		public double priority;
		public element e;
		public hashnode(double priority, element e){
			this.e=e;
			this.priority=priority;
		}
	}

}

