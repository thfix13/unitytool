using UnityEngine;
using System.Collections;
//using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

public class LayerPolygonUtil{

	/// <summary>
	/// Subdivides each layer.
	/// </summary>
	/// <param name="layers">Layers.</param>
	/// <param name="polygons">Polygons.</param>
	public static PolyLayerAndMap subdivide_layer(List<List<Vector3>> layers, List<List<int>> polygons, float layer_division){
		List<List<int>> new_polygons=new List<List<int>>();
		List<List<Vector3>> new_layers=new List<List<Vector3>>();
		List<int> mappingOriginalIndexToNewIndexOfPolygons= new List<int>();
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

		PolyLayerAndMap combb= new PolyLayerAndMap(new_polygons, new_layers, mappingOriginalIndexToNewIndexOfPolygons);
		return combb;
	}
	static void udl(object str){
		UnityEngine.Debug.Log(str);
	}

	public static List<List<Vector3>> addLayers(List<List<Vector3>> layers, float f){
		List<List<Vector3>> new_layers= new List<List<Vector3>>();
		for(int ilayer=0;ilayer<layers.Count-1;ilayer++){
			var current_layer=layers[ilayer];

			new_layers.Add(current_layer);
			var next_layer= layers[ilayer+1];
			Vector3 v1,v2;
//			UnityEngine.Debug.Log(current_layer.Count +" next layer #="+next_layer.Count);

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

}

public class PolyLayerAndMap{
	List<List<int>> poly;
	List<List<Vector3>> lay;
	List<int> mappingOriginalIndexToNewIndexOfPolygons;
	public PolyLayerAndMap(List<List<int>> polygons,List<List<Vector3>> layers, List<int> mapping)
	{
		this.poly=polygons;this.lay=layers;this.mappingOriginalIndexToNewIndexOfPolygons=mapping;
	}
	public List<List<int>> getPoly(){
		return this.poly;
	}
	public List<List<Vector3>> getLayer(){
		return this.lay;
	}
	public List<int> getMappingOriginalIndexToNewIndexOfPolygons(){
		return this.mappingOriginalIndexToNewIndexOfPolygons;
	}
}