using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum QuadrantEnum { First_0To90 = 0, Second_90To180 = 1, Third_180To270 = 2, Fourth_270To360 = 3}

public static class AngleHelper
{
	public static float ConvertToAngleMultipleOf(float p_angle, float p_multiple)
	{
		float v_trueAngle = GetSimplifiedAngle(p_angle);
		if(!AngleIsMultipleOf(p_angle, p_multiple))
		{
			if(p_multiple != 0)
			{
				float v_result = v_trueAngle/p_multiple;
				v_result = Mathf.RoundToInt(v_result);
				v_trueAngle = p_multiple * v_result;
			}
		}
		return v_trueAngle;
	}
	
	public static bool AngleIsMultipleOf(float p_angle, float p_multiple)
	{
		if(p_multiple != 0)
		{
			float v_result = p_angle%p_multiple;
			if(v_result != 0)
				return true;
		}
		return false;
	}

	public static bool IsQuadrantIntersection(float p_angle)
	{
		if((p_angle >= 89 && p_angle <= 91) || 
		   (p_angle >= 179 && p_angle <= 181) || 
		   (p_angle >= 269 && p_angle <= 271) || 
		   (p_angle >= 359 && p_angle <= 360) ||
		   (p_angle >= 0 && p_angle <= 1))
		{
			return true;
		}
		return false;
	}
	
	public static QuadrantEnum GetAngleQuadrant(float p_angle)
	{
		float v_trueAngle = GetSimplifiedAngle(p_angle);
		QuadrantEnum v_quadrant = QuadrantEnum.First_0To90;
		if(v_trueAngle >=0 && v_trueAngle < 90)
			v_quadrant = QuadrantEnum.First_0To90;
		else if(v_trueAngle >=90 && v_trueAngle < 180)
			v_quadrant = QuadrantEnum.Second_90To180;
		else if(v_trueAngle >=180 && v_trueAngle < 270)
			v_quadrant = QuadrantEnum.Third_180To270;
		else
			v_quadrant = QuadrantEnum.Fourth_270To360;
		return v_quadrant;
	}
	
	public static QuadrantEnum GetQuadrantInScreen(GameObject p_object)
	{
		QuadrantEnum v_quadrant = QuadrantEnum.First_0To90;
		if(p_object != null)
		{
			Camera v_camera = CameraManager.GetCameraThatDrawLayer(p_object.layer);
			if(v_camera != null)
			{
				Vector2 v_pointInViewPort = v_camera.WorldToViewportPoint(p_object.transform.position);
				if(v_pointInViewPort.x <= v_camera.rect.size.x/2.0f && v_pointInViewPort.y <= v_camera.rect.size.y/2.0f)
					v_quadrant = QuadrantEnum.Third_180To270;
				else if(v_pointInViewPort.x >= v_camera.rect.size.x/2.0f && v_pointInViewPort.y <= v_camera.rect.size.y/2.0f)
					v_quadrant = QuadrantEnum.Fourth_270To360;
				else if(v_pointInViewPort.x >= v_camera.rect.size.x/2.0f && v_pointInViewPort.y >= v_camera.rect.size.y/2.0f)
					v_quadrant = QuadrantEnum.First_0To90;
				else if(v_pointInViewPort.x <= v_camera.rect.size.x/2.0f && v_pointInViewPort.y >= v_camera.rect.size.y/2.0f)
					v_quadrant = QuadrantEnum.Second_90To180;
			}
		}
		return v_quadrant;
	}
	
	public static float GetSimplifiedAngle(float p_angle)
	{
		float v_trueAngle = p_angle%360;
		if(v_trueAngle < 0)
			v_trueAngle = 360 + v_trueAngle;
		return v_trueAngle;
	}
	
	public static float GetAngleInFirstQuadrant(float p_angle)
	{
		float v_trueAngleInFirstQuadrant = p_angle %90;
		if(v_trueAngleInFirstQuadrant == 0)
			v_trueAngleInFirstQuadrant = Mathf.RoundToInt(p_angle/90)%2 == 0? 0 : 90;
		return v_trueAngleInFirstQuadrant;
	}
}
