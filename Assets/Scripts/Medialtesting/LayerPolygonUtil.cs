using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace Medialtesting{
	public class LayerPolygonUtil{

		List<int> mappingOriginalIndexToNewIndexOfPolygons;
		List<List<Vector3>> layers;
		List<List<int>> polygons;
		List<int[]>[] covers;
		ArenasGenerator aGen;

		public LayerPolygonUtil(ArenasGenerator aGen, float layer_division, float numberofAddLayers){
			this.covers=aGen.getCovers();;
			this.polygons=aGen.getPolygons();
			this.layers=aGen.getLayers();;

			mappingOriginalIndexToNewIndexOfPolygons= new List<int>();

			//if layer_division is 0, don't do all that
			if(layer_division !=0){
				subdivide_layer( layer_division);
			}
				if(numberofAddLayers !=0)
					addLayers(numberofAddLayers);
			if(layer_division !=0){
				renewedCovers();
			}
		}

		public List<List<int>> getPoly(){
			return this.polygons;
		}
		public List<List<Vector3>> getLayer(){
			return this.layers;
		}
		public List<int[]>[] getCovers(){
			return this.covers;
		}

		/// <summary>
		/// Subdivides each layer. Change cover vertices accordingly
		/// </summary>
		private void subdivide_layer(float layer_division){

			List<List<int>> new_polygons=new List<List<int>>();
			List<List<Vector3>> new_layers=new List<List<Vector3>>();

			Hashtable divisions= new Hashtable();

			//for each layer
			for(int ilayer=0;ilayer<layers.Count;ilayer++){
				List<Vector3> layer, newlayervertices= new List<Vector3>();

				int NumberOfVertexInPolygon=0, VertexCounter=0, lastIndex=0;
				foreach(var polygon in polygons){
					//number of vertices in the polygon
					int polygonCount=polygon.Count;
	//				int totalpoints=0; //for this polygon

					for(int vertex=0;vertex<polygonCount;vertex++){
						layer=layers[ilayer];
						Vector3 v1= layer[polygon[vertex]],v2=layer[polygon[(vertex+1)%polygonCount]];
						//number of divisions
						int numberOfDivisions;
						if(ilayer==0){
							numberOfDivisions = Mathf.CeilToInt(Vector3.Distance(v1,v2)/layer_division);
							divisions.Add(polygon[vertex],numberOfDivisions);
						}
						else
						{numberOfDivisions = (int)divisions[polygon[vertex]];
	//						udl("yo "+divisions[polygon[vertex]]);
						}
						for(float i=0.0f; i<1f;i=i+1f/numberOfDivisions){
							newlayervertices.Add(v1*(1-i)+v2*i);

							if(ilayer==0 && i==0.0f){
								mappingOriginalIndexToNewIndexOfPolygons.Add(VertexCounter);
	//							udl ("totalpoints "+ NumberOfVertexInPolygon);
							}
							if(ilayer==0){
								NumberOfVertexInPolygon++;
								VertexCounter++;
							}

						}
					}
					if(ilayer==0)
					{
						new_polygons.Add(Enumerable.Range(lastIndex,NumberOfVertexInPolygon-lastIndex).ToList());
						lastIndex=NumberOfVertexInPolygon;
	//					NumberOfVertexInPolygon+=totalpoints;
					}
				}
				new_layers.Add(newlayervertices);
			}

			layers= new_layers;
			polygons=new_polygons;
		}

		/// <summary>
		/// Change the vertex id of covers after subdivide_layer and addLayers have been applied. 
		/// The indices do start from 0 now also... so Vertices_lastlayer has still to be added
		/// </summary>
		private void renewedCovers(){

			//starting point of vertex indices in last layer
	//		int nn=layers[0].Count;
	//		int vertices_lastlayer=nn*(layers.Count-1);
			for(int i=0; i<2;i++){
				
				foreach(var coverTrianglesIndex in covers[i]){
					//series in 0-7 
					// 9
					//			10
					// 8
					//see paper

					coverTrianglesIndex[0]=mappingOriginalIndexToNewIndexOfPolygons[coverTrianglesIndex[0]];
					coverTrianglesIndex[1]=mappingOriginalIndexToNewIndexOfPolygons[coverTrianglesIndex[1]];
					coverTrianglesIndex[2]=mappingOriginalIndexToNewIndexOfPolygons[coverTrianglesIndex[2]];
				}
			}
		}
		static void udl(object str){
			UnityEngine.Debug.Log(str);
		}

		private void addLayers(float f){

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
			layers= new_layers;

		}

	}
}