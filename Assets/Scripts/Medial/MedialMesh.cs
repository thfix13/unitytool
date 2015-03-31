using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Priority_Queue;
namespace Medial{
	public class MedialMesh {
		// takes points and traingles from buildobject (reading of file).. remember that the reverse directioned 
		//triangles have also been created... 
		List <Vector3> vertices;
		List <int> triangles;
		Graph graphObj;
		List<int> path= null;
		int startNearestNode, endNearestNode;
		GameObject meshGameObject;


		public MedialMesh(string InputFile, GameObject go, bool createGraphflag, bool filterNodesFlag){

			char[] delimiterChars = { ' ', '\t' };
			meshGameObject=go;
			string []objectFile;
			objectFile = System.IO.File.ReadAllLines(InputFile);
			int nvertices_totalTri = Convert.ToInt32(objectFile [0]) * 2;
			int ntriangles = Convert.ToInt32(objectFile [1]) *2;
			this.vertices = new List<Vector3>(nvertices_totalTri);
			this.triangles =  new List<int>(ntriangles);
			
			
			string []parsed;
			float a,b, c;
			int vPointer=2;
			
			for( int i=0; i <nvertices_totalTri/2;vPointer++, i++){
				parsed= objectFile[vPointer].Split(delimiterChars);
				a=float.Parse(parsed[0], System.Globalization.CultureInfo.InvariantCulture);
				b=float.Parse(parsed[1], System.Globalization.CultureInfo.InvariantCulture);
				c=float.Parse(parsed[2], System.Globalization.CultureInfo.InvariantCulture);
				this.vertices.Add(new Vector3(a,b,c));
				
			}

			for(int i=0, j=0; j< ntriangles/2 ;i=i+3,j++,vPointer++){
				parsed= objectFile[vPointer].Split(delimiterChars);
				this.triangles.Add(Convert.ToInt32(parsed[1]));
				this.triangles.Add(Convert.ToInt32(parsed[2]));
				this.triangles.Add(Convert.ToInt32(parsed[3]));
			}


			if(filterNodesFlag)
			{

				removeTopBottom();
				//update totalvertices count after removing vertices and triangles of top and bottom
				
				nvertices_totalTri=this.vertices.Count*2;
				ntriangles=this.triangles.Count*2/3;
			}
			if(createGraphflag){
				graphObj= new Graph(this.vertices.Count);
//				udl ("Total vertices= "+this.vertices.Count +" total tri="+ this.triangles.Count);
				
			}
			layMesh();
		}

		//Duplicate vertices, and triangles in the other orientation, to overcome the 
		//problem of disappearing of some triangles
		void layMesh(){
			int nvertices_totalTri=this.vertices.Count*2;
			int ntriangles=this.triangles.Count*2/3;

			List <Vector3> mesh_dupli_vertices= new List<Vector3>(nvertices_totalTri);
			List <int> mesh_dupli_triangles= new List<int>(ntriangles);

			mesh_dupli_vertices.AddRange(this.vertices.AsEnumerable());
			mesh_dupli_vertices.AddRange(this.vertices.AsEnumerable());
			mesh_dupli_triangles.AddRange(this.triangles.AsEnumerable());

			for (int i=0,j=ntriangles/2; j<ntriangles; i++, j++) {
				mesh_dupli_triangles.Add(this.triangles[ntriangles*3/2-i-1]);
				i++;
				mesh_dupli_triangles.Add(this.triangles[ntriangles*3/2-i-1]);
				i++;
				mesh_dupli_triangles.Add(this.triangles[ntriangles*3/2-i-1]);
			}

			List<Vector3> l = Enumerable.Repeat (Vector3.up, nvertices_totalTri/2).ToList();
			l.AddRange(Enumerable.Repeat(Vector3.down,nvertices_totalTri/2).ToList());

			//End
			MeshFilter ms = this.meshGameObject.GetComponent <MeshFilter> ();
			Mesh mesh = new Mesh ();
			ms.mesh = mesh;
			mesh.vertices = mesh_dupli_vertices.ToArray();
			mesh.triangles = mesh_dupli_triangles.ToArray();
			
			Color[] colors= new Color[mesh.vertices.Count()];
			Color triColor= Color.green;
			for (int i=0; i<mesh_dupli_triangles.Count; i++) {
				int vertIndex = mesh_dupli_triangles[i];
//				if (i % 3 == 0){
//					Vector3 v=this.vertices[vertIndex];
//					//				triColor = new Color(v.y/11f,UnityEngine.Random.Range(0f,1f),(v.y)/11f,150f/255f);
//				}
				colors[vertIndex] = triColor;
			}
			mesh.colors= colors;
			mesh.normals = l.ToArray();
		}

		void removeTopBottom(){
			List <Vector3> newVertices = new List<Vector3>(this.vertices.Count);
			List <int> newTriangles= new List<int>(this.triangles.Count);
			HashSet<int> removedVertices= new HashSet<int>();
			int []newindexes= new int[this.vertices.Count];
			for(int i=0, j=0; i<vertices.Count;i++){
				if(vertices[i].y < 0.9 || vertices[i].y > 19.1){
					removedVertices.Add(i);
					newindexes[i]=-1;
				}
				else{
					newVertices.Add(this.vertices[i]);
					newindexes[i]=j;
					j++;
				}
				
			}
			newVertices.TrimExcess();
			for(int i=0;i < triangles.Count;i+=3){
				if(removedVertices.Contains(triangles[i]) ||removedVertices.Contains(triangles[i+1])
				   ||removedVertices.Contains(triangles[i+2]))
					continue;
				newTriangles.Add(newindexes[triangles[i]]);
				newTriangles.Add(newindexes[triangles[i+1]]);
				newTriangles.Add(newindexes[triangles[i+2]]);
			}
			newTriangles.TrimExcess();
			this.vertices=newVertices;
			this.triangles=newTriangles;
		}

		public void createGraph(){
			int a,b,c;
			Vector3 av,bv,cv;
			float ab, bc,ac;

			for(int i=0; i<this.triangles.Count; i=i+3){
				a=this.triangles[i];b=this.triangles[i+1];c=this.triangles[i+2];
				av=vertices[a];bv=vertices[b];cv=vertices[c];
				ab=Vector3.Distance(av,bv);
				bc=Vector3.Distance(bv,cv);
				ac=Vector3.Distance(av,cv);
				if(bv.y- av.y >0){
					graphObj.addDirectededge(a,b,ab);
					graphObj.addUnDirectededge(a,b,ab);
				}
				else
				{
					graphObj.addDirectededge(b,a,ab);
					graphObj.addUnDirectededge(b,a,ab);
				}
				if(bv.y- cv.y >0){
					graphObj.addDirectededge(c,b,bc);
					graphObj.addUnDirectededge(c,b,bc);
				}
				else
				{	graphObj.addDirectededge(b,c,bc);
					graphObj.addUnDirectededge(b,c,bc);
				}
				if(cv.y- av.y >0)
				{	graphObj.addDirectededge(a,c,ac);
					graphObj.addUnDirectededge(a,c,ac);
				}
				else
				{	graphObj.addDirectededge(c,a,ac);
					graphObj.addUnDirectededge(c,a,ac);
				}
			}
		}
		
		/// <summary>
		/// Removes corners from medial mesh.. corners that are V shaped or dual layered meshes, creating 
		/// unneccessary complications in the medial mesh. They need to be removed.
		/// </summary>
		public void removeVs(){
			List<edgenode> adjacentnodes;
			HashSet<int>removedVertices= new HashSet<int>();

			bool breakflag=false, f=false;
			for(int i=0; i<this.vertices.Count;i++){
				var v=vertices[i];

				adjacentnodes= graphObj.unDirectedEdges[i];

				//check angles between two adjacent nodes. 
				//if they are in same angle in y direction, but different in y plane...leave them.. they not forming a corner

				//if they are in same angle in y direction, and in ~ same angle in y plane... cut this node off.

				//break the loop from searching for other neighbours
				breakflag=false;

				foreach(var neighbour1 in adjacentnodes){
					foreach(var neighbour2 in adjacentnodes){
						if(neighbour1.y>= neighbour2.y)
							continue;

						if(graphObj.unDirectedEdges[neighbour1.y].Contains(neighbour2))
							continue;

						var ay=angleY(vertices[neighbour1.y],vertices[neighbour2.y],v);
						var ayp=angleYPlane(vertices[neighbour1.y],vertices[neighbour2.y],v);

						if(ay<42 && ayp <10)
						{	
							breakflag=true;
							Debug.DrawLine(vertices[neighbour1.y],vertices[neighbour2.y], Color.magenta,100000);
							removedVertices.Add(i);
							break;
						}

					}
					if(breakflag)
						break;
				}
			}

			
			//remove only triangles containing removedVertices, otherwise indexes will change
			//and everything will have to be refreshed
			int l=this.triangles.Count;
			for(int i=0;i < l;i+=3){
				if(removedVertices.Contains(this.triangles[i]) ||removedVertices.Contains(this.triangles[i+1])
				   ||removedVertices.Contains(this.triangles[i+2])){
					this.triangles.RemoveAt(i+2);
					this.triangles.RemoveAt(i+1);
					this.triangles.RemoveAt(i);
					l=this.triangles.Count;
					i-=3;
				}
			}
			layMesh();

			//TODO:The graph also needs to be updated
			//TODO:links among neighbours of removed vertices to be made (in the graph as well as to show using drawline)

		}
		//failures can be removes by adding edges across a quadrilateral's diagonal vertices
		public void removeVs_failures(){
			List<edgenode> adjacentnodes;
			
			bool breakflag=false, f=false;
			for(int i=0; i<this.vertices.Count;i++){
				var v=vertices[i];
				
				
				adjacentnodes= graphObj.unDirectedEdges[i];
				
				//check angles between two adjacent nodes. 
				//if they are in same angle in y direction, but different in y plane...leave them.. they not forming a corner
				
				//if they are in same angle in y direction, and in ~ same angle in y plane... cut this node off.
				
				//break the loop from searching for other neighbours
				breakflag=false;
				f=false;
				if(v.x==-2.5f && v.z==-1.5f)
					f=true;
				
				foreach(var neighbour1 in adjacentnodes){
					
					foreach(var neighbour2 in adjacentnodes){
						
						if(neighbour1.y>= neighbour2.y){
							continue;
						}
						//						if(vertices[neighbour1.y]== vertices[ neighbour2.y])
						//							continue;
						if(graphObj.unDirectedEdges[neighbour1.y].Contains(neighbour2)){
							
							continue;
						}
						var ay=angleY(vertices[neighbour1.y],vertices[neighbour2.y],v);
						var ayp=angleYPlane(vertices[neighbour1.y],vertices[neighbour2.y],v);
						
						if(ay<42 && ayp <10)
						{	breakflag=true;
							break;
						}
						if(f && (ay<42.0f) && ayp >10.0f){
							//							breakflag=true;
							Debug.DrawLine(vertices[neighbour1.y],v, Color.magenta,100000);
							Debug.DrawLine(vertices[neighbour2.y],v,Color.blue,100000);
							
							
							//							if(f){
							//								udl ("n1_id= "+ neighbour1.y+ " n2_id="+neighbour2.y);
							var go2= GameObject.CreatePrimitive(PrimitiveType.Capsule);
							go2.transform.localScale= new Vector3(0.3f,0.3f,0.3f);
							go2.transform.position=v;
							go2.name= "Grey "+v.x+","+v.y+","+v.z +" i="+i;
							go2.gameObject.GetComponent<Renderer>().material.color= Color.cyan;
							
							udl (Vtostring(v)+" -- n1= "+Vtostring(vertices[neighbour1.y])+"  --  n2="+Vtostring(vertices[neighbour2.y])+" -- angle="+ay+ " --- ayp= "+ayp);
							var go= GameObject.CreatePrimitive(PrimitiveType.Sphere);
							go.transform.localScale= new Vector3(0.38f,0.38f,0.38f);
							go.transform.position=vertices[neighbour1.y];
							go.gameObject.GetComponent<Renderer>().material.color =Color.magenta;
							go.transform.parent=go2.transform;
							go.name=Vtostring(vertices[neighbour1.y])+"Mag "+neighbour1.y+" "+"ayp="+ayp+" ay="+ay;
							var go1= GameObject.CreatePrimitive(PrimitiveType.Cube);
							go1.transform.localScale= new Vector3(0.3f,0.3f,0.3f);
							go1.transform.position=vertices[neighbour2.y];
							go1.gameObject.GetComponent<Renderer>().material.color =Color.yellow;
							go1.name=Vtostring(vertices[neighbour2.y])+"Yel "+neighbour2.y+" "+"ayp="+ayp+" ay="+ay;
							go1.transform.parent=go2.transform;
							
							//							break;
							
						}
					}
					if(breakflag)
						break;
					
					
				}
				//				if(flag){
				//					var go2= GameObject.CreatePrimitive(PrimitiveType.Capsule);
				//					go2.transform.localScale= new Vector3(0.3f,0.3f,0.3f);
				//					go2.transform.position=v;
				//					go2.name= "Grey"+v.x+","+v.y+","+v.z;
				//					go2.gameObject.GetComponent<Renderer>().material.color= Color.cyan;
				//				}
				
				//				if(f){
				//					var go3= GameObject.CreatePrimitive(PrimitiveType.Sphere);
				//					go3.transform.localScale= new Vector3(0.3f,0.3f,0.3f);
				//					go3.transform.position=v;
				//					go3.name= "Grey"+v.x+","+v.y+","+v.z;
				//					go3.gameObject.GetComponent<Renderer>().material.color= Color.cyan;
				//				}
			}
		}


		string Vtostring(Vector3 v){

			return String.Format(v.x+","+v.y+","+v.z);
		}
		float angleY(Vector3 a, Vector3 b, Vector3 p){
			return Vector3.Angle(Vector3.ProjectOnPlane(a-p,Vector3.up),Vector3.ProjectOnPlane(b-p,Vector3.up));
		}
		float angleYPlane(Vector3 a, Vector3 b, Vector3 p){
//			return Mathf.Asin(Vector3.Dot(a-p,b-p)/(Vector3.Magnitude(a-p)*Vector3.Magnitude(b-p)))*Mathf.Rad2Deg;
			var prjtOnXZ= new Vector3(a.x,p.y,a.z)-p;
			Vector3 normalOfPlanePerpendicularToXZContainingA;
			if(prjtOnXZ!=Vector3.zero)
				normalOfPlanePerpendicularToXZContainingA= Vector3.Cross(a-p,prjtOnXZ);
			else
				normalOfPlanePerpendicularToXZContainingA= new Vector3(p.x+1.0f,p.y,p.z);
			var prjtOfB_OnPlane= Vector3.ProjectOnPlane(b-p,normalOfPlanePerpendicularToXZContainingA);
			return Vector3.Angle(a-p,prjtOfB_OnPlane);
		}

		//add edges from-to random vertices in medial skeleton
		public void addEdgesThatDontCollideWithArena(){

			int r= UnityEngine.Random.Range(0,graphObj.nvertices-1);
			int s=UnityEngine.Random.Range(0,graphObj.nvertices-1);
			RaycastHit obstacleHit;
			bool hit, rcontainss,scontainsr,hitbox, loopingflag=true;
	//		hit= Physics.Raycast(v[r],v[s]-v[r],out obstacleHit,Mathf.Infinity);//Vector3.Distance(v[r],v[s]));
	//		rcontainss= g.edges[r]!=null? g.edges[r].Contains( new edgenode(s,0)):true;
	//		scontainsr= g.edges[s]!=null? g.edges[s].Contains(new edgenode(r,0)):true ;
	//		hitbox= hit ? !obstacleHit.transform.gameObject.name.Equals("Map"):false;
	//		comboflag= (r==s|| rcontainss||scontainsr|| hitbox );
			int loop=10000;

			while(loop >0){

					
				r= UnityEngine.Random.Range(0,graphObj.nvertices-1);
				s=UnityEngine.Random.Range(0,graphObj.nvertices-1);
				hit= Physics.Raycast(vertices[r],vertices[s]-vertices[r],out obstacleHit,Mathf.Infinity);
				rcontainss= graphObj.directedEdges[r]!=null? graphObj.directedEdges[r].Contains( new edgenode(s,0)):false;
				scontainsr= graphObj.directedEdges[s]!=null? graphObj.directedEdges[s].Contains(new edgenode(r,0)):false ;
				hitbox= hit ? obstacleHit.transform.name.Contains("Box"):false;
				loopingflag= (vertices[r].y==vertices[s].y|| rcontainss||scontainsr|| hitbox );
				if(!loopingflag)
				{
					if(vertices[s].y <vertices[r].y)
						graphObj.addDirectededge(s,r,Vector3.Distance(vertices[s],vertices[r]));
					else
						graphObj.addDirectededge(r,s,Vector3.Distance(vertices[s],vertices[r]));
					loop--;
				}
			}
		}

		public void findNearest(Vector3 start,Vector3 end){
			float min=-1,diff;
			int minvertex=-1;
			//for now very inefficient
			for(int i=0;i<vertices.Count;i++){
				if(vertices[i].y<start.y)
					continue;
				diff=Vector3.Distance(vertices[i],start);
				if(min==-1)
					min=diff;
				if(diff<min){
					min=diff;
					minvertex=i;
				}
			}
			startNearestNode=minvertex;

			min=-1;
			for(int i=0;i<vertices.Count;i++){
				if(vertices[i].y<end.y)
					continue;
				diff=Vector3.Distance(vertices[i],end);
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
			HeapPriorityQueue<element> pq= new HeapPriorityQueue<element>(graphObj.nvertices);
			Hashtable backtrack = new Hashtable();
			for(int i=0;i< graphObj.nvertices;i++)
				backtrack.Add(i,-1);
			element currentv, tempElement;
			tempElement= new element(startNearestNode);
			pq.Enqueue(tempElement,0);
			pqSet.Add(startNearestNode, new hashnode(0,tempElement));
	//		udl ("Starting node= "+startNearestNode+ "Ending node="+endNearestNode);
			double currentdist, old_dist, new_dist;
			hashnode temp;
			udl(pq.Count);
			while(visitedNodes.Count!=graphObj.nvertices){
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
				if(graphObj.directedEdges[currentv.nodeId]==null)
					continue;
				foreach(var adjv in graphObj.directedEdges[currentv.nodeId]){

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
				UnityEngine.Debug.DrawLine(vertices[path[i]],vertices[path[i+1]],Color.cyan,200,false);
			
			udl ("nodes in path= "+path.Count);
		}


		//to be invoked from update
		private int currentpath_i=0;
		public Vector3 movePlayer(float t){
			if(t<vertices[path[0]].y)
				return Vector3.zero;
			while(currentpath_i<path.Count-1 && t>vertices[path[currentpath_i]].y)
				currentpath_i++;
			if(currentpath_i>=path.Count)
				return Vector3.zero;
			float frac1=t- vertices[path[currentpath_i-1]].y, frac2= vertices[path[currentpath_i]].y -t;
			var playerpos= (vertices[path[currentpath_i-1]]*frac2 + vertices[path[currentpath_i]] *frac1)/(frac1+frac2);
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
		public List<edgenode> [] directedEdges;
		public List<edgenode> [] unDirectedEdges;
		int [] degree;
		public int nvertices;

		public Graph(int vertices){
			nvertices=vertices;
			//TODO:: need to change it to array of hashmaps, so that .contains works better
			directedEdges= new List<edgenode>[nvertices];
			unDirectedEdges= new List<edgenode>[nvertices];
			UnityEngine.Debug.Log(nvertices);
			degree= new int[nvertices];
			for(int i=0;i<nvertices;i++)
			{
				degree[i]=0;
			}
		}
		
		public void addDirectededge(int from, int to, float w){
			if(directedEdges[from]==null)
				directedEdges[from]=new List<edgenode>();
			if(directedEdges[from].Contains(new edgenode(to,w))){
				return;
			}
			directedEdges[from].Add(new edgenode(to,w));
		}
		public void addUnDirectededge(int from, int to, float w){
			if(unDirectedEdges[from]==null)
				unDirectedEdges[from]=new List<edgenode>();
			if(unDirectedEdges[to]==null)
				unDirectedEdges[to]=new List<edgenode>();
			if(!unDirectedEdges[from].Contains(new edgenode(to,w))){
				unDirectedEdges[from].Add(new edgenode(to,w));

			}
			if(!unDirectedEdges[to].Contains(new edgenode(from,w))){
				unDirectedEdges[to].Add(new edgenode(from,w));
				
			}
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

