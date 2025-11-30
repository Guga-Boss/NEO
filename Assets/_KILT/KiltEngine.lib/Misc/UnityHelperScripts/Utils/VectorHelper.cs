using UnityEngine;
using System.Collections;

public static class VectorHelper 
{
	public static Vector3 Direction(Vector3 from , Vector3 to )
	{
		return (to - from).normalized;
	}
	
	public static Vector3 RotateX( Vector3 p, float angle )
	{
		Vector3 v = new Vector3(p.x, p.y, p.z);
		/*float sin = Mathf.Sin( angle );
		float cos = Mathf.Cos( angle );
		float ty = v.y;
		float tz = v.z;
		v.y = (cos * ty) - (sin * tz);
		v.z = (cos * tz) + (sin * ty);
		return v;*/
		v = Quaternion.AngleAxis(angle, new Vector3(1,0,0)) * v;
		return v;
	}

	public static Vector3 RotateY( Vector3 p, float angle )
	{
		Vector3 v = new Vector3(p.x, p.y, p.z);
		/*float sin = Mathf.Sin( angle );
		float cos = Mathf.Cos( angle );
		float tx = v.x;
		float tz = v.z;
		v.x = (cos * tx) + (sin * tz);
		v.z = (cos * tz) - (sin * tx);
		return v;*/
		v = Quaternion.AngleAxis(angle, new Vector3(0,1,0)) * v;
		return v;
	}
	public static Vector3 RotateZ( Vector3 p, float angle )
	{
		Vector3 v = new Vector3(p.x, p.y, p.z);
		/*float sin = Mathf.Sin( angle );
		float cos = Mathf.Cos( angle );
		float tx = v.x;
		float ty = v.y;
		v.x = (cos * tx) - (sin * ty);
		v.y = (cos * ty) + (sin * tx);
		return v;*/
		v = Quaternion.AngleAxis(angle, new Vector3(0,0,1)) * v;
		return v;
	}

	public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) 
	{
		Vector3 dir = point - pivot; // get point direction relative to pivot
		dir = Quaternion.Euler(angles) * dir; // rotate it
		point = dir + pivot; // calculate rotated point
		return point; // return it
	}


	public static Vector2 GetDistanceVectorWithRotationZ(float p_distance, float p_angle )
	{
		Vector3 v_distanceVector = RotateZ( new Vector3(p_distance,0,0), p_angle );
		return v_distanceVector;
	}

	
	public static bool LineIntersection( Vector2 p_line1_p1,Vector2 p_line1_p2, Vector2 p_line2_p1, Vector2 p_line2_p2, ref Vector2 p_intersection)
	{

		Vector2 p1= p_line1_p1;
		Vector2 p2 = p_line1_p2;
		Vector2 p3 = p_line2_p1;
		Vector2 p4 = p_line2_p2;
		float Ax,Bx,Cx,Ay,By,Cy,d,e,f,num/*,offset*/;
		float x1lo,x1hi,y1lo,y1hi;

		Ax = p2.x-p1.x;
		Bx = p3.x-p4.x;

		// X bound box test/
		
		if(Ax<0) 
		{
			x1lo=p2.x; 
			x1hi=p1.x;	
		} 
		else 
		{
			x1hi=p2.x; 
			x1lo=p1.x;	
		}

		if(Bx>0) 
		{
			if(x1hi < p4.x || p3.x < x1lo) 
				return false;
			
		} 
		else 
		{
			if(x1hi < p3.x || p4.x < x1lo) 
				return false;
		}

		Ay = p2.y-p1.y;		
		By = p3.y-p4.y;
		
		// Y bound box test//
		if(Ay<0) 
		{                  	
			y1lo=p2.y; 
			y1hi=p1.y;
		} 
		else 
		{	
			y1hi=p2.y; 
			y1lo=p1.y;
		}

		if(By>0) 
		{
			if(y1hi < p4.y || p3.y < y1lo) 
				return false;
		} 
		else 
		{
			if(y1hi < p3.y || p4.y < y1lo) 
				return false;
		}

		Cx = p1.x-p3.x;
		Cy = p1.y-p3.y;
		d = By*Cx - Bx*Cy;  // alpha numerator//
		f = Ay*Bx - Ax*By;  // both denominator//

		// alpha tests//
		
		if(f>0) 
		{
			if(d<0 || d>f) 
				return false;
			
		} 
		else 
		{
			if(d>0 || d<f) 
				return false;
		}
		e = Ax*Cy - Ay*Cx;  // beta numerator//

		// beta tests //
		
		if(f>0) 
		{                          
			if(e<0 || e>f) 
				return false;
		} 
		else 
		{
			if(e>0 || e<f) 
				return false;
		}

		// check if they are parallel
		if(f==0) 
			return false;
		
		// compute intersection coordinates //
		num = d*Ax; // numerator //
		
		//    offset = same_sign(num,f) ? f*0.5f : -f*0.5f;   // round direction //
		//    intersection.x = p1.x + (num+offset) / f;
		p_intersection.x = p1.x + num / f;
		num = d*Ay;
		
		//    offset = same_sign(num,f) ? f*0.5f : -f*0.5f;
		//    intersection.y = p1.y + (num+offset) / f;
		p_intersection.y = p1.y + num / f;
		return true;
	}


	
	public static float GetPitch( this Vector3 v )
	{
		float len = Mathf.Sqrt( (v.x * v.x) + (v.z * v.z) );    // Length on xz plane.
		return( -Mathf.Atan2( v.y, len ) );
	}
	

	public static float GetYaw( this Vector3 v )
	{
		return( Mathf.Atan2( v.x, v.z ) );
	}

}
