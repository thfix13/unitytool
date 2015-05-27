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
				
			Vector3 playerpos= generate2DFlag? medialMeshObj.movePlayerfn(t):Vector3.zero;
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
//				udl ("t= "+t);
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

			///and get the redefined layers
			var layers=arena.getLayers();
			
			///init t
			initT(layers[0][0].y,layers[1][0].y, layers[layers.Count-2][0].y);

			///divide each line in each layer in multiple parts horizontally of length= layer_division
			LayerPolygonUtil LPU= new LayerPolygonUtil(arena,arena.layer_division,0f);

			//add 2 layers in-between every two layers.
//			layers=LayerPolygonUtil.addLayers(layers,2f);
			
			List<VertTria> vt= PLYUtil.assignPLY(LPU);
			
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
				go.AddComponent<MeshCollider>();
			}
		}

		public void callRRT(){
			RRT r= new RRT(arena,2000,start.transform.position,end.transform.position, 10f, medialMeshObj.metrics1);
		}

		public void buildMedial(float angleConstraint)
		{

			runaProcess("/Users/dhsingh/Documents/Thesis/SM03Skeleton/run.sh",multi_triangle_input_file);
			
			string medial_output=dir+"output_medial_"+multi_triangle_input_file;
			var gameobj2 = GameObject.Find ("Medial");
			medialMeshObj= new MedialMesh(medial_output,gameobj2,true, true,
			                              arena,null,false, angleConstraint);

		}

		public void RemoveVs(){
			medialMeshObj.connect_Vs(1.7f);
			udl ("Nodes in "+1.7+" proximity connected");
		}


		public void Addextraedges(){


			medialMeshObj.addEdgesInsideTheArena();

		}
		public void findPaths(){

			medialMeshObj.PathFindfn(start.transform.position,end.transform.position, true);
		}
		
		public void projectPathOn2D(){

			var plane= GameObject.CreatePrimitive(PrimitiveType.Cube);
			plane.transform.localScale=new Vector3(arena.getMaxX()-arena.getMinX(),0.1f,arena.getMaxZ()-arena.getMinZ());
			plane.transform.position= new Vector3(0,arena.getMaxY2()+0.25f,0);
			plane.GetComponent<Renderer>().material.color=Color.black;

			player2Dprojection= GameObject.CreatePrimitive(PrimitiveType.Sphere);
			player2Dprojection.transform.localScale= new Vector3(2f,2f,2f);
			player2Dprojection.GetComponent<Renderer>().material.color= Color.cyan;
			player2Dprojection.transform.position=new Vector3(0,arena.getMaxY2()+0.5f,0);

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