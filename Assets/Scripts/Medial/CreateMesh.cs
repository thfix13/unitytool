using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
public class CreateMesh : MonoBehaviour {
	char[] delimiterChars = { ' ', '\t' };

	public string file_prefix="cube";
	string medial_output;
	GameObject gameobj2;
	Transform cam;
	void Start () {
		string dir="/Users/dhsingh/Documents/Thesis/SM03Skeleton/";

		
		string original_file=file_prefix+".ply2";
		string multi_triangle_input_file= file_prefix+"_multi.ply2";
		//string outputfile= dir+file_prefix+"_medial_output.ply2";
		subdivide_Mesh(dir+original_file,dir+multi_triangle_input_file);

		ProcessStartInfo startInfo = new ProcessStartInfo()
		{
			FileName = "/Users/dhsingh/Documents/Thesis/SM03Skeleton/run.sh",
			Arguments = multi_triangle_input_file,
		};
		Process proc = new Process()
		{
			StartInfo = startInfo,
		};
		proc.Start();
		waitforsometime();
//		while (!proc.StandardOutput.EndOfStream) {
//						string line = proc.StandardOutput.ReadLine();
//						UnityEngine.Debug.Log(line);
//					}

		proc.WaitForExit();
//		UnityEngine.Debug.Log("exited");
		proc.Close();
		medial_output="output_medial_"+multi_triangle_input_file;

		GameObject gameobj = GameObject.Find ("Box");
		buildObject (multi_triangle_input_file,gameobj);
		SetAlpha(gameobj.GetComponent<Renderer>().material,0.99f);
		
		gameobj2 = GameObject.Find ("Medial");
		buildObject (medial_output,gameobj2);
		//SetAlpha(gameobj2.renderer.material,0.6f);
		cam = GameObject.FindGameObjectWithTag("Cam").transform;
	}
	public static void udl(String s){
		UnityEngine.Debug.Log(s);
	}
	IEnumerator waitforsometime() {
		print(Time.time);
		yield return new WaitForSeconds(2);
		print(Time.time);
	}
	public static void SetAlpha (Material material, float value) {
		Color color = material.color;
		color.a = value;
		material.color = color;
	}
	

	void writePLY(List<Vector3> vertices_orig, List<Vector3>vertices_final, List<List<int>> polygons){

		foreach(var polygon in polygons){
			int i=0, n=polygon.Count;
			//ply_triangles.Add("3 "+);
			String s= "3 "+i+" "+(i+1)%n+" "+(i%n)+n;
			String t= "3 "+(i%n)+n+" "+(i+1)%n+" "+(i%n)+1+n;
			udl (s);
			udl (t);
		}

	}
	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.DownArrow))
		{
			cam.Translate(Vector3.down * .5f,Space.Self);
			cam.LookAt(new Vector3(2f,2f,2f));
//			cam.Rotate(Vector3.left);
		}
		if (Input.GetKey(KeyCode.UpArrow))
		{
			cam.Translate(Vector3.up * .50f,Space.Self);
			
			cam.LookAt(new Vector3(2f,2f,2f));
		}
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			cam.Translate(Vector3.left * 0.5f,Space.Self);
			
			cam.LookAt(new Vector3(2f,2f,2f));
		}
		if (Input.GetKey(KeyCode.RightArrow))
		{
			cam.Translate(Vector3.right * 0.5f,Space.Self);
			cam.LookAt(new Vector3(2f,2f,2f));
		}
	}

	
	void buildObject(string InputFile, GameObject go){
		List <Vector3> newVertices;
		List <int> newTriangles;
		string []objectFile;
		objectFile = System.IO.File.ReadAllLines(@"/Users/dhsingh/Documents/Thesis/SM03Skeleton/"+InputFile);
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

	void subdivide_Mesh(string inputfile, string outputfile){
		List <Vector3> Vertices;
		List <int> triangles;
		string []objectFile;
		objectFile = System.IO.File.ReadAllLines(inputfile);
		int nvertices = Convert.ToInt16(objectFile [0]) ;
		int ntriangles = Convert.ToInt16(objectFile [1]);
		Vertices = new List<Vector3>(nvertices);
		triangles =  new List<int>(ntriangles);
		
		
		string []parsed;
		float a,b, c;
		int vPointer=2,vFcounter=0;
		
		//Debug.Log(nvertices+","+ntriangles);
		
		for( vPointer=2; vFcounter <nvertices;vPointer++, vFcounter++){
			parsed= objectFile[vPointer].Split(delimiterChars);
			a=float.Parse(parsed[0], System.Globalization.CultureInfo.InvariantCulture);
			b=float.Parse(parsed[1], System.Globalization.CultureInfo.InvariantCulture);
			c=float.Parse(parsed[2], System.Globalization.CultureInfo.InvariantCulture);
			Vertices.Add(new Vector3(a,b,c));
			
		}
		
		int j=0;
		for(vFcounter=0; j< ntriangles ;vFcounter=vFcounter+3,j++,vPointer++){
			parsed= objectFile[vPointer].Split(delimiterChars);
			triangles.Add(Convert.ToInt16(parsed[1]));
			triangles.Add(Convert.ToInt16(parsed[2]));
			triangles.Add(Convert.ToInt16(parsed[3]));
		}
		
		
		
		List<Vector3> newvertices=new List<Vector3>();
		List<Vector3> lnew= new List<Vector3>();
		List<int> newtriangles= new List<int>();
		Dictionary<Vector3,int> mapping= new Dictionary<Vector3, int>();
		int vid=0;
		for(int ii=0; ii< triangles.Count;ii=ii+3){
			Vector3 aa,bb,cc;
			aa=Vertices[triangles[ii]];
			bb=Vertices[triangles[ii+1]];
			cc=Vertices[triangles[ii+2]];
			
			lnew=subdivide(aa,bb,cc,16);
			
			for(int iii=0;iii<lnew.Count;iii++){
				if(mapping.ContainsKey(lnew[iii]))
				{
					newtriangles.Add(mapping[lnew[iii]]);
				}
				else
				{
					mapping.Add(lnew[iii],vid);
					newtriangles.Add(vid++);
					newvertices.Add(lnew[iii]);
				}
				
			}
		}
		
		//Write to file
		using (System.IO.StreamWriter file= new System.IO.StreamWriter(outputfile)){
			file.WriteLine(newvertices.Count);
			file.WriteLine(newtriangles.Count/3);
			foreach(Vector3 vertex in newvertices){
				file.WriteLine(vertex.x+" "+vertex.y+" "+vertex.z);
			}
			for(int ii=0;ii<newtriangles.Count;ii=ii+3){
				file.WriteLine("3 " +newtriangles[ii]+" "+newtriangles[ii+1]+" "+newtriangles[ii+2]);
			}
			file.Close();
		}
		//using(Process.Start("./Skeleton",outputfile +" "+"medial_"+outputfile)){
		
		//}
	}

	List<Vector3> subdivide(Vector3 a, Vector3 b, Vector3 c, int level){
		List <Vector3> l1,l2,l3,l4;
		if(level >1){
			Vector3 d= (a+b)/2;
			Vector3 e= (b+c)/2;
			Vector3 f= (a+c)/2;
			
			l1=subdivide(f,e,c,level/2);
			l2=subdivide(f,a,d,level/2);
			l3=subdivide(e,d,b,level/2);
			l4=subdivide(d,e,f,level/2);
			l1.AddRange(l2);l1.AddRange(l3);l1.AddRange(l4);
			return l1;
		}
		else{
			return new List<Vector3>(){a,b,c};
		}
	}
}