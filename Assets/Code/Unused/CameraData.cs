using UnityEngine;
using System.Collections;

public class CameraData : MonoBehaviour 
{
	public float RoamingCameraZoom = 1.6f;
	public Vector3 LowerLeftCamLimit, UpperRightCamLimit;
    public Vector3 LowerLeftCamLimit2, UpperRightCamLimit2;
    public Vector3 LowerLeftCamLimit3, UpperRightCamLimit3;
	public float [] OptimalZoomY;  // optimal zoom when Y > X
	public float [] OptimalZoomX;  // optimal zoom when X > Y
	public float [] OptimalVerticalDifX;   // optimal Y val to add when inside area
	public float [] OptimalVerticalDifY;   // optimal Y val to add when inside area
    public float[ ] OptimalZoomSpeed; // Optimal zoom speed for each case when inside area
    public  float WholeCubeZoom = .5f;
    public Vector3 WholeCubeCenterVectorDistance = new Vector3( 0, -3.22f, 0 );
}
