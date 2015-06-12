//HEADERFILE
#include <set>
#include <map>
#include <list>
#include <cmath>
#include <ctime>
#include <deque>
#include <queue>
#include <stack>
#include <cctype>
#include <cstdio>
#include <string>
#include <vector>
#include <cassert>
#include <cstdlib>
#include <cstring>
#include <sstream>
#include <iostream>
#include <algorithm>
#include "visilibity.hpp"

using namespace std;

double eps = 1e-10;//1.8e-15;//1e-12
int N, caseno = 0;

int isSame( double a, double b ){
   double diff = abs(a - b);
   if( diff < eps ) return 1;
   else return 0;
}

int main(){
	freopen("mapwithtour.csv", "r", stdin);
	freopen("vps.csv", "w", stdout);
	int cases, i, j, res;
	string str1;
	i = 0;
	VisiLibity::Environment gamemap;
	vector<VisiLibity::Polygon> polyvect;       
        vector<VisiLibity::Point> ds;
	int env_set = 0;
	int samecnt = 0;
	int vpnum = 0;
	while( getline(std::cin, str1) ){
	    char str2[100];
	    i++;
            str1.copy( str2, str1.length(), 0);
	    if( str2[0] != 'H' && str2[0] != 'T' && str2[0] != 'M'){
	        char *pch = strtok( str2, "," );
		double x = atof(pch);
		pch = strtok(NULL, ",");
		double y = atof(pch);
		VisiLibity::Point p = VisiLibity::Point( x, y );
		ds.push_back(p);
	    }
	    else if( str2[0] == 'M' || str2[0] == 'H' ){
		VisiLibity::Polygon pol = VisiLibity::Polygon( ds );
		ds.clear();
		polyvect.push_back(pol);
	    }
	    else if( str2[0] == 'T' ){
		if( env_set == 0 ){
		   env_set = 1;
		   gamemap = VisiLibity::Environment(polyvect);
		   //printf("%d\n", gamemap.h() );  
		   gamemap.enforce_standard_form(); 
		   //cout<<gamemap.is_valid();
		   if( !gamemap.is_valid() ){
			cout<<"Invalid Environment"<<endl;
			return 0;
		   }
		}
	        char *pch = strtok( str2, "," );
                pch = strtok(NULL, ",");
		double x = atof(pch);
                pch = strtok(NULL, ",");
	        double y = atof(pch);
		VisiLibity::Point p = VisiLibity::Point( x, y );
		p.snap_to_boundary_of(gamemap, eps );
		p.snap_to_vertices_of(gamemap, eps );
		VisiLibity::Visibility_Polygon vp = VisiLibity::Visibility_Polygon( p, gamemap, eps );
		vp.eliminate_redundant_vertices(1e-5);
		samecnt = 0;
		for( int j = 0; j < vp.n(); j++ ){
		    for( int k = 0; k < vp.n(); k++ ){
			if( j == k ) continue;
			if( isSame( vp[j].x(), vp[k].x() ) == 1  && isSame( vp[j].y(), vp[k].y() ) == 1 ){
			    samecnt++;
			    //printf("Same point found\n");
			}
		    }
		}
		//iif( samecnt > 0 ) printf("%d\n", vpnum);
		vpnum++;
		
		printf("Start,%lf,%lf,\n", x, y);
		for( int j = 0; j < vp.n(); j++ ){
		    printf("L,%lf,%lf,%lf,%lf,\n", vp[j].x(), vp[j].y(), vp[(j+1)%vp.n()].x(), vp[(j+1)%vp.n()].y());
		}
		printf("End,\n");
	    }
	}
	//printf("%d\n", polyvect.size());
 	/*freopen("mapValidation.csv","w",stdout);
	for( i = 0; i < polyvect.size(); i++ ){
	   VisiLibity::Polygon vp = polyvect[i];
           if( i == 0 ){
	      for( j = 0; j < vp.n(); j++ )
	         printf("M;%lf,%lf;%lf,%lf\n", vp[j].x(), vp[j].y(), vp[(j+1)%vp.n()].x(), vp[(j+1)%vp.n()].y());
	   }
	   else{
	      printf("OBStart\n");
	      for( j = 0; j < vp.n(); j++ )
	         printf("OB;%lf,%lf;%lf,%lf\n", vp[j].x(), vp[j].y(), vp[(j+1)%vp.n()].x(), vp[(j+1)%vp.n()].y());
	      printf("OBEnd\n");
	   }
	   //printf("%d\n",vp.n());
	}*/
	//printf("%d\n",samecnt);
	return 0;
}

