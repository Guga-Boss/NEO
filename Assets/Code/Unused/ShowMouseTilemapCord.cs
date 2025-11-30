using UnityEngine;
using System.Collections;

[ExecuteInEditMode] 
public class ShowMouseTilemapCord : MonoBehaviour
{
	public Vector2 MousePosition;

	[ExecuteInEditMode]
	void FixedUpdate()
	{
		MousePosition = new Vector2( -1, -1 );
		Map.I.UpdateMouseTile();
		MousePosition = new Vector2( Map.I.Mtx, Map.I.Mty );
	}
}
