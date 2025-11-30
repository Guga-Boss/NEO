using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Reflection;

public class EditorExtension : Editor
{
    [MenuItem( "Window/Update Lists" )]
    static void Example()
    {
        EditorApplication.hierarchyWindowChanged += HierarchyChangedFunction;
        HierarchyChangedFunction();
    }
    
#if UNITY_EDITOR
    static void HierarchyChangedFunction()
    {
        if( Application.isPlaying ) return;
        //GameObject ob = GameObject.Find( "Farm" );
        //Farm f = ob.GetComponent<Farm>();
        //f.UpdateListsCallBack();
        //Inventory.InitReference();
        //Debug.Log( "Lists Updated" );
    }

#endif
}
