using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace Medial{
	public class Covers 
	{
		List <List<Vector3>> layers;
		List<List<int>> polygons;
		public Covers (List <List<Vector3>> layers,List<List<int>> polygons)
		{
			this.layers=layers;
			this.polygons=polygons;
		}


		/// <summary>
		/// Finds the cover triangles.
		/// </summary>
		/// <returns>{bottomlayer,upperlayer}.</returns>
		public List<int[]>[] triangulateCovers(ArenasGenerator agen){
			
			List<int[]>[] ret= new List<int[]>[]{new List<int[]>{}, new List<int[]>{}};

			///the lines that form the polygons of the arena
			List<Ligne> lines ;

			//the lines that will be added to form triangulations
			List<Ligne> newlines;

			//hashtable of vertex to line. will also contain mappings to newlines generated
			Hashtable vertexToLine;

			//adjacency matrix. will also contain adjacencies born due to new lines generated
			bool [,]adjacency;

			Ligne addline, newline;
			HashSet<Triangle> newtriangulations;

			///for finding delauney triangulations, find the two triangles related to a new line.
			Hashtable newlinesToTriangle=  new Hashtable();

			List<Ligne> temp2lines;
			
			for(int layeri=0;layeri<2; layeri++){
				lines =new List<Ligne>();
				newlines= new List<Ligne>();
				var layer= layers[layeri*(layers.Count-1)];
				

				float  maxRadii;

				///find the maximum raddi in which the arena can be contained
				/// used to find all possible intersections from the midpoint of a line to a point at radii distance
				/// and check if all intersection are even=> point if outside arena, or odd => point is inside arena
				/// so, to check if the newly created line is inside or outside
				maxRadii= Mathf.Max (agen.getMaxX()-agen.getMinX(),agen.getMaxZ()-agen.getMinZ()) * 2;

				int r, s;
				vertexToLine= new Hashtable();
				adjacency= new bool[layer.Count,layer.Count];

				for(int i=0; i<layer.Count;i++)
					for(int j=0; j<layer.Count;j++)
						adjacency[i,j]=false;

				///creating adjacency matrix and vertexToLine hashtable on currently present lines of the polygons
				foreach(var polygon in polygons){
					for(int i=0; i<polygon.Count;i++){
						r= polygon[i]; s= polygon[(i+1)%polygon.Count];
						adjacency[r,s]=true;
						adjacency[s,r]=true;
						
						addline=new Ligne(layer[r],layer[s],r,s);
						lines.Add(addline);
						if(vertexToLine[layer[r]]!=null) 
							((List<Ligne>)vertexToLine[layer[r]]).Add(addline);
						else
							vertexToLine.Add(layer[r],new List<Ligne>{addline});
						if(vertexToLine[layer[s]]!=null) 
							((List<Ligne>)vertexToLine[layer[s]]).Add(addline);
						else
							vertexToLine.Add(layer[s],new List<Ligne>{addline});
					}
				}

				///form lines that create triangulations
				for(int linei=0; linei< lines.Count; linei++){
					var line =lines[linei];
					
					var v1= line.vertex[0];
					var AdjLines= (List<Ligne>)vertexToLine[v1];
					foreach(var otherline in lines){
						if(line==otherline)
							continue;
						
						var v2= otherline.vertex[0];
						//check if v2 is not adjacent to v1
						if(adjacency[line.vertexIndex[0],otherline.vertexIndex[0]])
							continue;
						newline= new Ligne(v1,v2,line.vertexIndex[0],otherline.vertexIndex[0]);
//						bool flag=true;
//						foreach(var adjline in AdjLines)
//						{
//							if(newline.Equals(adjline))
//							{	flag=false;
//								break;
//							}
//						}
//						if(!flag)
//							continue;
						
						if(intersecting(newline,lines, newlines))
							continue;
						if(outside(newline,lines,maxRadii))
							continue;

						/// all test passed, welcome the newline
						newlines.Add(newline);

						///make changes to adjacency matrix too, and to vertexToLine
						adjacency[line.vertexIndex[0],otherline.vertexIndex[0]]=true;
						adjacency[otherline.vertexIndex[0],line.vertexIndex[0]]=true;
						((List<Ligne>)vertexToLine[v1]).Add(newline);
						((List<Ligne>)vertexToLine[v2]).Add(newline);

						///this hashtable will be created later while finding new triangles below
						/// and will be used to ensure delauney triangulations
						newlinesToTriangle.Add(newline,new HashSet<Triangle>());
					}
				}
				
				//find new triangulations thus created
				newtriangulations= new HashSet<Triangle>();

				foreach(var line in newlines){
					var v1= line.vertexIndex[0]; var v2= line.vertexIndex[1];
					var v3list= commonVertex(v1,v2,adjacency);
					if(v3list.Count==0)
						continue;
					foreach( var v3 in v3list){
						if(!validTriangulation(v1,v2,v3,layer))
							continue;
						
						Triangle t= new Triangle(new List<int>{v1,v2,v3});
						///find the two lines associated with v3 and two vertices of line
						temp2lines=getLinesfromCommonVertex(line,layer[v3],vertexToLine);
						t.setLines(line,temp2lines[0],temp2lines[1]);
						((HashSet<Triangle>)newlinesToTriangle[line]).Add(t);
						if(!newtriangulations.Contains(t)){

							newtriangulations.Add(t);

						}
					}
				}
				
				//make triangulations delauney.
				makeDelauney(newlines,layer,newlinesToTriangle, newtriangulations);
				makeDelauney(newlines,layer,newlinesToTriangle, newtriangulations);
				
				
				//make vertices in triangle counterclockwise, then add the triangulation
				
				foreach(var t in newtriangulations){
					var a=t.GetVertices();
//										var go= GameObject.CreatePrimitive(PrimitiveType.Sphere);
//										go.transform.localScale= new Vector3(0.3f,0.3f,0.3f);
//										go.transform.position=(layer[a[0]]+layer[a[1]]+layer[a[2]])/3;
					
					if(Ligne.CounterClockWise(layer[a[0]],layer[a[1]],layer[a[2]]))
					{
						ret[layeri].Add(a.ToArray());
					}
					else
					{
						ret[layeri].Add(new int[]{a[0],a[2],a[1]});
					}
				}
			}
			return ret;
		}
		
		class Triangle: IEquatable<Triangle>{
			int a,b,c;
			List<Ligne> l;

			public Triangle(List<int> list){
				int []arr= list.ToArray();
				Array.Sort(arr);
				a=arr[0];b=arr[1];c=arr[2];
			}

			public void setLines(Ligne l1,Ligne l2, Ligne l3){
				this.l= new List<Ligne>{l1,l2,l3};
			}

			public List<Ligne> getLines(){
				return l;
			}

			public Ligne getSidedLine(Ligne line,int v){
				if(l[0]==line)
					return l[1].ContainsVertex(v)?l[1]:l[2];
				if(l[1]==line)
					return l[0].ContainsVertex(v)?l[0]:l[2];
				return l[0].ContainsVertex(v)?l[0]:l[1];

			}

			public override int GetHashCode ()
			{
				int []arr = new int[]{a,b,c};
				Array.Sort(arr);
				return arr[0]*100+arr[1]*10+arr[2]*1;
			}

			public override bool Equals(System.Object e){
				return e!=null && this.a==((Triangle)e).a && this.b==((Triangle)e).b && this.c==((Triangle)e).c ;
			}

			public bool Equals(Triangle e){
				return e!=null && this.a==e.a && this.b==e.b && this.c==e.c ;
			}

			public List<int> GetVertices(){
				return new List<int>{a,b,c};
			}

			public int GetThirdVertex(Ligne line){
				var v1= line.vertexIndex[0]; var v2= line.vertexIndex[1];
				return a!=v1 &&a!=v2? a: (b!=v1 && b!= v2? b:c);
			}

			public float GetOppositeAngle( Ligne line, List<Vector3> layer){
				int third;
				int one=line.vertexIndex[0], sec= line.vertexIndex[1]; 

				third= GetThirdVertex(line);

				Vector2 p = new Vector2(layer[third].x - layer[one].x, layer[third].z - layer[one].z),
				q = new Vector2(layer[third].x - layer[sec].x, layer[third].z - layer[sec].z);
				return Mathf.Acos(Vector2.Dot(p,q)/(q.magnitude*p.magnitude))*Mathf.Rad2Deg;

			}
		}

		void makeDelauney(List<Ligne> newlines, List<Vector3> layer, Hashtable newlinesToTriangle, 
		                  HashSet<Triangle> newtriangulations){

			///get the two triangles associated with each newline, then check the angles across the adjacent sides
			///if angle >180, remove this line, and add another line from opposite vertices.
			for(int i=0; i<newlines.Count; i++){
				var line= newlines[i];
				//										udl (line+ "   ----  "+ ((HashSet<Triangle>)newlinesToTriangle[line]).Count);
				Triangle t1= ((HashSet<Triangle>)newlinesToTriangle[line]).First();
				Triangle t2= ((HashSet<Triangle>)newlinesToTriangle[line]).Last();

				//assume a-c is the line
				int a=line.vertexIndex[0], c= line.vertexIndex[1];
				int b= t1.GetThirdVertex(line), d= t2.GetThirdVertex(line);

				float t1angle= t1.GetOppositeAngle(line,layer)
					, t2angle= t2.GetOppositeAngle(line,layer);

				if(t1angle+t2angle >180)
				{
					//the new triangles
					Triangle ta= new Triangle(new List<int>{a,b,d}),
					tc= new Triangle(new List<int>{c,b,d});

					//the new line
					Ligne bd= new Ligne(layer[b],layer[d],b,d);
					
					Ligne t1a= t1.getSidedLine(line,a) , t1c=t1.getSidedLine(line,c),
					t2a=t2.getSidedLine(line,a), t2c= t2.getSidedLine(line,c);
					ta.setLines(t1a,bd,t2a);
					tc.setLines(t1c,bd,t2c);

					//make changes to newlinestoTriangle
					newlinesToTriangle.Remove(line);

					//if t1a was a newline... and so were others?
					if(newlinesToTriangle.Contains(t1a)){
						((HashSet<Triangle>)newlinesToTriangle[t1a]).Remove(t1);
						((HashSet<Triangle>)newlinesToTriangle[t1a]).Add(ta);
					}
					if(newlinesToTriangle.Contains(t1c)){
						((HashSet<Triangle>)newlinesToTriangle[t1c]).Remove(t1);
						((HashSet<Triangle>)newlinesToTriangle[t1c]).Add(tc);
					}
					if(newlinesToTriangle.Contains(t2a)){
						((HashSet<Triangle>)newlinesToTriangle[t2a]).Remove(t2);
						((HashSet<Triangle>)newlinesToTriangle[t2a]).Add(ta);	
					}
					if(newlinesToTriangle.Contains(t2c)){
						((HashSet<Triangle>)newlinesToTriangle[t2c]).Remove(t2);
						((HashSet<Triangle>)newlinesToTriangle[t2c]).Add(tc);
					}
					newlinesToTriangle.Add(bd,new HashSet<Triangle>());
					((HashSet<Triangle>)newlinesToTriangle[bd]).Add(ta);
					((HashSet<Triangle>)newlinesToTriangle[bd]).Add(tc);
					
					//make changes to newtriangulations
					newtriangulations.Remove(t1);
					newtriangulations.Remove(t2);
					newtriangulations.Add(ta);
					newtriangulations.Add(tc);

					//remove line from newlines and add bd to new lines;
					newlines[i]=bd;
				}
			}
		}
		
		List <Ligne> getLinesfromCommonVertex(Ligne l, Vector3 v3, Hashtable vertexToLine){
			var v1=l.vertex[0];var v2= l.vertex[1];
			Ligne l1=null,l2=null;
			foreach(var line in ((List<Ligne>)vertexToLine[v3])){
				if(line.GetOther(v3)==v1)
					l1=line;
				if(line.GetOther(v3)==v2)
					l2=line;
			}
			return new List<Ligne>{l1,l2};
		}

		/// <summary>
		/// Checks if there lies no point inside the Triangluation 
		/// </summary>
		bool validTriangulation(int v1,int v2, int v3, List<Vector3> layer){
			Vector3 a=layer[v1], b=layer[v2], c=layer[v3];
			foreach(var s in layer){
				
				if(s==a ||s==b||s==c)
					continue;
				if(point_inside_trigon(s,a,b,c))
					return false;
			}
			return true;
		}
		
		bool point_inside_trigon(Vector3 s, Vector3 a, Vector3 b, Vector3 c)
		{
			float as_x = s.x-a.x;
			float as_y = s.z-a.z;
			
			bool s_ab = (b.x-a.x)*as_y-(b.z-a.z)*as_x > 0;
			
			if((c.x-a.x)*as_y-(c.z-a.z)*as_x > 0 == s_ab) return false;
			
			if((c.x-b.x)*(s.z-b.z)-(c.z-b.z)*(s.x-b.x) > 0 != s_ab) return false;
			
			return true;
		}
		
		List<int> commonVertex(int v1, int v2, bool [,] adj){
			List<int> r=new List<int>();
			for(int i=0; i<adj.GetLength(1); i++){
				if(adj[v1,i] && adj[v2,i])
					r.Add(i);
			}
			return r;
		}
		
		bool intersecting(Ligne l, List<Ligne> lines, List<Ligne> newlines){
			foreach(var line in lines){
				if(line.LineIntersection(l))
					return true;
				
			}
			foreach(var line in newlines){
				if(line.LineIntersection(l))
					return true;
				
			}
			return false;
		}

		/// <summary>
		/// Check for if line L is outside of the polygon or not.
		/// </summary>
		bool outside(Ligne l, List<Ligne>lines, float radii){
			var v1= l.MidPoint();
			var v2= l.getPointAtDAndAngleA(radii,35);
			//		UnityEngine.Debug.DrawLine(v1,v2,Color.green,5000f);
			//		udl (v2);
			var ray= new Ligne(v1,v2,0,1);
			int count=0;
			foreach(var line in lines){
				if(ray.LineIntersection(line))
					count++;
			}
			//		udl (v2+" "+count);
			if(count%2 ==0)
				return true;
			return false;
			
		}

		public static void udl(object s){
			UnityEngine.Debug.Log(s);
		}
	}
}