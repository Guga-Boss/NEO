using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor(typeof(Rope2D))]
public class Rope2DInspector : Editor {

	Rope2D m_controller;

	#region Unity Functions

	public override void OnInspectorGUI ()
	{
		RecreateRopeButtons();
		DrawAttachedObjects();
		DrawImportantProperties();
		DrawPhysicsProperties();
		DrawNodeVisualProperties();
		DrawNodeProperties();
		DrawRopeProperties();
		DrawLineRendererProperties();
		DrawCutProperties();
		DrawBreakProperties();
		DrawTensionProperties();

		if(GUI.changed)
			EditorUtility.SetDirty(target);
	}

	#endregion

	#region Helper Drawer Functions

	protected void RecreateRopeButtons()
	{
		EditorGUILayout.BeginHorizontal();
		bool v_forceRecreate = InspectorUtils.DrawButton("Force Recreate Rope", Color.cyan);
		bool v_refreshRope = InspectorUtils.DrawButton("Refresh Rope", Color.cyan);
		if(m_controller != null && m_controller.gameObject != null && !KiltUtils.IsPrefab(m_controller.gameObject, PrefabSearchMethodEnum.Full))
		{
			if(v_forceRecreate)
				m_controller.CreateRope(true);
			else if(v_refreshRope)
				m_controller.CreateRope(false);
		}
		EditorGUILayout.EndHorizontal();
	}

	protected void DrawTitleText(string p_text)
	{
		DrawTitleText(p_text, new Color(1, 0.1f, 0.7f));
	}

	protected virtual void DrawTitleText(string p_text, Color v_color)
	{
		InspectorUtils.DrawTitleText(p_text, v_color);
	}

	protected virtual void DrawAttachedObjects()
	{
		if(m_controller == null)
			m_controller = target as Rope2D;
		
		//Attached Objects Properties
		DrawTitleText("Attached Objects Properties");
		m_controller.ObjectA = EditorGUILayout.ObjectField("Object A", m_controller.ObjectA, typeof(GameObject), true) as GameObject;
		m_controller.ObjectB = EditorGUILayout.ObjectField("Object B", m_controller.ObjectB, typeof(GameObject), true) as GameObject;
		EditorGUILayout.Separator();
	}

	protected virtual void DrawImportantProperties()
	{
		if(m_controller == null)
			m_controller = target as Rope2D;
		
		//Important Properties
		DrawTitleText("Important Properties");
		m_controller.AutoCalculateAmountOfNodes = EditorGUILayout.Toggle("Auto Calculate Amount Of Nodes", m_controller.AutoCalculateAmountOfNodes);
		if(m_controller.AutoCalculateAmountOfNodes)
			GUI.enabled = false;
		m_controller.AmountOfNodes = EditorGUILayout.IntField("Amount Of Nodes", m_controller.AmountOfNodes);
		GUI.enabled = true;
		EditorGUILayout.Separator();
	}

	protected virtual void DrawPhysicsProperties()
	{
		if(m_controller == null)
			m_controller = target as Rope2D;
		
		//Physics Properties
		DrawTitleText("Physics Properties");
		m_controller.NodeMass = EditorGUILayout.FloatField("Node Mass", m_controller.NodeMass);
		m_controller.NodeLinearDrag = EditorGUILayout.FloatField("Node Linear Drag", m_controller.NodeLinearDrag);
		m_controller.NodeAngularDrag = EditorGUILayout.FloatField("Node Angular Drag", m_controller.NodeAngularDrag);
		m_controller.NodeGravityScale = EditorGUILayout.FloatField("Node Gravity Scale", m_controller.NodeGravityScale);
		m_controller.NodeCollisionDetectionType = (RigidbodyCollisionDetectionTypeEnum)EditorGUILayout.EnumPopup("Node Collision Detection", m_controller.NodeCollisionDetectionType);
		m_controller.JointCollider = (JointColliderEnum)EditorGUILayout.EnumPopup("Joint Collider", m_controller.JointCollider);
		EditorGUILayout.Separator();
	}

	protected virtual void DrawNodeVisualProperties()
	{
		if(m_controller == null)
			m_controller = target as Rope2D;
		
		//Node Visual Properties
		DrawTitleText("Node Visual Properties");
		m_controller.NodeSprite = EditorGUILayout.ObjectField("Node Sprite", m_controller.NodeSprite, typeof(Sprite), false) as Sprite;
		if(m_controller.NodeSprite != null)
			m_controller.NodeMaterial = EditorGUILayout.ObjectField("Node Material", m_controller.NodeMaterial, typeof(Material), false) as Material;
		EditorGUILayout.Separator();
	}

	protected virtual void DrawNodeProperties()
	{
		if(m_controller == null)
			m_controller = target as Rope2D;
		
		//Node Properties
		DrawTitleText("Node Properties");
		m_controller.CustomNodePrefab = EditorGUILayout.ObjectField("Custom Node Prefab", m_controller.CustomNodePrefab, typeof(GameObject), false) as GameObject;
		m_controller.NodeDistanceOffSet = EditorGUILayout.FloatField("Node Distance Offset", m_controller.NodeDistanceOffSet);
		if(m_controller.NodeSprite != null)
			GUI.enabled = false;
		m_controller.NodeSpriteGlobalSize = EditorGUILayout.Vector2Field("Node Global Size", m_controller.NodeSpriteGlobalSize);
		GUI.enabled = true;
		m_controller.NodeLocalScale = EditorGUILayout.Vector2Field("Node Local Scale", m_controller.NodeLocalScale);
		EditorGUILayout.Separator();
	}

	protected virtual void DrawRopeProperties()
	{
		if(m_controller == null)
			m_controller = target as Rope2D;
		
		//Rope Properties
		DrawTitleText("Rope Properties");
		m_controller.StableFirstAndLastNodes = EditorGUILayout.Toggle("Plug First/Last by Center of Mass", m_controller.StableFirstAndLastNodes);
		m_controller.EditorRopeStyle = (EditorRopeStyleEnum)EditorGUILayout.EnumPopup("Editor Visual Option", m_controller.EditorRopeStyle);
		//Sorting Layer
		string[] v_sortingLayers = GetSortingLayerNames();
		int v_sortingIndex = EditorGUILayout.Popup("Rope Sorting Layer", GetObjectIndexInArrayOrStrings(v_sortingLayers, m_controller.RopeSortingLayerName), v_sortingLayers);
		m_controller.RopeSortingLayerName = v_sortingLayers.Length > v_sortingIndex && v_sortingIndex >=0? v_sortingLayers[v_sortingIndex] : "Default";

		m_controller.RopeDepth = EditorGUILayout.IntField("Rope Depth", m_controller.RopeDepth);
		m_controller.IsSpringNode = EditorGUILayout.Toggle("Spring Rope", m_controller.IsSpringNode);
		if(m_controller.IsSpringNode)
			m_controller.SpringFrequency = EditorGUILayout.FloatField("Spring Frequency", m_controller.SpringFrequency);
		EditorGUILayout.Separator();
	}

	protected virtual void DrawLineRendererProperties()
	{
		if(m_controller == null)
			m_controller = target as Rope2D;
		
		//LineRenderer Properties
		DrawTitleText("LineRenderer Properties");
		m_controller.UseLineRenderer = EditorGUILayout.Toggle("Use Line Renderer", m_controller.UseLineRenderer);
		if(m_controller.UseLineRenderer)
			m_controller.RopeMaterial = EditorGUILayout.ObjectField("Line Renderer Material", m_controller.RopeMaterial, typeof(Material), false) as Material;
		EditorGUILayout.Separator();
	}

	protected virtual void DrawCutProperties()
	{
		if(m_controller == null)
			m_controller = target as Rope2D;
		
		//Cut Properties
		DrawTitleText("Cut Properties");
		m_controller.UserCanCutTheRope = EditorGUILayout.Toggle("User Can Cut The Rope", m_controller.UserCanCutTheRope);
		EditorGUILayout.Separator();
	}

	protected virtual void DrawBreakProperties()
	{
		if(m_controller == null)
			m_controller = target as Rope2D;
		
		//Break Properties
		DrawTitleText("Break Properties");
		m_controller.RopeBreakAction = (RopeBreakActionEnum)EditorGUILayout.EnumPopup("On Rope Break", m_controller.RopeBreakAction);
		if(m_controller.RopeBreakAction == RopeBreakActionEnum.DestroySelf)
			m_controller.DestroyTime = EditorGUILayout.FloatField("Time to Destroy Self", m_controller.DestroyTime);
		m_controller.BreakEffect = EditorGUILayout.ObjectField("Break Effect Prefab", m_controller.BreakEffect, typeof(GameObject), false) as GameObject;
		m_controller.LowMassVariationWhenBroken = EditorGUILayout.Toggle("Enable Low Mass When Broken", m_controller.LowMassVariationWhenBroken);
		m_controller.RopeCanBreak = EditorGUILayout.Toggle("Rope Can Break", m_controller.RopeCanBreak);
		if(m_controller.RopeCanBreak)
		{
			m_controller.BreakAngle = EditorGUILayout.FloatField("Max Angle Before Break", m_controller.BreakAngle);
			m_controller.RopeMaxSizeMultiplier = EditorGUILayout.FloatField("Max Rope Size (Multiplier)", m_controller.RopeMaxSizeMultiplier);
			
		}
		EditorGUILayout.Separator();
	}

	protected virtual void DrawTensionProperties()
	{
		if(m_controller == null)
			m_controller = target as Rope2D;
		
		//Tension Properties
		DrawTitleText("Tension Properties");
		m_controller.TensionHelperAddOption = (TensionHelperAddOptionEnum)EditorGUILayout.EnumPopup("Tension Helper Add Option", m_controller.TensionHelperAddOption);
		m_controller.UseTensionColors = EditorGUILayout.Toggle("Use Tension Color", m_controller.UseTensionColors);
		if(m_controller.UseTensionColors)
		{
			m_controller.NonTensionedColor = EditorGUILayout.ColorField("Non Tensioned Color", m_controller.NonTensionedColor);
			m_controller.TensionedColor = EditorGUILayout.ColorField("Tensioned Color", m_controller.TensionedColor);
		}
		EditorGUILayout.Separator();
		
		if(GUI.changed)
			EditorUtility.SetDirty(m_controller);
	}

	#endregion

	#region Other Helper Functions

	public int GetObjectIndexInArrayOrStrings(string[] p_array, string p_textObject)
	{
		int v_index = -1;
		if(p_array != null)
		{
			v_index = 0;
			foreach(string v_string in p_array)
			{
				if(string.Equals(v_string, p_textObject))
				{
					break;
				}
				else
					v_index++;
			}
		}
		return v_index;
	}

	public string[] GetSortingLayerNames()
	{
		try
		{
			System.Type v_internalEditorUtilityType = typeof(UnityEditorInternal.InternalEditorUtility);
			PropertyInfo v_sortingLayersProperty = v_internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
			return (string[])v_sortingLayersProperty.GetValue(null, new object[0]);
		}
		catch{}
		List<string> v_list = new List<string>();
		v_list.Add("Default");
		return v_list.ToArray();
	}

	public int[] GetSortingLayerUniqueIDs()
	{
		try
		{
			System.Type v_internalEditorUtilityType = typeof(UnityEditorInternal.InternalEditorUtility);
			PropertyInfo v_sortingLayersProperty = v_internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic);
			return (int[])v_sortingLayersProperty.GetValue(null, new object[0]);
		}
		catch{}

		List<int> v_list = new List<int>();
		v_list.Add(0);
		return v_list.ToArray();
	}

	#endregion
}