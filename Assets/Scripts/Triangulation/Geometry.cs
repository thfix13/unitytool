using System;
using System.Collections.Generic;
using UnityEngine;
using Common;
using Exploration;
using Objects;

// [Serializable]
public class Geometry
{

    public bool debuggery = false;

    public List<Line> edges = new List<Line>();

    //Added for MST Code
    public List<Geometry> voisins = new List<Geometry>();
    public List<Line> voisinsLine = new List<Line>();
    public bool visited = false;

    public void DrawGeometry(GameObject parent)
    {
        Debug.Log("DRAWING LINES");
        Color c = new Color(UnityEngine.Random.Range(0.0f, 1.0f),
                           UnityEngine.Random.Range(0.0f, 1.0f),
                           UnityEngine.Random.Range(0.0f, 1.0f));

        foreach (Line l in edges) {
            l.DrawVector(parent, c);
        }
        //DrawVertex (parent);
    }



    public bool Collision(Geometry g)
    {
        foreach (Line l1 in edges)
        {
            foreach (Line l2 in g.edges)
            {
                if (l1 == l2)
                    continue;
                if (l1.LineIntersection(l2))
                    return true;
            }
        }
        return false;
    }

    public bool LineCollision(Line Lparam) {
        if (this.PointInside(Lparam.MidPoint()))
            return true;
        foreach (Line l1 in edges) {
            if (Lparam.LineIntersectMuntac(l1) == 1)
                return true;
        }
        return false;
    }

    //TODO: Switch to lineintmuntac
    public bool LineInside(Line l)
    {
        //Test if one of my line
        //This is not the best version should check is colinear inastead!
        foreach (Line myLine in edges)
        {
            
            if (myLine.vertex[0] == l.vertex[0]) {
                if (myLine.vertex[1] == l.vertex[1]) {
                    return false;
                }
            }
            else if (myLine.vertex[0] == l.vertex[1]) {
                if (myLine.vertex[1] == l.vertex[0]) {
                    return false;
                }
            }
            /*
        




			foreach(Vector3 v1 in myLine.vertex)
			{
				foreach(Vector3 v2 in l.vertex)
				{
					if(v1 == v2)
						return false; 
				}
			}*/
        }

        //Now we check count the intersection
        Vector3 mid = l.MidPoint();
        return PointInside(mid);
    }

    //TODO: Fix for lines colinear
    public bool PointInside(Vector3 pt)
    {
        //Debug.Log(pt);
        Line lray = new Line(pt, new Vector3(-100, 1, -100));
        int count = 0;
        /*if (debuggery) { 
            Debug.Log("LRAY:" + lray);
        }*/
        List<Vector3> intersectPoints = new List<Vector3>();
        Vector3 newPoint;
        bool alreadyThere = false;
        foreach (Line myLine in edges) {
            if (myLine.LineIntersectMuntacEndPt(lray) == 1) {
                newPoint = myLine.GetIntersectionPoint(lray);
                foreach(Vector3 point in intersectPoints) {
                    if((point- newPoint).magnitude < 0.001) {
                        alreadyThere = true;
                        break;
                    }
                }
                if (!alreadyThere) {
                    intersectPoints.Add(newPoint);
                    count++;
                }
                else {
                    alreadyThere = false;
                }
                //count++;
                /*if (debuggery){
                    Debug.Log(myLine);

                    Debug.Log("Intersection between" + myLine + " and " + lray);
                }*/
                //Check if the intersection point is on the polygon edge
                //Note: other checks tried but precision error kept coming up in cases
                Vector3 vtemp = myLine.GetIntersectionPoint(lray);
                if (Math.Abs(vtemp.x - pt.x) < 0.01 && Math.Abs(vtemp.z - pt.z) < 0.01)
                    return false;
            }
           // else {
            //    Debug.Log("NO Intersection between" + myLine + " and " + lray);
            //}
        }
        // Debug.Log(count);
        return count % 2 == 1;
    }

    public List<Vector3> GetVertex()
    {
        //Find vertex
        List<Vector3> vertex = new List<Vector3>();
        foreach (Line l in edges)
        {
            foreach (Vector3 v in l.vertex)
            {
                if (!vertex.Contains(v))
                    vertex.Add(v);
            }
        }
        return vertex;
    }

    public void DrawVertex(GameObject parent)
    {
        //Find vertex
        List<Vector3> vertex = new List<Vector3>();
        foreach (Line l in edges)
        {
            foreach (Vector3 v in l.vertex)
            {
                if (!vertex.Contains(v))
                    vertex.Add(v);

            }

        }

        //Draw
        foreach (Vector3 v in vertex)
        {
            GameObject inter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            inter.transform.position = v;
            inter.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            inter.transform.parent = parent.transform;
        }

    }

    public void CollisionDraw(Geometry g, GameObject parent)
    {
        foreach (Line l1 in edges)
        {
            foreach (Line l2 in g.edges)
            {
                if (l1 == l2)
                    continue;
                if (l1.LineIntersection(l2))
                {

                    Vector3 pos = l1.LineIntersectionVect(l2);
                    GameObject inter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    inter.transform.position = pos;
                    inter.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                    inter.transform.parent = parent.transform;
                }
            }
        }

    }

    //Figures out the boundary of the geometry
    public void BoundGeometry(Vector3[] boundary) {
        List<Line> removeLines = new List<Line>();
        int i;
        // Debug.Log (boundary [0]);
        // Debug.Log (boundary [2]);
        foreach (Line l in edges) {
            bool rem = false;
            for (i = 0; i < 2; i++) {
                if (l.vertex[i].x > boundary[0].x + 0.01 || l.vertex[i].x < boundary[2].x - 0.01 || l.vertex[i].z > boundary[0].z + 0.01 || l.vertex[i].z < boundary[2].z - 0.01)
                    rem = true;
            }
            if (rem)
                removeLines.Add(l);
        }
        foreach (Line l in removeLines)
            edges.Remove(l);
    }

    private int infiniLoopStop = 100;
    


    public Geometry GeometryMerge(Geometry G2) {
        if (GeometryInside(G2))
            return this;
        else if (G2.GeometryInside(this))
            return G2;

        //Geometry tempGeometry = new Geometry();
        //Two Geometry objects - G1 and G2
        Geometry G1 = this;
        //Create new called G3 which starts as an union of G1 and G2
        Geometry G3 = new Geometry();
        foreach (Line l in G1.edges) { 
             G3.edges.Add(l);

        }	
		foreach (Line l in G2.edges){
			G3.edges.Add(l);

        }

        Geometry G4 = new Geometry();

        foreach (Line l in G3.edges)
        {
            Vector3 v1 = l.vertex[0];
            Vector3 v2 = l.vertex[1];
            if (Vector3.SqrMagnitude(v1 - v2) > 0.000001f)
            {
                G4.edges.Add(l);
            }
        }




        //Check for intersection points among lines in G4
        bool done = false;
        bool difFound = false;
        int whileIndex = 1;
        int cs1 = 0;
        int cs2 = 0;
        int cs3 = 0;
        int cs4 = 0;
        int cs5 = 0;
        int cs6 = 0;

        while (!done)
        {
            for(int i = 0; i < G4.edges.Count; i++)
            {
                if (difFound)
                {
                    break;
                }
                for(int j = i+1; j < G4.edges.Count; j++)
                {
                    Line LA = G4.edges[i];
                    Line LB = G4.edges[j];
                    if (debuggery)
                    {
                        LA.debuggery2 = true;
                        LB.debuggery2 = true;
                    }
                    int caseType = LA.LineIntersectMuntac(LB);
                    LA.debuggery2 = false;
                    LB.debuggery2 = false;
                    if (caseType == 1)
                    {//Regular intersections
                        /*if (debuggery)
                        {
                            Debug.Log("CASETYPE 1");
                            Debug.Log(LA);
                            Debug.Log(LB);
                        }*/
                        cs1++;
                        Vector3 pt = LA.GetIntersectionPoint(LB);

                        if (false) {
                            Debug.Log("CASE11");
                            Debug.Log(pt);
                            Debug.Log(LA);
                            Debug.Log(LB);
                            //LA.debuggery2 = true;
                           // LB.debuggery2 = true;
                            //LA.LineIntersectMuntac(LB);
                            //LA.debuggery2 = false;
                            //LB.debuggery2 = false;

                        }

                        G4.edges.RemoveAt(j);
                        G4.edges.RemoveAt(i);
                        Line l;
                        if (Vector3.SqrMagnitude(pt - LA.vertex[0]) > 0.000001f) {
                            l = new Line(pt, LA.vertex[0]);
                            //if (!G2.LineInside(l) && !G1.LineInside(l)) {
                                G4.edges.Add(l);
                            //}
                            /*else {
                                Debug.Log(l);
                            }*/
                        }
                        if (Vector3.SqrMagnitude(pt - LA.vertex[1]) > 0.000001f) {
                            l = new Line(pt, LA.vertex[1]);
                            //if (!G2.LineInside(l) && !G1.LineInside(l)) {
                                G4.edges.Add(l);
                            //}
                            /*else {
                                Debug.Log(l);
                            }*/
                        }
                        if (Vector3.SqrMagnitude(pt - LB.vertex[0]) > 0.000001f) {
                            l = new Line(pt, LB.vertex[0]);
                            //if (!G2.LineInside(l) && !G1.LineInside(l)) {
                                G4.edges.Add(l);
                            //}
                            /*else {
                                Debug.Log(l);
                            }*/
                        }
                        if (Vector3.SqrMagnitude(pt - LB.vertex[1]) > 0.000001f) {
                            l = new Line(pt, LB.vertex[1]);
                            //if (!G2.LineInside(l) && !G1.LineInside(l)) {
                                G4.edges.Add(l);
                            //}
                            /*else {
                                Debug.Log(l);
                            }*/
                        }
                        difFound = true;
                        break;
                    }  
                }
            }
            
            if (difFound)
            {
                difFound = false;
            }
            else
            {
                done = true;
            }
            whileIndex++;
            if(whileIndex > infiniLoopStop)
            {
                Debug.Log("INFINILOOP3");
                Debug.Log(cs1 + "," + cs2 + "," + cs3 + "," + cs4 + "," + cs5 + "," + cs6);
                break;
            }
        }

        List<Line> newerEdges = new List<Line>();
        foreach (Line l in G4.edges) {
            if (!G2.LineInside(l) && !G1.LineInside(l)) {
                newerEdges.Add(l);
            }
            else {
               // Debug.Log("REMOVED LINE" + l);
            }
        }
        G4.edges = newerEdges;
        
        done = false;
        difFound = false;
        whileIndex = 1;
        //Debug.Log("LOOP 2");
        while (!done) {
             for (int i = 0; i < G4.edges.Count; i++) {
                 if (difFound) {
                       break;
                 }
                        for (int j = i + 1; j < G4.edges.Count; j++) {
                            Line LA = G4.edges[i];
                            Line LB = G4.edges[j];
                    // Debug.Log("PAIR OF LINES");
                    // Debug.Log(LA);
                    // Debug.Log(LB);

                    if (debuggery) {
                        LA.debuggery2 = true;
                        LB.debuggery2 = true;
                    }
                    int caseType = LA.LineIntersectMuntac(LB);
                    LA.debuggery2 = false;
                    LB.debuggery2 = false;
                    if (caseType == 1) {//Regular intersections
                                                /*if (debuggery)
                                                {
                                                    Debug.Log("CASETYPE 1");
                                                    Debug.Log(LA);
                                                    Debug.Log(LB);
                                                }*/
                                cs1++;
                                Vector3 pt = LA.GetIntersectionPoint(LB);

                                if (false) {
                                    Debug.Log("CASE1");
                                    Debug.Log(pt);
                                    Debug.Log(LA);
                                    Debug.Log(LB);


                                }

                                G4.edges.RemoveAt(j);
                                G4.edges.RemoveAt(i);
                                Line l;
                                if (Vector3.SqrMagnitude(pt - LA.vertex[0]) > 0.000001f) {
                                    l = new Line(pt, LA.vertex[0]);
                                    if (!G2.LineInside(l) && !G1.LineInside(l)) {
                                        G4.edges.Add(l);
                                    }
                                    /*else {
                                        Debug.Log(l);
                                    }*/
                                }
                                if (Vector3.SqrMagnitude(pt - LA.vertex[1]) > 0.000001f) {
                                    l = new Line(pt, LA.vertex[1]);
                                    if (!G2.LineInside(l) && !G1.LineInside(l)) {
                                        G4.edges.Add(l);
                                    }
                                    /*else {
                                        Debug.Log(l);
                                    }*/
                                }
                                if (Vector3.SqrMagnitude(pt - LB.vertex[0]) > 0.000001f) {
                                    l = new Line(pt, LB.vertex[0]);
                                    if (!G2.LineInside(l) && !G1.LineInside(l)) {
                                        G4.edges.Add(l);
                                    }
                                    /*else {
                                        Debug.Log(l);
                                    }*/
                                }
                                if (Vector3.SqrMagnitude(pt - LB.vertex[1]) > 0.000001f) {
                                    l = new Line(pt, LB.vertex[1]);
                                    if (!G2.LineInside(l) && !G1.LineInside(l)) {
                                        G4.edges.Add(l);
                                    }
                                    /*else {
                                        Debug.Log(l);
                                    }*/
                                }
                                difFound = true;
                                break;
                            }
                            else if (caseType == 2) {
                                cs2++;
                                Line LN = new Line(LA.vertex[0], LB.vertex[1]);

                                if (false) {
                                    Debug.Log("CASE2");
                                    Debug.Log(LN);
                                    Debug.Log(LA);
                                    Debug.Log(LB);

                                }
                                /*if (debuggery)
                                {
                                    Debug.Log("CASETYPE 2");
                                    Debug.Log(LA);
                                    Debug.Log(LB);
                                    Debug.Log(LN);
                                }*/
                                //ACBD
                                G4.edges.RemoveAt(j);
                                G4.edges.RemoveAt(i);
                                if (Vector3.SqrMagnitude(LN.vertex[0] - LN.vertex[1]) > 0.000001f) {
                                    G4.edges.Add(LN);
                                }
                                difFound = true;
                                break;
                            }
                            else if (caseType == 3) {
                                cs3++;
                                Line LN = new Line(LB.vertex[0], LA.vertex[1]);
                                if (false) {
                                    Debug.Log("CASE3");
                                    Debug.Log(LN);
                                    Debug.Log(LA);
                                    Debug.Log(LB);

                                }
                                /*if (debuggery)
                                {
                                    Debug.Log("CASETYPE 3");
                                    Debug.Log(LA);
                                    Debug.Log(LB);
                                    Debug.Log(LN);
                                }*/
                                //CADB
                                G4.edges.RemoveAt(j);
                                G4.edges.RemoveAt(i);
                                if (Vector3.SqrMagnitude(LN.vertex[0] - LN.vertex[1]) > 0.000001f) {
                                    G4.edges.Add(LN);
                                }
                                difFound = true;
                                break;
                            }
                            else if (caseType == 4) {
                                cs4++;
                                Line LN = new Line(LB.vertex[1], LA.vertex[1]);
                                if (false) {
                                    Debug.Log("CASE4");
                                    Debug.Log(LN);
                                    Debug.Log(LA);
                                    Debug.Log(LB);

                                }
                                G4.edges.RemoveAt(j);
                                G4.edges.RemoveAt(i);
                                if (Vector3.SqrMagnitude(LN.vertex[0] - LN.vertex[1]) > 0.000001f) {
                                    G4.edges.Add(LN);
                                }
                                difFound = true;
                                break;
                            }
                            else if (caseType == 5) {
                                cs5++;
                                if (false) {
                                    Debug.Log("CASE5");
                                    Debug.Log(LA);
                                    Debug.Log(LA);
                                    Debug.Log(LB);


                                }
                                G4.edges.RemoveAt(j);
                        difFound = true;
                        break;
                    }
                            else if (caseType == 6) {
                                cs6++;
                                if (false) {
                                    Debug.Log("CASE6");
                                    Debug.Log(LB);
                                    Debug.Log(LA);
                                    Debug.Log(LB);

                                }
                                G4.edges.RemoveAt(i);
                        difFound = true;
                        break;
                    }
                   /*else if(caseType == 0) {
                        if((Mathf.Abs((LA.MidPoint().x + 26)) < 0.05) && (Mathf.Abs((LB.MidPoint().x + 26)) < 0.05)) {
                            LA.debuggery2 = true;
                            LB.debuggery2 = true;
                            LA.LineIntersectMuntac(LB);
                            LA.debuggery2 = false;
                            LB.debuggery2 = false;
                        }
                    }*/
                        }
             }
                if (difFound) {
                    difFound = false;
                }
                else {
                    done = true;
                }
                whileIndex++;
                if (whileIndex > infiniLoopStop) {
                    Debug.Log("INFINILOOP3");
                    Debug.Log(cs1 + "," + cs2 + "," + cs3 + "," + cs4 + "," + cs5 + "," + cs6);
                    break;
                }
            }
        


        /*
		for (int i = 0; i < G4.edges.Count; i++) {
			for( int j = i + 1; j < G4.edges.Count; j++ ) {
				Line LA = G4.edges[i];
				Line LB = G4.edges[j];
				int caseType = LA.LineIntersectMuntac( LB );
				if( caseType == 1 ){//Regular intersections
                    Vector3 pt = LA.GetIntersectionPoint( LB );
					G4.edges.Add( new Line( pt, LA.vertex[0] ) );
					G4.edges.Add( new Line( pt, LA.vertex[1] ) );
					G4.edges.Add( new Line( pt, LB.vertex[0] ) );
					G4.edges.Add( new Line( pt, LB.vertex[1] ));
					G4.edges.RemoveAt(j);
					G4.edges.RemoveAt(i);
					i--;
					break;
				}
                if(caseType == 2)
                {
                    //ACBD
                    G4.edges.Add(new Line(LA.vertex[0], LB.vertex[1]));
                    G4.edges.RemoveAt(j);
                    G4.edges.RemoveAt(i);
                    i--;
                    break;
                }
                if(caseType == 3)
                {
                    //CADB
                    G4.edges.Add(new Line(LB.vertex[0], LA.vertex[1]));
                    G4.edges.RemoveAt(j);
                    G4.edges.RemoveAt(i);
                    i--;
                    break;
                }
			}
		}*/
        //Check: Points inside Polygon
        //Check all midpoint of each line in G4 to see if it lies in G1 or G2. If inside remove.
        Geometry toReturn = new Geometry();

		foreach(Line l in G4.edges){
            if (!G1.LineInside(l) && !G2.LineInside(l)){
                toReturn.edges.Add(l);
            }
		}

        //I Added this to prevent stupid things

        List<Line> newEdges = new List<Line>();
        foreach(Line l in toReturn.edges)
        {
            Vector3 v1 = l.vertex[0];
            Vector3 v2 = l.vertex[1];
            if(Vector3.SqrMagnitude(v1 - v2) > 0.000001f)
            {
                newEdges.Add(l);
            }
        }

        toReturn.edges = newEdges;

		return toReturn;
	}
	//Used only for merging polygons with the map boundary
	public Geometry GeometryMergeInner( Geometry G2 ){
		//Geometry tempGeometry = new Geometry ();
		//Two Geometry objects - G1 and G2
		Geometry G1 = this;
		//Create new called G3 which starts as an union of G1 and G2
		Geometry G3 = new Geometry ();
		foreach (Line l in G1.edges)
			G3.edges.Add(l);		
		foreach (Line l in G2.edges)
			G3.edges.Add(l);


        Geometry G4 = new Geometry();

        foreach (Line l in G3.edges)
        {
            Vector3 v1 = l.vertex[0];
            Vector3 v2 = l.vertex[1];
            if (Vector3.SqrMagnitude(v1 - v2) > 0.000001f)
            {
                G4.edges.Add(l);
            }
        }




        //Check for intersection points among lines in G4
        bool done = false;
        bool difFound = false;
        int whileIndex2 = 1;
        int cas1 = 0;
        int cas2 = 0;
        int cas3 = 0;
        int cas4 = 0;
        while (!done)
        {
            for (int i = 0; i < G4.edges.Count; i++)
            {
                if (difFound)
                {
                    break;
                }
                for (int j = i + 1; j < G4.edges.Count; j++)
                {
                    Line LA = G4.edges[i];
                    Line LB = G4.edges[j];
                    if (debuggery)
                    {
                        //Debug.Log("DEBUGGERY IS ACTIVATED");
                        //LA.debuggery2 = true;
                        //LB.debuggery2 = true;
                    }
                    int caseType = LA.LineIntersectMuntac(LB);
                    LA.debuggery2 = false;
                    LB.debuggery2 = false;
                    if (caseType == 1)
                    {
                        Vector3 pt = LA.GetIntersectionPoint(LB);
                        if (debuggery)
                        {
                            Debug.Log("CASE1");
                            Debug.Log(pt);
                            Debug.Log(LA);
                            Debug.Log(LB);

                        }


                        G4.edges.RemoveAt(j);
                        G4.edges.RemoveAt(i);
                        Line l;
                        if (Vector3.SqrMagnitude(pt - LA.vertex[0]) > 0.000001f)
                        {
                            l = new Line(pt, LA.vertex[0]);
                            //if (!G2.LineInside(l)){
                                G4.edges.Add(l);
                            //}
                            /*else {
                                Debug.Log(l);
                            }*/
                        }
                        if (Vector3.SqrMagnitude(pt - LA.vertex[1]) > 0.000001f)
                        {
                            l = new Line(pt, LA.vertex[1]);
                            //if (!G2.LineInside(l)) {
                                G4.edges.Add(l);
                            //}
                            /*else {
                                Debug.Log(l);
                            }*/
                        }
                        if (Vector3.SqrMagnitude(pt - LB.vertex[0]) > 0.000001f)
                        {
                            l = new Line(pt, LB.vertex[0]);
                            //if (!G2.LineInside(l)) {
                                G4.edges.Add(l);
                            //}
                            /*else {
                                Debug.Log(l);
                            }*/
                        }
                        if (Vector3.SqrMagnitude(pt - LB.vertex[1]) > 0.000001f)
                        {
                            l = new Line(pt, LB.vertex[1]);
                            //if (!G2.LineInside(l)) {
                                G4.edges.Add(l);
                           // }
                            /*else {
                                Debug.Log(l);
                            }*/
                        }
                        difFound = true;
                        cas1++;

                        break;
                    } 
                }
            }
            if (difFound)
            {
                difFound = false;
            }
            else
            {
                done = true;
            }
            whileIndex2++;
            if(whileIndex2 > infiniLoopStop)
            {
                Debug.Log("INFINILOOP4");
                Debug.Log(cas1 + ",<---->," + cas2 + ",<---->," + cas3 + ",<---->," + cas4);
                break;
            }
        }






        /*//Check for intersection points among lines in G3
		for (int i = 0; i < G3.edges.Count; i++) {
			for( int j = i + 1; j < G3.edges.Count; j++ ) {
				Line LA = G3.edges[i];
				Line LB = G3.edges[j];
				int caseType = LA.LineIntersectMuntac( LB );
				if( caseType == 1 ){//Regular intersections
					Vector3 pt = LA.GetIntersectionPoint( LB );
					G3.edges.Add( new Line( pt, LA.vertex[0] ) );
					G3.edges.Add( new Line( pt, LA.vertex[1] ) );
					G3.edges.Add( new Line( pt, LB.vertex[0] ) );
					G3.edges.Add( new Line( pt, LB.vertex[1] ));
					G3.edges.RemoveAt(j);
					G3.edges.RemoveAt(i);
					i--;
					break;
				}
			}
		}*/
        //Check: Points inside Polygon
        //Check all midpoint of each line in G3 to see if it lies in G1 or G2. If inside remove.
        Geometry toReturn = new Geometry();
		
		foreach(Line l in G4.edges){
			if(!G2.LineInside(l))
				toReturn.edges.Add(l);
		}

        List<Line> newEdges = new List<Line>();
        foreach (Line l in toReturn.edges)
        {
            Vector3 v1 = l.vertex[0];
            Vector3 v2 = l.vertex[1];
            if (Vector3.SqrMagnitude(v1 - v2) > 0.000001f)
            {
                newEdges.Add(l);
            }
        }

        toReturn.edges = newEdges;

        //Check pt inside in G2
        foreach (Line l in toReturn.edges) {
			if( G2.LineInside( l ) ){
				toReturn.edges.Remove(l);
				break;
			}
		}
		return toReturn;
	}

    //Check if two geometries intersect
    public bool GeometryIntersect(Geometry G2) {
        //Debug.Log("INTERSECT");
        //Debug.Log(this);
        //Debug.Log(G2);

        if (GeometryInside(G2))
        {
            //Debug.Log("G2 inside G1");
            return true;
        }
        else if (G2.GeometryInside(this)) {
           // Debug.Log("G1 inside G2");
            return true;
        }

		foreach( Line La in this.edges ){
            foreach (Line Lb in G2.edges)
            {
                if (La.LineIntersectMuntac(Lb) > 0) { 
                    //Debug.Log("Line Intersection");
                    //Debug.Log(La.vertex[0] + "," + La.vertex[1]);
                    //Debug.Log(Lb.vertex[0] + "," + Lb.vertex[1]);

                    return true;
                }
			}
		}
		return false;
	}
	
	private double CrossProduct( Vector2 a, Vector2 b ){
		return (a.x * b.y) - (a.y * b.x);
	}


    //Weird shape . geometryInside (square)
    //foreach line in square
    // if for each point, the point is inside shape
    // then return true.
	public bool GeometryInside( Geometry G2 ){
		foreach (Line L in G2.edges) {
			if( !PointInside( L.vertex[0] ) )
				return false;
			if( !PointInside( L.vertex[1] ) )
				return false;
		}
		return true;
	}

	//ported code. src:"Triangulation"
	public void SetVoisins(List<Geometry> geos)
	{
		foreach(Geometry g in geos)
		{
			if( g == this ) continue;
			Line voisinConnect = this.GetClosestLine(g,geos);
			if(voisinConnect != null)
			{
				voisins.Add (g);
				voisinsLine.Add(voisinConnect);
			}
		}
	}

	//ported code. src:"Triangulation"
	public Line GetClosestLine(Geometry g, List<Geometry> geos)
	{
		Line toReturn = null;
		float dist = 1000000; 
		
		foreach(Vector3 v1 in this.GetVertex())
		{
			foreach(Vector3 v2 in g.GetVertex())
			{
				Line l = new Line(v1,v2);
				
				//Check collision
				bool collisionFree = true;
				
				foreach(Geometry gCollision in geos)
				{
					
					if(gCollision.LineCollision(l))
						collisionFree = false; 
				}
				
				if(!collisionFree)
				{
					continue; 
				}
				
				if(l.Magnitude()<dist)
				{
					toReturn = l; 
					dist = l.Magnitude(); 
				}
			}
		}
		return toReturn; 
	}

	//ported code. src:"Triangulation"
	public Line GetClosestLine(Vector3 v,Geometry g, List<Geometry> geos)
	{
		Line toReturn = null;
		float dist = 1000000; 
		
		
		foreach(Vector3 v2 in g.GetVertex())
		{
			Line l = new Line(v,v2);
			
			//Check collision
			bool collisionFree = true;
			
			foreach(Geometry gCollision in geos)
			{
				if(this == gCollision)
					continue; 
				if(gCollision.LineCollision(l))
					collisionFree = false; 
			}
			
			if(!collisionFree)
			{
				continue; 
			}
			
			if(l.Magnitude()<dist)
			{
				toReturn = l; 
				dist = l.Magnitude(); 
			}
		}
		
		return toReturn; 
	}

	//ported code. src:"Triangulation"
	public Geometry findClosestQuad(Vector3 v,List<Geometry> geos,List<Geometry> already)
	{
		Geometry toReturn = null; 
		float dist = 100000000; 
		
		foreach(Geometry g in geos)
		{
			if(g == this || already.Contains(g))
				continue;
			
			Line l = this.GetClosestLine(v,g,geos); 
			
			if(l == null)
				continue;
			
			if( l.Magnitude() < dist)
			{
				toReturn = g; 
				dist = l.Magnitude (); 
			}
		}
		
		return toReturn; 
		
	}
    


    public override string ToString()
    {
        string toReturn = "";

        foreach(Line l in edges)
        {
            toReturn = toReturn + "-" + l.vertex[0] + "," + l.vertex[1];
        }
        return toReturn;

    }
}
