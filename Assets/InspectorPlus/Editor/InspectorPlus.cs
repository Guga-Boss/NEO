using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

public class InspectorPlus : Editor
{
	SerializedObject so;
	SerializedProperty[] properties;
	new string name;
    string dispName;
	Rect tooltipRect;
	
	InspectorPlusManager manager;
	InspectorPlusTracker tracker;
	List<InspectorPlusVar> vars;
	
	public string AssetPath { get { return Path.GetDirectoryName (AssetDatabase.GetAssetPath (MonoScript.FromScriptableObject (this))); } }
	public string FilePath { get { return Application.dataPath + Path.GetDirectoryName (AssetDatabase.GetAssetPath (MonoScript.FromScriptableObject (this)).Replace ("Assets", "")); } }

	public void OnEnable ()
	{
		so = serializedObject;
		
		manager = FindObjectOfType (typeof(InspectorPlusManager)) as InspectorPlusManager;

		if (manager == null)
			manager = (InspectorPlusManager)AssetDatabase.LoadAssetAtPath (AssetPath + "/InspectorPlus.asset", typeof(InspectorPlusManager));

		tracker = manager.GetTracker (target.GetType ().Name);

        if (tracker != null)
        {
            List<InspectorPlusVar> vars = tracker.GetVars();

            int count = vars.Count;

            properties = new SerializedProperty[count];
        }
	}

	void ProgressBar (float value, string label)
	{
		GUILayout.Space (3.0f);
		Rect rect = GUILayoutUtility.GetRect (18, 18, "TextField");
		EditorGUI.ProgressBar (rect, value, label);
		GUILayout.Space (3.0f);
	}

	void PropertyField (SerializedProperty sp, string name)
	{
		if (sp.hasChildren) {
            
            GUILayout.BeginVertical();
			while (true) {
				if (sp.propertyPath != name && !sp.propertyPath.StartsWith (name + "."))
					break;

				EditorGUI.indentLevel = sp.depth;
                bool child = false;

                child = sp.depth == 0 ? EditorGUILayout.PropertyField(sp, new GUIContent(dispName)) : EditorGUILayout.PropertyField(sp);

				if (!sp.NextVisible (child))
					break;
			}
            EditorGUI.indentLevel = 0;
            GUILayout.EndVertical();
		} else EditorGUILayout.PropertyField(sp, new GUIContent(dispName));
	}

	void ArrayGUI (SerializedProperty sp, string name)
	{
		EditorGUIUtility.LookLikeControls (100.0f, 40.0f);
		GUILayout.Space (4.0f);
		EditorGUILayout.BeginVertical ("box", GUILayout.MaxWidth(Screen.width));

		int i = 0;
		int del = -1;

		SerializedProperty array = sp.Copy ();
		SerializedProperty size = null;
		bool first = true;

		while (true) {
			if (sp.propertyPath != name && !sp.propertyPath.StartsWith (name + "."))
				break;

			bool child;
            EditorGUI.indentLevel = sp.depth;

			if (sp.depth == 1 && !first) {
				EditorGUILayout.BeginHorizontal ();

				if (GUILayout.Button ("", "OL Minus", GUILayout.Width (24.0f)))
					del = i;

				//GUILayout.Label ("" + i);
				child = EditorGUILayout.PropertyField (sp);

				GUI.enabled = i > 0;

				if (GUILayout.Button (manager.arrowUp, "ButtonLeft", GUILayout.Width (22.0f), GUILayout.Height(18.0f)))
					array.MoveArrayElement (i - 1, i);

				GUI.enabled = i < array.arraySize - 1;
                if (GUILayout.Button(manager.arrowDown, "ButtonRight", GUILayout.Width(22.0f), GUILayout.Height(18.0f)))
					array.MoveArrayElement (i + 1, i);

				++i;

				GUI.enabled = true;
				EditorGUILayout.EndHorizontal ();
			} else if (sp.depth == 1) {
				first = false;
				size = sp.Copy ();

				EditorGUILayout.BeginHorizontal ();

                if (!size.hasMultipleDifferentValues && GUILayout.Button("", "OL Plus", GUILayout.Width(24.0f)))
					array.arraySize += 1;


				child = EditorGUILayout.PropertyField (sp);

				EditorGUILayout.EndHorizontal ();
			} else {
                child = EditorGUILayout.PropertyField(sp);
			}

			if (!sp.NextVisible (child))
				break;
		}

		sp.Reset ();

		if (del != -1)
			array.DeleteArrayElementAtIndex (del);

		if (array.isExpanded && !size.hasMultipleDifferentValues) {
			EditorGUILayout.BeginHorizontal ();

            if (GUILayout.Button("", "OL Plus", GUILayout.Width(24.0f)))
				array.arraySize += 1;

			GUI.enabled = false;
			EditorGUILayout.PropertyField (array.GetArrayElementAtIndex (array.arraySize - 1), new GUIContent ("" + array.arraySize));
			GUI.enabled = true;

			EditorGUILayout.EndHorizontal ();
		}


        EditorGUI.indentLevel = 0;
		EditorGUILayout.EndVertical ();
		EditorGUIUtility.LookLikeControls (170.0f, 80.0f);
	}
	
	void RefreshVars()
	{
		vars = tracker.GetVars();
		int count = vars.Count;

        if (count != properties.Length)
            properties = new SerializedProperty[count];

		for (int i = 0; i < count; i += 1) 
		{
			InspectorPlusVar v = vars[i];
			properties[i] = so.FindProperty (v.name);			
		}
	}
	
	void Vector2Field(SerializedProperty sp)
	{
        EditorGUI.BeginProperty(new Rect(0.0f, 0.0f, 0.0f, 0.0f), new GUIContent(), sp);
		EditorGUI.BeginChangeCheck ();
		var newValue = EditorGUILayout.Vector2Field (dispName, sp.vector2Value);

		if (EditorGUI.EndChangeCheck ())
			sp.vector2Value = newValue;
		
		EditorGUI.EndProperty ();
	}
	
	void FloatField(SerializedProperty sp, InspectorPlusVar v)
	{
		if (v.limitType == InspectorPlusVar.LimitType.Min && !sp.hasMultipleDifferentValues)
			sp.floatValue = Mathf.Max (v.min, sp.floatValue);
		else if (v.limitType == InspectorPlusVar.LimitType.Max && !sp.hasMultipleDifferentValues)
			sp.floatValue = Mathf.Min (v.max, sp.floatValue);
		
		if (v.limitType == InspectorPlusVar.LimitType.Range) {
			if (!v.progressBar)
				EditorGUILayout.Slider (sp, v.min, v.max);
			else {
				if (!sp.hasMultipleDifferentValues) {
					sp.floatValue = Mathf.Clamp (sp.floatValue, v.min, v.max);
					ProgressBar ((sp.floatValue - v.min) / v.max, dispName);
				} else
					ProgressBar ((sp.floatValue - v.min) / v.max, dispName);
			}
		}
        else EditorGUILayout.PropertyField(sp, new GUIContent(dispName));
	}
	
	void IntField(SerializedProperty sp, InspectorPlusVar v)
	{
		if (v.limitType == InspectorPlusVar.LimitType.Min && !sp.hasMultipleDifferentValues)
			sp.intValue = Mathf.Max (v.iMin, sp.intValue);
		else if (v.limitType == InspectorPlusVar.LimitType.Max && !sp.hasMultipleDifferentValues)
			sp.intValue = Mathf.Min (v.iMax, sp.intValue);
		
		if (v.limitType == InspectorPlusVar.LimitType.Range)
		{
			if (!v.progressBar)
			{
                EditorGUI.BeginProperty(new Rect(0.0f, 0.0f, 0.0f, 0.0f), new GUIContent(), sp);
				EditorGUI.BeginChangeCheck ();

                var newValue = EditorGUI.IntSlider(GUILayoutUtility.GetRect(18.0f, 18.0f), new GUIContent(dispName), sp.intValue, v.iMin, v.iMax);
				
				if (EditorGUI.EndChangeCheck ())
					sp.intValue = newValue;
				EditorGUI.EndProperty ();
			}
			else {
				if (!sp.hasMultipleDifferentValues) {
					sp.intValue = Mathf.Clamp (sp.intValue, v.iMin, v.iMax);
					ProgressBar ((float)(sp.intValue - v.iMin) / v.iMax, dispName);
				} else
					ProgressBar ((float)(sp.intValue - v.iMin) / v.iMax, dispName);
			}
		}
        else EditorGUILayout.PropertyField(sp, new GUIContent(dispName));
	}
	
	void QuaternionField(SerializedProperty sp)
	{
        EditorGUI.BeginProperty(new Rect(0.0f, 0.0f, 0.0f, 0.0f), new GUIContent(), sp);

		EditorGUI.BeginChangeCheck ();
		SerializedProperty x = sp.FindPropertyRelative ("x");
		SerializedProperty y = sp.FindPropertyRelative ("y");
		SerializedProperty z = sp.FindPropertyRelative ("z");
		SerializedProperty w = sp.FindPropertyRelative ("w");

		var q = new Quaternion (x.floatValue, y.floatValue, z.floatValue, w.floatValue);
                   
		var newValue = EditorGUILayout.Vector3Field (dispName, q.eulerAngles);

		if (EditorGUI.EndChangeCheck ()) {
			Quaternion r = Quaternion.Euler (newValue);
			x.floatValue = r.x;
			y.floatValue = r.y;
			z.floatValue = r.z;
			w.floatValue = r.w;
		}

		EditorGUI.EndProperty ();
	}

    int BoolField(SerializedProperty sp, InspectorPlusVar v)
    {
        if (v.toggleStart)
        {
            EditorGUI.BeginProperty(new Rect(0.0f, 0.0f, 0.0f, 0.0f), new GUIContent(), sp);

            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.Toggle(dispName, sp.boolValue);
            
            if (EditorGUI.EndChangeCheck())
                sp.boolValue = newValue;
            
            EditorGUI.EndProperty();

            if (!sp.boolValue)
                return v.toggleSize;
        }
        else EditorGUILayout.PropertyField(sp, new GUIContent(dispName));

        return 0;
    }

    void TextureGUI(SerializedProperty sp, InspectorPlusVar v)
    {
        if (!v.largeTexture)
            PropertyField(sp, name);
        else
        {
            EditorGUI.BeginProperty(new Rect(0.0f, 0.0f, 0.0f, 0.0f), new GUIContent(), sp);

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(dispName);
            var newValue = EditorGUILayout.ObjectField(sp.objectReferenceValue, typeof(Texture2D), false, GUILayout.Width(v.textureSize), GUILayout.Height(v.textureSize));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
                sp.objectReferenceValue = newValue;

            EditorGUI.EndProperty();
        }
    }

	void TextGUI(SerializedProperty sp, InspectorPlusVar v)
	{
		if (v.textFieldDefault == "")
		{
			PropertyField(sp, name);
			return;
		}

		string focusName = "_focusTextField" + v.name;

		GUI.SetNextControlName(focusName);

		EditorGUI.BeginProperty(new Rect(0.0f, 0.0f, 0.0f, 0.0f), new GUIContent(), sp);

		EditorGUI.BeginChangeCheck();

		GUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(dispName);

		string newValue = "";

		newValue = !v.textArea ? EditorGUILayout.TextField("", sp.stringValue, GUILayout.Width(Screen.width)) : EditorGUILayout.TextArea(sp.stringValue, GUILayout.Width(Screen.width));

		if (GUI.GetNameOfFocusedControl() != focusName && !sp.hasMultipleDifferentValues && sp.stringValue == "")
		{
			GUI.color = new Color(0.7f, 0.7f, 0.7f);
			GUI.Label(GUILayoutUtility.GetLastRect(), v.textFieldDefault);
			GUI.color = Color.white;
		}
		
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		if (EditorGUI.EndChangeCheck())
			sp.stringValue = newValue;

		EditorGUI.EndProperty();
	}

	public override void OnInspectorGUI ()
	{
		if (manager == null)
			return;

        if (tracker == null)
            return;

        tracker.UpdateFields();
		
		so.Update ();
		RefreshVars();
		
		EditorGUIUtility.LookLikeControls (135.0f, 50.0f);

		for (int i = 0; i < properties.Length; i += 1) 
		{
			InspectorPlusVar v = vars[i];
			
			if (v.active && properties[i] != null) 
			{
				SerializedProperty sp = properties [i];
				string s = v.type;
				name = v.name;
                dispName = v.dispName;

				GUI.enabled = v.canWrite;

                GUILayout.BeginHorizontal();

                if (v.toggleLevel != 0)
                   GUILayout.Space(v.toggleLevel * 10.0f);

				if (s == typeof(Vector2).Name)
					Vector2Field(sp);
				else if (s == typeof(float).Name)
					FloatField(sp, v);
				else if (s == typeof(int).Name)
					IntField(sp, v);
				else if (s == typeof(Quaternion).Name)
					QuaternionField(sp);
				else if (s == typeof(bool).Name)
					i += BoolField(sp, v);
				else if (s == typeof(Texture2D).Name || s == typeof(Texture).Name)
					TextureGUI(sp, v);
				else if (s == typeof(string).Name)
					TextGUI(sp, v);
				else if (sp.isArray)
					ArrayGUI(sp, name);
				else
					PropertyField(sp, name);

                GUILayout.EndHorizontal();
                GUI.enabled = true;

				if (v.hasTooltip)
				{
	                Rect last = GUILayoutUtility.GetLastRect();
	                GUI.Label(last, new GUIContent("", v.tooltip));

					Vector2 size = new GUIStyle().CalcSize(new GUIContent(GUI.tooltip));
					tooltipRect = new Rect(Event.current.mousePosition.x + 4.0f, Event.current.mousePosition.y + 12.0f, 28.0f + size.x, 9.0f + size.y);

                    if (tooltipRect.width > 250.0f)
                    {
                        float delt = (tooltipRect.width - 250.0f);
                        tooltipRect.width -= delt;
                        tooltipRect.height += size.y * Mathf.CeilToInt(delt / 250.0f);
                    }
				}
			}
			
			if (v.space == 0.0f)
				continue;
			
			float usedSpace = 0.0f;
			for (int j = 0; j < v.numSpace; j += 1) {
                bool buttonLine = false;
                for (int k = 0; k < 4; k += 1) if (v.buttonEnabled[j * 4 + k]) buttonLine = true;
				if (v.labelEnabled [j] || buttonLine)
					usedSpace += 18.0f;
			}
			

			if (v.space == 0.0f)
				continue;
			
			
			float space = Mathf.Max (0.0f, (v.space - usedSpace) / 2.0f);
			
			GUILayout.Space (space);

			for (int j = 0; j < v.numSpace; j += 1) {
                bool buttonLine = false;
                for (int k = 0; k < 4; k += 1) if (v.buttonEnabled[j * 4 + k]) buttonLine = true;
                if (!v.labelEnabled[j] && !buttonLine)
                    continue;


                GUILayout.BeginHorizontal();
                if (v.labelEnabled[j])
                {
                    var boldItalic = new GUIStyle {margin = new RectOffset(5, 5, 5, 5)};

	                switch (v.labelAlign[j])
	                {
		                case 0:
			                boldItalic.alignment = TextAnchor.MiddleLeft;
			                break;
		                case 1:
			                boldItalic.alignment = TextAnchor.MiddleCenter;
			                break;
		                case 2:
			                boldItalic.alignment = TextAnchor.MiddleRight;
			                break;
	                }
                    
                    if (v.labelBold[j] && v.labelItalic[j])
                        boldItalic.fontStyle = FontStyle.BoldAndItalic;
                    else if (v.labelBold[j])
                        boldItalic.fontStyle = FontStyle.Bold;
                    else if (v.labelItalic[j])
                        boldItalic.fontStyle = FontStyle.Italic;

                    GUILayout.Label(v.label[j], boldItalic);
                    boldItalic.alignment = TextAnchor.MiddleLeft;
                }
                bool alignRight = (v.labelEnabled[j] && buttonLine);

                if (!alignRight)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }

                GUILayout.FlexibleSpace();
                for (int k = 0; k < 4; k += 1)
                {
                    if (v.buttonEnabled[j * 4 + k])
                    {
                        if (!v.buttonCondense[j] && !alignRight)
                            GUILayout.FlexibleSpace();

                        string style = "Button";
                        if (v.buttonCondense[j])
                        {
                            bool hasLeft = false;
                            bool hasRight = false;
                            for(int p = k - 1; p >= 0; p -= 1)
                                if (v.buttonEnabled[j * 4 + p])
                                    hasLeft = true;
                            for (int p = k + 1; p < 4; p += 1)
                                if (v.buttonEnabled[j * 4 + p])
                                    hasRight = true;

                            if (!hasLeft && hasRight)
                                style = "ButtonLeft";
                            else if (hasLeft && hasRight)
                                style = "ButtonMid";
                            else if (hasLeft && !hasRight)
                                style = "ButtonRight";
                            else if (!hasLeft && !hasRight)
                                style = "Button";
                        }

                        if (GUILayout.Button(v.buttonText[j * 4 + k], style, GUILayout.MinWidth(60.0f)))
                        {
                            foreach (object t in targets)
                            {
                                MethodInfo m = t.GetType().GetMethod(v.buttonCallback[j * 4 + k], BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic);
                                if (m != null)
                                    m.Invoke(target, null);
                            }
                        }

                        if (!v.buttonCondense[j] && !alignRight)
                            GUILayout.FlexibleSpace();

                        
                    }
                }

                GUILayout.Space(4.0f);

                if (!(v.labelEnabled[j] && buttonLine))
                    GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
			}

			GUILayout.Space (space);
		}

		
		if (!string.IsNullOrEmpty(GUI.tooltip))
        {
            GUI.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            GUI.Box(tooltipRect, new GUIContent());
			EditorGUI.HelpBox(tooltipRect, GUI.tooltip, MessageType.Info);
			Repaint();
		}
		
		GUI.tooltip = "";

		so.ApplyModifiedProperties (); 
	}

    object GetTargetField(string name) { return target.GetType().GetField(name).GetValue(target); }

    void SetTargetField(string name, object value) { target.GetType().GetField(name).SetValue(target, value); }
	
	void VectorScene(InspectorPlusVar v, string s, Transform t)
	{
		Vector3 val;
		
		if (s == typeof(Vector3).Name)
			val = (Vector3)GetTargetField(name);
		else 
			val = ((Vector2)GetTargetField(name));
		
		Vector3 newVal = Vector3.zero;
		Vector3 curVal;
		bool setVal = false;
		bool relative = v.relative;
		bool scale = v.scale;

		switch (v.vectorDrawType) {
		case InspectorPlusVar.VectorDrawType.Direction:
			curVal = relative ? val:val - t.position;
            float size = scale ? Mathf.Min(2.0f, Mathf.Sqrt(curVal.magnitude) / 2.0f) : 1.0f;
            size *= HandleUtility.GetHandleSize(t.position);
			Handles.ArrowCap (0, t.position, curVal != Vector3.zero ? Quaternion.LookRotation (val.normalized) : Quaternion.identity, size);
			break;

		case InspectorPlusVar.VectorDrawType.Point:
			curVal = relative ? val:t.position + val;
			Handles.SphereCap (0, curVal, Quaternion.identity, 0.1f);
			break;

		case InspectorPlusVar.VectorDrawType.PositionHandle:
			curVal = relative ? t.position + val:val;
			setVal = true;
			newVal = Handles.PositionHandle (curVal, Quaternion.identity) - (relative ? t.position : Vector3.zero);
			break;

		case InspectorPlusVar.VectorDrawType.Scale:
			setVal = true;
            curVal = relative ? t.localScale + val :val;
            newVal = Handles.ScaleHandle(curVal, t.position + v.offset, t.rotation, HandleUtility.GetHandleSize(t.position + v.offset)) - (relative ? t.localScale : Vector3.zero);
			break;
			
		case InspectorPlusVar.VectorDrawType.Rotation:
			setVal = true;
            curVal = relative ? val + t.rotation.eulerAngles : val;
			newVal = Handles.RotationHandle(Quaternion.Euler(curVal), t.position + v.offset).eulerAngles - (relative?t.rotation.eulerAngles:Vector3.zero);
			break;
		}
	
		if (setVal)
		{
			object newObjectVal = newVal;
			
			if (s==typeof(Vector2).Name)
				newObjectVal = (Vector2)newVal;
			else if (s == typeof(Quaternion).Name)
				newObjectVal = Quaternion.Euler(newVal);
						
			SetTargetField(name, newObjectVal);
		}
	}
	
	void QuaternionScene(Transform t, Vector3 offset)
	{
		var val = (Quaternion)GetTargetField(name);
		SetTargetField(name, Handles.RotationHandle (val, t.position + offset));
	}

	//some magic to draw the handles
	public void OnSceneGUI ()
	{
		if (manager == null)
			return;

		Transform t = ((MonoBehaviour)target).transform;
		
		foreach (InspectorPlusVar v in tracker.GetVars()) {
			if (!v.active)
				continue;

			string s = v.type;
			name = v.name;

			if (s == typeof(Vector3).Name || s == typeof(Vector2).Name) 
				VectorScene(v, s, t);
			else if (s == typeof(Quaternion).Name && v.QuaternionHandle) 
				QuaternionScene(t, v.offset);
		}
	}
}
