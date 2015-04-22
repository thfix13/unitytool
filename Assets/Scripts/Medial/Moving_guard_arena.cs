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
		ArenasGenerator arena= null;
		float layer_division=2f;
		string multi_triangle_input_file, dir=@"/Users/dhsingh/Documents/Thesis/SM03Skeleton/";

		/// <summary>
		/// the time t that increases from the lowest y to inifinity, and thus updates the player location 
		/// in the 2D view of the game from the top. 
		/// </summary>
		float t=0;

		/// <summary>
		/// Keep y_min for resetting t
		/// </summary>
		float t_min;

		MedialMesh medialMeshObj;
		bool generate2DFlag=false;
		GameObject player2Dprojection;

		void Start () {
			buildTriangulatedTimeVaryingArena(4);
//			buildMedial();
//			RemoveVs();
//			showPath();
		}

		// Update is called once per frame
		void Update () {
				
			Vector3 playerpos= generate2DFlag? medialMeshObj.movePlayer(t):Vector3.zero;
			if(playerpos==-Vector3.one)
			{
				udl ("Destination reached, Game paused");
				generate2DFlag=false;
				return;
			}
			if(playerpos!= Vector3.zero)
				player2Dprojection.transform.position= new Vector3(playerpos.x,player2Dprojection.transform.position.y,playerpos.z);
			if(generate2DFlag){
				arena.generate2D_movinggaurdarena(t);
				t+=0.013f;
				udl ("t= "+t);
			}

		}

		public void playpause(){
			generate2DFlag=!generate2DFlag;
		}
		public void goBack(){
			t=Math.Max (0,t-1);
		}
		public void Reset(){
			t=t_min;
		}

		///t_min to be set equal to layers[0].y
		///t_max to be set equal to layers[Last-1].y
		public void initT(float val, float val2, float val3){
			t_min=t= val;
			arena.setMinMaxXYZ(val2,val3);
		}

		public void buildTriangulatedTimeVaryingArena(int selGridInt){
			arena= new ArenasGenerator(selGridInt);
			file_prefix="moving_gaurd";

			///divide each line in each layer in multiple parts horizontally of length= layer_division
			LayerPolygonUtil LPU= new LayerPolygonUtil(arena,layer_division,0f);

			///and get the redefined layers
			var layers=LPU.getLayer();

			///init t
			initT(layers[0][0].y,layers[1][0].y, layers[layers.Count-2][0].y);

			//add 2 layers in-between every two layers.
//			layers=LayerPolygonUtil.addLayers(layers,2f);
			
			List<VertTria> vt= PLYUtil.assignPLY(LPU);
			
			multi_triangle_input_file= file_prefix+".ply2";
			PLYUtil.writePLY(dir+multi_triangle_input_file,vt[0].getVertices(),vt[0].getTriangles());


			foreach(Transform child in map) {
				Destroy(child);
			}
			map.transform.position.Set(0f,0f,0f);

			for(int goi=1; goi < vt.Count-2;goi++){
				var go=(GameObject) Instantiate(boxprefab);
				go.name="Box"+goi;
				go.transform.parent=map;
				PLYUtil.buildplyObject (go,vt[goi].getVertices(),vt[goi].getTriangles());
				go.AddComponent<MeshCollider>();
			}
		}
		public void buildMedial()
		{

			runaProcess("/Users/dhsingh/Documents/Thesis/SM03Skeleton/run.sh",multi_triangle_input_file);
			
			string medial_output=dir+"output_medial_"+multi_triangle_input_file;
			var gameobj2 = GameObject.Find ("Medial");
			medialMeshObj= new MedialMesh(medial_output,gameobj2,true, true,
			                              arena);

		}

		public void RemoveVs(){
			medialMeshObj.connect_Vs(1.7f);
		}


		public void Addextraedges(){


			medialMeshObj.addEdgesInsideTheArena();

		}
		public void findPaths(){

			medialMeshObj.findNearests(start.transform.position,end.transform.position);
			medialMeshObj.findPaths();
		}
		public void showPaths(){
			medialMeshObj.showPath();
		}
		public void projectPathOn2D(){

			var plane= GameObject.CreatePrimitive(PrimitiveType.Cube);
			plane.transform.localScale=new Vector3(arena.getMaxX()-arena.getMinX(),0.1f,arena.getMaxZ()-arena.getMinZ());
			plane.transform.position= new Vector3(0,arena.getMaxY2()+0.2f,0);
			plane.GetComponent<Renderer>().material=black;

			player2Dprojection= GameObject.CreatePrimitive(PrimitiveType.Sphere);
			player2Dprojection.transform.localScale= new Vector3(0.3f,0.3f,0.3f);
			player2Dprojection.transform.position=new Vector3(0,20.35f,0);

			generate2DFlag=true;}

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

		static void SetAlpha (Material material, float value) {
			Color color = material.color;
			color.a = value;
			material.color = color;
		}
	}
}