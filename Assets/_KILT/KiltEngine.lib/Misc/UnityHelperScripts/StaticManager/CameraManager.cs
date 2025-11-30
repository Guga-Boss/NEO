using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class CameraManager
{
	#region Static Properties

	public static Camera[] CamerasInScene 
	{
		get
		{
			return Camera.allCameras;
		}
	}

	#endregion

	#region Static Functions

	public static Camera CameraThatDrawObject(this GameObject p_object)
	{
		Camera v_returnCamera = GetCameraThatDrawLayer(p_object.layer);
		return v_returnCamera;
	}

	public static Camera GetCameraThatDrawLayer(int p_layer)
	{
		Camera v_returnCamera = null;
		foreach(Camera v_camera in CamerasInScene)
		{
			if(v_camera != null)
			{
				//Camera Draw Specific Layer
				if((v_camera.cullingMask & (1 << p_layer)) == (1 << p_layer))
				{
					v_returnCamera = v_camera;
					break;
				}
			}
		}
		return v_returnCamera;
	}

	#endregion
}
