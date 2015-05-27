using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;
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
			TestRRT();
			//			buildMedial();
			//			RemoveVs();
			//			showPath();
		}

		void Testt (){

				metrics1= new MedialMetrics();
				
				buildMedial(false,10f);
				medialMeshObj.connect_Vs(1.7f);
			medialMeshObj.PathFindfn(start.transform.position,end.transform.position, true);
			udl (metrics1.v_in_graph+" " +metrics1.e_in_graph);
			}

		void TestRRT(){
			metrics1= new MedialMetrics();
		#region RRT
			RRT r= new RRT(arena,22685,start.transform.position,end.transform.position, 10f, metrics1);
		#endregion
			udl (metrics1.RRT_running_time+" " +metrics1.v_in_graph+ " "+ metrics1.e_in_graph);
		}

		/// <summary>
		/// Test shortest path length, it's time 
		/// and time taken to run both algorithms for fixed start, end point and limited time (arena height)
		/// and collect data in metrics1
		/// </summary>
		void Test1 (){
			#region MA

			//GraphNodes\tGraphEdges\tGraphAngleConstraint\t
			string writeline="#\tGraphCreate\tConnectVsTotal\tTotal_MA_Time\n";
			for(int i=0; i<10; i++){
				metrics1= new MedialMetrics();

				buildMedial(false,10f);
				medialMeshObj.connect_Vs(1.7f);
//				writeline+=(i+1)+"\t"+metrics1.medial_algo_running_time+"\t"+
//					metrics1.remove_top_bottom_time+"\t"+
//						metrics1.creating_graph_time+"\t"+
//						metrics1.create_KDtree_time+metrics1.connect_Vs_time+"\t"+
//						(metrics1.medial_algo_running_time+metrics1.remove_top_bottom_time+metrics1.creating_graph_time+metrics1.create_KDtree_time+metrics1.connect_Vs_time)
//						+"\n";//+metrics1.

				writeline+=(i+1)+"\t"+
//					metrics1.medial_algo_running_time+"\t"+
//					metrics1.remove_top_bottom_time+"\t"+
						metrics1.creating_graph_time+"\t"+
						metrics1.create_KDtree_time+metrics1.connect_Vs_time+"\t"
//						(metrics1.medial_algo_running_time+metrics1.remove_top_bottom_time+metrics1.creating_graph_time+metrics1.create_KDtree_time+metrics1.connect_Vs_time)
						+"\n";

			}
			System.IO.File.WriteAllText(@"/Users/dhsingh/Documents/Thesis/testresults/testresults.txt", writeline);
			udl (writeline);
			#endregion


			//			SerializeObject(medialMeshObj,@"/Users/dhsingh/Documents/Thesis/unitytool/Assets/Scripts/Medial/medialmesh");

		}
		
		///t_min to be set equal to layers[0].y
		///t_max to be set equal to layers[Last-1].y
		public void initT(float val, float val2, float val3){
//			t_min=t= val;
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

		public void buildMedial(bool rerunQhull, float angleConstraint)
		{
			if(rerunQhull){
				var watch = Stopwatch.StartNew();

				runaProcess("/Users/dhsingh/Documents/Thesis/SM03Skeleton/run.sh",multi_triangle_input_file);
				watch.Stop();
				metrics1.medial_algo_running_time = watch.ElapsedMilliseconds;
			}
			string medial_output=dir+"output_medial_"+multi_triangle_input_file;
			var gameobj2 = GameObject.Find ("Medial");
			medialMeshObj= new MedialMesh(medial_output,gameobj2,true, true,
			                              arena, metrics1, true, angleConstraint);
			
		}
		
		
		public void Addextraedges(){
			
			
			medialMeshObj.addEdgesInsideTheArena();
			
		}
		public void findPaths(){
			
			medialMeshObj.PathFindfn(start.transform.position,end.transform.position, true);
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
		#region savegraph
		void saveGraph(Graph g){

		}
		#endregion
		#region serialize
		public void SerializeObject<T>(T serializableObject, string fileName)
		{
			if (serializableObject == null) { return; }
			
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
				using (MemoryStream stream = new MemoryStream())
				{
					serializer.Serialize(stream, serializableObject);
					stream.Position = 0;
					xmlDocument.Load(stream);
					xmlDocument.Save(fileName);
					stream.Close();
				}
			}
			catch (Exception ex)
			{
				//Log exception here

				udl (ex);
			}
		}
		
		
		/// <summary>
		/// Deserializes an xml file into an object list
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public T DeSerializeObject<T>(string fileName)
		{
			if (string.IsNullOrEmpty(fileName)) { return default(T); }
			
			T objectOut = default(T);
			
			try
			{
				string attributeXml = string.Empty;
				
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(fileName);
				string xmlString = xmlDocument.OuterXml;
				
				using (StringReader read = new StringReader(xmlString))
				{
					Type outType = typeof(T);
					
					XmlSerializer serializer = new XmlSerializer(outType);
					using (XmlReader reader = new XmlTextReader(read))
					{
						objectOut = (T)serializer.Deserialize(reader);
						reader.Close();
					}
					
					read.Close();
				}
			}
			catch (Exception ex)
			{
				//Log exception here
			}
			
			return objectOut;
		}
		#endregion
	}
	
	public class MedialMetrics{
		public long medial_algo_running_time, 
		remove_top_bottom_time,
		creating_graph_time, 
		connect_Vs_time,
		create_KDtree_time,
		
		run_dijkstra_on_graph_time
			;
		
		public long RRT_running_time;
		
		
		public int v_in_graph,
			e_in_graph;

		
		public float time_for_shortest_path_MA_xyz, time_for_shortest_path_MA_xz,
		length_of_shortest_path_xz_MA,
		length_of_shortest_path_xyz_MA;
		
		public float time_for_shortest_path_RRT_xyz, time_for_shortest_path_RRT_xz, 
		length_of_shortest_path_xz_RRT,
		length_of_shortest_path_xyz_RRT;

		public float angleConstraint;
	}


}
