using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
namespace Medial{
/// <summary>
/// Modified a lot for Convex example arena. Switching over to Moving_guard_arena.cs
/// </summary>
	public class CreateLayeredMesh : MonoBehaviour {
		char[] delimiterChars = { ' ', '\t' };
		string file_prefix;//="convex";//"moving_gaurd";
		GameObject gameobj2;
		Transform cam;
		public GameObject prefab;
		private float layer_division=10f;
		string multi_triangle_input_file;
		string dir=@"/Users/dhsingh/Documents/Thesis/SM03Skeleton/";
		public Transform map;
		Vector3 mp_13, mp_13u;
		void Start () {}

		public void buildArena(int selGridInt){
			ArenasGenerator a= new ArenasGenerator(selGridInt);
			switch (selGridInt){
			case 0:file_prefix="moving_gaurd"; 
				break;
			case 1:file_prefix="convex";
				break;
			case 2:file_prefix="arena";
				break;
			}
			
			List <List<Vector3>> layers =a.getLayers();
			List<List<int>> polygons= a.getPolygons();
			List<int[]>[] covers=a.getCovers();
			mp_13=(layers[0][1]+layers[0][3])/2;
			mp_13u=(layers.Last()[1]+layers.Last()[3])/2;

			//divide each line in each layer in 10 parts horizontally
			comb combb= subdivide_layer(layers,polygons,layer_division);
			layers=combb.getLayer();
			polygons=combb.getPoly();
			
			//add 4 layers in-between every two layers.
					layers=addLayers(layers,4f);

			
			List<VertTria> vt= assignPLY(layers,polygons,covers);
			
			multi_triangle_input_file= file_prefix+".ply2";
			writePLY(dir+multi_triangle_input_file,vt[0].getVertices(),vt[0].getTriangles());

			//create 'Map' Gameobject

			foreach(Transform child in map) {
				Destroy(child);
			}
			map.transform.position.Set(0f,0f,0f);
			for(int goi=1; goi < vt.Count;goi++){
				var go=(GameObject) Instantiate(prefab);
				go.name="Box"+goi;
				go.transform.parent=map;
				buildplyObject (go,vt[goi].getVertices(),vt[goi].getTriangles());
				SetAlpha(go.GetComponent<Renderer>().material,0.9f);
			}
		}
		public void BuildMedial()
		{

			runaProcess("/Users/dhsingh/Documents/Thesis/SM03Skeleton/run.sh",multi_triangle_input_file);
			
			string medial_output=dir+"output_medial_"+multi_triangle_input_file;
			
			gameobj2 = GameObject.Find ("Medial");
			buildObject (medial_output,gameobj2);
			SetAlpha(gameobj2.GetComponent<Renderer>().material,0.6f);
			
			
			//cam = GameObject.FindGameObjectWithTag("Cam").transform;
		}


		void runaProcess(string filename,string arguments){
			ProcessStartInfo startInfo = new ProcessStartInfo()
			{
				FileName = filename,
				Arguments = arguments,
			};
			Process proc = new Process(){StartInfo = startInfo};
			proc.Start();
			proc.WaitForExit();
			proc.Close();
		}

		public static void udl(object s){
			UnityEngine.Debug.Log(s);
		}

		public static void SetAlpha (Material material, float value) {
			Color color = material.color;
			color.a = value;
			material.color = color;
		}

		/// <summary>
		/// Writes the vertices and triangles to PLY2 format file
		/// The file can then be used to run Skeleton
		/// </summary>
		/// <param name="outputfile">Outputfile.</param>
		/// <param name="ply_vertices">Ply_vertices.</param>
		/// <param name="ply_triangles">Ply_triangles.</param>
		void writePLY(string outputfile,List<Vector3> ply_vertices, List<int> ply_triangles){
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


		/// <summary>
		/// Obtains vertices and triangles required in the PLY2 format 
		/// from list of layers and list of closed polygons.
		/// Used to apply medial skeleton algo and also create mesh
		/// </summary>
		/// <param name="layers">Layers.</param>
		/// <param name="polygons">Polygons.</param>
		List<VertTria> assignPLY(List<List<Vector3>> layers, List<List<int>> polygons, List<int[]>[]covers){

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
			foreach(var layer in layers){
				ply_vertices[0].AddRange(layer);
				int n=0;
				int ipolygon=0;
				foreach(var polygon in polygons){
					ipolygon++;

					ply_vertices[ipolygon].AddRange(layer.GetRange(n,polygon.Count));
					n+=polygon.Count;
				}
			}

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

			///
			int mp13,mp13_2,
			mp13u,mp13u_2;
			ply_vertices[0].Add(mp_13); mp13= ply_vertices[0].Count-1;
			ply_vertices[polygons.Count+1].Add(mp_13);mp13_2=ply_vertices[polygons.Count+1].Count-1;

			ply_vertices[0].Add(mp_13u); mp13u= ply_vertices[0].Count-1;
			ply_vertices[polygons.Count+2].Add(mp_13u);mp13u_2=ply_vertices[polygons.Count+2].Count-1;

	//		ply_vertices[0].Add(mp_01); mp01= ply_vertices[0].Count-1;
	//		ply_vertices[polygons.Count+1].Add(mp_01);mp01_2=ply_vertices[polygons.Count+1].Count-1;
	//
	//		ply_vertices[0].Add(mp_03); mp03= ply_vertices[0].Count-1;
	//		ply_vertices[polygons.Count+1].Add(mp_03);mp03_2=ply_vertices[polygons.Count+1].Count-1;
	//
	//		ply_vertices[0].Add(mp_12); mp12= ply_vertices[0].Count-1;
	//		ply_vertices[polygons.Count+1].Add(mp_12);mp12_2=ply_vertices[polygons.Count+1].Count-1;
	//
	//		ply_vertices[0].Add(mp_23); mp23= ply_vertices[0].Count-1;
	//		ply_vertices[polygons.Count+1].Add(mp_23);mp23_2=ply_vertices[polygons.Count+1].Count-1;
			///
			for(int i=0; i<2;i++){

				foreach(var triangles in covers[i]){

					if(i==1){
						//ply_triangles[0].AddRange(new List<int>{10*triangles[0]+vertices_lastlayer,10*triangles[1]
						//+vertices_lastlayer,vertices_lastlayer+ 10*triangles[2]});
						ply_triangles[0].AddRange(new List<int>{0+vertices_lastlayer,5+vertices_lastlayer, mp13u});
						ply_triangles[0].AddRange(new List<int>{5+vertices_lastlayer,10+vertices_lastlayer, mp13u});
						ply_triangles[0].AddRange(new List<int>{10+vertices_lastlayer,15+vertices_lastlayer, mp13u});
						ply_triangles[0].AddRange(new List<int>{15+vertices_lastlayer,20+vertices_lastlayer, mp13u});
						ply_triangles[0].AddRange(new List<int>{20+vertices_lastlayer,25+vertices_lastlayer, mp13u});
						ply_triangles[0].AddRange(new List<int>{25+vertices_lastlayer,30+vertices_lastlayer, mp13u});
						ply_triangles[0].AddRange(new List<int>{30+vertices_lastlayer,35+vertices_lastlayer, mp13u});
						ply_triangles[0].AddRange(new List<int>{35+vertices_lastlayer,0+vertices_lastlayer, mp13u});
						//					ply_triangles[polygons.Count+1+i].AddRange(new List<int>{10*triangles[0],10*triangles[1],10*triangles[2]});
						ply_triangles[polygons.Count+1+i].AddRange(new List<int>{0,5, mp13u_2});
						ply_triangles[polygons.Count+1+i].AddRange(new List<int>{5,10, mp13u_2});
						ply_triangles[polygons.Count+1+i].AddRange(new List<int>{10,15, mp13u_2});
						ply_triangles[polygons.Count+1+i].AddRange(new List<int>{15,20, mp13u_2});
						ply_triangles[polygons.Count+1+i].AddRange(new List<int>{20,25, mp13u_2});
						ply_triangles[polygons.Count+1+i].AddRange(new List<int>{25,30, mp13u_2});
						ply_triangles[polygons.Count+1+i].AddRange(new List<int>{30,35, mp13u_2});
						ply_triangles[polygons.Count+1+i].AddRange(new List<int>{35,0, mp13u_2});


					}
					if(i==0){

	//					ply_triangles[0].AddRange(new List<int>{10*triangles[0],10*triangles[1], 10*triangles[2]});
						ply_triangles[0].AddRange(new List<int>{0,5, mp13});
						ply_triangles[0].AddRange(new List<int>{5,10, mp13});
						ply_triangles[0].AddRange(new List<int>{10,15, mp13});
						ply_triangles[0].AddRange(new List<int>{15,20, mp13});
						ply_triangles[0].AddRange(new List<int>{20,25, mp13});
						ply_triangles[0].AddRange(new List<int>{25,30, mp13});
						ply_triangles[0].AddRange(new List<int>{30,35, mp13});
						ply_triangles[0].AddRange(new List<int>{35,0, mp13});
	//					ply_triangles[polygons.Count+1+i].AddRange(new List<int>{10*triangles[0],10*triangles[1],10*triangles[2]});
						ply_triangles[polygons.Count+1+i].AddRange(new List<int>{0,5, mp13_2});
						ply_triangles[polygons.Count+1+i].AddRange(new List<int>{5,10, mp13_2});
						ply_triangles[polygons.Count+1+i].AddRange(new List<int>{10,15, mp13_2});
						ply_triangles[polygons.Count+1+i].AddRange(new List<int>{15,20, mp13_2});
						ply_triangles[polygons.Count+1+i].AddRange(new List<int>{20,25, mp13_2});
						ply_triangles[polygons.Count+1+i].AddRange(new List<int>{25,30, mp13_2});
						ply_triangles[polygons.Count+1+i].AddRange(new List<int>{30,35, mp13_2});
						ply_triangles[polygons.Count+1+i].AddRange(new List<int>{35,0, mp13_2});
					}

					udl (10*triangles[0]+" "+10*triangles[1]+" "+10*triangles[2]);
				}
			}
			udl (ply_vertices[polygons.Count+2].Count);
			List<VertTria> list_of_vertria=new List<VertTria>();
			for(int i=0; i<1+polygons.Count+2;i++)
				list_of_vertria.Add(new VertTria(ply_vertices[i],ply_triangles[i]));
			return list_of_vertria;
		}


		/// <summary>
		/// Subdivides the each layer.
		/// </summary>
		/// <param name="layers">Layers.</param>
		/// <param name="polygons">Polygons.</param>
		comb subdivide_layer(List<List<Vector3>> layers, List<List<int>> polygons, float f){
			List<List<int>> new_polygons=new List<List<int>>();
			List<List<Vector3>> new_layers=new List<List<Vector3>>();
			//for each layer
			for(int ilayer=0;ilayer<layers.Count;ilayer++){
				List<Vector3> layer, newlayervertices= new List<Vector3>();

				int nvertex=0;
				foreach(var polygon in polygons){
					//number of vertices in the polygon
					int n=polygon.Count;

					if(ilayer==0)
					{
						//udl (Enumerable.Range(nvertex,10*n));
						new_polygons.Add(Enumerable.Range(nvertex,(int)(f*n)).ToList());
						nvertex+=10*n;
					}

					for(int vertex=0;vertex<n;vertex++){
						layer=layers[ilayer];
						Vector3 v1= layer[polygon[vertex]],v2=layer[polygon[(vertex+1)%n]];
						for(float i=0.0f; i<1f;i=i+1f/f){
							newlayervertices.Add(v1*(1-i)+v2*i);
						}
					}
				}
				new_layers.Add(newlayervertices);
			}
			comb combb= new comb(new_polygons, new_layers);
			return combb;
		}

		List<List<Vector3>> addLayers(List<List<Vector3>> layers, float f){
			List<List<Vector3>> new_layers= new List<List<Vector3>>();
			for(int ilayer=0;ilayer<layers.Count-1;ilayer++){
				var current_layer=layers[ilayer];

				new_layers.Add(current_layer);
				var next_layer= layers[ilayer+1];
				Vector3 v1,v2;
				for (float i=1f/f; i<1f;i=i+1f/f){
					List<Vector3> newlayervertices= new List<Vector3>();
					for(int j=0; j< current_layer.Count;j++){

						v1=current_layer[j];
						v2=next_layer[j];

						newlayervertices.Add(v1*(1-i)+v2*i);
					}
					new_layers.Add(newlayervertices);
				}
				//new_layers.Add(next_layer);

			}
			new_layers.Add(layers.Last());
			return new_layers;

		}

		// Update is called once per frame
		void Update () {
		}

		void buildplyObject(GameObject go,List<Vector3> ply_vertices, List<int> ply_triangles){
			List <Vector3> newVertices;
			List <int> newTriangles;
			int nvertices = Convert.ToInt16(ply_vertices.Count) * 2;
			int ntriangles = Convert.ToInt16(ply_triangles.Count) *2;
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
			mesh.normals = l.ToArray();
			mesh.triangles = newTriangles.ToArray();
		}
		void buildObject(string InputFile, GameObject go){
			List <Vector3> newVertices;
			List <int> newTriangles;
			string []objectFile;
			objectFile = System.IO.File.ReadAllLines(InputFile);
			int nvertices = Convert.ToInt16(objectFile [0]) * 2;
			int ntriangles = Convert.ToInt16(objectFile [1]) *2;
			newVertices = new List<Vector3>(nvertices);
			newTriangles =  new List<int>(ntriangles);


			MeshFilter ms = go.GetComponent <MeshFilter> ();
			Mesh mesh = new Mesh ();
			ms.mesh = mesh;

			string []parsed;
			float a,b, c;
			int vPointer=2,i=0;
			//UnityEngine.Debug.Log(nvertices+","+ntriangles);
			for( vPointer=2; i <nvertices/2;vPointer++, i++){
				parsed= objectFile[vPointer].Split(delimiterChars);
				a=float.Parse(parsed[0], System.Globalization.CultureInfo.InvariantCulture);
				b=float.Parse(parsed[1], System.Globalization.CultureInfo.InvariantCulture);
				c=float.Parse(parsed[2], System.Globalization.CultureInfo.InvariantCulture);
				newVertices.Add(new Vector3(a,b,c));

			}
			for(i=0;i < nvertices/2; i++){
				newVertices.Add (newVertices[i]);
			}
			int j=0;
			for(i=0; j< ntriangles/2 ;i=i+3,j++,vPointer++){
				parsed= objectFile[vPointer].Split(delimiterChars);
				newTriangles.Add(Convert.ToInt16(parsed[1]));
				newTriangles.Add(Convert.ToInt16(parsed[2]));
				newTriangles.Add(Convert.ToInt16(parsed[3]));
			}

			for (i=0; j<ntriangles; i++, j++) {
				newTriangles.Add(newTriangles[ntriangles*3/2-i-1]);
				i++;
				newTriangles.Add(newTriangles[ntriangles*3/2-i-1]);
				i++;
				newTriangles.Add(newTriangles[ntriangles*3/2-i-1]);
			}

			int k = 0;
			mesh.vertices = newVertices.ToArray();

			List<Vector3> l = Enumerable.Repeat (Vector3.up, nvertices/2).ToList();
			l.AddRange(Enumerable.Repeat(Vector3.down,nvertices/2).ToList());
			mesh.normals = l.ToArray();
			mesh.triangles = newTriangles.ToArray();
			Color32[] colors= new Color32[mesh.vertices.Count()];
			for (i=0; j<ntriangles; i++) {
				colors[i]= new Color32((byte)UnityEngine.Random.Range(0,255),(byte)UnityEngine.Random.Range(0,255),(byte)UnityEngine.Random.Range(0,255),100);
			}
			mesh.colors32= colors;
		}

		class VertTria{
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
		class comb{
			List<List<int>> poly;
			List<List<Vector3>> lay;
			public comb(List<List<int>> poly,List<List<Vector3>> lay)
			{
				this.poly=poly;this.lay=lay;
			}
			public List<List<int>> getPoly(){
				return this.poly;
			}
			public List<List<Vector3>> getLayer(){
				return this.lay;
			}
		}

	}
}