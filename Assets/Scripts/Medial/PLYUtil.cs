using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
namespace Medial{
	public class PLYUtil {

		/// <summary>
		/// Writes the vertices and triangles to PLY2 format file
		/// The file can then be used to run Skeleton
		/// </summary>
		/// <param name="outputfile">Outputfile.</param>
		/// <param name="ply_vertices">Ply_vertices.</param>
		/// <param name="ply_triangles">Ply_triangles.</param>
		public static void writePLY(string outputfile,List<Vector3> ply_vertices, List<int> ply_triangles){
			using (System.IO.StreamWriter file= new System.IO.StreamWriter(outputfile)){
				file.WriteLine(ply_vertices.Count);
				file.WriteLine(ply_triangles.Count/3);
				foreach(Vector3 vertex in ply_vertices){
					file.WriteLine(vertex.x+" "+vertex.y+" "+vertex.z);
				}
				for(int ii=0;ii<ply_triangles.Count;ii=ii+3){
					file.WriteLine("3 " +ply_triangles[ii]+" "+ply_triangles[ii+1]+" "+ply_triangles[ii+2]);
				}
				file.Close();
			}
		}
		static void udl(object str){
			UnityEngine.Debug.Log(str);
		}
		
		/// <summary>
		/// Obtains vertices and triangles required in the PLY2 format 
		/// from list of layers and list of closed polygons.
		/// Used to apply medial skeleton algo and also create mesh
		/// </summary>
		/// <param name="layers">Layers.</param>
		/// <param name="polygons">Polygons.</param>
		public static List<VertTria> assignPLY(List<List<Vector3>> layers, List<List<int>> polygons, List<int[]>[]covers,
       		List<int> mappingOriginalIndexToNewIndexOfPolygons){
	
			//a List of vertices for each polygon, so that we can draw different meshes for different polygons
			//the first list however is the whole set of all the vertices in the given object, used to find medial skeleton
			//plus two more for top and bottom cover
			List<List<Vector3>> ply_vertices=new List<List<Vector3>>(1+polygons.Count+2);
			//similarly with triangle list
			List<List<int>> ply_triangles=new List<List<int>>(1+polygons.Count+2);
			
			for(int i=0;i<1+polygons.Count+2;i++){
				ply_vertices.Add( new List<Vector3>());
				ply_triangles.Add(new List<int>());
			}
			int iilayer=0;

			foreach(var layer in layers){
				ply_vertices[0].AddRange(layer);
				int n=0;
				int ipolygon=0;
				foreach(var polygon in polygons){
					ipolygon++;
	//				udl ("ipolygon"+ipolygon+" this polygon# "+polygon.Count +" layer range="+layer.Count);
	//				udl ("n-n1 "+ n+"-"+(n+polygon.Count));
					ply_vertices[ipolygon].AddRange(layer.GetRange(n,polygon.Count));
					n+=polygon.Count;
				}
	//			udl ("ilayer ="+iilayer+++" vertices total till now= "+n); 
			}
	//		udl ("vertices in polygon 1=" +ply_vertices[1].Count);
	//		udl ("vertices in polygon 2=" +ply_vertices[2].Count);

			int nn=layers[0].Count;
			for(int ilayer=0;ilayer<layers.Count-1;ilayer++){
				int vertices_yet=0;
				//for each layer, add the triangulations involved to ply_traingles
				int i=0;
				int ipolygon=0;
				foreach(var polygon in polygons){
					//number of vertices in the polygon
					int n=polygon.Count;
					for(; i<n+vertices_yet;i++){
						ply_triangles[0].AddRange(new List<int>{((i%n)+nn*ilayer+vertices_yet),
							(((i+1)%n)+nn*ilayer+vertices_yet),((i%n)+nn*(1+ilayer)+vertices_yet),
							((i%n)+nn*(1+ilayer)+vertices_yet),
							(((i+1)%n)+nn*ilayer+vertices_yet),(((i+1)%n)+nn*(1+ilayer)+vertices_yet)
						});
						
					}
					for(int pi=0; pi<n;pi++)
					{
						ply_triangles[ipolygon+1].AddRange(new List<int>{(pi%n)+n*ilayer,(((pi+1)%n)+n*ilayer)
							,((pi%n)+n*(1+ilayer)),((pi%n)+n*(1+ilayer))
							,(((pi+1)%n)+n*ilayer),(((pi+1)%n)+n*(1+ilayer)) });
					}
					vertices_yet+=n;
					ipolygon++;
				}
			}
			
			//starting point of vertex indices in last layer
			int vertices_lastlayer=nn*(layers.Count-1);
			//now add triangles for upper and lower cover using covers[]
			//we will later subdivide each triangle into smaller triangles
			ply_vertices[polygons.Count+1].AddRange(layers[0]);
			ply_vertices[polygons.Count+2].AddRange(layers.Last());
			
			
			for(int i=0; i<2;i++){
				
				foreach(var coverTrianglesIndex in covers[i]){
					//series in 0-7 
					// 9
					//			10
					// 8
					//see paper
					if(i==1){
						ply_triangles[0].AddRange(new List<int>{mappingOriginalIndexToNewIndexOfPolygons[coverTrianglesIndex[0]]+vertices_lastlayer
							,mappingOriginalIndexToNewIndexOfPolygons[coverTrianglesIndex[1]]+vertices_lastlayer
							,mappingOriginalIndexToNewIndexOfPolygons[coverTrianglesIndex[2]]+vertices_lastlayer});
						
						ply_triangles[polygons.Count+1+i].AddRange(new List<int>{mappingOriginalIndexToNewIndexOfPolygons[coverTrianglesIndex[0]]
							,mappingOriginalIndexToNewIndexOfPolygons[coverTrianglesIndex[1]]
							,mappingOriginalIndexToNewIndexOfPolygons[coverTrianglesIndex[2]]});
						
						
					}
					if(i==0){
						
						ply_triangles[0].AddRange(new List<int>{mappingOriginalIndexToNewIndexOfPolygons[coverTrianglesIndex[0]]
							,mappingOriginalIndexToNewIndexOfPolygons[coverTrianglesIndex[1]]
							, mappingOriginalIndexToNewIndexOfPolygons[coverTrianglesIndex[2]]});
						
						ply_triangles[polygons.Count+1+i].AddRange(new List<int>{mappingOriginalIndexToNewIndexOfPolygons[coverTrianglesIndex[0]]
							,mappingOriginalIndexToNewIndexOfPolygons[coverTrianglesIndex[1]]
							,mappingOriginalIndexToNewIndexOfPolygons[coverTrianglesIndex[2]]});
						
					}
					
				}
			}
			
			List<VertTria> list_of_vertria=new List<VertTria>();
			for(int i=0; i<1+polygons.Count+2;i++)
				list_of_vertria.Add(new VertTria(ply_vertices[i],ply_triangles[i]));
			return list_of_vertria;
		}

		public static void buildplyObject(GameObject go,List<Vector3> ply_vertices, List<int> ply_triangles){
			List <Vector3> newVertices;
			List <int> newTriangles;
	//		udl (ply_vertices.Count);
	//		udl ("tri"+ ply_triangles.Count);
			int nvertices = Convert.ToInt32(ply_vertices.Count) * 2;
			int ntriangles = Convert.ToInt32(ply_triangles.Count) *2;
			newVertices = new List<Vector3>(nvertices);
			newTriangles =  new List<int>(ntriangles);

			
			MeshFilter ms = go.GetComponent <MeshFilter> ();
			Mesh mesh = new Mesh ();
			ms.mesh = mesh;
			
			
			
			newVertices.AddRange(ply_vertices.AsEnumerable());
			for(int i=0;i < nvertices/2; i++){
				newVertices.Add (newVertices[i]);
			}
			newTriangles.AddRange(ply_triangles.AsEnumerable());
			//udl(newTriangles.Count);
			for (int i=0; i<ntriangles/2; i++) {
				//udl (" -- "+(ntriangles/2-i-1));
				newTriangles.Add(newTriangles[ntriangles/2-i-1]);
				i++;
				newTriangles.Add(newTriangles[ntriangles/2-i-1]);
				i++;
				newTriangles.Add(newTriangles[ntriangles/2-i-1]);
			}
			
			int k = 0;
			mesh.vertices = newVertices.ToArray();
			
			List<Vector3> l = Enumerable.Repeat (Vector3.up, nvertices/2).ToList();
			l.AddRange(Enumerable.Repeat(Vector3.down,nvertices/2).ToList());

			mesh.triangles = newTriangles.ToArray();


			//assign colors to vertices
			Color[] colors = new Color[newVertices.Count];
			Color triColor = Color.blue;
			for (int i = 0; i < newTriangles.Count; i++)
			{
				int vertIndex = newTriangles[i];
	//			if (i % 3 == 0)
	//				triColor = getColor(newVertices[vertIndex]);
				colors[vertIndex] = triColor;
			}
			mesh.colors=colors;
			mesh.normals = l.ToArray();
		}

		static Color getColor(Vector3 v){
	//		float r=Vector2.Distance(new Vector2(v.x,v.z),new Vector2(0f,2f));
	//		return new Color(216f/255,(v.y/21f),96f/255,150f/255);

			return new Color(UnityEngine.Random.Range(0f,1f),(v.y)/21f,UnityEngine.Random.Range(0f,1f),150f/255f);
		}
		
	}

	public class VertTria{
		List<Vector3> ply_vertices;
		List<int> ply_triangles;
		public VertTria(List <Vector3> vertices, List<int>triangles){
			this.ply_vertices=vertices;
			this.ply_triangles=triangles;
		}
		public List<Vector3> getVertices(){
			return this.ply_vertices;
		}
		public List<int> getTriangles(){
			return this.ply_triangles;
		}
	}
}