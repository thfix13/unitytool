using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
namespace Medial{
	public class Testing : MonoBehaviour {
		
		public GameObject boxprefab, start, end;
		public Transform map;
		public Material black;
		//="convex";//"moving_gaurd";
		string file_prefix;
		ArenasGenerator arena= null;
		string multi_triangle_input_file, dir=@"/Users/dhsingh/Documents/Thesis/SM03Skeleton/";

		
		MedialMesh medialMeshObj;

		public MedialMetrics metrics1;

		void Start () {
			buildTriangulatedTimeVaryingArena(4);
			//			buildMedial();
			//			RemoveVs();
			//			showPath();
		}
		
	
		public void buildTriangulatedTimeVaryingArena(int selGridInt){
			arena= new ArenasGenerator(selGridInt);
			file_prefix="moving_gaurd";
			
			///divide each line in each layer in multiple parts horizontally of length= layer_division
			LayerPolygonUtil LPU= new LayerPolygonUtil(arena,arena.layer_division,0f);
			
			///and get the redefined layers
			var layers=LPU.getLayer();
			
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
			var watch = Stopwatch.StartNew();

			runaProcess("/Users/dhsingh/Documents/Thesis/SM03Skeleton/run.sh",multi_triangle_input_file);
			watch.Stop();
			metrics1.medial_algo_running_time = watch.ElapsedMilliseconds;

			string medial_output=dir+"output_medial_"+multi_triangle_input_file;
			var gameobj2 = GameObject.Find ("Medial");
			medialMeshObj= new MedialMesh(medial_output,gameobj2,true, true,
			                              arena, metrics1, true);
			
		}
		
		public void RemoveVs(){
			var watch = Stopwatch.StartNew();
			medialMeshObj.connect_Vs(1.7f);
			watch.Stop();
			metrics1.connect_Vs_time = watch.ElapsedMilliseconds;

			udl ("Nodes in "+1.7+" proximity connected");
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
	}

	public class MedialMetrics{
		public long medial_algo_running_time, 
		remove_top_bottom_time,
		creating_graph_time, 
		run_dijkstra_on_graph_time,
		connect_Vs_time,
		create_KDtree_time;

		public int v_unidirected_uncut, 
		e_unidirected_uncut,
		v_in_graph,
		e_in_graph;

		public float time_for_shortest_path;

	}
}
