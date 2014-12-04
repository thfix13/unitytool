using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
public class CreateLayeredMesh : MonoBehaviour {
	char[] delimiterChars = { ' ', '\t' };
	bool cond=true;
	public string file_prefix="rotation_gaurd";
	GameObject gameobj2;
	Transform cam;

	void readcsv(){
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

	}
	string dir=@"/Users/dhsingh/Documents/Thesis/SM03Skeleton/";
	void Start () {

		List <List<Vector3>> layers =generate_arena2(40);
		layers=generate_moving_gaurd(layers,40);

		List<List<int>> polygons= new List<List<int>>{new List<int>{0,1,2,3,4,5,6,7}, new List<int>{10,9,8}};

		comb combb= subdivide_layer(layers,polygons,10f);
		layers=combb.getLayer();
		polygons=combb.getPoly();
//		layers=addLayers(layers,4f);


		VertTria vt= assignPLY(layers,polygons);

		string multi_triangle_input_file= file_prefix+".ply2";
		writePLY(dir+multi_triangle_input_file,vt.getVertices(),vt.getTriangles());

		GameObject gameobj = GameObject.Find ("Box");
		buildplyObject (gameobj,vt.getVertices(),vt.getTriangles());
		SetAlpha(gameobj.renderer.material,0.99f);

		
//		ProcessStartInfo startInfo = new ProcessStartInfo()
//		{
//			FileName = "/Users/dhsingh/Documents/Thesis/SM03Skeleton/run.sh",
//			Arguments = multi_triangle_input_file,
//		};
//		Process proc = new Process()
//		{
//			StartInfo = startInfo,
//		};
//		proc.Start();
//
//		proc.WaitForExit();
//
//		proc.Close();
//		
//		string medial_output=dir+"output_medial_"+multi_triangle_input_file;
//
//
//		gameobj2 = GameObject.Find ("Medial");
//		buildObject (medial_output,gameobj2);
//		SetAlpha(gameobj2.renderer.material,0.6f);
//		//cam = GameObject.FindGameObjectWithTag("Cam").transform;
	}
	List<List<Vector3>> generate_arena2(int nlayers){
		Vector3 one= new Vector3(-6f,0f,0f),
		two=new Vector3(-2f,0f,0f), 
		three= new Vector3(-2f,0f,-2f), 
		four= new Vector3(2f,0f,-2f),
		five=new Vector3(2f,0f,0f), 
		six=new Vector3(6f,0f,0f),
		sev=new Vector3(6f,0f,3f), 
		ei=new Vector3(-6f,0f,3f); 
		List<List<Vector3>> arena= new List<List<Vector3>>();
		for(int ilayer=0;ilayer<nlayers;ilayer++){
			one.y= two.y= three.y= four.y=five.y=six.y=sev.y=ei.y=1f*ilayer/2;
			arena.Add(new List<Vector3>{one, two,three,four,five,six,sev,ei});
		}
		return arena;
	}

	List<List<Vector3>> generate_arena(int nlayers){
		Vector3 one= new Vector3(-4f,0f,-4f),
		two=new Vector3(4f,0f,-4f), 
		three= new Vector3(4f,0f,4f), 
		four= new Vector3(-4f,0f,4f);
		List<List<Vector3>> arena= new List<List<Vector3>>();
		for(int ilayer=0;ilayer<nlayers;ilayer++){
			one.y= two.y= three.y= four.y=1f*ilayer;
			arena.Add(new List<Vector3>{one, two,three,four});
		}
		return arena;
	}

	List<List<Vector3>> generate_moving_gaurd(List<List<Vector3>> arena ,int nlayers){
		udl ("number of nodes= "+arena.Count);
		Vector3 fo=new Vector3(-4f,0,2.5f), so=new Vector3(-4f,0,0.5f), svo= new Vector3(-5.5f,0,1.5f);
		Vector3 ff=new Vector3(5.5f,0,2.5f), sf=new Vector3(5.5f,0,0.5f), svf= new Vector3(4f,0,1.5f);
		int ilayer;
		Vector3 f= new Vector3(),s=new Vector3(),sv=new Vector3();
		for(ilayer=0;ilayer<nlayers/4f;ilayer++){
			f=(ff*ilayer+fo*(nlayers/4 -ilayer))/(nlayers/4);
			s=(sf*ilayer+so*(nlayers/4 -ilayer))/(nlayers/4);
			sv=(svf*ilayer+svo*(nlayers/4 -ilayer))/(nlayers/4);
			f.y=s.y=sv.y= 1f*ilayer/2f;
			arena[ilayer].Add(f);arena[ilayer].Add(s);arena[ilayer].Add(sv);
		}

		var theta = 180f/(nlayers/2f);
		for(;ilayer<3f*nlayers/4f;ilayer++){
			f=RotatePointAroundPivot(f,sv,new Vector3(0, -theta, 0));
			s=RotatePointAroundPivot(s,sv,new Vector3(0, -theta, 0));
			f.y=s.y=sv.y=1f*ilayer/2f;
			
			arena[ilayer].Add(f);arena[ilayer].Add(s);arena[ilayer].Add(sv);
		}

		fo=f;so=s;svo=sv;
		ff.x=-5.5f; sf.x=-5.5f; svf.x=-4f;
		float n= nlayers/4;
		for(float i=0f;ilayer<nlayers;ilayer++, i+=1/n){
			f=(sf*i+fo*(1-i));
			s=ff*i+so*(1-i);
			sv=svf*i+svo*(1-i);
			f.y=s.y=sv.y= 1f*ilayer/2f;
			
			arena[ilayer].Add(f);arena[ilayer].Add(s);arena[ilayer].Add(sv);
		}
		udl ("number of nodes final= "+arena.Count);
		return arena;
	}

	List<List<Vector3>> generate_rotating_gaurd(List<List<Vector3>> arena ,int nlayers){
		Vector3 f=new Vector3(-2f,0,-1f), s=new Vector3(-2f,0,3f), sv= new Vector3(2f,0,1f);
		for(int ilayer=0;ilayer<nlayers;ilayer++){
			f=RotatePointAroundPivot(f,sv,new Vector3(0, -18f, 0));
			s=RotatePointAroundPivot(s,sv,new Vector3(0, -18f, 0));
			//sv=q * sv;
			f.y=s.y=sv.y=1f*ilayer;

			arena[ilayer].Add(f);arena[ilayer].Add(s);arena[ilayer].Add(sv);
		}
		return arena;
	}
 	Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
		Vector3 dir = point - pivot; // get point direction relative to pivot
		dir = Quaternion.Euler(angles) * dir; // rotate it
		point = dir + pivot; // calculate rotated point
		return point; // return it
	}
	public static void udl(object s){
		UnityEngine.Debug.Log(s);
	}

	public static void SetAlpha (Material material, float value) {
		Color color = material.color;
		color.a = value;
		material.color = color;
	}

	int n=0;
//	List<Vector3> ply_vertices;
//	List<int> ply_triangles;

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
	/// Obtains vertices and traingles from list of layers and list of closed polygons.
	/// Used to apply medial skeleton algo and also create mesh
	/// </summary>
	/// <param name="layers">Layers.</param>
	/// <param name="polygons">Polygons.</param>
	VertTria assignPLY(List<List<Vector3>> layers, List<List<int>> polygons){
		List<Vector3> ply_vertices; 
		List<int> ply_triangles;
		ply_vertices= new List<Vector3>();
		ply_triangles= new List<int>();
		foreach(var layer in layers){
			ply_vertices.AddRange(layer);
		}
		int nn=layers[0].Count;
		for(int ilayer=0;ilayer<layers.Count-1;ilayer++){
			int vertices_yet=0;
			//for each layer, add the triangulations involved to ply_traingles
			int i=0;
			foreach(var polygon in polygons){
				//number of vertices in the polygon
				int n=polygon.Count;
				for(; i<n+vertices_yet;i++){
					ply_triangles.AddRange(new List<int>{((i%n)+nn*ilayer+vertices_yet),
						(((i+1)%n)+nn*ilayer+vertices_yet),((i%n)+nn*(1+ilayer)+vertices_yet),
						((i%n)+nn*(1+ilayer)+vertices_yet),
							(((i+1)%n)+nn*ilayer+vertices_yet),(((i+1)%n)+nn*(1+ilayer)+vertices_yet)
					});
//					String s= ((i%n)+nn*ilayer+vertices_yet)+" "+
//						(((i+1)%n)+nn*ilayer+vertices_yet)+" "+((i%n)+nn*(1+ilayer)+vertices_yet);
//					String t= ((i%n)+nn*(1+ilayer)+vertices_yet)+" "+
//						(((i+1)%n)+nn*ilayer+vertices_yet)+" "+(((i+1)%n)+nn*(1+ilayer)+vertices_yet);
//
//					udl (s);
//					udl (t);
				}
				vertices_yet+=n;
			}
		}
		VertTria vt= new VertTria(ply_vertices,ply_triangles);
		return vt;
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
//					layer=layers[ilayer+1];
//					
//					//get the corresponding vertices  in the next layer
//					Vector3 vnext1=layer[polygon[vertex]],vnext2=layer[polygon[(vertex+1)%n]];
//					for(float i=0; i<1;i=i+0.1f){
//						newlayervertices.Add(v1*(1-i)+v2*i);
//					}
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