using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
public class Arena  {
	List <List<Vector3>> layers;
	List<List<int>> polygons;
	int option;
	int nlayers;
	public Arena(int selGridInt){
		option=selGridInt;
		layers =new List<List<Vector3>>();

		switch(option){
		//for polygon with moving guard
		case 0:polygons= new List<List<int>>{new List<int>{0,1,2,3,4,5,6,7}, new List<int>{10,9,8}};
			nlayers=40;
			generate_arena_with_room();
			generate_movrot_gaurd();
			break;

		//for convex polygon test
		case 1:polygons= new List<List<int>>{new List<int>{0,1,2,3}};
			nlayers=4;
			generate_convex_arena();
			break;
		case 2: nlayers=20;
			generate_arena();generate_moving_gaurd();
			break;
		}
	}
	public List <List<Vector3>> getLayers(){
		return layers;
	}
	public List<List<int>> getPolygons(){
		return polygons;
	}

	//to be called on update
	public void generate2D_movinggaurdarena(int t){
		var thislayer=layers[t];
		Vector3 u,v;
		foreach(var poly in polygons){
			for(int i=0;i<poly.Count;i++){
				u=thislayer[poly[i]];v=thislayer[poly[(i+1)%poly.Count]];
				u.y=v.y=20.25f;
				UnityEngine.Debug.DrawLine(u,v,Color.magenta);
			}
		}

	}

	//return {bottomlayer,upperlayer}
	public List<int[]>[] getCovers(){
		List<int[]> upperlayer;
	switch(option){
		case 0: return new List<int[]>[]{
				new List<int[]>{new int[] {0,1,9}, new int[]{1,2,3},new int[]{1,3,4},new int[]{9,1,10},
					new int[]{10,1,4},new int[]{10,4,5},new int[]{10,5,6},new int[]{10,7,8},
					new int[]{7,0,8},new int[]{8,0,9}, new int[]{10,6,7}},
				new List<int[]>{new int[] {1,0,10}, new int[]{2,1,3},new int[]{3,1,4},new int[]{8,1,10},
					new int[]{8,4,1},new int[]{8,5,4},new int[]{8,6,5},new int[]{8,9,6},new int[]{9,7,6},
					new int[]{7,9,0}, new int[]{0,9,10}}};
			break;
		case 1: return  new List<int[]>[]{new List<int[]>{new int[3]{0,1,3},new int[3]{1,2,3}},
				new List<int[]>{new int[3]{0,1,3},new int[3]{1,2,3}}};
			break;
		}
		return null;
	}

	public static void udl(String s){
		UnityEngine.Debug.Log(s);
	}
	public void generate_convex_arena(){
		Vector3 one= new Vector3(-3f,0f,-2f),
		two=new Vector3(4f,0f,0f), 
		three= new Vector3(-3f,0f,2f), 
		four= new Vector3(-4f,0f,0f);
		for(int ilayer=0;ilayer<nlayers;ilayer++){
			one.y= two.y= three.y= four.y=4f*ilayer;
			layers.Add(new List<Vector3>{one, two,three,four});
		}

	}

	public void generate_arena_with_room(){
		Vector3 one= new Vector3(-6f,0f,0f),
		two=new Vector3(-2f,0f,0f), 
		three= new Vector3(-2f,0f,-2f), 
		four= new Vector3(2f,0f,-2f),
		five=new Vector3(2f,0f,0f), 
		six=new Vector3(6f,0f,0f),
		sev=new Vector3(6f,0f,4f), 
		ei=new Vector3(-6f,0f,4f); 

		for(int ilayer=0;ilayer<nlayers;ilayer++){
			one.y= two.y= three.y= four.y=five.y=six.y=sev.y=ei.y=1f*ilayer/2;
			layers.Add(new List<Vector3>{one, two,three,four,five,six,sev,ei});
		}

	}
	
	public void generate_arena(){
		Vector3 one= new Vector3(-4f,0f,-4f),
		two=new Vector3(4f,0f,-4f), 
		three= new Vector3(4f,0f,4f), 
		four= new Vector3(-4f,0f,4f);
//		List<List<Vector3>> arena= new List<List<Vector3>>();
		for(int ilayer=0;ilayer<nlayers;ilayer++){
			one.y= two.y= three.y= four.y=1f*ilayer;
			layers.Add(new List<Vector3>{one, two,three,four});
		}
	}

	public void generate_moving_gaurd(){
		udl ("number of nodes= "+layers.Count);
		Vector3 fo=new Vector3(-4f,0,2.5f), so=new Vector3(-4f,0,0.5f), svo= new Vector3(-5.5f,0,1.5f);
		Vector3 ff=new Vector3(5.5f,0,2.5f), sf=new Vector3(5.5f,0,0.5f), svf= new Vector3(4f,0,1.5f);//y don't matter here
		int ilayer;
		Vector3 f= new Vector3(),s=new Vector3(),sv=new Vector3();
		for(ilayer=0;ilayer<nlayers/2f;ilayer++){
			f=(ff*ilayer+fo*(nlayers/2 -ilayer))/(nlayers/2f);
			s=(sf*ilayer+so*(nlayers/2 -ilayer))/(nlayers/2f);
			sv=(svf*ilayer+svo*(nlayers/2 -ilayer))/(nlayers/2f);
			f.y=s.y=sv.y= 1f*ilayer/2f;
			layers[ilayer].Add(f);layers[ilayer].Add(s);layers[ilayer].Add(sv);
		}
		
		//		var theta = 180f/(nlayers/2f);
		//		for(;ilayer<3f*nlayers/4f;ilayer++){
		//			f=RotatePointAroundPivot(f,sv,new Vector3(0, -theta, 0));
		//			s=RotatePointAroundPivot(s,sv,new Vector3(0, -theta, 0));
		//			f.y=s.y=sv.y=1f*ilayer/2f;
		//			
		//			arena[ilayer].Add(f);arena[ilayer].Add(s);arena[ilayer].Add(sv);
		//		}
		
		fo=s;so=f;svo=sv;
		ff.x=-4f; sf.x=-4f; svf.x=-5.5f;
		float n= nlayers/2;
		for(float i=0f;ilayer<nlayers;ilayer++, i+=1/n){
			f=(sf*i+fo*(1-i));
			s=ff*i+so*(1-i);
			sv=svf*i+svo*(1-i);
			f.y=s.y=sv.y= 1f*ilayer/2f;
			
			layers[ilayer].Add(f);layers[ilayer].Add(s);layers[ilayer].Add(sv);
		}
		udl ("number of nodes final= "+layers.Count);
//		return arena;
	}
	
	public void generate_movrot_gaurd(){
		udl ("number of nodes= "+layers.Count);
		Vector3 fo=new Vector3(-4f,0,2.5f), so=new Vector3(-4f,0,0.5f), svo= new Vector3(-5.5f,0,1.5f);
		Vector3 ff=new Vector3(5.5f,0,2.5f), sf=new Vector3(5.5f,0,0.5f), svf= new Vector3(4f,0,1.5f);
		int ilayer;
		Vector3 f= new Vector3(),s=new Vector3(),sv=new Vector3();
		for(ilayer=0;ilayer<nlayers/4f;ilayer++){
			f=(ff*ilayer+fo*(nlayers/4 -ilayer))/(nlayers/4);
			s=(sf*ilayer+so*(nlayers/4 -ilayer))/(nlayers/4);
			sv=(svf*ilayer+svo*(nlayers/4 -ilayer))/(nlayers/4);
			f.y=s.y=sv.y= 1f*ilayer/2f;
			layers[ilayer].Add(f);layers[ilayer].Add(s);layers[ilayer].Add(sv);
		}
		
		var theta = 180f/(nlayers/2f);
		for(;ilayer<3f*nlayers/4f;ilayer++){
			f=RotatePointAroundPivot(f,sv,new Vector3(0, -theta, 0));
			s=RotatePointAroundPivot(s,sv,new Vector3(0, -theta, 0));
			f.y=s.y=sv.y=1f*ilayer/2f;
			
			layers[ilayer].Add(f);layers[ilayer].Add(s);layers[ilayer].Add(sv);
		}
		
		fo=f;so=s;svo=sv;
		ff.x=-5.5f; sf.x=-5.5f; svf.x=-4f;
		float n= nlayers/4;
		for(float i=0f;ilayer<nlayers;ilayer++, i+=1/n){
			f=(sf*i+fo*(1-i));
			s=ff*i+so*(1-i);
			sv=svf*i+svo*(1-i);
			f.y=s.y=sv.y= 1f*ilayer/2f;
			
			layers[ilayer].Add(f);layers[ilayer].Add(s);layers[ilayer].Add(sv);
		}
		udl ("number of nodes final= "+layers.Count);
//		return layers;
	}
	
	public void generate_rotating_gaurd(){
		Vector3 f=new Vector3(-2f,0,-1f), s=new Vector3(-2f,0,3f), sv= new Vector3(2f,0,1f);
		for(int ilayer=0;ilayer<nlayers;ilayer++){
			f=RotatePointAroundPivot(f,sv,new Vector3(0, -18f, 0));
			s=RotatePointAroundPivot(s,sv,new Vector3(0, -18f, 0));
			//sv=q * sv;
			f.y=s.y=sv.y=1f*ilayer;
			
			layers[ilayer].Add(f);layers[ilayer].Add(s);layers[ilayer].Add(sv);
		}
//		return layers;
	}

	
	
	Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
		Vector3 dir = point - pivot; // get point direction relative to pivot
		dir = Quaternion.Euler(angles) * dir; // rotate it
		point = dir + pivot; // calculate rotated point
		return point; // return it
	}
	//	void TriangulateFirstandLastLayer(List<List<Vector3>> layers, List<List<int>> polygons){
	//		List<Ligne> allines,actuallines= new List<Ligne>();
	//		List<Vector3>allvertices=new List<Vector3>();
	//		allvertices.AddRange(layers[0]);
	//		bool [,]LineMap= new bool[allvertices.Count,allvertices.Count];
	//		int ii=0;
	//		foreach(var polygon in polygons){
	//			for(int i=0; i<polygon.Count; i++ ){
	//				actuallines.Add(new Ligne(allvertices[i+ii],allvertices[((i+1)%polygon.Count)+ii
	//				                                                    ,i+ii,((i+1)%polygon.Count)+ii]));
	//				LineMap[i+ii,((i+1)%polygon.Count)+ii]=true;
	//			}
	//			ii+=polygon.Count;
	//		}
	//
	//		allines= new List<Ligne>(actuallines);
	//
	//		foreach(var line in actuallines){
	//			var v=line.vertexIndex;
	//			Vector3 v3=alreadyConnectedNotAdjacentVertex(v[0],v[1],LineMap);
	//			SortedList sl= ClosestVertex(allvertices[v[0]],allvertices[v[1]],allvertices);
	//			int k=0;
	//			//get the next closest vertex
	//			while(k<allvertices.Count-2){
	//				v3=sl[k];
	//				Ligne l1= new Ligne(v3,v[0]),l2= new Ligne(v3,v[1]);
	//
	//				//check if v3-v1 and v3-v2 doesn't intersect other lines
	//				foreach(var otherline in allines){
	//					//ignore lines passing through the 3rd vertex
	//					if(otherline.vertex[0]== v3||otherline.vertex[1]== v3)
	//						continue;
	//					if(l1.LineIntersection(otherline)||l2.LineIntersection(otherline)){
	//						break;
	//					}
	//				}
	//			}
	//		}
	//	}
	//
	//
	//	bool notIntersectAnyLine(){}
	
	//
	//	Vector3 alreadyConnectedNotAdjacentVertex(int a, int b, bool[,] LineMap){
	//		for(int i=0;i<LineMap.GetLength(1);i++){
	//			if(i!=a && i!=b && (LineMap[i][a] || LineMap[a][i]) && (LineMap[i][b] || LineMap[b][i])){
	//				return true;
	//			}
	//		}
	//		return false;
	//	}
	//	SortedList ClosestVertex(Vector3 v1,Vector3 v2, List<Vector3> list){
	//		var m= (v1 +v2)/2f;SortedList sl= new SortedList();
	//		for(int i=0;i<list.Count;i++){
	//			var v3=list[i];
	//			if(!v3.Equals(v1) && !v3.Equals(v2)){
	//				sl.Add(Vector3.Distance(m,v3),i);
	//			}
	//		}
	//		return sl;
	//	}

	/*void readcsv(){
		var reader = new StreamReader(File.OpenRead(@"C:\test.csv"));
		List<string> listA = new List<string>();
		List<string> listB = new List<string>();
		while (!reader.EndOfStream)
		{
			var line = reader.ReadLine();
			var values = line.Split(';');

			listA.Add(values[0]);
			listB.Add(values[1]);
		}

	}*/

}
