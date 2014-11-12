using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
public class TriangleSubdivide : MonoBehaviour {
	char[] delimiterChars = { ' ', '\t' };

	void Start () {
		string dir="/Users/dhsingh/Documents/Thesis/SM03Skeleton/";
		string file_prefix="cube2";

		string original_file=file_prefix+".ply2";
		string multi_triangle_input_file= file_prefix+"_multi.ply2";
		//string outputfile= dir+file_prefix+"_medial_output.ply2";
		subdivide_Mesh(dir+original_file,dir+multi_triangle_input_file);

//		ProcessStartInfo procs = new ProcessStartInfo ();
//		procs.FileName = "sh";
//		procs.WorkingDirectory = dir;
//		procs.Arguments = "run.sh "+multi_triangle_input_file;
//		procs.UseShellExecute = true;
//		procs.RedirectStandardOutput = false;
//		procs.CreateNoWindow = true;
//			
//		Process proc=new Process();
//		proc.StartInfo=procs;
//
//		proc.Start();
//		proc.Close();
//		proc.WaitForExit();

//		while (!proc.StandardOutput.EndOfStream) {
//			string line = proc.StandardOutput.ReadLine();
//			UnityEngine.Debug.Log(line);
//		}

		//UnityEngine.Debug.Log(multi_triangle_input_file+"\ndone");


	}


	///<summary>
	/// Reads the ply2 format file and subdivides the triangulations into smaller multiple number
	/// of triangles and writes to new file
	/// </summary>
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
	// Update is called once per frame
	void Update () {
	
	}
	List<Vector3> subdivide(Vector3 a, Vector3 b, Vector3 c, int level){
		List <Vector3> l1,l2,l3,l4,r1,r2,r3;
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
	/*
	void test(){
		List <Vector3> Vertices=new List<Vector3>(){new Vector3(0,0,0),new Vector3(0,2,0), new Vector3(2,2,0),new Vector3(2,0,0)};
		List <int> triangles= new List<int>(){0,1,3,2,1,3};
		List<Vector3> l=new List<Vector3>();
		List<Vector3> lnew= new List<Vector3>();
		List<int> newtriangles= new List<int>();
		Dictionary<Vector3,int> mapping= new Dictionary<Vector3, int>();
		int vid=0;
		for(int ii=0; ii< triangles.Count;ii=ii+3){
			Vector3 aa,bb,cc;
			aa=Vertices[triangles[ii]];
			bb=Vertices[triangles[ii+1]];
			cc=Vertices[triangles[ii+2]];
			
			lnew=subdivide(aa,bb,cc,4);
			
			for(int i=0;i<lnew.Count;i++){
				if(mapping.ContainsKey(lnew[i]))
				{
					newtriangles.Add(mapping[lnew[i]]);
				}
				else
				{
					mapping.Add(lnew[i],vid);
					newtriangles.Add(vid++);
					l.Add(lnew[i]);
				}
				
			}
		}
		for(int i=0;i<l.Count;i++)
			Debug.Log(i+" = "+l[i]);
		for(int i=0; i<newtriangles.Count;i=i+3){
			Debug.Log(newtriangles[i]+" ,"+newtriangles[i+1]+" ,"+newtriangles[i+2]);
		}
		//Color32 currentColor = new Color32();
		Color32[] colors = new Color32[l.Count];
		for(int i=0; i<l.Count;i++){
			colors[i]=new Color(
				UnityEngine.Random.Range (0.0f, 1.0f),
				UnityEngine.Random.Range (0.0f, 1.0f),
				UnityEngine.Random.Range (0.0f, 1.0f),
				1.0f);
		}
		
		GameObject gameobj = GameObject.Find ("Box");
		MeshFilter ms = gameobj.GetComponent <MeshFilter> ();
		Mesh mesh = new Mesh ();
		ms.mesh = mesh;
		mesh.vertices=l.ToArray();
		mesh.triangles=newtriangles.ToArray();
		mesh.colors32=colors;


	}*/
}