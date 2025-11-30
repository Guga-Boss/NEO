#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using System;
using System.IO;

public class InspectorPlusWindow : EditorWindow
{
    public bool editing = false;
    public InspectorPlusManager manager;
    List<string> names;
    InspectorPlusTracker editComp;
    Vector2 scrollPosition;
    Vector2 openScrollPosition;
    string searchFilter = "";

    public string assetPath { get { return Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this))); } }

    public string filePath
    {
        get
        {
            int index = Application.dataPath.LastIndexOf("Assets");
            return Application.dataPath.Substring(0, index) + Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)));
        }
    }


    void OnEnable()
    {
        manager = (InspectorPlusManager)AssetDatabase.LoadAssetAtPath(assetPath + "/InspectorPlus.asset", typeof(InspectorPlusManager));

	    if (manager != null) return;
	    manager = (InspectorPlusManager) CreateInstance(typeof(InspectorPlusManager));
	    AssetDatabase.CreateAsset(manager, assetPath + "/InspectorPlus.asset");
    }

    void OnDisable()
    {
        manager.Save();

    }

    [MenuItem("Window/Inspector++")]
    static void ShowWindow()
    {
        GetWindow(typeof(InspectorPlusWindow));
    }

    void CreateFile(string name, string inspector = "")
    {
        string fileName = name + "InspectorPlus";

        File.WriteAllText(filePath + "/" + fileName + ".cs",
@"using UnityEngine;
using System.Collections;
using UnityEditor;
[CanEditMultipleObjects]
[CustomEditor(typeof(" + name + @"))]
public class " + fileName + @" : InspectorPlus" + inspector + @" {
}");
    }

    public void CreateNew(string name, string path, string group = "")
    {
        if (manager.GetTracker(name) != null)
            return;
        if (group != "PlayMaker")
            CreateFile(name);
        else
            CreateFile(name, "PlayMaker");

        manager.AddInspector(name, path, group);
    }

    void OnSelectionChange()
    {
        Repaint();
    }

    public void OnGUI()
    {
        if (!editing)
            DrawOpen();
        else
            DrawEditor();
    }

    bool CanHaveEditor(MonoScript m)
    {
        if (m.GetClass() == null)
            return false;

        if (m.GetClass().IsSubclassOf(typeof(MonoBehaviour)))
            return true;

        if (m.GetClass().IsSubclassOf(typeof(ScriptableObject)))
        {
            if (!m.GetClass().IsSubclassOf(typeof(Editor)) && !m.GetClass().IsSubclassOf(typeof(EditorWindow)))
                return true;
        }

        return false;
    }

    void DrawOpenNameList(List<string> names)
    {
        foreach (string n in names)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(n, GUILayout.Width(200.0f));
            GUILayout.Space(100.0f);

            if (GUILayout.Button("Edit", GUILayout.Width(180.0f)))
            {
                editing = true;
                manager.editName = n;
            }

            if (GUILayout.Button("Save to file", GUILayout.Width(100.0f)))
            {
                var filer = new InspectorPlusFiler();
                filer.WriteToFile(n, manager.GetTracker(n), "InspectorPlusOutput/Editor");
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Inspector save to file", @"
Your inspector has been saved to " + n + @"InspectorPlus.cs. Feel free to distribute this file! (Note that this file must be in a folder called Editor to activate)
	
To support Inspector++ a subtle watermark has been added. If you really disagree with this you are free to go in the .cs file and remove the watermark.", "Ok");
            }

            if (GUILayout.Button("Delete", GUILayout.Width(100.0f)))
            {
                manager.DeleteInspector(n);
                AssetDatabase.Refresh();
                Repaint();
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    void DrawExtensions()
    {
        GUILayout.BeginHorizontal();



        //Included inspectors

        //NGUI Button
        if (!manager.groups.Contains("NGUI"))
        {
            if (AssetDatabase.LoadAssetAtPath(assetPath + "/" + "NGUI.asset", typeof(InspectorPlusManager)) != null)
            {
                if (GUILayout.Button(new GUIContent(manager.NGUIImage), GUILayout.Width(115.0f), GUILayout.Height(40.0f)) && EditorUtility.DisplayDialog("NGUI Import", "You are about to import custom Inspectors for NGUI. Only do this when you are sure you have (the latest) NGUI installed", "I have the latest NGUI", "Cancel"))
                {
                    manager.ImportManager("NGUI");
                    var imp = new InspectorPlusImporter();
                    List<InspectorPlusTracker> group = manager.GetGroupTrackers("NGUI");

                    foreach (MonoScript m in imp.Importable("NGUI"))
                    {
                        CreateFile(m.name);

                        InspectorPlusTracker t = group.Find(tracker => tracker.name == m.name);

                        if (t == null)
                            continue;

                        t.filePath = Application.dataPath + AssetDatabase.GetAssetPath(m).Replace("Assets", "");
                    }

                    AssetDatabase.Refresh();
                }
            }
        }
        else if (GUILayout.Button("Delete NGUI Inspectors"))
        {
            var i = new InspectorPlusImporter();

            foreach (MonoScript m in i.Importable("NGUI"))
                manager.DeleteInspector(m.name);

            AssetDatabase.Refresh();
        }

        GUILayout.FlexibleSpace();


        //Playmaker
	    if (Type.GetType("InspectorPlusPlayMakerWindow") == null) return;

	    if (GUILayout.Button(manager.playMakerImage, GUILayout.Width(115.0f), GUILayout.Height(40.0f)))
	    {
		    MethodInfo m = Type.GetType("InspectorPlusPlayMakerWindow").GetMethod("ShowWindow", BindingFlags.Static | BindingFlags.Public);
		    m.Invoke(null, null);
	    }
    }

    void DrawOpen()
    {
        //gets only existing editors.

        GUILayout.BeginHorizontal(GUILayout.Width(Screen.width));
        GUILayout.FlexibleSpace();//left sidebar

        GUILayout.BeginVertical();
        GUILayout.Space(50.0f);
        GUILayout.BeginVertical("Box", GUILayout.Width(700.0f));


        GUILayout.BeginHorizontal();
        searchFilter = GUILayout.TextField(searchFilter, "SearchTextField", GUILayout.Width(670.0f));
        if (GUILayout.Button("", "SearchCancelButton", GUILayout.Width(20.0f)))
            searchFilter = "";
        GUILayout.EndHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(700.0f));
        openScrollPosition = GUILayout.BeginScrollView(openScrollPosition, false, false, GUIStyle.none, GUI.skin.GetStyle("verticalScrollbar"), GUILayout.Width(700.0f));

        //draw existing editors
        for (int i = 0; i < manager.groups.Count; i += 1)
        {
            if (manager.groups[i] != "")
                manager.groupOpen[i] = EditorGUILayout.Foldout(manager.groupOpen[i], manager.groups[i]);

            if (manager.groups[i] == "" || manager.groupOpen[i])
                DrawOpenNameList(manager.GetGroup(manager.groups[i], searchFilter));
        }

        GUILayout.Space(20.0f);

        /*
        //this is something i use to produce the NGUI inspectors
        if (GUILayout.Button("produce inspectors"))
        {
            InspectorPlusFiler filer = new InspectorPlusFiler();
			
            foreach (string s in manager.GetGroup("NGUI"))
            {
                filer.WriteToFile(s, manager.GetTracker(s), "InspectorPlusOutput/Editor");
            }

            AssetDatabase.Refresh();
        }
        */

        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.EndVertical();
        GUILayout.Space(40.0f);

        GUILayout.BeginHorizontal(GUILayout.Width(700.0f));
        GUILayout.FlexibleSpace();

        GUILayout.BeginVertical("Button", GUILayout.Width(180.0f));



        UnityEngine.Object[] objs = Selection.GetFiltered(typeof(MonoScript), SelectionMode.Assets);
        var selected = new List<MonoScript>();

        foreach (UnityEngine.Object o in objs)
        {
            var m = (MonoScript)o;

            if (m != null && CanHaveEditor(m) && !File.Exists(filePath + "/" + m.GetClass().Name + "InspectorPlus.cs"))
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(m.GetClass().Name);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                selected.Add(m);
            }
        }

        if (selected.Count == 0)
        {
            GUILayout.Label("Select scripts from the project view that need Inspector++'s magic!");
            GUI.enabled = false;
        }

        if (GUILayout.Button("Create"))
        {
            foreach (MonoScript m in selected)
                CreateNew(m.GetClass().Name, Application.dataPath + AssetDatabase.GetAssetPath(m).Replace("Assets", ""));

            AssetDatabase.Refresh();
        }

        GUI.enabled = true;

        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();


        GUILayout.EndVertical();

        //right sidebar
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();



        DrawExtensions();
    }

    void DrawEditor()
    {
        editComp = manager.GetTracker(manager.editName);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        editComp.DrawGUI();
        GUILayout.EndScrollView();

        GUILayout.Space(10.0f);
        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Back"))
            editing = false;

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(10.0f);
        GUILayout.FlexibleSpace();
    }
}

#endif