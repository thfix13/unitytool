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
		List<List<int>> paths= null;
		int startNearestNode; 
		List<int> endNearestNodes;
		GameObject meshGameObject;
		IntervalKDTree<int> tree;
		ArenasGenerator arena;

		///for connecting opposite vertices of two common triangles
		Hashtable linesToTriangle;

		///assigned in removeVs; never instantiated by itself
		HashSet<int>removedVertices_global;

		public MedialMesh(string InputFile, GameObject go, bool createGraphflag, bool filterNodesFlag, 
		                  ArenasGenerator arena){
			//.getMinX(),arena.getMaxX(), y2_min, y2_max, arena.getMinZ(), arena.getMaxZ()
			//float x_min, float x_max,float y_min, float y_max,float z_min, float z_max
			char[] delimiterChars = { ' ', '\t' };
			this.meshGameObject=go;
			string []objectFile;
			objectFile = System.IO.File.ReadAllLines(InputFile);
			int nvertices_totalTri = Convert.ToInt32(objectFile [0]) * 2;
			int ntriangles = Convert.ToInt32(objectFile [1]) *2;
			this.vertices = new List<Vector3>(nvertices_totalTri);
			this.triangles =  new List<int>(ntriangles);
			this.arena=arena;
			this.removedVertices_global= new HashSet<int>();
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

				removeTopBottom(arena.getMinY2(), arena.getMaxY2());
				//update totalvertices count after removing vertices and triangles of top and bottom
				
				nvertices_totalTri=this.vertices.Count*2;
				ntriangles=this.triangles.Count*2/3;
			}
			if(createGraphflag){
				graphObj= new Graph(this.vertices,0f);
				
			}

			layMesh();
			this.meshGameObject.AddComponent<MeshCollider>();
			udl ("Medial Skeleton created");

			createGraph();

			double maxrange=Mathf.Max(arena.getMaxX()-arena.getMinX(),arena.getMaxZ()-arena.getMinZ());
			tree = new IntervalKDTree<int>(maxrange/2, 10);
		}

		private void createGraph(){
			int a,b,c;
			linesToTriangle=  new Hashtable();

			for(int i=0; i<this.triangles.Count; i=i+3){
				a=this.triangles[i];b=this.triangles[i+1];c=this.triangles[i+2];

				graphObj.addEdge(a,b);
				joinOppositeVertices(CantorFunction(a,b),i);

				graphObj.addEdge(c,b);
				joinOppositeVertices(CantorFunction(c,b),i);

				graphObj.addEdge(a,c);
				joinOppositeVertices(CantorFunction(c,a),i);

			}
		}

		/// <summary>
		/// add opposite edges of two adjacent triangles
		/// </summary>
		private void joinOppositeVertices(int key, int t){
			if(!linesToTriangle.ContainsKey(key))
				linesToTriangle.Add(key,t);
			else{
				int t1= (int)linesToTriangle[key];
				if(t==t1)
					return;
				var opp=oppositeVertices(t,t1).ToList();
				if(opp.Count !=2)
					udl ("triangle count="+opp.Count);
				graphObj.addEdge(opp[0],opp[1]);
//					Debug.DrawLine(vertices[opp[0]],vertices[opp[1]], Color.blue,10000);

			}
		}

		/// <summary>
		/// http://en.wikipedia.org/wiki/Pairing_function#Cantor_pairing_function
		/// </summary>
		/// <returns>gives same value of cantor for both pairs of value, i.e, CantorFn(a,b) = CantorFn(b,a)</returns>
		private static int CantorFunction(int k1, int k2){

			return ((k1 + k2)*(k1 + k2 + 1))/2 + (k2>k1?k2:k1);
		}

		private IEnumerable<int> oppositeVertices(int t1,int t2){
//			HashSet<int> t1s= new HashSet<int>();
			HashSet<int> t2s= new HashSet<int>();
			HashSet<int> Un= new HashSet<int>();
			HashSet<int> In= new HashSet<int>();
			
			Un.Add(triangles[t1]);
			Un.Add(triangles[t1+1]);
			Un.Add(triangles[t1+2]);
			In.Add(triangles[t1]);
			In.Add(triangles[t1+1]);
			In.Add(triangles[t1+2]);
			
			t2s.Add(triangles[t2]);
			t2s.Add(triangles[t2+1]);
			t2s.Add(triangles[t2+2]);

			Un.UnionWith(t2s);
			In.IntersectWith(t2s);
			return Un.Except(In);
		}


		private void createKDTreeDictionary(){
			
			for(int i=0; i<vertices.Count;i++){
				tree.Put( Mathf.FloorToInt(vertices[i].x), Mathf.FloorToInt(vertices[i].y), Mathf.FloorToInt(vertices[i].z),
				         Mathf.CeilToInt(vertices[i].x),Mathf.CeilToInt(vertices[i].y),Mathf.CeilToInt(vertices[i].z),
				         i);
			}
		}

		///Duplicate vertices, and triangles in the other orientation, to overcome the 
		///problem of disappearing of some triangles
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
			Color triColor= new Color(UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f));
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


		/// <summary>
		/// Removes the top bottom. lowest and highest y range to be changed
		/// </summary>
		void removeTopBottom(float y_min, float y_max){
			List <Vector3> newVertices = new List<Vector3>(this.vertices.Count);
			List <int> newTriangles= new List<int>(this.triangles.Count);
			HashSet<int> removedVertices= new HashSet<int>();
			int []newindexes= new int[this.vertices.Count];
			for(int i=0, j=0; i<vertices.Count;i++){

				///remove the covering layer of the medial mesh
				if((vertices[i].y < y_min || vertices[i].y > y_max)){
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



		#region Remove Vs

		public void connect_Vs(float r){

			createKDTreeDictionary();

			float dist;
			float vy;
			
			///scan all the vertices within a 2rx2rx2r box and connect them if their dist is less than r
			/// move this box by r in each direction
			HashSet<int> foundNodes;
			float x,y,z;
			for(x= arena.getMinX()-1;x< arena.getMaxX()-r; x+=r){
				for( y= arena.getMinY2()-1;y< arena.getMaxY2()-r;y+=r){
					for( z= arena.getMinZ()-1; z< arena.getMaxZ()-r;z+=r){
						foundNodes= tree.GetValues(x,y,z,x+2*r,y+2*r,z+2*r,new HashSet<int>());
						foreach(var i in foundNodes){
							foreach(var j in foundNodes){
								//TODO: do we have to relax the v1.y== v2.y constraint here ?
								if(graphObj.unDirectedEdges[i]!=null && graphObj.unDirectedEdges[i].Contains(new edgenode(j,0)))
									continue;

								dist=Vector3.Distance(vertices[i],vertices[j]);
								if(dist>r)
									continue;
								
								graphObj.addEdge(i,j,dist);
//									Debug.DrawLine(vertices[i],vertices[j], Color.magenta,100000);
							}
						}
					}
				}
			}
			
		}

		#region NotUsed
		private HashSet<string> bluelines;
		private HashSet<string> redlines;
		//it updates the neighbours, and drawlines between them.
		HashSet<int> updateGraph(HashSet<int> removedVerticesSmallSet, ref HashSet<int> notRemovedVertices_W_is0){
			if(removedVerticesSmallSet==null ||removedVerticesSmallSet.Count<=0)
				return new HashSet<int>();
			HashSet<int> removedVertices_W_is0= new HashSet<int>();


			foreach(var node in removedVerticesSmallSet){
				///removedVertices_W_is0 had to be sent to check for containment 'containment check', as
				///the nodes with W=0 can't be added to removedVerticesSmallSet (becoz of the loop outside, HasEt can't
				/// be modified while it was iterated over.

				removedVertices_W_is0.UnionWith(join_neighbours2(node,removedVerticesSmallSet, removedVertices_W_is0, 
				                                                 ref notRemovedVertices_W_is0));
			}

			return removedVertices_W_is0;
		}
		
		
		HashSet<int> join_neighbours2(int v,HashSet<int>removedVerticesSmallSet, HashSet<int>removedVertices_W_is0, 
		                              ref HashSet<int> notRemovedVertices_W_is0){

			HashSet<edgenode> adjacentnodes= graphObj.unDirectedEdges[v];

			foreach(var n1 in adjacentnodes){
				foreach(var n2 in adjacentnodes){
					if(vertices[n1.nodeId].y>vertices[n2.nodeId].y)
						continue;

					//containment check
					if(n1.Equals(n2)|| removedVerticesSmallSet.Contains(n1.nodeId)|| removedVerticesSmallSet.Contains(n2.nodeId) ||
					   removedVertices_W_is0.Contains(n1.nodeId) || removedVertices_W_is0.Contains(n2.nodeId))
						continue;
					//check if there isn't an edge already between n1 and n2
					if(!graphObj.unDirectedEdges[n1.nodeId].Contains(n2)){
						float w=Vector3.Distance(vertices[n1.nodeId],vertices[n2.nodeId]);
						//if w is 0 then no need to add edge. merge both of them, i.e.
						//make all the nodes neighbour to n2 as neighbour of n1 
						//and remove node n2. Also include n2 in removedVerticesSmallSet
						//and return n2 to be added to removedVertices.
						if(Mathf.Round(w*100)/100==0)
						{
							List<float[]> temp= new List<float[]>();
							foreach (var n2_ in graphObj.unDirectedEdges[n2.nodeId]) 
							{
								if(n2_==n1 || removedVerticesSmallSet.Contains(n2_.nodeId)||removedVertices_W_is0.Contains(n2_.nodeId)) 
									continue;
								bluelines.Add(n1.nodeId+"+"+n2_.nodeId);
								temp.Add(new float[]{n1.nodeId,n2_.nodeId,n2_.weight});
							}
							foreach(var t in temp){
								graphObj.addEdge((int)t[0],(int)t[1]);
							}

							//will leaD to problems in the loop outside as it will remove the node from the list of undirectedEdges
							//graphObj.removeNode(n2.y);
							removedVertices_W_is0.Add(n2.nodeId);
							notRemovedVertices_W_is0.Add(n1.nodeId);
//							udl(Vtostring(vertices[n1.y])+" merged with \n"+Vtostring(vertices[n2.y]));

						}
						else{
							redlines.Add(n1.nodeId+"+"+n2.nodeId);
							graphObj.addEdge(n1.nodeId,n2.nodeId);
						}
					}
				}
			}
			foreach(var n2_ in removedVertices_W_is0)
				graphObj.removeNode(n2_,ref bluelines, ref redlines);
			graphObj.removeNode(v,ref bluelines, ref redlines);
			return removedVertices_W_is0;
		}

		/// <summary>
		/// Removes corners from medial mesh.. corners that are V shaped or dual layered meshes, creating 
		/// unneccessary complications in the medial mesh. They need to be removed.
		/// </summary>
		public void removeVs_Random(){
			redlines= new HashSet<string>();
			bluelines= new HashSet<string>();
			HashSet<edgenode> adjacentnodes;
			HashSet<int>removedVertices= new HashSet<int>(), removedVerticesSmallSet= new HashSet<int>();
			HashSet<int>vertexConsidered=new HashSet<int>();
			List<int> verticesTobeConsidered= new List<int>();
			bool breakflag=false, f=false;
			int j;
			for(int i=0; i<this.vertices.Count/3;i++){
				if(verticesTobeConsidered.Count>0){
					j=verticesTobeConsidered[0];
					verticesTobeConsidered.RemoveAt(0);
					i--;
					if(vertexConsidered.Contains(j))
						continue;
				}
				else
				{
					///keep this hashset to consider those nodes as postential V nodes cases, who were remnents of merging process
					/// due to W=0
					HashSet<int> notRemovedVertices_W_is0= new HashSet<int>();

					if(removedVerticesSmallSet.Count>0){

						///update the graph now, as no more neighbours can be detected as vertex to be removed
						removedVertices.UnionWith(updateGraph(removedVerticesSmallSet, ref notRemovedVertices_W_is0));
						removedVerticesSmallSet= new HashSet<int>();
						verticesTobeConsidered.AddRange(notRemovedVertices_W_is0);
						i--; continue;
					}
					else{
						j= UnityEngine.Random.Range(0,this.vertices.Count-1);
						if(vertexConsidered.Contains(j))
							{i--;continue;}
					}
				}
				
				

				vertexConsidered.Add(j);

				var v=vertices[j];
				
				adjacentnodes= graphObj.unDirectedEdges[j];
				
				//check angles between two adjacent nodes. 
				//if they are in same angle in y direction, but different in y plane...leave them.. they not forming a corner
				
				//if they are in same angle in y direction, and in ~ same angle in y plane... cut this node off.
				
				//break the loop from searching for other neighbours
				breakflag=false;
				
				foreach(var neighbour1 in adjacentnodes){
					foreach(var neighbour2 in adjacentnodes){
						if(neighbour1.nodeId>= neighbour2.nodeId)
							continue;
						
						if(graphObj.unDirectedEdges[neighbour1.nodeId].Contains(neighbour2))
							continue;
						
						var ay=angleY(vertices[neighbour1.nodeId],vertices[neighbour2.nodeId],v);
						var ayp=angleYPlane(vertices[neighbour1.nodeId],vertices[neighbour2.nodeId],v);
						if(ay==0 && ayp ==0)
						{	continue;
						}
						if(ay<42.0f && ayp <7.0f)
						{	
							breakflag=true;

							//add neighbours to the verticesTobeConsidered
							foreach(var temp in adjacentnodes)
								verticesTobeConsidered.Add(temp.nodeId);


							removedVertices.Add(j);
							removedVerticesSmallSet.Add(j);
							break;
						}
					}
					if(breakflag)
						break;
				}
			}
//			udl ("detected nodes");
			
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

			///Final touch 1
			foreach(var rnode in removedVertices){
				graphObj.removeNode(rnode,ref bluelines, ref redlines);

			}
			///Final touch 2: while drawing lines
			foreach (var line in bluelines) 
			{
				var ns= line.Split('+');
				if(removedVertices.Contains(int.Parse(ns[0])) ||removedVertices.Contains(int.Parse(ns[1]))){
			    	graphObj.removeEdge(int.Parse(ns[0]),int.Parse(ns[1]));
				}
				else
					Debug.DrawLine(vertices[int.Parse(ns[0]) ],vertices[int.Parse(ns[1])], Color.blue,100000);
			}
			foreach (var line in redlines) 
			{
				var ns= line.Split('+');
				if(removedVertices.Contains(int.Parse(ns[0])) ||removedVertices.Contains(int.Parse(ns[1]))){
					graphObj.removeEdge(int.Parse(ns[0]),int.Parse(ns[1]));
				}
				else
					Debug.DrawLine(vertices[int.Parse(ns[0])],vertices[int.Parse(ns[1])], Color.red,100000);
			}
			
			this.removedVertices_global=removedVertices;
		}

		public void removeVs_Linear(){
			HashSet<edgenode> adjacentnodes;
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
						if(neighbour1.nodeId>= neighbour2.nodeId)
							continue;

						if(graphObj.unDirectedEdges[neighbour1.nodeId].Contains(neighbour2))
							continue;

						var ay=angleY(vertices[neighbour1.nodeId],vertices[neighbour2.nodeId],v);
						var ayp=angleYPlane(vertices[neighbour1.nodeId],vertices[neighbour2.nodeId],v);
						if(ay==0 && ayp ==0)
						{	continue;
						}
						if(ay<42 && ayp <7)
						{	
							breakflag=true;
//							Debug.DrawLine(vertices[neighbour1.y],vertices[neighbour2.y], Color.magenta,100000);
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
			int x,y,z;
			for(int i=0;i < l;i+=3){
				if(removedVertices.Contains(this.triangles[i]) ||removedVertices.Contains(this.triangles[i+1])
				   ||removedVertices.Contains(this.triangles[i+2])){
					//Graph can't be updated this way, because you don't know if some other triangle
					//is creating an edge between these two vertices or not
					//also update the graph 
//					graphObj.removeEdge(this.triangles[i],this.triangles[i+1]);
//					graphObj.removeEdge(this.triangles[i],this.triangles[i+2]);
//					graphObj.removeEdge(this.triangles[i+2],this.triangles[i+1]);

					join_neighbours(this.triangles[i],this.triangles[i+1],this.triangles[i+2], removedVertices);
					this.triangles.RemoveAt(i+2);
					this.triangles.RemoveAt(i+1);
					this.triangles.RemoveAt(i);
					l=this.triangles.Count;
					i-=3;
				}
			}




			// UPDATE: use removeVs_Random


//			foreach(var vertex in removedVertices){
//				join_neighbours(vertex,removedVertices);
//			}
//
//			//The graph also needs to be updated--- graph needs to be created again
//			//recreate graph
//			this.graphObj= new Graph(this.vertices.Count);
//			createGraph();

			layMesh();


			this.removedVertices_global=removedVertices;

		}
		void join_neighbours(int a,int b, int c,HashSet<int>removedVertices){
			int v;
			if(removedVertices.Contains(a)){
				v= a;}
			else{ 
				if(removedVertices.Contains(b)){
					v=b;}
				else{
					v=c;
				}
			}
			HashSet<edgenode> adjacentnodes= graphObj.unDirectedEdges[v];
			foreach(var n1 in adjacentnodes){
				foreach(var n2 in adjacentnodes){
					if(n1.Equals(n2)|| removedVertices.Contains(n1.nodeId)|| removedVertices.Contains(n2.nodeId))
						continue;
					//check if there isn't an edge already between n1 and n2
					if(!graphObj.unDirectedEdges[n1.nodeId].Contains(n2)){
						Debug.DrawLine(vertices[n1.nodeId],vertices[n2.nodeId], Color.magenta,100000);
					}
				}
			}
		
		}


		//failures can be removes by adding edges across a quadrilateral's diagonal vertices
		public void removeVs_failures(){
			HashSet<edgenode> adjacentnodes;
			
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
				if(Mathf.Round(v.x*100)==4000f && Mathf.Round(v.z*100)==-3900f)
					f=true;

				foreach(var neighbour1 in adjacentnodes){

					foreach(var neighbour2 in adjacentnodes){

						if(neighbour1.nodeId>= neighbour2.nodeId){
							continue;
						}
						//						if(vertices[neighbour1.y]== vertices[ neighbour2.y])
						//							continue;
						if(graphObj.unDirectedEdges[neighbour1.nodeId].Contains(neighbour2)
						   ){
							
							continue;
							}
						
						var ay=angleY(vertices[neighbour1.nodeId],vertices[neighbour2.nodeId],v);
						var ayp=angleYPlane(vertices[neighbour1.nodeId],vertices[neighbour2.nodeId],v);
						
						if((ay==0 && ayp ==0)||(ay==90) ||ayp==90)
						{	continue;
						}
						if(f||(ay<42.0f && ay>1.0f && ayp <25.0f)){
							if(!f)
								breakflag=true;
							Debug.DrawLine(vertices[neighbour1.nodeId],v, Color.magenta,100000);
							Debug.DrawLine(vertices[neighbour2.nodeId],v,Color.blue,100000);
			
							var go2= GameObject.CreatePrimitive(PrimitiveType.Capsule);
							go2.transform.localScale= new Vector3(0.3f,0.3f,0.3f);
							go2.transform.position=v;
							go2.name= "Grey "+v.x+","+v.y+","+v.z +" i="+i;
							go2.gameObject.GetComponent<Renderer>().material.color= Color.cyan;

							var go= GameObject.CreatePrimitive(PrimitiveType.Sphere);
							go.transform.localScale= new Vector3(0.38f,0.38f,0.38f);
							go.transform.position=vertices[neighbour1.nodeId];
							go.gameObject.GetComponent<Renderer>().material.color =Color.magenta;
							go.transform.parent=go2.transform;
							go.name=Vtostring(vertices[neighbour1.nodeId])+"Mag "+neighbour1.nodeId+" "+"ayp="+ayp+" ay="+ay;
							var go1= GameObject.CreatePrimitive(PrimitiveType.Cube);
							go1.transform.localScale= new Vector3(0.3f,0.3f,0.3f);
							go1.transform.position=vertices[neighbour2.nodeId];
							go1.gameObject.GetComponent<Renderer>().material.color =Color.yellow;
							go1.name=Vtostring(vertices[neighbour2.nodeId])+"Yel "+neighbour2.nodeId+" "+"ayp="+ayp+" ay="+ay;
							go1.transform.parent=go2.transform;

							if(!f)
								break;
							
						}
					}
					if(breakflag)
						break;
					
					
				}
			}
		}


		string Vtostring(Vector3 v){

			return String.Format(v.x+","+v.y+","+v.z);
		}

		float angleY(Vector3 a, Vector3 b, Vector3 p){
			return Vector3.Angle(Vector3.ProjectOnPlane(a-p,Vector3.up),Vector3.ProjectOnPlane(b-p,Vector3.up));
		}

		float angleYPlane(Vector3 a, Vector3 b, Vector3 p){
			var prjtOnXZ= new Vector3(a.x,p.y,a.z)-p;
			Vector3 normalOfPlanePerpendicularToXZContainingA;
			if(prjtOnXZ!=Vector3.zero)
				normalOfPlanePerpendicularToXZContainingA= Vector3.Cross(a-p,prjtOnXZ);
			else
				normalOfPlanePerpendicularToXZContainingA= new Vector3(p.x+1.0f,p.y,p.z);
			var prjtOfB_OnPlane= Vector3.ProjectOnPlane(b-p,normalOfPlanePerpendicularToXZContainingA);
			return Vector3.Angle(a-p,prjtOfB_OnPlane);
		}

		#endregion 
		#endregion 

		#region Add random edges

		/// <summary>
		/// Add edges from-to random vertices in medial skeleton, not colliding with Arena
		/// </summary>
		public void addEdgesInsideTheArena(){

			int r= UnityEngine.Random.Range(0,graphObj.nvertices-1);
			int s=UnityEngine.Random.Range(0,graphObj.nvertices-1);
			RaycastHit obstacleHit;
			bool hit, rcontainss,scontainsr,hitbox;
			//This edge is possible as it doesn't collide with Box, and isn't already an edge
			bool eligibleEdge=false;
	//		hit= Physics.Raycast(v[r],v[s]-v[r],out obstacleHit,Mathf.Infinity);//Vector3.Distance(v[r],v[s]));
	//		rcontainss= g.edges[r]!=null? g.edges[r].Contains( new edgenode(s,0)):true;
	//		scontainsr= g.edges[s]!=null? g.edges[s].Contains(new edgenode(r,0)):true ;
	//		hitbox= hit ? !obstacleHit.transform.gameObject.name.Equals("Map"):false;
	//		comboflag= (r==s|| rcontainss||scontainsr|| hitbox );
			int loop=10000;

			while(loop >0){

					
				r= UnityEngine.Random.Range(0,graphObj.nvertices-1);
				s=UnityEngine.Random.Range(0,graphObj.nvertices-1);
				if(vertices[r].y==vertices[s].y || this.removedVertices_global.Contains(r) || this.removedVertices_global.Contains(s)){
					loop--;
					continue;
				}
				hit= Physics.Raycast(vertices[r],vertices[s]-vertices[r],out obstacleHit,Mathf.Infinity);
				rcontainss= graphObj.directedEdges[r]!=null? graphObj.directedEdges[r].Contains( new edgenode(s,0)):false;
				scontainsr= graphObj.directedEdges[s]!=null? graphObj.directedEdges[s].Contains(new edgenode(r,0)):false;
				hitbox= hit ? obstacleHit.transform.name.Contains("Box"):false;
				eligibleEdge= !(rcontainss||scontainsr|| hitbox );
				if(eligibleEdge)
				{
					graphObj.addEdge(r,s);
					loop--;
				}
			}
		}

		public void addEdgesInsideTheSkeletonBody(){
			
			int r= UnityEngine.Random.Range(0,graphObj.nvertices-1);
			int s=UnityEngine.Random.Range(0,graphObj.nvertices-1);
			RaycastHit obstacleHit;
			bool hit, rcontainss,scontainsr,hitbox;
			//This edge is possible as it doesn't collide with Box, and isn't already an edge
			bool eligibleEdge=false;
			//		hit= Physics.Raycast(v[r],v[s]-v[r],out obstacleHit,Mathf.Infinity);//Vector3.Distance(v[r],v[s]));
			//		rcontainss= g.edges[r]!=null? g.edges[r].Contains( new edgenode(s,0)):true;
			//		scontainsr= g.edges[s]!=null? g.edges[s].Contains(new edgenode(r,0)):true ;
			//		hitbox= hit ? !obstacleHit.transform.gameObject.name.Equals("Map"):false;
			//		comboflag= (r==s|| rcontainss||scontainsr|| hitbox );
			int loop=10000;
			
			while(loop >0){
				
				
				r= UnityEngine.Random.Range(0,graphObj.nvertices-1);
				s=UnityEngine.Random.Range(0,graphObj.nvertices-1);
				if(vertices[r].y==vertices[s].y || this.removedVertices_global.Contains(r) || this.removedVertices_global.Contains(s)){
					loop--;
					continue;
				}
				hit= Physics.Raycast(vertices[r],vertices[s]-vertices[r],out obstacleHit,Mathf.Infinity);
				rcontainss= graphObj.directedEdges[r]!=null? graphObj.directedEdges[r].Contains( new edgenode(s,0)):false;
				scontainsr= graphObj.directedEdges[s]!=null? graphObj.directedEdges[s].Contains(new edgenode(r,0)):false;
				hitbox= hit ? obstacleHit.transform.name.Contains("Medial"):false;
				eligibleEdge= !(vertices[r].y==vertices[s].y|| rcontainss||scontainsr|| hitbox );
				if(eligibleEdge)
				{
					graphObj.addEdge(r,s);
					loop--;
				}
			}
		}
		#endregion

		private HashSet<int> treeSearchOnebyOnebyOne(Vector3 v, HashSet<int> foundNodes){
			float r= 1f;
			while(foundNodes.Count==0){
				foundNodes= tree.GetValues(v.x-r,v.y-r,v.z-r,v.x+r,v.y+r,v.z+r,new HashSet<int>());
				r++;
			}
//			udl (r);
			return foundNodes;
		}

		#region Path Finding

		public void findNearests(Vector3 start,Vector3 end){
			float min=-1,diff;
			int minvertex=-1;

			HashSet<int> foundNodes= new HashSet<int>();
			var v= start;
			float r;
			for(r=1f;foundNodes.Count==0;){
				foundNodes= tree.GetValues(v.x-r,v.y,v.z-r,v.x+r,v.y+r,v.z+r,new HashSet<int>());
				r++;
			}
			
			foreach(var i in foundNodes){
				diff=Vector3.Distance(vertices[i],start);
				if(min==-1 || diff<min){
					min=diff;
					minvertex=i;
				}
			}

			startNearestNode=minvertex;
			v=end;
			endNearestNodes= new List<int>();
			for(float y=arena.getMinY2(); y< arena.getMaxY2()-1; y++){
				foundNodes= new HashSet<int>();
				for(r=1f ;foundNodes.Count==0;){
					foundNodes= tree.GetValues(v.x-r, y, v.z-r,
					                           v.x+r, y+1 ,v.z+r,new HashSet<int>());
					r++;
				}
				endNearestNodes.AddRange(foundNodes);
			}
			var go2= new GameObject();
			go2.name=  "endnearest";
			foreach(var i in endNearestNodes){
				var go= GameObject.CreatePrimitive(PrimitiveType.Sphere);
				go.transform.localScale= new Vector3(0.3f,0.3f,0.3f);
				go.transform.position=vertices[i];
				go.name= "end";
				go.transform.parent=go.transform;
			}
		}

		public void findPaths(){

			//holds the visited node
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

			paths= new List<List<int>>(endNearestNodes.Count);
			List<int> path;
			
			///do this for every endNearestNode
			foreach(var endNearestNode in endNearestNodes){

				//create reverse path
				path = new List<int>();
				path.Insert(0,endNearestNode);
				try{
					for(int i=endNearestNode; i!=startNearestNode;i=(int)backtrack[i]){
						path.Insert(0,(int)backtrack[i]);
						}
				}
				catch(NullReferenceException){
					udl ("Path not found for this endNode: "+vertices[endNearestNode]);
					continue;
				}
				paths.Add(path);
			}
			udl ("#of endnodes"+endNearestNodes.Count);
			udl ("#of paths"+paths.Count);
		}

		public void showPath(){

			if(paths==null){
				udl ("no paths");
				return;
			}
			foreach(var path in paths){
				Color triColor= Color.red;
				for(int i=0;i<path.Count-1; i++)
					UnityEngine.Debug.DrawLine(vertices[path[i]],vertices[path[i+1]],triColor,2000,false);
//				udl ("nodes in path= "+path.Count);
			}
		}


		//to be invoked from update
		private int currentpath_i=0;

		public Vector3 movePlayer(float t){
			if(t<vertices[paths[0][0]].y)
				return Vector3.zero;

			while(currentpath_i<paths[0].Count
			      && t>vertices[paths[0][currentpath_i]].y)
				currentpath_i++;

			if(currentpath_i>=paths[0].Count)
				return -Vector3.one;
			float frac1=t- vertices[paths[0][currentpath_i-1]].y, frac2= vertices[paths[0][currentpath_i]].y -t;
			var playerpos= (vertices[paths[0][currentpath_i-1]]*frac2 + vertices[paths[0][currentpath_i]] *frac1)/(frac1+frac2);
			return playerpos;
		}

		public static void udl(object s){
			UnityEngine.Debug.Log(s);
		}

		#endregion

	}


	class edgenode:IEquatable<edgenode> {
		public int nodeId;
		public float weight;
		public edgenode(int nodeid, float weight){
			this.nodeId=nodeid;
			this.weight=weight;
		}
		public override bool Equals(System.Object e){
			return e!=null && this.nodeId==((edgenode)e).nodeId;
		}
		public bool Equals(edgenode e){
			return e!=null && this.nodeId==e.nodeId;
		}
		public override int GetHashCode ()
		{
			return this.nodeId;
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

