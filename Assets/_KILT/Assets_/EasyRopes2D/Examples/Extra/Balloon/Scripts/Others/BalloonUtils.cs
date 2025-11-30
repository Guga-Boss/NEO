using UnityEngine;
using System.Collections;

public static class BalloonUtils {

	#region Ballon Functions
	
	public static void PlugBallonToObject(GameObject p_object, BalloonProperty p_ballon, Rope2D p_rope, Vector2 p_minDistance, Vector2 p_maxDistance, bool p_ballonIsAPrefab = true, bool p_ropeIsAPrefab = true, bool p_useBallonPrefabScale = true, bool p_useRopePrefabScale = true, string p_errorSolverDirectory = "Prefabs")
	{
		GameObject v_ropeParentObject = GameObject.Find("RopesContainer");
		GameObject v_blockParentObject = GameObject.Find("BlocksContainer");
		
		GameObject v_ballonInScene = null;
		GameObject v_ropeInScene = null;
		
		//Try Solve Ballon Errors
		if(p_ballon == null)
		{
			BalloonProperty[] v_ballons = KiltUtils.FindAllResourcesComponentsAtFolder<BalloonProperty>(p_errorSolverDirectory);
			if(v_ballons.Length <= 0)
				v_ballons = KiltUtils.FindAllResourcesComponentsOfType<BalloonProperty>();
			if(v_ballons.Length > 0 && v_ballons[0] != null)
			{
				v_ballonInScene = KiltUtils.Instantiate(v_ballons[0].gameObject);
				v_ballonInScene.transform.parent = v_blockParentObject != null? v_blockParentObject.transform : null;
				if(p_useBallonPrefabScale)
					v_ballonInScene.transform.localScale = v_ballons[0].gameObject.transform.localScale;
			}
		}
		else if(p_ballonIsAPrefab)
		{
			v_ballonInScene = KiltUtils.Instantiate(p_ballon.gameObject);
			v_ballonInScene.transform.parent = v_blockParentObject != null? v_blockParentObject.transform : null;
			if(p_useBallonPrefabScale)
				v_ballonInScene.transform.localScale = p_ballon.gameObject.transform.localScale;
		}
		else
			v_ballonInScene = p_ballon.gameObject;
		
		//Try Solve Rope Errors
		if(p_rope == null)
		{
			Rope2D[] v_ropes = KiltUtils.FindAllResourcesComponentsAtFolder<Rope2D>(p_errorSolverDirectory);
			if(v_ropes.Length <= 0)
				v_ropes = KiltUtils.FindAllResourcesComponentsOfType<Rope2D>();
			if(v_ropes.Length > 0 && v_ropes[0] != null)
			{
				v_ropeInScene = KiltUtils.Instantiate(v_ropes[0].gameObject);
				v_ropeInScene.transform.parent = v_ropeParentObject != null? v_ropeParentObject.transform : null;
				if(p_useRopePrefabScale)
					v_ropeInScene.transform.localScale = v_ropes[0].gameObject.transform.localScale;
			}
		}
		else if(p_ropeIsAPrefab)
		{
			v_ropeInScene = KiltUtils.Instantiate(p_rope.gameObject);
			v_ropeInScene.transform.parent = v_ropeParentObject != null? v_ropeParentObject.transform : null;
			if(p_useRopePrefabScale)
				v_ropeInScene.transform.localScale = p_rope.gameObject.transform.localScale;
		}
		else
			v_ropeInScene = p_rope.gameObject;
		
		if(v_ballonInScene != null && p_object != null && v_ropeInScene != null)
		{
			v_ballonInScene.transform.localPosition = new Vector3(v_ballonInScene.transform.localPosition.x, v_ballonInScene.transform.localPosition.y, 20); // Prevent Rope Collider to be in front of this Object
			v_ballonInScene.name = v_ballonInScene.name.Replace("(Clone)","");
			v_ropeInScene.name = v_ropeInScene.name.Replace("(Clone)", "") + "[(" + p_object.name.Replace("(Selected)","") +") to ("+ v_ballonInScene.name + ")]";
			TackRope v_tackRope = v_ropeInScene.GetComponent<TackRope>();
			if(v_tackRope != null)
			{
				v_tackRope.ObjectA = p_object;
				v_tackRope.ObjectB = v_ballonInScene;
				
				Vector2 v_newOffSetPosition = new Vector2(KiltUtils.RandomRange(p_minDistance.x, p_maxDistance.x), KiltUtils.RandomRange(p_minDistance.y, p_maxDistance.y));
				v_ballonInScene.transform.position = new Vector3(p_object.transform.position.x + v_newOffSetPosition.x, p_object.transform.position.y + v_newOffSetPosition.y, p_object.transform.position.z);
				if(Application.isPlaying)
					v_tackRope.CreateRope();
			}
		}
		if(v_ballonInScene != null && v_ballonInScene.GetComponent<BlockSelector>() == null)
			v_ballonInScene.AddComponent<BlockSelector>();
	}
	
	#endregion
}
