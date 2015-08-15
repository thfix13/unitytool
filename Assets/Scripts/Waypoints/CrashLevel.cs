using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
public partial class Visibility1 : MonoBehaviour 
{
public void drawingFromFile_LevelCrash ()
{
	
	new GameObject ("temp");
	
	new GameObject ("Walls");
	
	new GameObject ("Obstacles");
	
	new GameObject ("Borders");
	
	float scalingFactor = 20f;
	
	float displacementFactorX = -40f;
	
	float displacementFactorZ = 40f;
	
	// float scalingFactor = 1f;
	
	// float displacementFactorX = 0f;
	
	// float displacementFactorZ = 0f;
	
	// Line a = new Line (new Vector3 (0, 1, 0), new Vector3 (10, 1, 0));
	
	// a.DrawVector (GameObject.Find ("temp"));
	
	//scalingFactor and dispFact need to be changed for this -> 
	
	var reader = new StreamReader(File.OpenRead(Application.dataPath+"\\Levels\\Crash.csv"));
	
	List<string> coord = new List<string> ();
	
	Geometry walls = new Geometry ();
	
	int wallcnt = 0;
	
	int mapcnt = 0;
	
	while ( !reader.EndOfStream )
		
	{
		
		var line = reader.ReadLine();
		
		var values = line.Split(';');
		
		//Check the symbol in first column of line
		
		if( values[0].Equals("M") )
		{
			
			//Coordinates in 2nd and 3rd column. Take them in as separate lists of floats.
			
			var prev = values[1].Split(',');
			
			var current = values[2].Split(',');
			
			Vector3 a = new Vector3();
			
			Vector3 b = new Vector3(); 
			
			a.x = float.Parse( prev[0] )/scalingFactor;
			
			a.z = (-float.Parse( prev[1] )/scalingFactor) + displacementFactorZ;
			
			// a.z = (float.Parse( prev[1] )/scalingFactor) + displacementFactorZ;
			
			a.y = b.y = 1;
			
			b.x = float.Parse( current[0] )/scalingFactor;
			
			b.z = (-float.Parse( current[1] )/scalingFactor) + displacementFactorZ;
			
			// b.z = (float.Parse( current[1] )/scalingFactor) + displacementFactorZ;
			
			Line ln = new Line( a, b );
			
			ln.name = "Border "+mapcnt++.ToString();
			
			//ln.DrawVector(GameObject.Find ("Borders"));
			
			mapBG.edges.Add(ln);
			
		}
		
		else if( values[0].Equals("OBS") )
		{
			
			float x = float.Parse(values[1])/scalingFactor;
			
			float z = (-float.Parse(values[2])/scalingFactor) + displacementFactorZ;
			
			float width  = float.Parse(values[3])/scalingFactor;
			
			float height = -float.Parse(values[4])/scalingFactor;
			
			Vector3 a = new Vector3();
			
			Vector3 b = new Vector3();
			
			Vector3 c = new Vector3();
			
			Vector3 d = new Vector3();
			
			a.y = b.y = c.y = d.y = 1;
			
			a.x = x; a.z = z;
			
			b.x = x + width; b.z = z;
			
			c.x = x + width; c.z = z + height;
			
			d.x = x; d.z = z + height;
			
			Geometry g = new Geometry();
			
			g.edges.Add( new Line( a, b ) );
			
			g.edges.Add( new Line( b, c ) );
			
			g.edges.Add( new Line( c, d ) );
			
			g.edges.Add( new Line( d, a ) );
			
			obsGeos.Add(g);
			
			//g.DrawGeometry(GameObject.Find ("Obstacles"));
			
		}
		
		else
		{
			
			line = reader.ReadLine();
			
			values = line.Split(';');
			
			Geometry g = new Geometry();
			
			while( values[0].Equals("OB") ){
				
				var prev = values[1].Split(',');
				
				var current = values[2].Split(',');
				
				Vector3 a = new Vector3();
				
				Vector3 b = new Vector3(); 
				
				a.x = float.Parse( prev[0] )/scalingFactor;
				
				a.z = (-float.Parse( prev[1] )/scalingFactor) + displacementFactorZ;
				
				// a.z = (float.Parse( prev[1] )/scalingFactor) + displacementFactorZ;
				
				a.y = b.y = 1;
				
				b.x = float.Parse( current[0] )/scalingFactor;
				
				b.z = (-float.Parse( current[1] )/scalingFactor) + displacementFactorZ;
				
				// b.z = (float.Parse( current[1] )/scalingFactor) + displacementFactorZ;
				
				Line ln = new Line( a, b );
				
				g.edges.Add( ln );
				
				ln.name = wallcnt.ToString();
				
				//ln.DrawVector(GameObject.Find("temp"));
				
				line = reader.ReadLine();
				
				values = line.Split(';');
				
			}
			
			wallcnt++;
			
			obsGeos.Add(g);
			
		}
		
	}
	
	/*if( mapBG.validate() == false )
		Debug.Log("Map Border Invalid");
	
	foreach (Geometry g in obsGeos) 
	{
		
		if( !g.validate() )
		{
			Debug.Log("Obstacle Invalid");
			g.DrawGeometry(GameObject.Find("temp"));
			
		}
		
	}*/
	
}
}