using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
namespace Medial{
	public class Moving_guard_arena : MonoBehaviour {

		public GameObject boxprefab, start, end;
		public Transform map;
		public Material black;
		//="convex";//"moving_gaurd";
		string file_prefix;
		ArenasGenerator a= null;
		float layer_division=1f;
		string multi_triangle_input_file, dir=@"/Users/dhsingh/Documents/Thesis/SM03Skeleton/";
		float t=0;
		GraphUtil MGU;
		bool generate2Dcheck=false;
		GameObject player2Dprojection;

		void Start () {
			buildTriangulatedTimeVaryingArena(3);
	//		buildMedial();
		}

		// Update is called once per frame
		void Update () {
				
			Vector3 playerpos= generate2Dcheck? MGU.movePlayer(t):Vector3.zero;
			if(playerpos!= Vector3.zero)
				player2Dprojection.transform.position= new Vector3(playerpos.x,player2Dprojection.transform.position.y,playerpos.z);
			if(generate2Dcheck){
				a.generate2D_movinggaurdarena(t);
				t+=0.013f;
				udl ("t= "+t);
			}

		}

		public void playpause(){
			generate2Dcheck=!generate2Dcheck;
		}
		public void goBack(){
			t=Math.Max (0,t-1);
		}
		public void Reset(){
			t=0;
		}
		//t to be set equal to layers[0].y
		public void initT(float val){
			t= val;
		}
		public void buildTriangulatedTimeVaryingArena(int selGridInt){
			a= new ArenasGenerator(selGridInt);
			file_prefix="moving_gaurd";

			
			var layers =a.getLayers();
			var polygons= a.getPolygons();
			List<int[]>[] covers=a.getCovers();
	//		udl (layers.Count+" "+layers[0].Count);

			//divide each line in each layer in multiple parts horizontally of length= layer_division
			PolyLayerAndMap combb= LayerPolygonUtil.subdivide_layer(layers,polygons,layer_division);
			layers=combb.getLayer();
			polygons=combb.getPoly();
			List<int> mappingOriginalIndexToNewIndexOfPolygons= 
				combb.getMappingOriginalIndexToNewIndexOfPolygons();
	//		udl (layers[0].Count+" "+layers[1].Count);
			//init t
			initT(layers[0][0].y);

			//add 2 layers in-between every two layers.
//			layers=LayerPolygonUtil.addLayers(layers,2f);
			
			List<VertTria> vt= PLYUtil.assignPLY(layers,polygons,covers, mappingOriginalIndexToNewIndexOfPolygons);
			
			multi_triangle_input_file= file_prefix+".ply2";
			PLYUtil.writePLY(dir+multi_triangle_input_file,vt[0].getVertices(),vt[0].getTriangles());


			foreach(Transform child in map) {
				Destroy(child);
			}
			map.transform.position.Set(0f,0f,0f);
			for(int goi=1; goi < vt.Count;goi++){
				var go=(GameObject) Instantiate(boxprefab);
				go.name="Box"+goi;
				go.transform.parent=map;
				PLYUtil.buildplyObject (go,vt[goi].getVertices(),vt[goi].getTriangles());
				go.AddComponent("MeshCollider");
			}
		}
		public void buildMedial()
		{

			runaProcess("/Users/dhsingh/Documents/Thesis/SM03Skeleton/run.sh",multi_triangle_input_file);
			
			string medial_output=dir+"output_medial_"+multi_triangle_input_file;
			
			var gameobj2 = GameObject.Find ("Medial");
			buildObject (medial_output,gameobj2,true);

		}

		public void Addextraedges(){
			MGU.createGraph();
			MGU.addEdgesThatDontCollideWithArena();
			udl ("AddedEdgesThatDontCollideWithArena");

		}
		public void projectPath(){

			MGU.findNearest(start.transform.position,end.transform.position);
			MGU.findPath();
			MGU.showPath();

			var plane= GameObject.CreatePrimitive(PrimitiveType.Cube);
			plane.transform.localScale=new Vector3(16,0.1f,16);
			plane.transform.position= new Vector3(0,20.2f,0);
			plane.renderer.material=black;

			player2Dprojection= GameObject.CreatePrimitive(PrimitiveType.Sphere);
			player2Dprojection.transform.localScale= new Vector3(0.3f,0.3f,0.3f);
			player2Dprojection.transform.position=new Vector3(0,20.35f,0);

			generate2Dcheck=true;}

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

		static void udl(object s){
			UnityEngine.Debug.Log(s);
		}

		public static void SetAlpha (Material material, float value) {
			Color color = material.color;
			color.a = value;
			material.color = color;
		}
		

		void buildObject(string InputFile, GameObject go, bool createGraph){
			
			char[] delimiterChars = { ' ', '\t' };
			List <Vector3> newVertices;
			List <int> newTriangles;
			string []objectFile;
			objectFile = System.IO.File.ReadAllLines(InputFile);
			int nvertices = Convert.ToInt32(objectFile [0]) * 2;
			int ntriangles = Convert.ToInt32(objectFile [1]) *2;
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

			int j=0;
			for(i=0; j< ntriangles/2 ;i=i+3,j++,vPointer++){
				parsed= objectFile[vPointer].Split(delimiterChars);
				newTriangles.Add(Convert.ToInt32(parsed[1]));
				newTriangles.Add(Convert.ToInt32(parsed[2]));
				newTriangles.Add(Convert.ToInt32(parsed[3]));
			}

			if(createGraph){
				MGU= new GraphUtil(newVertices,newTriangles);

			}
			for(i=0;i < nvertices/2; i++){
				newVertices.Add (newVertices[i]);
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

			mesh.triangles = newTriangles.ToArray();

			Color[] colors= new Color[newVertices.Count()];
			Color triColor= Color.green;
			for (i=0; i<newTriangles.Count; i++) {
				int vertIndex = newTriangles[i];
				if (i % 3 == 0){
					Vector3 v=newVertices[vertIndex];
	//				triColor = new Color(v.y/11f,UnityEngine.Random.Range(0f,1f),(v.y)/11f,150f/255f);
				}
				colors[vertIndex] = triColor;
			}
			mesh.colors= colors;
			mesh.normals = l.ToArray();
		}
		


	}
}